 using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class CellLinkGenEx: AnalyzerBaseV3{
		private int GStageMemo;
		private List<UCellEx> BVCellLst;

        public CellLinkGenEx( GNPX_AnalyzerManEx pAnMan ): base(pAnMan){ }
		private void Prepare(){
			if(pAnMan.GStage!=GStageMemo) {
				GStageMemo=pAnMan.GStage;
				CeLKMan.Initialize();
				BVCellLst=null;
			}      
		}

        public bool Skyscraper(){ //Using Strong
			Prepare();
			CeLKMan.PrepareCellLink(1);    //Generate strongLink

            for( int no=0; no<9; no++ ){
                int noB=(1<<no);               
                var SSLst = CeLKMan.IEGetNoType(no,1).ToList(); 
                if( SSLst.Count<=2 ) continue;

                var prm=new Permutation(SSLst.Count,2);
                int nxtX=99;
                while( prm.Successor(nxtX) ){                
                    UCellLinkEx UCLa=SSLst[prm.Pnum[0]], UCLb=SSLst[prm.Pnum[1]];
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

                    bool SSfond=false;
                    foreach( UCellEx P in candHit.IEGetUCeNoB(pBDL,noB) ){     
                        P.CancelB = P.FreeB&noB;
                        SSfond=true;
                    }

                    if(SSfond){
                        SolCode =2;         
                        Result = "Skyscraper #"+(no+1);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}