using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPXcore {
    public partial class SuperLinkMan{
		static public DevelopWin devWin; //<<>>development
        private const int      S=1, W=2;
        private GNPX_AnalyzerMan  pAnMan;
        private Bit81[]        pHouseCells;
        private List<UCell>    pBDL{ get{ return pAnMan.pBDL; } }

		public CellLinkMan     rCeLKMan;
        public CellLinkMan     CeLKMan{
			get{ 
				if(rCeLKMan==null)	rCeLKMan=new CellLinkMan(pAnMan);
				return rCeLKMan;
			}
		}

		public GroupedLinkMan  rGLKMan;
        public GroupedLinkMan  GLKMan{
			get{ 
				if(rGLKMan==null)	rGLKMan=new GroupedLinkMan(pAnMan);
				return rGLKMan;
			}
		}

		public ALSLinkMan      rALSMan;
        public ALSLinkMan      ALSMan{
			get{ 
				if(rALSMan==null)	rALSMan=new ALSLinkMan(pAnMan);
				return rALSMan;
			}
		}

		public Bit81[]         L2SprLkB81;
        public USuperLink[,]   L2SprLK;

//		private bool DevelopB=false;  //true; false;

        public SuperLinkMan( GNPX_AnalyzerMan pAnMan ){
            this.pAnMan  = pAnMan;
            this.pHouseCells = AnalyzerBaseV2.HouseCells;
		}

        public void Initialize(){
			CeLKMan.Initialize();
			GLKMan.Initialize();
			ALSMan.Initialize();
		}

        public void PrepareSuperLinkMan( bool AllF=false ){
            CeLKMan.PrepareCellLink(1+2);    //StrongLink,WeakLink
            if( AllF || GNPXApp000.GMthdOption["GroupedCells"]=="1" )  GLKMan.Initialize();
            if( AllF || GNPXApp000.GMthdOption["ALS"]=="1" )           ALSMan.QSearch_AlsInnerLink();

			L2SprLkB81 = new Bit81[9];
			for(int k=0; k<9; k++) L2SprLkB81[k]=new Bit81();
			L2SprLK=new USuperLink[81,9];
        }

        //##### There is a bug #####
        public USuperLink get_L2SprLK( int rc, int no, bool FullSearchB, bool DevelopB ){
            USuperLink USLK=null;
            if( FullSearchB || !L2SprLkB81[no].IsHit(rc) ){                             // Search when Unconditional or Unprocessed
                USLK = Eval_SuperLinkChain(rc,no,Gbreak:false,typeSel:3,DevelopB:DevelopB);       // Origin(rc#no)       
				L2SprLkB81[no].BPSet(rc);                                               // Set the searched flag
                if(DevelopB) developDisp( rc, no, USLK, DevelopB );
                L2SprLK[rc,no]=USLK;
            }

            USLK = L2SprLK[rc,no];
			//if(DevelopB) developDisp( rc, no, USLK, DevelopB );        
            return USLK;
        }
 
        //##### There is a bug #####
        public USuperLink Eval_SuperLinkChain( int rc0, int no0, bool Gbreak, int typeSel, bool DevelopB ){
            // rc0: start cell's rc index 
            // no0: start digit
            // Gbreak(bool): T:If the evaluation values ​​are inconsistent, the search is suspended.
            // typeSel(int): Start link type1  1:Strong  2:Weak  3:S+W  0:-

			try{	
				int dbX=0;

				USuperLink USLK=new USuperLink(rc0,no0);
				var Qtrue =USLK.Qtrue;
				var Qfalse=USLK.Qfalse;
				var chainDesLKT=USLK.chainDesLKT;
				var chainDesLKF=USLK.chainDesLKF;
				var rcQue=new Queue<GroupedLink>();

				UCell P0=pBDL[rc0];     //Origin Cell
				int   no0B=1<<no0;      //Origin Digit
				Qtrue[no0].BPSet(rc0);
				GroupedLink GLKnoR0 = new GroupedLink(P0,no0,no0,W,rootF:true);
				GLKnoR0.preGrpedLink=null;
				chainDesLKT[rc0,no0]=GLKnoR0;

            #region stsrt
                if( (typeSel&2)>0 ){//====================== Start with WeakLink(origin:rc#no0) ======================
				    foreach( var GLKH in IEGet_SuperLinkFirst(P0,no0).Where(X=>X.type==W) ){	
						    if(DevelopB) WriteLine("*1st:"+GLKH.GrLKToString());
					    GLKH.UsedCs |= GLKH.UGCellsB.B81;                           //set usedcells(accumulate)
					    GLKH.preGrpedLink = GLKnoR0;                                //set pre-GLink
					    rcQue.Enqueue(GLKH);                                        //enqueue next-GLink to RadiationSearchQueue

					    foreach( var P in GLKH.UGCellsB.Where(p=>(p.FreeB&no0B)>0) ){
							int no2=GLKH.no2;
							Qfalse[no2].BPSet(P.rc);                                //set p.rc#no2 to false
							chainDesLKF[P.rc,no2]=GLKH;                             //record in retroactive chain
							if( Gbreak && Qtrue[no2].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],no2) ) goto FTBreak; //found error
						
							if(P.FreeBC==2){                                        //If the next is a binary cell
								int nox=P.FreeB.DifSet(1<<no2).BitToNum();            //nox:another digit in the cell
								Qtrue[nox].BPSet(P.rc);                               //P.rc#nox is true
								GroupedLink GLKbv = new GroupedLink(P,no2,nox,S);     //GLKbv:GLink in cell
								GLKbv.preGrpedLink=GLKH;                              //set preLink
								chainDesLKT[P.rc,nox]=GLKbv;                          //record in retroactive chain
                                    if(DevelopB) WriteLine($"        12 W *Rad->{P}  --#{no2} is false.");
								if( Gbreak && Qfalse[nox].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],nox) ) goto FTBreak; //found error
							}
					    }
				    }

				    //S start cell, non-conscious digits
				    foreach( var nox in P0.FreeB.IEGet_BtoNo().Where(n=>(n!=no0)) ){//non-conscious digits in originCell
					    Qfalse[nox].BPSet(rc0);                                     // is false
				    }
                }

                if( (typeSel&1)>0 ){//====================== Start with StrongLink(origin:rc#no0) ======================
				    foreach( var GLKH in IEGet_SuperLinkFirst(P0,no0).Where(X=>X.type==S) ){	
					    int nox=GLKH.no2;
					    GroupedLink GLKnoR1 = new GroupedLink(P0,no0,nox,W,rootF:false);//nextGLink
					    GLKnoR1.preGrpedLink=GLKnoR0;                               //set pre-GLink to next-GLink
					    chainDesLKF[rc0,nox]=GLKnoR1;                               //record in retroactive chain

					    GLKH.UsedCs |= GLKH.UGCellsB.B81;                           //set usedcells(accumulate)
					    GLKH.preGrpedLink = GLKnoR1;                                //set pre-GLink
					    rcQue.Enqueue(GLKH);                                        //enqueue next-GLink to RadialSearchQueue
					    if( GLKH.UGCellsB.Count==1 ){                               //if the link's next element is a single cell
						    int no2=GLKH.no2;                                         //no2:next digit
						    var P=GLKH.UGCellsB[0];                                   //P:next cell
						    Qtrue[no2].BPSet(P.rc);                                   //set P.rc#no2 is true
						    chainDesLKT[P.rc,no2]=GLKH;                               //record in retroactive chain
						    if( Gbreak && Qfalse[no2].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],no2) ) goto FTBreak; //found error       
						    Qtrue[nox].BPSet(P.rc);                                   //set P.rc#no is true

						    GroupedLink GLKno = new GroupedLink(P,nox,no2,W);         //GLKno:GLink in cell
						    GLKno.preGrpedLink=GLKH;                                  //set preLink
						    chainDesLKF[P.rc,no2]=GLKno;                              //record in retroactive chain
						    if( Gbreak && Qtrue[nox].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],nox) ) goto FTBreak; //found error
					    }
				    }
                }
            #endregion stsrt

            #region Radial Search
                //====================== Radial Search ===============================
                while(rcQue.Count>0){   
					GroupedLink R = rcQue.Dequeue();                                            //dequeue next element
						if(DevelopB) WriteLine($"{dbX++}---Queue:"+R.GrLKToString());
					
                    foreach( var GLKH in IEGet_SuperLink(R) ){                                  //foreach next GLink                            
							if(DevelopB) WriteLine($"   {dbX++}--GLKH:"+GLKH.GrLKToString());
						if(R.type!=GLKH.type && R.UGCellsA.Equals(GLKH.UGCellsB) )  continue;   //Skip back links
                        
						GLKH.preGrpedLink=R;                                                    //set preLink
						int no2=GLKH.no2;                                                       //no2:next digit
						if(GLKH.type==S){                                                   //Check connection conditions. case StrongLink
							if(!(GLKH.UGCellsB.B81&Qtrue[GLKH.no2]).IsZero() )  continue;   //if the next element is setted true, go to the next process
							if(GLKH.UGCellsB.Count==1){                                     //if the link's next element is a single cell
								var P=GLKH.UGCellsB[0];                                       //P:next cell
								Qtrue[no2].BPSet(P.rc);                                       //set P.rc#no2 is true
								chainDesLKT[P.rc,no2]=GLKH;                                   //record in retroactive chain
								if( Gbreak && Qfalse[no2].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],no2) ) goto FTBreak; //found error
								foreach( var nox in P.FreeB.IEGet_BtoNo().Where(q=>(q!=no2)) ){                                
									Qfalse[nox].BPSet(P.rc);                                  //set p.rc#no2 to false
									GroupedLink GLKno = new GroupedLink(P,no2,nox,W);         //GLKbv:GLink in cell(P#no2 WeakLink)
									GLKno.preGrpedLink=GLKH;                                  //set preLink
									chainDesLKF[P.rc,nox]=GLKno;                              //record in retroactive chain

									if( Gbreak && Qtrue[nox].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],nox) ) goto FTBreak; //found error
                                        if(DevelopB) WriteLine($"        31S ->P:{P}  --#{nox} is false.");
								}
							}
						}
						if(GLKH.type==W){                                                   //Check connection conditions. case WeakLink
							if(!(GLKH.UGCellsB.B81&Qfalse[GLKH.no2]).IsZero() )  continue;  //if the next element is setted false, go to the next process
                            
							foreach( var P in GLKH.UGCellsB ){                              //foreach next GLink                     
								Qfalse[no2].BPSet(P.rc);                                      //set P.rc#no2 is true
								chainDesLKF[P.rc,no2]=GLKH;                                   //record in retroactive chain
								if( Gbreak && Qtrue[no2].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],no2) ) goto FTBreak; //found error
                                    if(DevelopB) WriteLine($"        40S ->P:{P}  ++#{no2} is false."); 
							}

							if( GLKH.UGCellsB.Count==1 ){                                   //if the link's next element is a single cell                  
								var P=GLKH.UGCellsB[0];   
								if(P.FreeBC==2){                                              //If the next is a binary cell
									int nox=P.FreeB.DifSet(1<<no2).BitToNum();	                                
									Qtrue[nox].BPSet(P.rc);		                  		      //set P.rc#no2 is true	
                                        if(DevelopB) WriteLine($"        41S ->P:{P}  ++#{nox} is true."); 
									GroupedLink GLKno = new GroupedLink(P,no2,nox,S);         //GLKbv:GLink in cell(P#no2 StrongLink)
									GLKno.preGrpedLink=GLKH;                                  //set preLink
									chainDesLKT[P.rc,nox]=GLKno;                              //record in retroactive chain
									if( Gbreak && Qfalse[nox].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],nox) ) goto FTBreak; //found error
								}
							}
						}
                        GLKH.GenNo = R.GenNo+1;
						rcQue.Enqueue(GLKH);                                                //enqueue next-GLink to RadiationSearchQueue						
					}

				} 
            #endregion Radial Search

				USLK.SolFound=true;                                                         //Solution found
				return USLK;

			  FTBreak:                                                                      //Failed
				USLK.SolFound=false;
				return USLK;
			}
			catch( Exception ex ){
				WriteLine($"{ex.Message}+\r{ex.StackTrace}");
			}
			return null;
        }

        public IEnumerable<GroupedLink> IEGet_SuperLinkFirst( UCell UC, int no ){
            List<UCellLink> Plst=CeLKMan.CeLK81[UC.rc];
            if( Plst!=null ){
                foreach( var LK in Plst.Where(p=> ((p.no==no)&&(p.type==W))) )  yield return (new GroupedLink(LK));      
                foreach( var LK in Plst.Where(p=> ((p.no!=no)&&(p.type==S))) )  yield return (new GroupedLink(LK));      
            }

            UGrCells GUC=new UGrCells(-9,no,UC);
            if( GNPXApp000.GMthdOption["GroupedCells"]=="1" ){
                foreach( var GLK in GLKMan.GrpCeLKLst ) if( GLK.UGCellsA.Equals(GUC) ) yield return GLK;
            }

            //first link is not ALS
            yield break;
        }       
        public IEnumerable<GroupedLink> IEGet_SuperLink( GroupedLink GLKpre ){
            int SWCtrl=GLKpre.type;
            bool ALSpre=GLKpre is ALSLink;

            if(GLKpre.UGCellsB.Count==1){
                UCell U=GLKpre.UGCellsB[0];
                List<UCellLink> Plst=CeLKMan.CeLK81[U.rc];
                if( Plst!=null ){
                    foreach( var LK in Plst ){
                        if( ALSpre && LK.type!=W ) continue;
                        GroupedLink GLK = new GroupedLink(LK);
                        if( Check_SuperLinkSequence(GLKpre,GLK) ) yield return GLK;      
                    }
                }
            }

            if(GNPXApp000.GMthdOption["GroupedCells"]=="1"){
                foreach( var GP in GLKMan.GrpCeLKLst){
                    if( ALSpre && GP.type!=W ) continue;                       
                    if( !GLKpre.UGCellsB.EqualsRC(GP.UGCellsA) )  continue;
                    if( GLKpre.no2!=GP.no ) continue;
                    if( Check_SuperLinkSequence(GLKpre,GP) ) yield return GP; 
                }
            }

            if(GNPXApp000.GMthdOption["ALS"]=="1" && ALSMan.AlsInnerLink!=null){
                if( GLKpre.type==W ){
                    foreach( var GP in ALSMan.AlsInnerLink.Where(p=>(p.ALSbase.Level==1)) ){
                        if( GLKpre.no2!=GP.no ) continue;
                        if( GLKpre.UGCellsB.Equals(GP.UGCellsA) ) yield return GP; 
                    }
                }
            }

            if(ALSpre){
                ALSLink ALK=GLKpre as ALSLink;
                int noB = 1<<ALK.no2;
                Bit81 BPnoB = new Bit81(pBDL,noB);

                Bit81 BP= BPnoB&ALK.UGCellsB.B81;
            //      ALK.UGCellsB.ForEach(P=>{ if((P.FreeB&noB)>0) BP.BPSet(P.rc); });

                Bit81 UsedCs=GLKpre.UsedCs;
                for(int tfx=0; tfx<27; tfx++ ){
                    Bit81 HS = BPnoB&pHouseCells[tfx];
                    if(!(BP-HS).IsZero())  continue;
                    if( (HS-BP).IsZero())  continue;

                    Bit81 NxtBP= HS-BP-UsedCs;
                    if(NxtBP.IsZero())  continue;

//C                        WriteLine("\n tfx:"+tfx );
//C                        WriteLine( "   BP:"+BP );
//C                        WriteLine( "   HS:"+HS );
//C                        WriteLine( "HS-BP:"+(HS-BP) );
//C                        WriteLine( "NxtBP:"+NxtBP );

                    List<UCell> NxtCs= NxtBP.ToList().ConvertAll(rc=>pBDL[rc]);
                    for(int k=1; k<(1<<NxtCs.Count); k++ ){
                        UGrCells NxtGrpdCs=new UGrCells(tfx,ALK.no2);
                        int kb=k;
                        for(int n=0; n<NxtCs.Count; n++){
                            if( (kb&1)>0 )  NxtGrpdCs.Add( new UGrCells(NxtCs[n],ALK.no2) );
                            kb>>=1;
                        }
                        GroupedLink GP = new GroupedLink(GLKpre.UGCellsB,NxtGrpdCs,tfx,W);
//C                        WriteLine( GP );
                        yield return GP; 
                    }

                }
            }
            yield break;
        }         
        public bool Check_SuperLinkSequence( GroupedLink GLKpre, GroupedLink GLKnxt ){
            if(GLKpre==null)  WriteLine("null");
            int typP=GLKpre.type;
            if( GLKpre is ALSLink )  typP=S;
            int noP =GLKpre.no2;
                 
            int typN=GLKnxt.type;
            int noN=GLKnxt.no;
                
            UCellLink LKpre = GLKpre.UCelLK;
            UCellLink LKnxt = GLKnxt.UCelLK;

            int FreeBC=0;
            if( LKpre!=null ){ 
                FreeBC = pBDL[LKpre.rc2].FreeBC;

                if(LKnxt!=null){  //singleLink -> singleLink
                    return _Check_SWSequenceSub(typP,noP, LKnxt.type,noN, FreeBC);
                }
                else{               //singleLink -> multiLink
                    UGrCells UGrCs=GLKnxt.UGCellsA;
                    if(UGrCs.Count==1){ //singleCell -> singleCell
                        return _Check_SWSequenceSub(typP,noP, typN,noN, FreeBC);
                    }
                }
            }
            else if( GLKpre.UGCellsB.Count==1 && LKnxt!=null ){ // multiLink -> singleLink
                FreeBC=GLKpre.UGCellsB.FreeB.BitCount();
                return _Check_SWSequenceSub(typP,noP, typN,noN, FreeBC);
            }

            FreeBC=GLKpre.UGCellsB.FreeB.BitCount();
            return _Check_SWSequenceSub(typP,noP, typN,noN, FreeBC);
        }
		private bool _Check_SWSequenceSub( int typPre, int noPre, int typNxt, int noNxt, int FreeBC ){
            //true: continuous   false:discontinuous
            switch(typPre){
                case S:
                    switch(typNxt){
                        case S: return (noPre!=noNxt);  //S->S
                        case W: return (noPre==noNxt);  //S->W
                    }
                    break;
                case W:
                    switch(typNxt){
                        case S: return (noPre==noNxt);  //W->S
                        case W: return ((noPre!=noNxt)&&(FreeBC==2)); //W->W
                    }
                    break;
            }
            return false;
        }

