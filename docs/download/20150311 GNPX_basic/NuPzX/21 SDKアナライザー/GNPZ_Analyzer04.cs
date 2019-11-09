using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool GNP00_XWing(){     return _GNP00_Fish(2); }
        public bool GNP00_SwordFish(){ return _GNP00_Fish(3); }
        public bool GNP00_JellyFish(){ return _GNP00_Fish(4); }
        public bool GNP00_Squirmbag(){ return _GNP00_Fish(5); }
        public bool GNP00_Whale(){     return _GNP00_Fish(6); }
        public bool GNP00_Leviathan(){ return _GNP00_Fish(7); }

        public bool _GNP00_Fish( int sz ){
            int rowSel=0x1FF, colSel=(rowSel<<9);
            for( int no=0; no<9; no++ ){
                if( GNP00_ExtFish_FishSub(sz,no,18,rowSel,colSel,FinnedF:false) ) return true;
                if( GNP00_ExtFish_FishSub(sz,no,18,colSel,rowSel,FinnedF:false) ) return true;
            }
            return false;
        }

// *==*==*==*==*==*==*==*==* Old Version *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
        public bool GNP00_XWingOld(){     return _GNP00_FishOld(2); }
        public bool GNP00_SwordFishOld(){ return _GNP00_FishOld(3); }
        public bool GNP00_JellyFishOld(){ return _GNP00_FishOld(4); }

        public bool _GNP00_FishOld( int sz ){
            for( int no=0; no<9; no++ ){
                List<BaseSet> BSLst = __GetBaseSet(no,sz);
                if( BSLst.Count<sz )  continue;
                if( __nFishSub(no,sz,BSLst) ) return true;
            }
            return false;
        }
        private List<BaseSet> __GetBaseSet( int no, int sz ){
            int noB = 1<<no;
            var BSLst = new List<BaseSet>();
            for( int tfx=0; tfx<18; tfx++ ){
                int nxB = pBDL.IEGet(tfx,noB).Aggregate(0,(Q,P)=>Q|(1<<P.nx));
                int nc=nxB.BitCount();
                if( (2<=nc) && (nc<=sz) )  BSLst.Add( new BaseSet(no,tfx,nxB) );
            }
            return BSLst;
        }
        private bool __nFishSub( int no, int sz, List<BaseSet> BSLst ){
            int noB = 1<<no;
            string msg = "";

            for( int tp=0; tp<=1; tp++ ){
                List<BaseSet> BSLsj = BSLst.FindAll(P=> (P.tp==tp));
                if( BSLsj.Count<sz )  continue;
                int tpR = 1-tp;
                
                Combination cmb = new Combination(BSLsj.Count,sz);
                while( cmb.Successor() ){
                    int bsB=0, csB=0;
                    for( int n=0; n<sz; n++ ){
                        BaseSet P=BSLsj[cmb.Cmb[n]];
                        bsB |= (1<<P.fx); //BaseSet
                        csB |= P.nxB;     //CoverSet
                        
                    }
                    if( csB.BitCount()!=sz )  continue;

                    //=== œŠO‚Å‚«‚é”Žš‚ª‚ ‚é‚© ===
                    bool XWingFond = false;
                    foreach( int fx in csB.IEGet_BtoNo() ){
                        foreach( var P in pBDL.IEGet_SelTFBRC(tpR*9+fx,noB,rcSel:(bsB^0x1FF)) ){
                            P.CancelB=noB; XWingFond=true;  //œŠO‚Å‚«‚é”Žš‚ð”­Œ©
                        }
                    }

                    if( XWingFond ){
                        SolCode = 2;
                        if( SolInfoDsp ){
                            for( int n=0; n<sz; n++ ){
                                BaseSet P = BSLsj[cmb.Cmb[n]];
                                foreach( var Q in P.rUCellLst.Select(q=>pBDL[q]) ){
                                    Q.SetNoBBgColor(noB,AttCr,SolBkCr);
                                }
                            }
                            msg = "\r    Digit: " + (no+1) + "\r  BaseSet:";
                            string rcSt = (tp==0)? " r": " c";
                            foreach( var nx in bsB.IEGet_BtoNo() )  msg += rcSt+(nx+1);
                        
                            msg += "\r CoverSet:";
                            rcSt = (tp==0)? " c": " r";
                            foreach( var nx in csB.IEGet_BtoNo() )   msg += rcSt+(nx+1);
                        }        
                        goto lblXwingEnd;
                    }
                }
            }

          lblXwingEnd:
            if( SolCode<=0 )  return false;
            string[] FishNames=
                { "Xwing", "SwordFish", "JellyFish", "Squirmbag", "Whale",  "Leviathan" };
            Result = FishNames[sz-2]; 
            if( SolInfoDsp )  ResultLong = Result + msg;

            return true;
        }

    }
}