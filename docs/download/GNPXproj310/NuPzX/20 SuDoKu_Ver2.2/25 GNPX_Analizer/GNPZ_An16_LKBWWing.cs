using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{

        //W-Wing is an algorithm composed of bivalue cell and link.
        //http://csdenpe.web.fc2.com/page43.html
        public bool  Wwing( ){ 
			Prepare(); 
            CeLKMan.PrepareCellLink(1);    //Generate StrongLink

            if(BVCellLst==null)  BVCellLst = pBDL.FindAll(p=>(p.FreeBC==2));//BV:BiValue
            if(BVCellLst.Count<2) return false;    
            BVCellLst.Sort((A,B)=>(A.FreeB-B.FreeB));                       //Important!!!

            bool Wwing=false;
            var  cmb = new Combination(BVCellLst.Count,2);
            int nxt=99;
            while(cmb.Successor(nxt)){
                UCell P=BVCellLst[cmb.Index[0]];
                UCell Q=BVCellLst[cmb.Index[1]];
                nxt=1;
                if(P.FreeB!=Q.FreeB){ nxt=0; continue; }                    //BVCellLst is sorted, so possible to skip.            
                if(ConnectedCells[P.rc].IsHit(Q.rc)) continue;

                foreach(var L in CeLKMan.IEGetCellInHouse(1)){              //1:StrongLink(Link has a direction. The opposite link is different.)
                    int no1B=(1<<L.no);
                    if((P.FreeB&no1B)==0) continue;
                    if(L.rc1==P.rc || L.rc2==Q.rc) continue;
                    if(!ConnectedCells[P.rc].IsHit(L.rc1))  continue;       //L.rc1 is in the connected area of ​​P.rc?
                    if(!ConnectedCells[Q.rc].IsHit(L.rc2))  continue;       //L.rc2 is in the connected area of Q.rc?
                    int no2B=P.FreeB.BitReset(L.no);                        //another digit
                    
                    string msg2="";
                    Bit81 ELM= ConnectedCells[P.rc] & ConnectedCells[Q.rc]; //ELM:Common part of the influence area of ​​cell P and cell Q
                    foreach( var E in ELM.IEGetUCeNoB(pBDL,no2B) ){         //Cell/digit in ELM can be excluded
                        E.CancelB=no2B; Wwing=true;                         //W-Wing found
                        if(SolInfoB) msg2 += " "+E.rc.ToRCString();
                    }
                    if(!Wwing) continue;

                    SolCode=2;
                    if(SolInfoB){
                        UCell A=pBDL[L.rc1], B=pBDL[L.rc2];
                        int noBX=P.FreeB.DifSet(no2B);
                        P.SetNoBBgColor(noBX,AttCr,SolBkCr2);
                        Q.SetNoBBgColor(noBX,AttCr,SolBkCr2);
                     
                        A.SetNoBBgColor(no1B,AttCr,SolBkCr);
                        B.SetNoBBgColor(no1B,AttCr,SolBkCr);

                        string msg0= $" bvCell: {_XYwingResSub(P)} ,{_XYwingResSub(Q)}";
                        string msg1= $"  SLink: {A.rc.ToRCNCLString()}-{B.rc.ToRCNCLString()}(#{(L.no+1)})";
                        Result = $"W Wing Eli.;#{(no2B.BitToNum()+1)} in {msg2.ToString_SameHouseComp()}";
                        ResultLong = "W Wing\r"+msg0+"\r"+msg1
                                   + $"\r Eliminated: #{(no2B.BitToNum()+1)} in {msg2.ToString_SameHouseComp()}";
                    }

                    if(__SimpleAnalizerB__)  return true;
                    if(!pAnMan.SnapSaveGP(true))  return true;
                    Wwing=false;
                }
            }
            return false;
        }
    }
}