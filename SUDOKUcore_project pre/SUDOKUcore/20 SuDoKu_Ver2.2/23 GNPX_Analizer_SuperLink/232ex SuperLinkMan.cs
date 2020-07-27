using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;

using GIDOO_space;

namespace GNPXcore {
    public partial class SuperLinkMan{
        // *** development is over ***
        public USuperLink get_L2SprLKEx( int rc0, int no0, bool FullSearchB, bool DevelopB ){
            USuperLink USLK=null;
            if( FullSearchB || !L2SprLkB81[no0].IsHit(rc0) ){                             // Search when Unconditional or Unprocessed    
                USLK = Eval_SuperLinkChainEx(rc0,no0,DevelopB:DevelopB);     // Origin(rc0#no0)       

                if(USLK==null)  return null;
				L2SprLkB81[no0].BPSet(rc0);                                               // Set the searched flag
                //if(DevelopB) developDispEx( rc0, no0, USLK, DevelopB );
                L2SprLK[rc0,no0]=USLK;    //origin#no0
            }

            USLK = L2SprLK[rc0,no0];
            //if(DevelopB) developDispEx( rc0, no0, USLK, DevelopB );        
            return USLK;
        }			

        //Search for groupedLink_sequence starting from rc0#no0 as true.
        public USuperLink Eval_SuperLinkChainEx( int rc0, int no0, bool DevelopB ){
			try{	
				int dbX=0;
                _dbCC=0;

				USuperLink GNL_Result=new USuperLink(rc0,no0);
				var Qtrue =GNL_Result.Qtrue;
				var Qfalse=GNL_Result.Qfalse;
				var chainDesLKT=GNL_Result.chainDesLKT;
				var chainDesLKF=GNL_Result.chainDesLKF;

				var rcQue=new Queue<GroupedLink>();

				UCell P0=pBDL[rc0];     //Origin Cell
				int   no0B=1<<no0;      //Origin Digit
				Qtrue[no0].BPSet(rc0);

                //P is Cell.  R,RR,RRR is GroupedLink
                {//====================== Start with WeakLink(origin:rc#no0) ======================
				    foreach( var R in IEGet_SuperLinkFirst(P0,no0).Where(X=>X.type==W) ){	
					            if(DevelopB){ Write("*1st:"+R.GrLKToString()); __Debug_ChainPrint(R); }

					    R.UsedCs = R.UGCellsA.B81;                                      //set usedcells(accumulate)
					    R.preGrpedLink = null;  //GLKnoR0;                              //set pre-GLink
					    rcQue.Enqueue(R);                                               //enqueue next-GLink to RadiationSearchQueue

					    foreach( var P in R.UGCellsB.Where(p=>(p.FreeB&no0B)>0) ){
							int no2=R.no2;
							Qfalse[no2].BPSet(P.rc);                                    //set p.rc#no2 to false
							chainDesLKF[P.rc,no2]=R;  //                               //record in retroactive chain
                                if(DevelopB) WriteLine($"        12 W *Rad->{P}  --#{no2} is false.");
					    } 
				    }
                    foreach( var nox in pBDL[rc0].FreeB.IEGet_BtoNo().Where(p=>p!=no0)){
                        Qfalse[nox].BPSet(rc0);
                    }
                }

				//====================== Radiation Search ===============================
				while(rcQue.Count>0){ 
                    {   //----- Distinct -----
                        var Q2=rcQue.ToList().Distinct();
                        rcQue.Clear();
                        foreach(var X in Q2) rcQue.Enqueue(X);
                            //if(DevelopB){ WriteLine("\r"); foreach(var P in rcQue) WriteLine($"--rcQue---{P.GrLKToString()}"); WriteLine("\r"); }
                    }

					GroupedLink R = rcQue.Dequeue();                                     //dequeue next element
						if(DevelopB) WriteLine($"\r{dbX++}---Queue:"+R.GrLKToString());

					foreach(var RR in IEGet_SuperLink(R).Where(p=>p.AvailableF) ){      //foreach next GLink 
                        RR.preGrpedLink=R;  //set preLink
                        if( !(R.UsedCs&RR.UGCellsB.B81).IsZero() )  continue;            //Skip by used Cells
                            if(DevelopB) WriteLine($"\r   {dbX++}--RR:"+RR.GrLKToString());
                       
                        RR.UsedCs = R.UsedCs | RR.UGCellsA.B81;                                                
                            if(DevelopB){ __Debug_ChainPrint(RR); WriteLine($"    {dbX++}-{++chkX}--RR:"+RR.GrLKToString()); }

						int no2=RR.no2;                                                  //no2:next digit
						if(RR.type==S){         // F->T   *=*=* Check connection conditions. case StrongLink *=*=*
                            if(RR.UGCellsB.Count==1){                                    //if the link's next element is a single cell
							    var P=RR.UGCellsB[0];                                    //P:next cell
                                if( Qtrue[no2].IsHit(P.rc) )  continue;                  //Already setted. Eliminate unnecessary trials and speed up.
								Qtrue[no2].BPSet(P.rc);                                  //set P.rc#no2 is true
								chainDesLKT[P.rc,no2]=RR;                                //record in retroactive chain
                                        if(DevelopB) WriteLine($"        31S ->P:{P}  ++#{no2} is true.");  		
                                    
                                foreach( var nox in P.FreeB.IEGet_BtoNo().Where(q=>(q!=no2)) ){   
									Qfalse[nox].BPSet(P.rc);                             //set p.rc#nox to false
                                        if(DevelopB) WriteLine($"        32S ->P:{P}  --#{nox} is false.");
								}
							}
						}

						else if(RR.type==W){    // T->F   *=*=* Check connection conditions. case WeakLink *=*=*
							foreach( var P in RR.UGCellsB ){                             //foreach next GLink                                
								Qfalse[no2].BPSet(P.rc);                                 //set P.rc#no2 is false
                                        if(DevelopB) WriteLine($"        40W ->P:{P}  ++#{no2} is false."); 
								chainDesLKF[P.rc,no2]=RR;                                //record in retroactive chain
							}

							if( RR.UGCellsB.Count==1 ){                                  //if the link's next element is a single cell                  
								var P=RR.UGCellsB[0];   
								if(P.FreeBC==2){                                         //If the next is a binary cell
									int nox=P.FreeB.DifSet(1<<no2).BitToNum();
                                    if( Qtrue[nox].IsHit(P.rc) )  continue;              //Already setted. Eliminate unnecessary trials and speed up.
									Qtrue[nox].BPSet(P.rc);	        	           		 //set P.rc#no2 is true
                                        if(DevelopB) WriteLine($"        41W ->P:{P}  ++#{nox} is true.");  
                                        
									GroupedLink RRR = new GroupedLink(P,no2,nox,S);      //GLKbv:GLink in cell(P#no2 StrongLink)
									RRR.preGrpedLink=RR;                                 //set preLink
									chainDesLKT[P.rc,nox]=RRR;                           //record in retroactive chain
								}
							}
						}
						rcQue.Enqueue(RR);                                               //enqueue next-GLink to RadiationSearchQueue                                              //enqueue next-GLink to RadiationSearchQueue						
					}
				}

				GNL_Result.SolFound=true;                                                //Solution found
				return GNL_Result;
			}
			catch( Exception ex ){
				WriteLine($"{ex.Message}+\r{ex.StackTrace}");
			}
			return null;
        }

