using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class CellLinkGenEx: AnalyzerBaseV3{
        public bool XYwing( ){
			Prepare(); 
            CeLKMan.PrepareCellLink(2);    //Generate WeakLink

            if(BVCellLst==null)  BVCellLst = pBDL.FindAll(p=>(p.FreeBC==2)); //BV:bivalue
            if(BVCellLst.Count<3) return false;     

            bool XYwing=false;
            foreach( var P0 in BVCellLst ){
                List<UCellLinkEx> BVLKLst =CeLKMan.IEGetRcNoBTypB(P0.rc,0x1FF,2).Where(R=>R.BVFlag).ToList();
                        //foreach( var P in BVLKLst ) WriteLine(P);
                if(BVLKLst.Count<2) continue;

                var cmb = new Combination(BVLKLst.Count,2);
                int nxt=1;
                while(cmb.Successor(nxt)){
                    UCellLinkEx LKA=BVLKLst[cmb.Cmb[0]], LKB=BVLKLst[cmb.Cmb[1]];
                    UCellEx  Q=LKA.UCe2, R=LKB.UCe2;
                    if( Q.rc==R.rc || LKA.no==LKB.no ) continue;

                    Bit81 Q81 = ConnectedCells[LKA.rc2]&ConnectedCells[LKB.rc2];
                    if(Q81.Count<=0) continue;

                    int noB = Q.FreeB.DifSet(1<<LKA.no) & R.FreeB.DifSet(1<<LKB.no);
                    if(noB<0) continue;
                    int no=noB.BitToNum();

                    string msg2="";
                    foreach( var A in Q81.IEGetUCeNoB(pBDL,noB) ){
                        if( A==P0 || A==Q || A==R ) continue;
                        A.CancelB=noB; XYwing=true;
                        if(SolInfoB) msg2+=" "+A.rc.ToRCString()+"(#"+(no+1)+")";
                    }

                    if( XYwing ){
                        SolCode=2;

                        string msg0= " Pivot: "+_XYwingResSub(P0);
                        string msg1= " Pin: "+_XYwingResSub(R) +" ,"+_XYwingResSub(Q);
                        Result="XY Wing"+" Pivot: "+_XYwingResSub(P0);
                        return true;
                    }
                }
            }
            return false;
        }
        private string _XYwingResSub( UCellEx P ){
            string st=P.rc.ToRCString()+"(#"+P.FreeB.ToBitString(9).Replace(".","")+")";
            return st;
        }
    }
}