using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public class LockedSetGen: AnalyzerBaseV2{
        public LockedSetGen( GNPX_AnalyzerMan AnMan ): base(AnMan){ }

        public bool LockedSet2(){ return LockedSetSub(2,false); }
        public bool LockedSet3(){ return LockedSetSub(3,false); }
        public bool LockedSet4(){ return LockedSetSub(4,false); }
        public bool LockedSet5(){ return LockedSetSub(5,false); }
        public bool LockedSet6(){ return LockedSetSub(6,false); }
        public bool LockedSet7(){ return LockedSetSub(7,false); }
     
        public bool LockedSet2Hidden(){ return LockedSetSub(2,true); } 
        public bool LockedSet3Hidden(){ return LockedSetSub(3,true); }
        public bool LockedSet4Hidden(){ return LockedSetSub(4,true); }
        public bool LockedSet5Hidden(){ return LockedSetSub(5,true); } 
        public bool LockedSet6Hidden(){ return LockedSetSub(6,true); }
        public bool LockedSet7Hidden(){ return LockedSetSub(7,true); }

        public bool LockedSetSub( int sz, bool HiddenFlag ){
            string resST="";
            for(int tfx=0; tfx<27; tfx++ ){
                List<UCell>  BDLstF = pBDL.IEGetCellInHouse(tfx,0x1FF).ToList();
                int ncF = BDLstF.Count;
                if(ncF<=sz) continue;
                
                Combination cmbG = new Combination(ncF,sz);
                while(cmbG.Successor()){
                    BDLstF.ForEach(p=>p.Selected=false);
                    Array.ForEach(cmbG.Index, p=> BDLstF[p].Selected=true );

                    int noBSel=0, noBNon=0;
                    BDLstF.ForEach(p=>{
                        if(p.Selected) noBSel |= p.FreeB;
                        else           noBNon |= p.FreeB;
                    } );                  
                    if( (noBSel&noBNon)==0 ) continue;

                    //===== Naked Locked Set =====
                    if( !HiddenFlag ){
                        if(noBSel.BitCount()==sz){
                            resST="";
                            foreach(var P in BDLstF){
                                if(P.Selected){
                                    P.SetNoBBgColor(noBSel,AttCr,SolBkCr);
                                    resST += " "+P.rc.ToRCString();
                                }
                                else P.CancelB=P.FreeB&noBSel;
                            }
                            resST = resST.ToString_SameHouseComp()+" #"+noBSel.ToBitStringN(9);
                            _LockedSetResult(sz,resST,HiddenFlag);
                            if(__SimpleAnalizerB__)  return true;
                            if(!pAnMan.SnapSaveGP())  return true;
                        }
                    }

                    //===== Hidden Locked Set =====
                    if(HiddenFlag){
                        if(noBNon.BitCount()==(ncF-sz)){
                            resST="";
                            foreach(var P in BDLstF.Where(p=>p.Selected)){
                                P.CancelB = P.FreeB&noBNon;
                                P.SetNoBBgColor(noBSel,AttCr,SolBkCr);
                                resST += " "+P.rc.ToRCString();
                            }
                            int nobR = noBSel.DifSet(noBNon);
                            resST = resST.ToString_SameHouseComp()+" #"+nobR.ToBitStringN(9);
                            _LockedSetResult(sz,resST,HiddenFlag);
                            if(__SimpleAnalizerB__)  return true;
                            if(!pAnMan.SnapSaveGP())  return true;
                        }
                    }
                }
            }
            return false;
        }

        private void _LockedSetResult( int sz, string resST, bool HiddenFlag ){
            SolCode = 2;
            string LSmsg="";
            switch(sz){
                case 2: LSmsg = "Pair[2D]"; break;
                case 3: LSmsg = "Triple[3D]"; break;
                case 4: LSmsg = "Quartet[4D]"; break;

                case 5: LSmsg = "Set[5D]"; break;
                case 6: LSmsg = "Set[6D]"; break;
                case 7: LSmsg = "Set{7D}"; break;
            }
            string SolMsg="Locked"+LSmsg;
            if( HiddenFlag ) SolMsg += " (hidden)";
            SolMsg += " "+resST;
            Result=SolMsg;
            ResultLong=SolMsg;
        }
    }
}