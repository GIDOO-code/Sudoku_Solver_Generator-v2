using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using GIDOO_space;

namespace GNPXcore{
    //First, understand Bit81, UCell, ConnectedCells, HouseCells, and IEGetCellInHouse.
    //Then the following algorithm is almost trivial.
    //http://csdenpe.web.fc2.com/page31a.html

    //*==*==*==*==* Last Digit *==*==*==*==*==*==*==*==* 
    public class SimpleSingleGen: AnalyzerBaseV2{
        public SimpleSingleGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }
        public bool LastDigit( ){
            bool  SolFound=false;
            for(int tfx=0; tfx<27; tfx++ ){ //house(row,column,block)
                if(pBDL.IEGetCellInHouse(tfx,0x1FF).Count()==1){    // only one element(digit) in house
                    SolFound=true;
                    var P=pBDL.IEGetCellInHouse(tfx,0x1FF).First();
                    P.FixedNo=P.FreeB.BitToNum()+1;                 
                    if(!chbConfirmMultipleCells)  goto LFound;
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
            foreach( UCell P in pBDL.Where(p=>p.FreeBC==1) ){   // only one element(digit) in cell
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
            for(int no=0; no<9; no++ ){ //no:digit
                int noB=1<<no;
                for(int tfx=0; tfx<27; tfx++ ){
                    if(pBDL.IEGetCellInHouse(tfx,noB).Count()==1){  //only one cell in house(tfx)
                        try{
                            var PLst=pBDL.IEGetCellInHouse(tfx,noB).Where(Q=>Q.FreeBC>1);
                            if(PLst.Count()<=0)  continue;
                            SolFound=true;  
                            var P=PLst.First();
                            P.FixedNo=no+1;            
                            if(!chbConfirmMultipleCells)  goto LFound;
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