		public  void developDispEx( int rc0, int no0, USuperLink USLK, bool DevelopB ){ //Color the cells. Generate link string.
            if( USLK==null || USLK.Qtrue==null )  return;
            List<UCell> qBDL=new List<UCell>();
            pBDL.ForEach(p=>qBDL.Add(p.Copy()));

            var pQtrue =USLK.Qtrue;
            var pQfalse=USLK.Qfalse;
            var pchainDesLKT=USLK.chainDesLKT;
            var pchainDesLKF=USLK.chainDesLKF;

            foreach( var P in qBDL ){
                if(P.No!=0)  continue;
                int noT=0, noF=0;
                foreach( var no in P.FreeB.IEGet_BtoNo()){
                    if( pQtrue[no].IsHit(P.rc) )  noT|=(1<<no);
                    if( pQfalse[no].IsHit(P.rc) ) noF|=(1<<no);
                }
                if(noT>0) P.SetNoBBgColor(noT, Colors.Red, Colors.LemonChiffon  );
                if(noF>0) P.SetNoBBgColorRev(noF, Colors.Red, Colors.LemonChiffon  );
                if((noT&noF)>0){
                    P.SetNoBBgColor(noT&noF, Colors.White , Colors.PowderBlue );
                    P.SetNoBColorRev(noT&noF,Colors.Blue );
                }
            }
            qBDL[rc0].SetNoBBgColor(1<<no0, Colors.Red, Colors.Yellow);

            devWin.Set_dev_GBoard( qBDL, dispOn:false );

            if(DevelopB){
                WriteLine("\r\r\r");
				string stMsg="";
                foreach( var P in pBDL ){
                    if(P.No!=0) continue;
                    foreach( var no in P.FreeB.IEGet_BtoNo() ){
                        if( pQfalse[no].IsHit(P.rc) && pQtrue[no].IsHit(P.rc) ){
                            WriteLine("------------error");
                        }
						string st= _GenMessage( USLK, P, no);
                        if(st.Length>4){
							WriteLine(st);
							stMsg += st;
						}
                    }
                }
				USLK.stMsg=stMsg;
            }
        }
    }
}