using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{

        //XYwing is an algorithm that consists of two WeakLinks with common cells.
        //http://csdenpe.web.fc2.com/page42.html
        public bool XYwing( ){
			Prepare(); 
            CeLKMan.PrepareCellLink(2);                                         //Generate WeakLinks

            if(BVCellLst==null)  BVCellLst = pBDL.FindAll(p=>(p.FreeBC==2));    //Generate BVs(BV:bivalue).
            if (BVCellLst.Count<3) return false;     

            bool XYwing=false;
            foreach( var P0 in BVCellLst ){                                     //Choose one BV_Cell(=>PS)
                List<UCellLink> BVLKLst =CeLKMan.IEGetRcNoBTypB(P0.rc,0x1FF,2).Where(R=>R.BVFlag).ToList();
                                                                                //Extract WeakLinks starting from P0
                    //foreach( var P in BVLKLst ) WriteLine(P);
                if(BVLKLst.Count<2) continue;

                var cmb = new Combination(BVLKLst.Count,2);
                int nxt=1;
                while(cmb.Successor(nxt)){                                      //Combine two WLinks from BVLKLst
                    UCellLink LKA=BVLKLst[cmb.Index[0]], LKB=BVLKLst[cmb.Index[1]];//(
                    UCell Q=LKA.UCe2, R=LKB.UCe2;
                    if(Q.rc==R.rc || LKA.no==LKB.no) continue;                  //Two WLinks have different end and different digits

                    Bit81 Q81 = ConnectedCells[LKA.rc2]&ConnectedCells[LKB.rc2];
                    if(Q81.Count<=0) continue;                                  //Two WLinks have cells connected indirectly

                    int noB = Q.FreeB.DifSet(1<<LKA.no) & R.FreeB.DifSet(1<<LKB.no);
                    if(noB==0) continue;                                        //Two WLinks have common digit(=>no).
                    int no=noB.BitToNum();

                    string msg2="";
                    foreach(var A in Q81.IEGetUCeNoB(pBDL,noB)){
                        if(A==P0 || A==Q || A==R) continue;
                        A.CancelB=noB; XYwing=true;                             //cell(A)/digit(no) can be excluded
                        if(SolInfoB) msg2+= $" {A.rc.ToRCNCLString()}(#{no+1})";
                    }

                    if(XYwing){
                        SolCode=2;
                        P0.SetNoBColor(P0.FreeB,AttCr); P0.SetCellBgColor(SolBkCr);
                        Q.SetCellBgColor(SolBkCr); R.SetCellBgColor(SolBkCr);

                        string msg0= $" Pivot: {_XYwingResSub(P0)}";
                        string msg1= $" Pin: {_XYwingResSub(R)} ,{_XYwingResSub(Q)}";
                        Result="XY Wing"+msg0;
                        if(SolInfoB){
                            ResultLong=$"XY Wing\r     {msg0}\r       {msg1}\r Eliminated:{msg2}";
                        }

                        if(__SimpleAnalizerB__)  return true;
                        if(!pAnMan.SnapSaveGP()) return true;
                        XYwing=false;
                    }
                }
            }
            return false;
        }
        private string _XYwingResSub( UCell P ){
            string st=P.rc.ToRCNCLString()+"(#"+P.FreeB.ToBitStringNZ(9)+")";
            return st;
        }
    }
}