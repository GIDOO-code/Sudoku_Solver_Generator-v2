using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media;
using static System.Console;
using System.Threading;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class GroupedLinkGen: AnalyzerBaseV2{

		public bool ForceChain_HouseEx( ){
            if(GNPXApp000.GMthdOption["ForceChainCellHouseOn"] != "1") return false;

            GroupedLink._IDsetB=false;  //ID set for debug

			Prepare();
            pSprLKsMan.PrepareSuperLinkMan( AllF:true );
            string dspOpt = GNPXApp000.GMthdOption["ForceLx"];
              
            Bit81[] sPass=new Bit81[9];
            for(int k=0; k<9; k++ ) sPass[k]=new Bit81();

            bool solvedA=false;
			for(int hs0=0; hs0<27; hs0++ ){
				int noBs=pBDL.IEGetCellInHouse(hs0).Aggregate(0,(Q,P)=>Q|(P.FreeB));
      
                bool solvedB=false;
				foreach( var no0 in noBs.IEGet_BtoNo() ){
					int noB=(1<<no0);
					Bit81[] sTrue=new Bit81[9];
					for(int k=0; k<9; k++ ) sTrue[k]=new Bit81(all_1:true);

					foreach( var P0 in pBDL.IEGetCellInHouse(hs0,noB) ){
						USuperLink USLK=pSprLKsMan.get_L2SprLK(P0.rc,no0,FullSearchB:false,DevelopB:false);
						if( USLK==null || !USLK.SolFound )  goto nextSearch;

                        for(int k=0; k<9; k++ ){
                            sTrue[k] &= (USLK.Qtrue[k] - USLK.Qfalse[k]);
                            sTrue[k].BPReset(P0.rc);
                        }
					}
                    				  
                    for(int nox=0; nox<9; nox++){
					    if(!sTrue[nox].IsZero()){ solvedB=true; break; }
				    }

                    if(solvedB){                        
                        solvedA=true;
                        _ForceChainHouseDispEx(sPass,sTrue,hs0,no0);
                        if(!SDK_Ctrl.MltAnsSearch && dspOpt=="ForceL0") return true;                 
                    }
                }   

                if(solvedA && dspOpt=="ForceL1"){	
                    _ForceChainHouseDispEx(sPass,null,hs0,-1);
                    if(!SDK_Ctrl.MltAnsSearch)  return true; 
				}

			  nextSearch:
				continue;
            }
            if(solvedA && !SDK_Ctrl.MltAnsSearch && dspOpt=="ForceL2") _ForceChainHouseDispEx(sPass,null,-1,-1);

            return (SolCode>0);
        }


		private  bool _ForceChainHouseDispEx( Bit81[] sPass, Bit81[] sTrue, int hs0, int no0 ){
            string dspOpt = GNPXApp000.GMthdOption["ForceLx"];
            if(hs0<0){//  dspOpt:ForceL2"
                Result = ResultLong = "ForceChain_House";
                if(__SimpleAnalizerB__)  return true;
				pAnMan.SnapSaveGP(true);
                return (SolCode>0);
            }


			string st0="", st2="";            
			for(int nox=0; nox<9; nox++ ){                  
                if(sTrue!=null){
				    if( sTrue[nox].IsZero() )  continue;

				    foreach( var rc in sTrue[nox].IEGet_rc() ){
                        if(sPass[nox].IsHit(rc)) continue;
                        sPass[nox].BPSet(rc);

					    UCell Q=pBDL[rc];
					    Q.FixedNo=nox+1;
					    int elm=Q.FreeB.DifSet(1<<nox);
					    Q.CancelB = elm;			
                        SolCode=1;

					    if(SolInfoB){
						    Q.SetNoBBgColor(1<<nox, Colors.Red , Colors.LightGreen );
						    Q.SetNoBColorRev(elm,Colors.Red );

                            st0 = $"ForceChain_House({_HouseToString(hs0)}/#{(no0+1)}) r{(Q.r+1)}c{(Q.c+1)}/#{(nox+1)} is true";
						    string st1="";
						    foreach( var P in pBDL.IEGetCellInHouse(hs0,1<<no0) ){
							    USuperLink USLK = pSprLKsMan.get_L2SprLK(P.rc,no0,FullSearchB:true,DevelopB:false); //Accurate path
							    st1 += "\r"+pSprLKsMan._GenMessage2true(USLK,Q,nox);
							    if(dspOpt!="ForceL2") P.SetNoBBgColor(1<<no0, Colors.Green , Colors.Yellow );
						    }

						    st2 = st0+st1;
                            extRes += "\r"+st2;                             //(Description of each solution)
                            extRes = extRes.TrimStart();

						    if(dspOpt=="ForceL0"){
                                Result = ResultLong = st0;
                                if(__SimpleAnalizerB__)  return true;
							    if(!pAnMan.SnapSaveGP(false))  return true;
                                extRes=""; st2="";
                                if(!SDK_Ctrl.MltAnsSearch)  return true;
						    }
					    }
				    }
                }

                if(SolInfoB && dspOpt=="ForceL1" && st2!=""){
                    st0 = $"ForceChain_House({_HouseToString(hs0)}/#{(no0+1)})";
                    Result = ResultLong = st0;
                    if(__SimpleAnalizerB__)  return true;
				    if(!pAnMan.SnapSaveGP(false))  return true;			    
                    extRes=""; st2="";
                    if(!SDK_Ctrl.MltAnsSearch)  return true;
			    }
            }
			return (SolCode>0);
        }

        private string _HouseToString(int hs){
			string st;
			if(hs<9)  st=$"row{(hs+1)}";
			else if(hs<18) st=$"col{(hs-8)}";
			else st="blk"+(hs-17);
			return st;
		}
    }  
}
