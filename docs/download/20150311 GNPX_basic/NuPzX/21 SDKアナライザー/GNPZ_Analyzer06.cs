using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        //通常のFinned Fish も解くが計算量は多い。EmptyRectangle...を先にトライする。
        private int rcbSel=0x7FFFFFF;
        public bool GNP00_FrankenMutantFish( ){       
            for( int sz=2; sz<=4; sz++ ){   //対称性からサイズ4まで
                for( int no=0; no<9; no++ ){
                    if( GNP00_ExtFish_FishSub(sz,no,27,rcbSel,rcbSel,false) ) return true;
                    if( CheckTimeOut() ) return false;
                }
            }
            return false;
        }

        public bool GNP00_FinnedFrankenMutantFish( ){
            for( int sz=2; sz<=7; sz++ ){   //Finあり ではサイズ7まで(5:Squirmbag 6:Whale 7:Leviathan)
                for( int no=0; no<9; no++ ){
                    if( GNP00_ExtFish_FishSub(sz,no,27,rcbSel,rcbSel,true) ) return true;
                    if( CheckTimeOut() ) return false;
                }
            }
            return false;
        }

        public bool GNP00_ExtFish_FishSub( int sz, int no, int FMSize, int BaseSel, int CoverSel,
                                 bool FinnedF, bool EndoF=false, bool CannF=false ){            

            int noB=(1<<no);
            FishMan FMan=new FishMan(this,FMSize,no,sz);
            foreach( var Bas in FMan.IEGet_BaseSet(BaseSel,EndoF) ){            //BaseSet生成
                if( CheckTimeOut() ) return false;

                foreach( var Cov in FMan.IEGet_CoverSet(Bas,CoverSel,CannF) ){   //CoverSet生成
                    Bit81 FinB81 = Cov.FinB81;

                    Bit81 ELM =null;
                    if( FinB81.IsZero() ){  //===== Finなし =====
                        if( !FinnedF && (ELM=Cov.CoverB81-Bas.BaseB81).Count>0 ){
                            SolCode=2;
                            foreach( var P in ELM.IEGet_rc().Select(p=>pBDL[p]) ) P.CancelB=noB;      
                            if( SolInfoDsp ){
                                _Fish_FishResult(no,sz,Bas,Cov,(FMSize==27)); //FMSize 18:regular 27:Franken/Mutant
                            }
                          //Console.WriteLine(ResultLong);
                            if( !SnapSaveGP(true) ) return true; 
                        }
                    }
                    else if( FinnedF ){     //===== Finあり =====
                        Bit81 E=Cov.CoverB81-Bas.BaseB81;
                        ELM=new Bit81();
                        foreach( var rc in E.IEGet_rc() ){
                            if( (FinB81-ConnectedCells[rc]).Count==0 ) ELM.BPSet(rc);
                        }
                        if( ELM.Count>0 ){
                            SolCode=2;
                            foreach( var P in ELM.IEGet_rc().Select(p=>pBDL[p]) ) P.CancelB=noB;      
                            if( SolInfoDsp ){
                                _Fish_FishResult(no,sz,Bas,Cov,(FMSize==27)); //FMSize 18:regular 27:Franken/Mutant
                            }
                          //Console.WriteLine(ResultLong);
                            if( !SnapSaveGP(true) ) return true; 
                        }
                    }
                    continue;
                }
            }
            return false;
        }

        private void _Fish_FishResult( int no, int sz, UFish Bas, UFish Cov, bool FraMut ){
            int   HB=Bas.HouseB, HC=Cov.HouseC;
            Bit81 PB=Bas.BaseB81, PFin=Cov.FinB81; 
            Bit81 EndoFin=Bas.EndoFin, CnaaFin=Cov.CannFin;
            string[] FishNames={ "Xwing","SwordFish","JellyFish","Squirmbag","Whale", "Leviathan" };
    
            PFin-=EndoFin;
            try{
                int noB=(1<<no);                 
                foreach( var P in PB.IEGet_rc().Select(p=>pBDL[p]) )   P.SetNoBBgColor(noB,AttCr,SolBkCr);
                foreach( var P in PFin.IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,SolBkCr2);
                foreach( var P in EndoFin.IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,SolBkCr3);
                foreach( var P in CnaaFin.IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,SolBkCr3);

                string msg = "\r     Digit: " + (no+1);                 
                msg += "\r   BaseSet: " + _HouseToString(HB);
                msg += "\r  CoverSet: " + _HouseToString(HC);
 
                string FinmsgH="", FinmsgT="";
                if( PFin.Count>0 ){
                    FinmsgH = "Finned ";
                    string st="";
                    foreach( var rc in PFin.IEGet_rc() ) st += " "+rc.ToRCString();
                    msg += "\r    FinSet: "+st.ToString_SameHouseComp();
                
                }
                 
                if( !EndoFin.IsZero() ){
                    FinmsgT = " with Endo Fin";
                    string st="";
                    foreach( var rc in EndoFin.IEGet_rc() ) st += " "+rc.ToRCString();
                    msg += "\r  Endo Fin: "+st.ToString_SameHouseComp();
                }

                if( !CnaaFin.IsZero() ){
                    FinmsgH = "Cannibalistic ";
                    if( PFin.Count>0 ) FinmsgH = "Finned Cannibalistic ";
                    string st="";
                    foreach( var rc in CnaaFin.IEGet_rc() ) st += " "+rc.ToRCString();
                    msg += "\r  Cannibalistic: "+st.ToString_SameHouseComp();
                }

                string Fsh = FishNames[sz-2];
                if( FraMut) Fsh = "Franken/Mutant "+Fsh;
                Fsh = FinmsgH+Fsh+FinmsgT;
                ResultLong = Fsh+msg;  
                if( Fsh.Length>40 ) Fsh=Fsh.Replace("Franken/Mutant","F/M");
                Result = Fsh+" #"+(no+1);

            }
            catch( Exception ex ){
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        public string _HouseToString( int HH ){
            string st="";
            if( (HH&0x1FF)>0 ) st += "r"+(HH&0x1FF).ToBitStringN(9)+" ";
            if( ((HH>>=9)&0x1FF)>0 ) st += "c"+(HH&0x1FF).ToBitStringN(9)+" ";
            if( ((HH>>=9)&0x1FF)>0 ) st += "B"+(HH&0x1FF).ToBitStringN(9)+" ";
            return st.Trim();
        }
    }  
}
