 using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{
		private int GStageMemo;
		private List<UCell> BVCellLst;

        public NXGCellLinkGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }
		private void Prepare(){
			if(pAnMan.GStage!=GStageMemo){
				GStageMemo=pAnMan.GStage;
				CeLKMan.Initialize();
				BVCellLst=null;
			}      
		}

        public bool Skyscraper(){ //Using Strong
			Prepare();
			CeLKMan.PrepareCellLink(1);    //Generate strongLink

            for(int no=0; no<9; no++ ){
                int noB=(1<<no);               
                var SSLst = CeLKMan.IEGetNoType(no,1).ToList(); 
                if( SSLst.Count<=2 ) continue;

                var prm=new Permutation(SSLst.Count,2);
                int nxtX=99;
                while( prm.Successor(nxtX) ){                
                    UCellLink UCLa=SSLst[prm.Index[0]], UCLb=SSLst[prm.Index[1]];
                    nxtX=0;
                    if( UCLa.ID<UCLb.ID ) continue; //
                    nxtX=1;
                    if( (UCLa.B81|UCLb.B81).Count!=4 )  continue;       
                    //All cells are different

                    Bit81 ConA1=ConnectedCells[UCLa.rc1], ConA2=ConnectedCells[UCLa.rc2]; 
                    if( !ConA1.IsHit(UCLb.rc1) || ConA1.IsHit(UCLb.rc2) ) continue;
                    if(  ConA2.IsHit(UCLb.rc1) || ConA2.IsHit(UCLb.rc2) ) continue;
                    //(UCLa.rc1)(UCLb.rc1):belongs to the same house only

                    Bit81 candHit = ConA2 & ConnectedCells[UCLb.rc2];
                    candHit = candHit - (ConA1 | ConnectedCells[UCLb.rc1]);

                    bool SSfound=false;
                    foreach( UCell P in candHit.IEGetUCeNoB(pBDL,noB) ){     
                        P.CancelB = P.FreeB&noB;
                        SSfound=true;
                    }

                    if(SSfound){
                #region Result
                        SolCode =2;                

                        string msg2="";
                        if(SolInfoB){
                            pBDL[UCLa.rc1].SetNoBBgColor(noB,AttCr,SolBkCr);
                            pBDL[UCLa.rc2].SetNoBBgColor(noB,AttCr,SolBkCr);
                            pBDL[UCLb.rc1].SetNoBBgColor(noB,AttCr,SolBkCr);
                            pBDL[UCLb.rc2].SetNoBBgColor(noB,AttCr,SolBkCr);

                            string msg = "\r";
                            msg += "  on " + (no+1) + " in";
                            msg += " r" + (UCLa.rc1/9+1) + "c" + (UCLa.rc1%9+1);
                            msg += " r" + (UCLb.rc1/9+1) + "c" + (UCLb.rc1%9+1);
                            msg += "\r  connected by";
                            msg += " r" + (UCLa.rc2/9+1) + "c" + (UCLa.rc2%9+1);
                            msg += " r" + (UCLb.rc2/9+1) + "c" + (UCLb.rc2%9+1);
                            msg += "\r  eliminated ";

                            foreach( UCell P in candHit.IEGetUCeNoB(pBDL,noB) ){ msg2 += " "+P.rc.ToRCString(); }

                            msg2=msg2.ToString_SameHouseComp();
                            ResultLong = "Skyscraper" + msg+msg2;
                            Result = "Skyscraper #"+(no+1) +" in "+msg2;
                        }
                        else Result = "Skyscraper #"+(no+1);
                #endregion Result
                        if(__SimpleAnalizerB__)  return true;
                        if(!pAnMan.SnapSaveGP(true))  return true;
                    }
                }
            }
            return false;
        }
    }
}