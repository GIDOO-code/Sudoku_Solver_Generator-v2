using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public class patternGenerator{
        public SDK_Ctrl      pSDKCntrl;
        public int[,]        GPat;
        public int[]         PatB = new int[9];

        public patternGenerator( SDK_Ctrl SDKCx ){
            this.pSDKCntrl = SDKCx;
            GPat = new int[9,9];
        }

        //===== Automatic Pattern Generation =====
        public int patternAutoMaker( int patSel ){
            int CellNumMax = pSDKCntrl.CellNumMax;
            int nc, rB=0, cB=0, bB=0;

            do{
                GPat = new int[9,9];
                for( nc=0; nc<CellNumMax; ){
                    int r, c;
                    do{
                        r = SDK_Ctrl.GRandom.Next(0,9);
                        c = SDK_Ctrl.GRandom.Next(0,9);
                    }while(GPat[r,c]!=0);
                    nc = symmetryPattern(patSel,r,c,true);
                    if(nc>CellNumMax) break;
                }

                for(int r=0; r<9; r++ ){
                    for(int c=0; c<9; c++ ){
                        if( GPat[r,c]==0 ) continue;
                        rB |= 1<<r;
                        cB |= 1<<c;
                        bB |= 1<<(r/3*3+c/3);
                    }
                }

                rB = rB.BitCount();
                cB = cB.BitCount();
                bB = bB.BitCount();
            }while( rB<8 || cB<8 || bB<8 );
            _PatternToBit( );
            return nc;
        }
        public int symmetryPattern( int patSel, int r, int c, bool setFlag ){
            int pat = setFlag? 1: 1-GPat[r,c];
            GPat[r,c] = pat;
            switch(patSel){
                case 0: break;
                case 1: GPat[8-c,r]  =GPat[8-r,8-c]=GPat[c,8-r]=pat; break;
                case 2: GPat[8-r,8-c]=pat; break;
                case 3: GPat[r,8-c]  =pat; break;
                case 4: GPat[8-r,c]  =pat; break;
                case 5: GPat[c,r]    =pat; break;
                case 6: GPat[8-c,8-r]=pat; break;
            }
            int nn=0;
            for(int rc=0; rc<81; rc++ ) if( GPat[rc/9,rc%9]!=0) nn++;
            _PatternToBit( );

            return nn;
        }
        private void _PatternToBit( ){
            for(int r=3; r<9; r++ ){
                int pb=0;
                for(int c=3; c<9 ; c++ ){
                    if(GPat[r,c]>0) pb |= (1<<c);
                }
                PatB[r] = pb;
            }
            return;
        }
 
        //===== Pattern capture =====
        public int patternImport( UPuzzle pGP ){
            int nc=0;
            foreach( var P in pGP.BDL ){
                int n = P.No>0? 1: 0;
                GPat[P.r,P.c]= n;
                nc += n;
            }
            _PatternToBit( );   //Bit Representation of the Pattern
            return nc;
        }
    }
}