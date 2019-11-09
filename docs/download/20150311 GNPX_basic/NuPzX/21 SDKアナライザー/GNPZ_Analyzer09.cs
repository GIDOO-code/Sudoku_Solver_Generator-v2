using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{  
        public bool  GNP00_XYwing( ){
            List<UCell> BVCellLst = pBDL.FindAll(p=>(p.FreeBC==2)); //BV:bivalue
            if( BVCellLst.Count<3 ) return false;

            bool XYwing=false;
            foreach( var P in BVCellLst ){
                int X=0, Y=0, ZB;
                P.FreeB.BitTo2Nums( ref X, ref Y );

                var prm = new Permutation(BVCellLst.Count,2);
                int nxt=1, XB=(1<<X), YB=(1<<Y);
                while(prm.Successor(nxt)){
                    nxt=0;
                    UCell Q=BVCellLst[prm.Pnum[0]];
                    if( P==Q || (Q.FreeB&XB)==0 || P.FreeB==Q.FreeB ) continue;
                    if( !ConnectedCells[P.rc].IsHit(Q.rc) ) continue;
                    
                    nxt=1;
                    UCell R=BVCellLst[prm.Pnum[1]];
                    if( P==R || (R.FreeB&YB)==0 || P.FreeB==R.FreeB ) continue;
                    if( !ConnectedCells[P.rc].IsHit(R.rc) ) continue;
                    if( ConnectedCells[Q.rc].IsHit(R.rc) )  continue;
                    
                    //ZB = Q.FreeB.DifSet(XB) & R.FreeB.DifSet(YB);
                    ZB = (Q.FreeB&R.FreeB);
                    if( ZB==0 ) continue;

                    int no=ZB.BitToNum();
                    string msg2="";
                    foreach( var A in pBDL.Where(p=>((p.FreeB&ZB)>0)) ){
                        if( A==P || A==Q || A==R ) continue;
                        if( !ConnectedCells[Q.rc].IsHit(A.rc) ) continue;
                        if( !ConnectedCells[R.rc].IsHit(A.rc) ) continue;
                        A.CancelB=ZB; XYwing=true;
                        if( SolInfoDsp ){
                            msg2+=" "+A.rc.ToRCString()+"(#"+(no+1)+")";
                        }
                    }

                    if( XYwing ){
                        SolCode=2;
                        P.SetNoBColor(P.FreeB,AttCr); P.SetCellBgColor(SolBkCr);
                        P.SetNoBColor(P.FreeB,AttCr); Q.SetCellBgColor(SolBkCr); 
                        P.SetNoBColor(P.FreeB,AttCr); R.SetCellBgColor(SolBkCr);

                        string msg0= " Pivot: "+_GNP00_XYwingResSub(P);
                        string msg1= " Pin: "+_GNP00_XYwingResSub(R) +" ,"+_GNP00_XYwingResSub(Q);
                        Result="XY Wing"+msg0;
                        if( SolInfoDsp ){
                            ResultLong="XY Wing\r     "+msg0+"\r       "+msg1;
                            ResultLong+="\r Eliminated:"+msg2;
                        }
                        if( !SnapSaveGP(true) )  return true;
                        XYwing=false;
                    }
                }
            }
            return false;
        }
        private string _GNP00_XYwingResSub( UCell P ){
            string st=P.rc.ToRCString()+"(#"+P.FreeB.ToBitString(9).Replace(".","")+")";
            return st;
        }
    }
}
