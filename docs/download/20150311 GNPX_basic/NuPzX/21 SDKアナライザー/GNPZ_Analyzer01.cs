using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        // singles
        //*==*==*==*==* Last Digit *==*==*==*==*==*==*==*==* 
        public bool GNP00_LastDigit( ){
            for( int tfx=0; tfx<27; tfx++ ){
                if( pBDL.IEGet(tfx,0x1FF).Count()==1 ){
                    SolCode=1;
                    var P = pBDL.IEGet(tfx,0x1FF).First();
                    P.FixedNo = P.FreeB.BitToNum()+1;                 
                    if( !MltSolOn )  goto LFond;
                }
            }

          LFond:
            if( SolCode>0 ){
                if( SolInfoDsp ) ResultLong = "Last Digit";
                Result = "Last Digit";
                SnapSaveGP();
                return true;
            }
            return false;
        }

        //*==*==*==*==* Naked Single *==*==*==*==*==*==*==*==*
        public bool GNP00_NakedSingle( ){
            foreach( UCell P in pBDL.Where(p=>p.FreeBC==1) ){
                SolCode   = 1;
                P.FixedNo = P.FreeB.BitToNum()+1;      
                if( !MltSolOn )  goto LFond;
            }
          LFond:
            if( SolCode>0 ){
                Result = "Naked Single";
                if( SolInfoDsp ) ResultLong = Result;

                SnapSaveGP();
                return true;
            }
            return false;
        }

        //*==*==*==*==* Hidden Single *==*==*==*==*==*==*==*==*
        public bool GNP00_HiddenSingle( ){
            for( int no=0; no<9; no++ ){
                int noB=1<<no;
                for( int tfx=0; tfx<27; tfx++ ){
                    if( pBDL.IEGet(tfx,noB).Count()==1 ){
                        SolCode = 1;
                        var P = pBDL.IEGet(tfx,noB).First();
                        if( P.FreeBC==1 )  continue;
                        P.FixedNo = no+1;             
                        if( !MltSolOn )  goto LFond;
                    }
                }
            }
          LFond:
            if( SolCode>0 ){
                if( SolInfoDsp ) ResultLong = "Hidden Single";
                Result = "Hidden Single";
                SnapSaveGP();
                return true;
            }
            return false;
        }


        //===== old style =====  公開時は削除
        public bool GNP00_LastDigitOld( ){
            int rc=0;
            UCell P0=null;
            
            for( int tfx=0; tfx<27; tfx++ ){
                int cc=0;
                for( int nx=0; nx<9; nx++ ){
                    UCell P = GetCell_House( pBDL, tfx, nx, ref rc );
                    if( P.No==0 ){
                        if( ++cc>=2 ) goto nextTry;
                        P0 = P;
                    }
                }
                if( cc==1 ){
                    SolCode=1;
                    P0.FixedNo = P0.FreeB.BitToNum()+1;
                    if( !MltSolOn )  goto LFond;
                }
            nextTry:
                continue;
            }

          LFond:
            if( SolCode<=0 )  return  false;
            if( SolInfoDsp ) ResultLong = "Last Digit";
            Result = "Last Digit";
            return true;
        }
        private UCell GetCell_House( List<UCell> pBDL, int tfx, int nx, ref int rc ){ //nx=0...8
            int r=0, c=0, fx=tfx%9;
            switch(tfx/9){
                case 0: r=fx; c=nx; break;//行
                case 1: r=nx; c=fx; break;//列
                case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//ブロック
            }
            return pBDL[r*9+c];
        }
    }
}