//============================================================================================================
		public bool GenMessage2( USuperLink USLK, UCell P, int no ){
            var pQtrue =USLK.Qtrue;
            var pQfalse=USLK.Qfalse;
			if( !pQfalse[no].IsHit(P.rc) || !pQtrue[no].IsHit(P.rc) )  return false;

			var pchainDesLKT=USLK.chainDesLKT;    //■ 
            var pchainDesLKF=USLK.chainDesLKF;　　//□

			GroupedLink Pdes=(GroupedLink)pchainDesLKT[P.rc,no];  //■ 
			string st = "";
			if(Pdes!=null)   st += "   "+_chainToString2(P,Pdes,-(no+1));   //■_chainToString2
			Pdes=(GroupedLink)pchainDesLKF[P.rc,no];　//□
			if(Pdes!=null){
				if(st.Length>4)  st += "\r";
                else  st="";
				st += "   "+_chainToString2(P,Pdes,no+1);                   //■_chainToString2
			}
			USLK.stMsg=st;
			return true;
		}
	
        public string _GenMessage2true( USuperLink USLK, UCell PX, int noX ){
			GroupedLink Pdes=(GroupedLink)USLK.chainDesLKT[PX.rc,noX];   //■ 
			string st = "";
			if(Pdes!=null)   st += "   "+_chainToString2(PX,Pdes,-(noX+1));  //■_chainToString2
			return st;
		}
        public string GenMessage2FakeProposition(int rc0, int no, USuperLink USLK, UCell Q ){
            string st="";
            st += $"ForceChain_Cell r{(rc0/9+1)}c{(rc0%9)+1}/{(no+1)} is false(contradition)";
            st += "\r"+ _GenMessage2true(USLK,Q,no);
            st += "\r"+ _GenMessage2false(USLK,Q,no);
            return st;
        }
		public string _GenMessage2false( USuperLink USLK, UCell P, int no ){
			GroupedLink Pdes=(GroupedLink)USLK.chainDesLKF[P.rc,no];  //□
			string st = "";
			if(Pdes!=null)   st += "   "+_chainToString2(P,Pdes,-(no+1));   //■_chainToString2
			return st;
		}
		public string _chainToString2( UCell U, GroupedLink Gdes, int noRem ){
            string st="";
            try{     
                if(Gdes==null)  return st;
                var Qlst=new List<GroupedLink>();

                var X=Gdes;
                while( !(X.tfx==-1 && X.no==X.no2 && X.rootF) ){
                    Qlst.Add(X);
                    X=X.preGrpedLink as GroupedLink;
				    if(X==null || Qlst.Count>20) break;
                }
                Qlst.Reverse();
                st = "";
                foreach( var R in Qlst ){ st += R.GrLKToString()+ " => "; };
                if(st.Length>4)  st = st.Substring(0,st.Length-4);

                if(Qlst.Count>20) st=" ## loop? error ##"+st;
            }
            catch(Exception e){ WriteLine($"{e.Message}\r{e.StackTrace}"); }
            return st;
        }
//============================================================================================================
    }
}