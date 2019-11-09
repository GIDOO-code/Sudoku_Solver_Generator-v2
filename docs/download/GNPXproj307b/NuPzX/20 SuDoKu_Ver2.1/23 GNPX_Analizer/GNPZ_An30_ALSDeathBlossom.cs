using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GIDOO_space;

namespace GNPZ_sdk{
    public partial class ALSTechGen: AnalyzerBaseV2{

        public bool ALS_DeathBlossom(){
			Prepare();
            if( ALSMan.ALSLst==null || ALSMan.ALSLst.Count<=2 ) return false;
            ALSMan.QSearch_Cell2ALS_Link();

            for(int sz=2; sz<=4; sz++ ){ //Size 4 and over ALS DeathBlossom was not found ?
                if( _ALS_DeathBlossomSubEx(sz,false) ) return true;
            }
            return false;
        }
        public bool ALS_DeathBlossomExt(){
			Prepare();
            if( ALSMan.ALSLst==null || ALSMan.ALSLst.Count<=2 ) return false;
            ALSMan.QSearch_Cell2ALS_Link();

            for(int sz=2; sz<=4; sz++ ){
                if( _ALS_DeathBlossomSubEx(sz,true) ) return true;    
            }
            return false;
        }
        private bool _ALS_DeathBlossomSub( int sz ){
            foreach( var SC in pBDL.Where(p=>p.FreeBC==sz) ){ //Stem Cell
                if(pAnMan.CheckTimeOut()) return false;
                List<LinkCellALS> LinkCeAlsLst=ALSMan.LinkCeAlsLst[SC.rc];
                if( LinkCeAlsLst==null || LinkCeAlsLst.Count<sz ) continue;

                int nxt=0, PFreeB=SC.FreeB;
                var cmb=new Combination(LinkCeAlsLst.Count,sz);
                while(cmb.Successor(nxt)){
                    int FreeB=SC.FreeB, AFreeB=0x1FF;
                    for(int k=0; k<sz; k++ ){
                        nxt=k;
                        var LK=LinkCeAlsLst[cmb.Index[k]];      //Link[cell-ALS]
                        if( (FreeB&(1<<LK.nRCC))==0 ) goto LNxtCmb;
                        FreeB = FreeB.BitReset(LK.nRCC);
                        AFreeB &= LK.ALS.FreeB;
                        if( AFreeB==0 ) goto LNxtCmb;
                    }
                    if( FreeB!=0 || AFreeB==0 ) continue;

                    AFreeB = AFreeB.DifSet(SC.FreeB);
                    foreach( var no in AFreeB.IEGet_BtoNo() ){
                        int noB=(1<<no);
                        Bit81 Ez=new Bit81();
                        for(int k=0; k<sz; k++ ){
                            var ALS=LinkCeAlsLst[cmb.Index[k]].ALS;
                            var UClst=ALS.UCellLst;
                            foreach( var P in UClst.Where(p=>(p.FreeB&noB)>0) )  Ez.BPSet(P.rc);
                        }

                        foreach( var P in pBDL.Where(p=>(p.FreeB&noB)>0) ){
                            if( (Ez-ConnectedCells[P.rc]).IsZero() ){ P.CancelB=noB; SolCode=2; }
                        }
                        if(SolCode<1) continue;
                        
                        var LKCAsol=new List<LinkCellALS>();
                        Array.ForEach(cmb.Index,nx=> LKCAsol.Add(LinkCeAlsLst[nx]) );
                        _DeathBlossom_SolResult(LKCAsol,SC,no);
                        if(!pAnMan.SnapSaveGP(true))  return true;
                    }
                LNxtCmb:
                    continue;
                }

            }
            return false;
        }

