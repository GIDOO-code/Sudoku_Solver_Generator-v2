using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;
namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool GNP00_FinnedXWing(){     return _GNP00_FinnedFish(2); }
        public bool GNP00_FinnedSwordFish(){ return _GNP00_FinnedFish(3); }
        public bool GNP00_FinnedJellyFish(){ return _GNP00_FinnedFish(4); }
        public bool GNP00_FinnedSquirmbag(){ return _GNP00_FinnedFish(5); }
        public bool GNP00_FinnedWhale(){     return _GNP00_FinnedFish(6); }
        public bool GNP00_FinnedLeviathan(){ return _GNP00_FinnedFish(7); } 

        public bool _GNP00_FinnedFish( int sz ){         
            int rowSel=0x1FF, colSel=(rowSel<<9);
            for( int no=0; no<9; no++ ){
                if( GNP00_ExtFish_FishSub(sz,no,18,rowSel,colSel,FinnedF:true) ) return true;
                if( GNP00_ExtFish_FishSub(sz,no,18,colSel,rowSel,FinnedF:true) ) return true;
            }
            return false;
        }

// *==*==*==*==*==*==*==*==* Old Version *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
        public bool GNP00_FinnedXWingOld(){     return _GNP00_FinnedFishOld(2); }
        public bool GNP00_FinnedSwordFishOld(){ return _GNP00_FinnedFishOld(3); }
        public bool GNP00_FinnedJellyFishOld(){ return _GNP00_FinnedFishOld(4); }
        public bool GNP00_FinnedSquirmbagOld(){ return _GNP00_FinnedFishOld(5); }
        public bool GNP00_FinnedWhaleOld(){     return _GNP00_FinnedFishOld(6); }
        public bool GNP00_FinnedLeviathanOld(){ return _GNP00_FinnedFishOld(7); } 

        public bool _GNP00_FinnedFishOld( int sz ){
            for( int blk=0; blk<9; blk++ ){
                for( int no=0; no<9; no++ ){
                    List<BaseSet> BSLst = __GetFinnedBaseSet(no,sz,blk);
                    if( BSLst.Count<sz )  continue;
                    if( __FinnedFishGSub(no,sz,blk,BSLst) ) return true;
                }
            }
            return false;
        }
        private List<BaseSet> __GetFinnedBaseSet( int no, int sz, int blk ){
            int noB = 1<<no;
            var BSLst  = new List<BaseSet>();
            for( int tfx=0; tfx<18; tfx++ ){             
                int nxB = pBDL.IEGet(tfx,noB).Where(p=>p.b!=blk).Aggregate(0,(Q,P)=>Q|(1<<P.nx));
                int nc=nxB.BitCount();
                if( (1<=nc) && (nc<=sz) )  BSLst.Add(new BaseSet(no,tfx,nxB));
            }
            return BSLst;
        }
        private bool __FinnedFishGSub( int no, int sz, int blk, List<BaseSet> BSLst ){
            int noB = 1<<no;
            string msg="", finMsg="";

            for( int tp=0; tp<=1; tp++ ){ 　//0:行　1:列
                List<BaseSet> BSLsj = BSLst.FindAll(P=>P.tp==tp);
                if( BSLsj.Count<sz )  continue;

                int tpR = 1-tp;   
                Combination cmb = new Combination(BSLsj.Count,sz);
                while( cmb.Successor() ){
                    int bsB=0, csB=0;
                    for( int n=0; n<sz; n++ ){
                        BaseSet P=BSLsj[cmb.Cmb[n]];
                        bsB |= (1<<P.fx);
                        csB |= P.nxB;       
                    }
                    if( csB.BitCount()!=sz )  continue;

                    for( int fx=0; fx<9; fx++ ){ //=== FinCheck ===
                        if( (bsB&(1<<fx))==0 )  continue;
                        List<UCell> BDLa = GetTupleList(pBDL,tp,fx,noB);
                        if( BDLa.FindIndex(p=>(p.b==blk)) >= 0 )  goto LFinFound;
                    }
                    continue;

                LFinFound:
                    //=== CoverSet Check ===
                    bool XWingFond = false;
                    for( int fx=0; fx<9; fx++ ){
                        if( (csB&(1<<fx))==0 )  continue;                                        
                        foreach( var P in pBDL.IEGet_SelTFBRC(tpR*9+fx,noB,rcSel:(bsB^0x1FF)) ){
                            if( P.b==blk ){ P.CancelB=noB; XWingFond=true; }
                        }
                    }

                    if( XWingFond ){
                        SolCode = 2;
                        for( int fx=0; fx<9; fx++ ){
                            if( (bsB&(1<<fx))==0 )  continue;
                            List<UCell> BDLa = GetTupleList(pBDL,tp,fx,noB);
                            BDLa.ForEach( P =>{
                                if( (csB&(1<<P.nx))==0 ){
                                    if( P.b==blk ){     //CoverSetHouseは除外
                                        if( SolInfoDsp ){
                                            P.SetNoBBgColor(noB,AttCr,SolBkCr2); //Finのマーキング
                                            if( finMsg=="" ) finMsg = "\r   FinSet:";
                                            finMsg += " r"+(P.rc/9+1) + "c"+((P.rc%9)+1);
                                        }
                                    }
                                }
                                else{
                                    if( SolInfoDsp && (P.FreeB&noB)>0 ) P.SetNoBBgColor(noB,AttCr,SolBkCr);
                                }
                            } );
                        }

                        if( SolInfoDsp ){
                            msg = "\r    Digit: " + (no+1) + "\r  BaseSet:";                     
                            string rcSt = (tp==0)? " r": " c";
                            foreach( var nx in bsB.IEGet_BtoNo() )  msg += rcSt+(nx+1);

                            msg += "\r CoverSet:";
                            rcSt = (tp==0)? " c": " r";
                            foreach( var nx in csB.IEGet_BtoNo() )   msg += rcSt+(nx+1);

                            msg += finMsg;
                        }
                        goto LXwingEnd;
                    }
                }
            }
          
          LXwingEnd:
            if( SolCode<=0 )  return false;
            string[] FishNames=new string[]{ "Xwing","SwordFish","JellyFish","Squirmbag","Whale", "Leviathan" };
            Result = "Finned "+FishNames[sz-2];
            if( SolInfoDsp ) ResultLong = Result + msg;
            return true;
        }
    }
}