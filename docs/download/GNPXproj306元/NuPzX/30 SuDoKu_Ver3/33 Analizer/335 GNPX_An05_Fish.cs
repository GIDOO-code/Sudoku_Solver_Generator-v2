using System;
using System.Linq;
using static System.Console;
using GIDOO_space;

namespace GNPZ_sdk{
    public partial class FishGenEx: AnalyzerBaseV3{
        public FishGenEx( GNPX_AnalyzerManEx pAnMan ): base(pAnMan){ }

    //Fish
        public bool XWing(){     return Fish_Basic(2); }
        public bool SwordFish(){ return Fish_Basic(3); }
        public bool JellyFish(){ return Fish_Basic(4); }
        public bool Squirmbag(){ return Fish_Basic(5); }
        public bool Whale(){     return Fish_Basic(6); }
        public bool Leviathan(){ return Fish_Basic(7); }

    //FinnedFish
        public bool FinnedXWing(){     return Fish_Basic(2,fin:true); }
        public bool FinnedSwordFish(){ return Fish_Basic(3,fin:true); }
        public bool FinnedJellyFish(){ return Fish_Basic(4,fin:true); }
        public bool FinnedSquirmbag(){ return Fish_Basic(5,fin:true); }
        public bool FinnedWhale(){     return Fish_Basic(6,fin:true); }
        public bool FinnedLeviathan(){ return Fish_Basic(7,fin:true); }

        //=======================================================================
        public bool Fish_Basic( int sz, bool fin=false ){
            int rowSel=0x1FF, colSel=(0x1FF<<9);
            for( int no=0; no<9; no++ ){
                if( ExtFishSub(sz,no,18,rowSel,colSel,FinnedF:fin) ) return true;
                if( ExtFishSub(sz,no,18,colSel,rowSel,FinnedF:fin, _Fdef:false) ) return true;
            }
            return false;
        }

    //Frankenn/MutantFish
        private int rcbSel=0x7FFFFFF;
        public bool FrankenMutantFish( ){       
            for( int sz=2; sz<=4; sz++ ){   //no fin: max size is 4
                for( int no=0; no<9; no++ ){
                    if( ExtFishSub(sz,no,27,rcbSel,rcbSel,false) ) return true;
                }
            }
            return false;
        }
    //FinnedFrankenn/MutantFish
        public bool FinnedFrankenMutantFish( ){
            for( int sz=2; sz<=7; sz++ ){   //Finned:  max size is 7 (5:Squirmbag 6:Whale 7:Leviathan)
                for( int no=0; no<9; no++ ){
                    if( ExtFishSub(sz,no,27,rcbSel,rcbSel,true) ) return true;
                }
            }
            return false;
        }

    //=======================================================================
        private FishManEx FManEx=null;
        public bool ExtFishSub( int sz, int no, int FMSize, int BaseSel, int CoverSel, bool FinnedF, bool _Fdef=true ){       
            int noB=(1<<no);
            bool extFlag = (sz>=3 && ((BaseSel|CoverSel).BitCount()>18));
            if(_Fdef) FManEx=new FishManEx(this,FMSize,no,sz,extFlag);

            foreach( var Bas in FManEx.IEGet_BaseSetEx(BaseSel) ){                    //BaseSet

                foreach( var Cov in FManEx.IEGet_CoverSet(Bas,CoverSel,FinnedF) ){  //CoverSet
                    Bit81 FinB81 = Cov.FinB81;

                    Bit81 ELM =null;
                    if( FinB81.IsZero() ){  //===== no Fin =====
                        if( !FinnedF && (ELM=Cov.CoverB81-Bas.BaseB81).Count>0 ){                      
                            foreach( var P in ELM.IEGetUCeNoB(pBDL,noB) ){ P.CancelB=noB; SolCode=2; }
                            if(SolCode>0){
                                if( SolInfoB ){
                                    _Fish_FishResult(no,sz,Bas,Cov,(FMSize==27)); //FMSize 18:regular 27:Franken/Mutant
                                }
                                return true; 
                            }
                        }
                    }
                    else if( FinnedF ){     //===== Finned ===== 
                        Bit81 Ecand=Cov.CoverB81-Bas.BaseB81;
                        ELM=new Bit81();
                        foreach( var P in Ecand.IEGetUCeNoB(pBDL,noB) ){
                            if( (FinB81-ConnectedCells[P.rc]).Count==0 ) ELM.BPSet(P.rc);
                        }
                        if(ELM.Count>0){                           
                            foreach( var P in ELM.IEGet_rc().Select(p=>pBDL[p]) ){ P.CancelB=noB; SolCode=2; }   
                            if(SolCode>0){
                                if( SolInfoB ){
                                    _Fish_FishResult(no,sz,Bas,Cov,(FMSize==27)); //FMSize 18:regular 27:Franken/Mutant
                                }
                                return true;
                            }
                        }
                    }
                    continue;
                }
            }
            return false;
        }

        private void _Fish_FishResult( int no, int sz, UFishEx Bas, UFishEx Cov, bool FraMut ){
            int   HB=Bas.HouseB, HC=Cov.HouseC;
            Bit81 PB=Bas.BaseB81, PFin=Cov.FinB81; 
            string[] FishNames={ "Xwing","SwordFish","JellyFish","Squirmbag","Whale", "Leviathan" };
    
            try{
                string FinmsgH=( PFin.Count>0 )? "Finned ": "";
                int noB=(1<<no);         
                string Fsh = FishNames[sz-2]+" #"+(no+1);
                if(FraMut) Fsh = "Franken/Mutant "+Fsh;
                Fsh = FinmsgH+Fsh;
                Result=Fsh.Replace("Franken/Mutant","F/M");
            }
            catch( Exception ex ){
                WriteLine(ex.Message);
                WriteLine(ex.StackTrace);
            }
        }
    }  
}