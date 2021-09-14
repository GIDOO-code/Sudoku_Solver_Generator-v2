using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPXcore{
    public partial class FishGen: AnalyzerBaseV2{
    //  http://forum.enjoysudoku.com/search.php?keywords=Endo&t=4993&sf=msgonly
    //  latest viewpoint
    //  Fin Cell: Any cell that's in more Base Sectors than Cover Sectors.
    //  Possible Elimination Cell: Any cell that's in more Cover Sectors than Base Sectors.
    //  Actual Elimination Cell: All possible elimination cells if no fin cells exist. 
    //  Otherwise, all possible elimination cells that are a buddy to every fin cell. 
    //  An exception to the buddy restriction exists for Kraken fish.

    //  Endo-fin
    //  http://www.dailysudoku.com/sudoku/forums/viewtopic.php?p=32379&sid=8fb87da8d9beec9c11a2909cae5adecf

        public bool EndoFinnedFMFish( ){
            for(int sz=2; sz<=7; sz++){   //(5:Squirmbag 6:Whale 7:Leviathan)
                for(int no=0; no<9; no++){
                    if( EndoFinnedFMFish_sub(sz,no,FMSize:27,FinnedF:true,EndoF:true,CannF:false) ) return true;
                }
            }
            return false;
        }

        public bool EndoFinnedFMFish_sub( int sz, int no, int FMSize, bool FinnedF, bool EndoF=false, bool CannF=false ){   
            int noB=(1<<no);
            int BaseSel=0x7FFFFFF, CoverSel=0x7FFFFFF;
            FishMan FMan=new FishMan(this,FMSize,no,sz,(sz>=3));
            foreach( var Bas in FMan.IEGet_BaseSet(BaseSel,FinnedF:FinnedF,EndoFlg:EndoF) ){ //BaseSet

                foreach(var Cov in FMan.IEGet_CoverSet(Bas,CoverSel,CannF)){               //CoverSet
                    if(pAnMan.CheckTimeOut()) return false; 
                    Bit81 FinB81 = Cov.FinB81 | Bas.EndoFin;
                    Bit81 E=Cov.CoverB81-Bas.BaseB81;
                    Bit81 ELM=new Bit81();

                    //see latest viewpoint
                    foreach( var rc in E.IEGet_rc() ){
                        if((FinB81-ConnectedCells[rc]).Count==0) ELM.BPSet(rc);
                    }
                    if( ELM.Count>0 ){
                        foreach(var P in ELM.IEGetUCeNoB(pBDL,noB)){ P.CancelB=noB; SolCode=2; }
                        if(SolCode>0){
                            if(SolInfoB){
                                _Fish_FishResult(no,sz,Bas,Cov,(FMSize==27)); //27:Franken/Mutant
                            }
                            //WriteLine(ResultLong);
                            if(__SimpleAnalyzerB__)  return true;
                            if(!pAnMan.SnapSaveGP(true)) return true;
                        }
                    }
                }
            }
            return false;
        }
    }  
}