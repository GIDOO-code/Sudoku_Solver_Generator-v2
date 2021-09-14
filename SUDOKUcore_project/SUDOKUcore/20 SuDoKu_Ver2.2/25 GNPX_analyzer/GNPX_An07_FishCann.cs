using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using static System.Math;
using static System.Diagnostics.Debug;
using GIDOO_space;

namespace GNPXcore{
    public partial class FishGen: AnalyzerBaseV2{
        //Autocannibalism
        //http://www.dailysudoku.com/sudoku/forums/viewtopic.php?p=26306&sid=13490447f6255f8d78a75b647a9096b9

        //http://forum.enjoysudoku.com/als-chains-with-overlap-cannibalism-t6580-30.html
        //http://www.dailysudoku.com/sudoku/forums/viewtopic.php?t=219&sid=dae2c2133114ee9513a6a37124374e7c
        //http://www.dailysudoku.co.uk/sudoku/forums/viewtopic.php?p=1180&highlight=#1180

        //.6...52..4..1..65.....6..3....3...65.5........7.5.....681457.......2.517.2.9..846

        public bool CannibalisticFMFish( ){
            for(int sz=2; sz<=7; sz++ ){// 4-->7  //Up to size 7 with Fin(5:Squirmbag 6:Whale 7:Leviathan)
                for(int no=0; no<9; no++ ){
                    if( CannibalisticFMFish_sub(sz,no,FMSize:27,FinnedF:true,EndoF:false,CannF:true) ) return true;
                }
            }
            return false;
        }

        public bool CannibalisticFMFish_sub( int sz, int no, int FMSize, bool FinnedF, bool EndoF=false, bool CannF=false ){
            int noB=(1<<no);
            int BaseSel=0x7FFFFFF, CoverSel=0x7FFFFFF;
            FishMan FMan=new FishMan(this,FMSize,no,sz,(sz>=3));
            foreach( var Bas in FMan.IEGet_BaseSet(BaseSel,FinnedF:FinnedF,EndoFlg:EndoF) ){    //BaseSet 
                foreach( var Cov in FMan.IEGet_CoverSet(Bas,CoverSel,CannF) ){                  //CoverSet
                    if( pAnMan.CheckTimeOut() ) return false;
                    Bit81 FinB81 = Bas.BaseB81 - Cov.CoverB81;

                    if( FinB81.Count==0 ){
                        foreach( var P in Cov.CannFin.IEGetUCeNoB(pBDL,noB) ){ P.CancelB=noB; SolCode=2; }
                        if(SolCode>0){
                            if(SolInfoB){
                                _Fish_FishResult(no,sz,Bas,Cov,(FMSize==27)); //FMSize 27:Franken/Mutant
                            }
                            //WriteLine(ResultLong); //___Debug_CannFish("Cannibalistic");
                            if(__SimpleAnalyzerB__)  return true;
                            if(!pAnMan.SnapSaveGP(true)) return true; 
                        }
                    }
                    else{
                        FinB81 |= Cov.CannFin;
                        Bit81 ELM =null;
                        Bit81 E=(Cov.CoverB81-Bas.BaseB81) | Cov.CannFin;
                        ELM=new Bit81();
                        foreach( var rc in E.IEGet_rc() ){
                            if( (FinB81-ConnectedCells[rc]).Count==0 ) ELM.BPSet(rc);
                        }
                        if( ELM.Count>0 ){
                            foreach( var P in ELM.IEGetUCeNoB(pBDL,noB) ){ P.CancelB=noB; SolCode=2; }
                            if( SolCode>0 ){
                                if(SolInfoB)_Fish_FishResult(no,sz,Bas,Cov,(FMSize==27));
                                //WriteLine(ResultLong); //___Debug_CannFish("Finned Cannibalistic");
                                if(__SimpleAnalyzerB__)  return true;
                                if(!pAnMan.SnapSaveGP(true)) return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void ___Debug_CannFish(string MName){
            using( var fpX=new StreamWriter(" ##DebugP.txt",append:true,encoding:Encoding.UTF8) ){
                string st="";
                pBDL.ForEach(q =>{ st += (Max(q.No,0)).ToString(); } );
                st=st.Replace("0",".");
                fpX.WriteLine(st+" "+MName);
            }
        }
    }  
}