using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool GNP00_LockedSet2(){ return _GNP00_LockedSet(2,false); }
        public bool GNP00_LockedSet3(){ return _GNP00_LockedSet(3,false); }
        public bool GNP00_LockedSet4(){ return _GNP00_LockedSet(4,false); }
        public bool GNP00_LockedSet5(){ return _GNP00_LockedSet(5,false); }
        public bool GNP00_LockedSet6(){ return _GNP00_LockedSet(6,false); }
        public bool GNP00_LockedSet7(){ return _GNP00_LockedSet(7,false); }
     
        public bool GNP00_LockedSet2Hidden(){ return _GNP00_LockedSet(2,true); } 
        public bool GNP00_LockedSet3Hidden(){ return _GNP00_LockedSet(3,true); }
        public bool GNP00_LockedSet4Hidden(){ return _GNP00_LockedSet(4,true); }
        public bool GNP00_LockedSet5Hidden(){ return _GNP00_LockedSet(5,true); } 
        public bool GNP00_LockedSet6Hidden(){ return _GNP00_LockedSet(6,true); }
        public bool GNP00_LockedSet7Hidden(){ return _GNP00_LockedSet(7,true); }

        private bool _GNP00_LockedSet( int sz, bool HiddenFlag ){
            string resST="";
            for( int tfx=0; tfx<27; tfx++ ){
                List<UCell>Å@BDLstF = pBDL.IEGet(tfx,0x1FF).ToList();
                int ncF = BDLstF.Count;
                if( ncF<=sz ) continue;
                
                Combination cmbG = new Combination(ncF,sz);
                while( cmbG.Successor() ){
                    BDLstF.ForEach(p=>p.Selected=false);
                    Array.ForEach(cmbG.Cmb, p=> BDLstF[p].Selected=true );

                    int noBSel=0, noBNon=0;
                    BDLstF.ForEach(p=>{
                        if( p.Selected ) noBSel |= p.FreeB;
                        else             noBNon |= p.FreeB;
                    } );                  
                    if( (noBSel&noBNon)==0 ) continue;

                    //===== Naked Locked Set =====
                    if( !HiddenFlag ){
                        if( noBSel.BitCount()==sz ){
                            resST="";
                            foreach( var P in BDLstF ){
                                if( P.Selected ){
                                    P.SetNoBBgColor(noBSel,AttCr,SolBkCr);
                                    resST += " "+P.rc.ToRCString();
                                }
                                else P.CancelB=P.FreeB&noBSel;
                            }
                            resST = resST.ToString_SameHouseComp()+" #"+noBSel.ToBitStringN(9);
                            _LockedSetResult(sz,resST,HiddenFlag);
                            if( !SnapSaveGP() )  return true;
                        }
                    }

                    //===== Hidden Locked Set =====
                    if( HiddenFlag ){
                        if( noBNon.BitCount()==(ncF-sz) ){
                            resST="";
                            foreach( var P in BDLstF.Where(p=>p.Selected) ){
                                P.CancelB = P.FreeB&noBNon;
                                P.SetNoBBgColor(noBSel,AttCr,SolBkCr);
                                resST += " "+P.rc.ToRCString();
                            }
                            int nobR = noBSel.DifSet(noBNon);
                            resST = resST.ToString_SameHouseComp()+" #"+nobR.ToBitStringN(9);
                            _LockedSetResult(sz,resST,HiddenFlag);
                            if( !SnapSaveGP() )  return true;
                        }
                    }
                }
            }
            return false;
        }

        private void _LockedSetResult( int sz, string resST, bool HiddenFlag ){
            SolCode = 2;
            Result = "Locked";

            string LSmsg="";
            switch(sz){
                case 2: LSmsg = "Pair[2D]"; break;
                case 3: LSmsg = "Triple[3D]"; break;
                case 4: LSmsg = "Quartet[4D]"; break;

                case 5: LSmsg = "Set[5D]"; break;
                case 6: LSmsg = "Set[6D]"; break;
                case 7: LSmsg = "Set{7D}"; break;

            }
            Result = "Locked "+LSmsg;
            if( HiddenFlag ) Result += " (hidden)";
            Result += " "+resST;
            ResultLong = Result;
        }
    }
}