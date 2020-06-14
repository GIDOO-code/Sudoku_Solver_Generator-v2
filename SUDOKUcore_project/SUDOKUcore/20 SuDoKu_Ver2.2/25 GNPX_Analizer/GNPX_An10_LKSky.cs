 using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPXcore{
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

        //Skyscraper is an algorithm consisting of two StrongLinks.
        //http://csdenpe.web.fc2.com/page40.html
        public bool Skyscraper(){
			Prepare();
			CeLKMan.PrepareCellLink(1);                                 //Generate StrongLink

            for(int no=0; no<9; no++){
                int noB=(1<<no);               
                var SSLst = CeLKMan.IEGetNoType(no,1).ToList();         //select only StrongLink of #no
                if(SSLst.Count<=2) continue;

                var prm=new Permutation(SSLst.Count,2);
                int nxtX=99;
                while(prm.Successor(nxtX)){                
                    UCellLink UCLa=SSLst[prm.Index[0]], UCLb=SSLst[prm.Index[1]];
                 
                    nxtX=1;
                    if(UCLa.ID>UCLb.ID){ nxtX=0; continue; }            //next permutation data skip_generation(nxtX=0)
                    if((UCLa.B81|UCLb.B81).Count!=4)  continue;         //All cells are different?

                    Bit81 ConA1 =ConnectedCells[UCLa.rc1];              //ConA1:cell group related to cell rc1
                    if(!ConA1.IsHit(UCLb.rc1) || ConA1.IsHit(UCLb.rc2)) continue;

                    Bit81 ConA2=ConnectedCells[UCLa.rc2];               //ConA2:cell group related to cell rc1
                    if(ConA2.IsHit(UCLb.rc1) || ConA2.IsHit(UCLb.rc2)) continue;
                    //Only UCLa.rc1 and UCLb.rc1 belong to the same house.

                    Bit81 ELM = ConA2 & ConnectedCells[UCLb.rc2];
                    ELM -= (ConA1 | ConnectedCells[UCLb.rc1]);          //ELM:eliminatable cells

                    bool SSfound=false;
                    foreach(UCell P in ELM.IEGetUCeNoB(pBDL,noB)){ P.CancelB=P.FreeB&noB; SSfound=true; }
                    if(!SSfound)  continue; //Skyscraper found

                #region Result
                    SolCode =2;                
                    if(SolInfoB){
                        pBDL[UCLa.rc1].SetNoBBgColor(noB,AttCr,SolBkCr);
                        pBDL[UCLa.rc2].SetNoBBgColor(noB,AttCr,SolBkCr);
                        pBDL[UCLb.rc1].SetNoBBgColor(noB,AttCr,SolBkCr);
                        pBDL[UCLb.rc2].SetNoBBgColor(noB,AttCr,SolBkCr);

                        string msg="\r", msg2="";
                        msg += $"  on {(no+1)} in {UCLa.rc1.ToRCNCLString()} {UCLb.rc1.ToRCNCLString()}";
                        msg += $"\r  connected by {UCLa.rc2.ToRCNCLString()} {UCLb.rc2.ToRCNCLString()}";
                        msg += "\r  eliminated ";
                        foreach(UCell P in ELM.IEGetUCeNoB(pBDL,noB)){ msg2 += " "+P.rc.ToRCString(); }
                        msg2 += " "+msg2.ToString_SameHouseComp();
                        ResultLong = "Skyscraper" + msg+msg2;
                        Result = $"Skyscraper #{(no+1)} in {msg2}";
                    }
                    else Result = $"Skyscraper #{(no+1)}";
                #endregion Result
                    if(__SimpleAnalizerB__)  return true;
                    if(!pAnMan.SnapSaveGP(true))  return true;
                }
            }
            return false;
        }
    }
}