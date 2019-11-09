using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using GIDOO_space;

namespace GNPZ_sdk{
    //*==*==*==*==* Last Digit *==*==*==*==*==*==*==*==* 
    public class SimpleSingleGen: AnalyzerBaseV2{
        public SimpleSingleGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }

        public bool LastDigit( ){
            bool  SolFound=false;
            for(int tfx=0; tfx<27; tfx++ ){
                if( pBDL.IEGetCellInHouse(tfx,0x1FF).Count()==1 ){
                    SolFound=true;
                    var P=pBDL.IEGetCellInHouse(tfx,0x1FF).First();
                    P.FixedNo=P.FreeB.BitToNum()+1;                 
                    if( !chbConfirmMultipleCells )  goto LFound;
                }
            }

          LFound:
            if(SolFound){
                SolCode=1;
                Result="Last Digit";
                if(__SimpleAnalizerB__)  return true;
                if(SolInfoB) ResultLong="Last Digit";
                pAnMan.SnapSaveGP();
                return true;
            }
            return false;
        }

        //*==*==*==*==* Naked Single *==*==*==*==*==*==*==*==* 
        public bool NakedSingle( ){
            bool  SolFound=false;
            foreach( UCell P in pBDL.Where(p=>p.FreeBC==1) ){
                SolFound=true;
                P.FixedNo=P.FreeB.BitToNum()+1;      
                if(!chbConfirmMultipleCells)  goto LFound;
            }

          LFound:
            if(SolFound){
                SolCode=1;
                Result="Naked Single";
                if(__SimpleAnalizerB__)  return true;
                if(SolInfoB) ResultLong="Naked Single";
                pAnMan.SnapSaveGP();
                return true;
            }
            return false;
        }

        //*==*==*==*==* Hidden Single *==*==*==*==*==*==*==*==*

        public bool HiddenSingle( ){
            bool  SolFound=false;
            for(int no=0; no<9; no++ ){
                int noB=1<<no;
                for(int tfx=0; tfx<27; tfx++ ){
                    if( pBDL.IEGetCellInHouse(tfx,noB).Count()==1 ){
                        try{
                            var PLst=pBDL.IEGetCellInHouse(tfx,noB).Where(Q=>Q.FreeBC>1);
                            if(PLst.Count()<=0)  continue;
                            var P=PLst.First();
                            var PL=pBDL.IEGetCellInHouse(tfx,noB).Where(Q=>Q.FreeBC>1).ToList();    
                            var P2Lst=pBDL.IEGetCellInHouse(tfx,noB);
                            if(P2Lst.Count()<=0)  continue;
                            var P2=P2Lst.First();
                            if(P2.FreeBC==1)  continue;   

                            P.FixedNo=no+1;             
                            if( !chbConfirmMultipleCells )  goto LFound;
                            SolFound=true;    
                        }
                        catch(Exception e){ WriteLine($"{e.Message}\r{e.StackTrace}"); }
                    }
                }
                
            }

          LFound:
            if(SolFound){
                SolCode=1;
                Result="Hidden Single";
                if(__SimpleAnalizerB__)  return true;
                if(SolInfoB) ResultLong="Hidden Single";
                pAnMan.SnapSaveGP();
                return true;
            }
            return false;
        }

    }
}