        private bool _ALS_DeathBlossomSubEx( int sz, bool stmLink=false ){
            int szM= (stmLink? sz-1: sz);
            foreach( var SC in pBDL.Where(p=>p.FreeBC==sz) ){ //Stem Cell
                if(pAnMan.CheckTimeOut()) return false;
                List<LinkCellALS> LinkCeAlsLst=ALSMan.LinkCeAlsLst[SC.rc];
                if( LinkCeAlsLst==null || LinkCeAlsLst.Count<sz ) continue;

                int nxt=0, PFreeB=SC.FreeB;
                var cmb=new Combination(LinkCeAlsLst.Count,szM);
                while(cmb.Successor(nxt)){
                    int FreeB=SC.FreeB, AFreeB=0x1FF;
                    for(int k=0; k<szM; k++ ){
                        nxt=k;
                        var LK=LinkCeAlsLst[cmb.Index[k]];      //Link[cell-ALS]
                        if( (FreeB&(1<<LK.nRCC))==0 ) goto LNxtCmb;
                        FreeB = FreeB.BitReset(LK.nRCC);
                        AFreeB &= LK.ALS.FreeB;
                        if( AFreeB==0 ) goto LNxtCmb;
                    }

                    if(stmLink){
                        if( FreeB.BitCount()!=1 || (FreeB&AFreeB)==0 )  continue;
                        int no=FreeB.BitToNum();
                        int noB=FreeB;

                        Bit81 Ez=new Bit81();
                        for(int k=0; k<szM; k++ ){
                            var ALS=LinkCeAlsLst[cmb.Index[k]].ALS;
                            var UClst=ALS.UCellLst;
                            foreach( var P in UClst.Where(p=>(p.FreeB&noB)>0) )  Ez.BPSet(P.rc);
                        }

                        foreach( var P in ConnectedCells[SC.rc].IEGet_rc().Select(rc=>pBDL[rc]) ){
                            if( (P.FreeB&noB)==0 ) continue;
                            if( (Ez-ConnectedCells[P.rc]).IsZero() ){ P.CancelB=noB; SolCode=2; }
                        }
                        if(SolCode<1) continue;
                        
                        var LKCAsol=new List<LinkCellALS>();
                        Array.ForEach(cmb.Index,nx=> LKCAsol.Add(LinkCeAlsLst[nx]) );
                        _DeathBlossom_SolResult(LKCAsol,SC,no,stmLink);

                        if(__SimpleAnalizerB__)  return true;
                        if(!pAnMan.SnapSaveGP(true))  return true;

                    }
                    else if( FreeB==0 && AFreeB>0 ){
                        AFreeB = AFreeB.DifSet(SC.FreeB);
                        foreach( var no in AFreeB.IEGet_BtoNo() ){
                            int noB=(1<<no);
                            Bit81 Ez=new Bit81();
                            for(int k=0; k<sz; k++ ){
                                var ALS=LinkCeAlsLst[cmb.Index[k]].ALS;
                                var UClst=ALS.UCellLst;
                                foreach( var P in UClst.Where(p=>(p.FreeB&noB)>0) )  Ez.BPSet(P.rc);
                            }

                            foreach( var P in pBDL.Where(p=>(p.FreeB&noB)>0) ){
                                if( (Ez-ConnectedCells[P.rc]).IsZero() ){ P.CancelB=noB; SolCode=2; }
                            }
                            if(SolCode<1) continue;
                        
                            var LKCAsol=new List<LinkCellALS>();
                            Array.ForEach(cmb.Index,nx=> LKCAsol.Add(LinkCeAlsLst[nx]) );
                            _DeathBlossom_SolResult(LKCAsol,SC,no,stmLink);

                            if(__SimpleAnalizerB__)  return true;
                            if(!pAnMan.SnapSaveGP(true))  return true;
                        }
                    }
                
                LNxtCmb:
                    continue;
                }
            }
            return false;
        }

        private void _DeathBlossom_SolResult( List<LinkCellALS> LKCAsol, UCell SC, int no, bool stmLink=false ){
            string st0 = "ALS Death Blossom";
            if(stmLink) st0 += "Ext";
            Color cr = _ColorsLst[0];////Colors.Gold;
            SC.SetNoBBgColor(SC.FreeB,AttCr3,cr);
            string st = $"\r Cell r{(SC.r+1)}c{(SC.c+1)} #{SC.FreeB.ToBitStringNZ(9)}";
            bool Overlap=false;
            Bit81 OV=new Bit81();
            int   k=0, noB=(1<<no);
            foreach( var LK in LKCAsol ){
                int noB2=1<<LK.nRCC;
                cr = _ColorsLst[++k];
                LK.ALS.UCellLst.ForEach( P=> {
                    P.SetNoBBgColor(noB,AttCr,cr);
                    P.SetNoBBgColor(noB2,AttCr3,cr);
                    if( OV.IsHit(P.rc) ) Overlap=true;
                    OV.BPSet(P.rc);
                } );
                st += $"\r     -#{(LK.nRCC+1)}-ALS{k} {LK.ALS.ToStringRCN()}";
            }

            if(Overlap) st0+=" [overlapping]";
            Result = st0;
            if( SolInfoB ) ResultLong=st0+st;
        }
    }
}