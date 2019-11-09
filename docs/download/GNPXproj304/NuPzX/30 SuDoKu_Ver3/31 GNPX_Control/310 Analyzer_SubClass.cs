using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

using GIDOO_space;

namespace GNPZ_sdk{
  #region Extended Function
    static public class StaticSAEx{ 
        static public IEnumerable<UCellEx> IEGetCellInHouse( this List<UCellEx> pBDL, int tfx, int FreeB=0x1FF ){
            int r=0, c=0, tp=tfx/9, fx=tfx%9;
            for( int nx=0; nx<9; nx++ ){
                switch(tp){
                    case 0: r=fx; c=nx; break;//row
                    case 1: r=nx; c=fx; break;//column
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//block
                }
                UCellEx P=pBDL[r*9+c];
                P.nx=nx;
                if( (P.FreeB&FreeB)>0 ) yield return P;
            }
        }
        static public IEnumerable<UCellEx> IEGetFixed_Pivot27( this List<UCellEx> pBDL, int rc0 ){
            int r0=rc0/9, c0=rc0%9, r=0, c=0;
            for( int tfx=0; tfx<27; tfx++ ){
                int fx=tfx%9;
                switch(tfx/9){
                    case 0: r=r0; c=fx; break; //row   
                    case 1: r=fx; c=c0; break; //Column
                    case 2: int b0=r0/3*3+c0/3; r=(b0/3)*3+fx/3; c=(b0%3)*3+fx%3; break;//block
                }
                if( r==r0 && c==c0 ) continue; //Exclude axis Cell
                int rc=r*9+c;
                if( pBDL[rc].No==0 ) continue; //Exclude unfixed Cell
                yield return pBDL[rc];
            }
        }

        static public IEnumerable<UCellEx> IEGetUCeNoB( this Bit81 BX, List<UCellEx> pBDL, int noBX ){ //nx=0...8        
            for( int n=0; n<3; n++ ){
                int bp = BX._BP[n];
                for( int k=0; k<27; k++){
                    if( ((bp>>k)&1)==0 ) continue;
                    UCellEx P=pBDL[n*27+k];
                    if( (P.FreeB&noBX)>0 )  yield return P;
                }
            }
        }
    }

  #endregion Extended Function
}