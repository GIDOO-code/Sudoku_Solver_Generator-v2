using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class CellLinkGenEx: AnalyzerBaseV3{
        public bool  Wwing( ){ 
			Prepare(); 
            CeLKMan.PrepareCellLink(1);    //Generate StrongLink

            if(BVCellLst==null)  BVCellLst = pBDL.FindAll(p=>(p.FreeBC==2)); //BV:bivalue
            if(BVCellLst.Count<2) return false;    
            BVCellLst.Sort((A,B)=>(A.FreeB-B.FreeB));// !! Important

            bool WwingB=false;
            var  cmb = new Combination(BVCellLst.Count,2);
            int nxt=99;
            while(cmb.Successor(nxt)){
                UCellEx P=BVCellLst[cmb.Index[0]];
                UCellEx Q=BVCellLst[cmb.Index[1]];
                nxt=0;
                if( P.FreeB!=Q.FreeB ) continue;//(The selected two cells have the same number)
                nxt=1;
                if( ConnectedCells[P.rc].IsHit(Q.rc) ) continue;

                foreach( var L in CeLKMan.IEGetCellInHouse(1) ){//1:StrongLink
                    int no1B=(1<<L.no);
                    if( (P.FreeB&no1B)==0 ) continue;
                    if( L.rc1==P.rc || L.rc2==Q.rc ) continue;
                    if( !ConnectedCells[P.rc].IsHit(L.rc1) )  continue;
                    if( !ConnectedCells[Q.rc].IsHit(L.rc2) )  continue;
                    int no2B=P.FreeB.BitReset(L.no);
                    
                    string msg2="";
                    Bit81 Elm= ConnectedCells[P.rc] & ConnectedCells[Q.rc];
                    foreach( var E in Elm.IEGetUCeNoB(pBDL,no2B) ){
                        E.CancelB=no2B; WwingB=true; //W-Wing found
                        msg2 += " "+E.rc.ToRCString();
                    }

                    if(WwingB){
                        SolCode=2;
                        Result = "W Wing Eli.;#"+(no2B.BitToNum()+1)+" in "+ msg2.ToString_SameHouseComp();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}