using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    //*==*==*==*==* Last Digit *==*==*==*==*==*==*==*==* 
    public class SimpleSingleGenEx: AnalyzerBaseV3{
        public SimpleSingleGenEx( GNPX_AnalyzerManEx pAnMan ): base(pAnMan){ }

        public bool LastDigitEx( ){
            bool  SolFond=false;
            for( int tfx=0; tfx<27; tfx++ ){
                if( pBDL.IEGetCellInHouse(tfx,0x1FF).Count()==1 ){
                    SolFond=true;
                    var P=pBDL.IEGetCellInHouse(tfx,0x1FF).First();
                    P.FixedNo=P.FreeB.BitToNum()+1;                 
                }
            }

            if(SolFond){
                SolCode=1;
                Result="Last Digit";
                return true;
            }
            return false;
        }

        //*==*==*==*==* Naked Single *==*==*==*==*==*==*==*==* 
        public bool NakedSingleEx( ){
            bool  SolFond=false;
            foreach( UCellEx P in pBDL.Where(p=>p.FreeBC==1) ){
                SolFond=true;
                P.FixedNo=P.FreeB.BitToNum()+1;      
            }

            if(SolFond){
                SolCode=1;
                Result="Naked Single";
                return true;
            }
            return false;
        }

        //*==*==*==*==* Hidden Single *==*==*==*==*==*==*==*==*
        public bool HiddenSingleEx( ){
            bool  SolFond=false;
            for( int no=0; no<9; no++ ){
                int noB=1<<no;
                for( int tfx=0; tfx<27; tfx++ ){
                    if( pBDL.IEGetCellInHouse(tfx,noB).Count()==1 ){
                        SolFond=true;
                        var P=pBDL.IEGetCellInHouse(tfx,noB).First();
                        if(P.FreeBC==1)  continue;
                        P.FixedNo=no+1;             
                    }
                }
            }

            if(SolFond){
                SolCode=1;
                Result="Hidden Single";
                return true;
            }
            return false;
        }
    }
}