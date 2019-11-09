using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Console;

using GIDOO_space;

namespace GNPZ_sdk {
    public partial class SuperLinkMan{
		public static DevelopWin devWin; //<<>>development
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

		public GroupedLinkMan     rGLKMan;
        public GroupedLinkMan     GLKMan{
			get{ 
				if(rGLKMan==null)	rGLKMan=new GroupedLinkMan(pAnMan);
				return rGLKMan;
			}
		}

		public ALSLinkMan     rALSMan;
        public ALSLinkMan     ALSMan{
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
            CeLKMan.PrepareCellLink(1+2);    //strongLink,weakLink
            if( AllF || GNPXApp000.GMthdOption["GroupedCells"]=="1" )  GLKMan.Initialize();
            if( AllF || GNPXApp000.GMthdOption["ALS"]=="1" )           ALSMan.QSearch_AlsInnerLink();

			L2SprLkB81 = new Bit81[9];
			for(int k=0; k<9; k++ ) L2SprLkB81[k]=new Bit81();
			L2SprLK=new USuperLink[81,9];
        }

		public USuperLink get_L2SprLK( int rc, int no, bool CorrectF, bool DevelopB ){
            USuperLink USLK=null;
            if( CorrectF || !L2SprLkB81[no].IsHit(rc) ){
                USLK = Eval_SuperLinkChain(rc,no,Gbreak:false,DevelopB:DevelopB);
				L2SprLkB81[no].BPSet(rc);
                if( DevelopB ) developDisp( rc, no, USLK, DevelopB );
                L2SprLK[rc,no]=USLK;
            }

            USLK = L2SprLK[rc,no];
			//if( DevelopB ) developDisp( rc, no, USLK, DevelopB );        
            return USLK;
        }			
		public USuperLink Eval_SuperLinkChain( int rc0, int no0, bool Gbreak, bool DevelopB ){  //37
								//Gbreak True:discontinue if find false False:continue
			try{	
				int dbX=0;

				USuperLink USLK=new USuperLink(rc0,no0);
				var Qtrue =USLK.Qtrue;
				var Qfalse=USLK.Qfalse;
				var chainDesLKT=USLK.chainDesLKT;
				var chainDesLKF=USLK.chainDesLKF;

				var rcQue=new Queue<GroupedLink>();

				//W start cell, number
				UCell P0=pBDL[rc0];
				int   no0B=1<<no0;
				Qtrue[no0].BPSet(rc0);
				GroupedLink GLKnoR0 = new GroupedLink(P0,no0,no0,W,rootF:true);
				GLKnoR0.preGrpedLink=null;
				chainDesLKT[rc0,no0]=GLKnoR0;

				foreach( var GLKH in IEGet_SuperLinkFirst(P0,no0).Where(X=>X.type==W) ){	
						if(DevelopB) WriteLine("*1st:"+GLKH.GrLKToString());
					GLKH.UsedCs |= GLKH.UGCellsB.B81;
					GLKH.preGrpedLink = GLKnoR0;
					rcQue.Enqueue(GLKH);

					foreach( var P in GLKH.UGCellsB ){
						if( (P.FreeB&no0B)>0 ){
							int no2=GLKH.no2;
							Qfalse[no2].BPSet(P.rc);
							chainDesLKF[P.rc,no2]=GLKH;
							if( Gbreak && Qtrue[no2].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],no2) ) goto FTBreak;
						
							if(P.FreeBC==2){
								int nox=P.FreeB.DifSet(1<<no2).BitToNum();								                                
								Qtrue[nox].BPSet(P.rc);
								GroupedLink GLKbv = new GroupedLink(P,no2,nox,S);
								GLKbv.preGrpedLink=GLKH;
								chainDesLKT[P.rc,nox]=GLKbv;
								if( Gbreak && Qfalse[nox].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],nox) ) goto FTBreak;
							}
						}
					}
				}

				//S start cell, non-conscious number
				foreach( var nox in P0.FreeB.IEGet_BtoNo().Where(n=>(n!=no0)) ){
					Qfalse[nox].BPSet(rc0);
				}

				foreach( var GLKH in IEGet_SuperLinkFirst(P0,no0).Where(X=>X.type==S) ){	
					int nox=GLKH.no2;
					GroupedLink GLKnoR1 = new GroupedLink(P0,no0,nox,W,rootF:false);
					GLKnoR1.preGrpedLink=GLKnoR0;
					chainDesLKF[rc0,nox]=GLKnoR1;

					GLKH.UsedCs |= GLKH.UGCellsB.B81;
					GLKH.preGrpedLink = GLKnoR1;
					rcQue.Enqueue(GLKH);
					if( GLKH.UGCellsB.Count==1 ){
						int no2=GLKH.no2;
						var P=GLKH.UGCellsB[0];                    
						Qtrue[no2].BPSet(P.rc);
						chainDesLKT[P.rc,no2]=GLKH;
						if( Gbreak && Qfalse[no2].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],no2) ) goto FTBreak;            
						Qtrue[nox].BPSet(P.rc);

						GroupedLink GLKno = new GroupedLink(P,nox,no2,W);
						GLKno.preGrpedLink=GLKnoR0;
						GLKno.preGrpedLink=GLKH;
						chainDesLKF[P.rc,no2]=GLKno;
						if( Gbreak && Qtrue[nox].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],nox) ) goto FTBreak;
					}
				}

				//=====================================================
				while(rcQue.Count>0){
					GroupedLink R = rcQue.Dequeue();
						if(DevelopB) WriteLine($"{dbX++}---Queue:"+R.GrLKToString());
					foreach( var GLKH in IEGet_SuperLink(R) ){
							if(DevelopB) WriteLine($"   {dbX++}--GLKH:"+GLKH.GrLKToString());
						if(R.type!=GLKH.type && R.UGCellsA.Equals(GLKH.UGCellsB) )  continue;
                        
						GLKH.preGrpedLink=R;
						int no2=GLKH.no2;
						if(GLKH.type==S){
							if(!(GLKH.UGCellsB.B81&Qtrue[GLKH.no2]).IsZero() )  continue;
							if(GLKH.UGCellsB.Count==1){
								var P=GLKH.UGCellsB[0];                            
								Qtrue[no2].BPSet(P.rc);
								chainDesLKT[P.rc,no2]=GLKH;
								if( Gbreak && Qfalse[no2].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],no2) ) goto FTBreak;
								foreach( var nox in P.FreeB.IEGet_BtoNo().Where(q=>(q!=no2)) ){                                
									Qfalse[nox].BPSet(P.rc);

									GroupedLink GLKno = new GroupedLink(P,no2,nox,W);
									GLKno.preGrpedLink=GLKH;
									chainDesLKF[P.rc,nox]=GLKno;

									if( Gbreak && Qtrue[nox].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],nox) ) goto FTBreak;
								}
							}
						}
						if(GLKH.type==W){
							if(!(GLKH.UGCellsB.B81&Qfalse[GLKH.no2]).IsZero() )  continue;
                            
							foreach( var P in GLKH.UGCellsB ){                            
								Qfalse[no2].BPSet(P.rc);
								chainDesLKF[P.rc,no2]=GLKH;
								if( Gbreak && Qtrue[no2].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],no2) ) goto FTBreak;
							}

							if( GLKH.UGCellsB.Count==1 ){
								var P=GLKH.UGCellsB[0];   
								if(P.FreeBC==2){
									int nox=P.FreeB.DifSet(1<<no2).BitToNum();								                                
									Qtrue[nox].BPSet(P.rc);

									GroupedLink GLKno = new GroupedLink(P,no2,nox,S);
									GLKno.preGrpedLink=GLKH;
									chainDesLKT[P.rc,nox]=GLKno;

									if( Gbreak && Qfalse[nox].IsHit(P.rc) && GenMessage2(USLK,pBDL[P.rc],nox) ) goto FTBreak;
								}
							}

						}
						rcQue.Enqueue(GLKH);						
					}
				}

				USLK.SolFound=true;
				return USLK;

			  FTBreak:
				USLK.SolFound=false;
				return USLK;
			}
			catch( Exception ex ){
				WriteLine(ex.Message);
				WriteLine(ex.StackTrace);
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

            if( GLKpre.UGCellsB.Count==1 ){
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

            if( GNPXApp000.GMthdOption["GroupedCells"]=="1" ){
                foreach( var GP in GLKMan.GrpCeLKLst){
                    if( ALSpre && GP.type!=W ) continue;                       
                    if( !GLKpre.UGCellsB.EqualsRC(GP.UGCellsA) )  continue;
                    if( GLKpre.no2!=GP.no ) continue;
                    if( Check_SuperLinkSequence(GLKpre,GP) ) yield return GP; 
                }
            }

            if( GNPXApp000.GMthdOption["ALS"]=="1" && ALSMan.AlsInnerLink!=null ){
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
		public bool GenMessage2( USuperLink USLK, UCell P, int no ){
            var pQtrue =USLK.Qtrue;
            var pQfalse=USLK.Qfalse;
			if( !pQfalse[no].IsHit(P.rc) || !pQtrue[no].IsHit(P.rc) )  return false;

			var pchainDesLKT=USLK.chainDesLKT;
            var pchainDesLKF=USLK.chainDesLKF;

			GroupedLink Pdes=(GroupedLink)pchainDesLKT[P.rc,no];
			string st = "";
			if(Pdes!=null)   st += "   "+_chainToString2(P,Pdes,-(no+1));
			Pdes=(GroupedLink)pchainDesLKF[P.rc,no];
			if(Pdes!=null){
				if(st.Length>2)  st += "\r";
				st += "   "+_chainToString2(P,Pdes,no+1);
			}
			USLK.stMsg=st;
			return true;
		}
		public string _GenMessage2true( USuperLink USLK, UCell P, int no ){
			GroupedLink Pdes=(GroupedLink)USLK.chainDesLKT[P.rc,no];
			string st = "";
			if(Pdes!=null)   st += "   "+_chainToString2(P,Pdes,-(no+1));
			return st;
		}
		public string _GenMessage2false( USuperLink USLK, UCell P, int no ){
			GroupedLink Pdes=(GroupedLink)USLK.chainDesLKF[P.rc,no];
			string st = "";
			if(Pdes!=null)   st += "   "+_chainToString2(P,Pdes,-(no+1));
			return st;
		}
		private string _chainToString2( UCell U, GroupedLink Gdes, int noRem ){
            string st="";
            if(Gdes==null)  return st;
            var Qlst=new List<GroupedLink>();

            var X=Gdes;
            while( !(X.tfx==-1 && X.no==X.no2 && X.rootF) ){
                Qlst.Add(X);
                X=X.preGrpedLink as GroupedLink;
				if(Qlst.Count>20) break;
            }
            Qlst.Reverse();
            st = "";
            foreach( var R in Qlst ){ st += R.GrLKToString()+ " => "; };
            if(st.Length>4)  st = st.Substring(0,st.Length-4);

            if(Qlst.Count>20) st=" ## loop? error ##"+st;
            return st;
        }
    }

    public class USuperLink{
        public int rc;
        public int no;
        public Bit81[]      Qtrue;
        public Bit81[]      Qfalse;
        public object[,]    chainDesLK;
        public object[,]    chainDesLKT;
        public object[,]    chainDesLKF;
		public bool			SolFound;
		public string       stMsg;

        public USuperLink( int rc, int no ){
            this.rc=rc; this.no=no;
            Qtrue=new Bit81[9];
            Qfalse=new Bit81[9];
            for(int k=0; k<9; k++ ){ Qtrue[k]=new Bit81();  Qfalse[k]=new Bit81(); }
            chainDesLK=new object[81,9];
            chainDesLKT=new object[81,9];
            chainDesLKF=new object[81,9];
        }
    }
}