using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool  GNP00_Wwing( ){ 
            List<UCell> BVCellLst = pBDL.FindAll(p=>(p.FreeBC==2)); //BV:bivalue
            if( BVCellLst.Count<2 ) return false;
            CeLKMan.PrepareCellLink(1);    //strongLink¶¬

            bool Wwing=false;
            var  cmb = new Combination(BVCellLst.Count,2);
            while(cmb.Successor()){
                UCell P=BVCellLst[cmb.Cmb[0]];
                UCell Q=BVCellLst[cmb.Cmb[1]];
                if( P.FreeB!=Q.FreeB ) continue;
                if( ConnectedCells[P.rc].IsHit(Q.rc) ) continue;

                foreach( var L in CeLKMan.IEGet(1) ){//1:strongƒŠƒ“ƒN
                    int no1B=(1<<L.no);
                    if( (P.FreeB&no1B)==0 ) continue;
                    if( L.rc1==P.rc || L.rc2==Q.rc ) continue;
                    if( !ConnectedCells[P.rc].IsHit(L.rc1) )  continue;
                    if( !ConnectedCells[Q.rc].IsHit(L.rc2) )  continue;
                    int no2B=P.FreeB.BitReset(L.no);
                    
                    string msg2="";
                    Bit81 Elm= ConnectedCells[P.rc] & ConnectedCells[Q.rc];
                    foreach( var E in Elm.IEGetUCeNoB(pBDL,no2B) ){
                        E.CancelB=no2B; Wwing=true; //W-Wing fond
                        if( SolInfoDsp ) msg2 += " "+E.rc.ToRCString();
                    }

                    if( Wwing ){
                        SolCode=2;
                        Result="W Wing";
                        ResultLong="";

                        if( SolInfoDsp ){
                            UCell A=pBDL[L.rc1], B=pBDL[L.rc2];
                            int noBX=P.FreeB.DifSet(no2B);
                            P.SetNoBBgColor(noBX,AttCr,SolBkCr2);
                            Q.SetNoBBgColor(noBX,AttCr,SolBkCr2);
                     
                            A.SetNoBBgColor(no1B,AttCr,SolBkCr);
                            B.SetNoBBgColor(no1B,AttCr,SolBkCr);

                            string msg0= " bvCell: "+_GNP00_XYwingResSub(P) +" ,"+_GNP00_XYwingResSub(Q);
                            string msg1= "  SLink: "+A.rc.ToRCString() +"-"+B.rc.ToRCString()+"(#"+(L.no+1)+")";
                            Result += msg0;
                            ResultLong = "W Wing\r"+msg0+"\r"+msg1;
                            ResultLong += "\r Eliminated: #"+(no2B.BitToNum()+1)+" in "+msg2.ToString_SameHouseComp();
                        }
                        if( !SnapSaveGP(true) )  return true;
                        Wwing=false;
                    }
                }
            }
            return false;
        }
    }
}
