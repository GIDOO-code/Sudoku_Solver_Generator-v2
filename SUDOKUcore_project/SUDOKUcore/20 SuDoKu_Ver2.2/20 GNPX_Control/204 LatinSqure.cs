using System;
using static System.Console;

namespace GNPXcore{
    public partial class LatinSqureGen{
        static  public int? SeedX=null;
        private bool    __DEBUGmode__=false;  //true;

        private readonly int LatSqrParaMax = 56*6*6*6*6*6*6;
        private readonly int[,] permList = {{0,1,2}, {0,2,1}, {1,0,2}, {2,0,1}, {1,2,0}, {2,1,0} };
        private readonly int[,] p123     = {{1,2,3}, {2,3,1}, {3,1,2} };
        private readonly int[,] U456789  = {
                                           {4,5,6,7,8,9}, {4,5,7,6,8,9}, {4,5,8,6,7,9}, {4,5,9,6,7,8}, {4,6,7,5,8,9},
                                           {4,6,8,5,7,9}, {4,6,9,5,7,8}, {4,7,8,5,6,9}, {4,7,9,5,6,8}, {4,8,9,5,6,7} };
        private readonly int[,] U258369  = {
                                           {2,5,8,3,6,9}, {2,3,5,6,8,9}, {2,5,6,3,8,9}, {2,5,9,3,6,8}, {2,3,8,5,6,9},
                                           {2,6,8,3,5,9}, {2,8,9,3,5,6}, {2,3,6,5,8,9}, {2,6,9,3,5,8}, {2,3,9,5,6,8} };
        private readonly int[,] p147     = {{1,4,7}, {4,7,1}, {7,1,4} };
        
        public int[] PTop=new int[8];
        public int[] PLft=new int[8];

        private int[,] pSol99;  //int[9,9];

        public LatinSqureGen( ){ }

        public void GeneratePara(int s1,int s2,ref int[,] pSol99,ref int[] PTopR,ref int[] PLftR){
            GeneratePara( ref pSol99, s1, s2 );
            PTopR=PTop; PLftR=PLft;
        }
        public void GeneratePara(ref int[,] pSol99,int s1=0,int s2=0){
            this.pSol99 = pSol99;
            _SetPara(s1,ref PTop);
            _SetPara(s2,ref PLft);           
            //WriteLine("-------------------- Top:{0} Left:{1}", s1, s2 );
                       
            _LatinSqureSub_00();
            _LatinSqureSub_01();
            _LatinSqureSub_02();
            
            _LatinSqureSub_11();
            _LatinSqureSub_12();

            if(__DEBUGmode__) __DBUGprint( );
        }

        public void _SetPara( int s, ref int[] p){
            if(s==0) s=SDK_Ctrl.GRandom.Next(1,LatSqrParaMax);
            int s2=s%56;
            if(s2==0) p[0]=p[1]=0;
            else if(s2<28){ s2+=2; p[0]=s2/3; p[1]=s2%3; }
            else if(s2==28){ p[0]=10; p[1]=0; }
            else{ s2+=4; p[0]=s2/3; p[1]=s2%3; }
                //Write( $"s{s:#0} s2:{s2:#0} /" );

            s/=56; p[2]=s%6; p[3]=(s/6)%6; p[4]=(s/36)%6;
            p[5]=(s/216)%6; p[6]=(s/1296)%6; p[7]=(s/7776)%6;
                //for(int k=0; k<8; k++) Write( $"{p[k]} " );
                //WriteLine();         
        }
      
        private void _LatinSqureSub_00( ){
            for(int rc=0; rc<81; rc++) pSol99[rc/9,rc%9]=0;
            for(int rc=0; rc<9; rc++)  pSol99[rc/3,rc%3]=rc+1;
        }

        private void  _LatinSqureSub_01( ){
            int r=0;
            int px=PTop[0]%10;
            int cA=(PTop[0]<10)? 0: 3;
            int cB=3-cA;

            for(int c=0; c<3; c++) pSol99[r,permList[PTop[2],c]+3] = U456789[px,c+cA];
            for(int c=0; c<3; c++) pSol99[r,permList[PTop[3],c]+6] = U456789[px,c+cB];
        }
        private void  _LatinSqureSub_02(){
            int nn, cc=0;
            int px=PTop[0]%10;
            int cA=(PTop[0]<10)? 0: 3;
            int cB=3-cA;

            //block2
            for(int c=0; c<3; c++){
                nn = U456789[px,c+cB];
                pSol99[1,permList[PTop[4],c]+3] = (nn<7)? p123[PTop[1],c]: nn;
                pSol99[2,permList[PTop[5],c]+3] = (nn>6)? p123[PTop[1],c]: nn;
                if(nn<7) cc++;
            }
            
            //block3
            for(int c=0; c<3; c++){
                cc %= 3;
                nn = U456789[px,c+cA];
                pSol99[1,permList[PTop[6],c]+6] = (nn<7)? p123[PTop[1],cc]: nn;
                pSol99[2,permList[PTop[7],c]+6] = (nn>6)? p123[PTop[1],cc]: nn;
                cc++;
            }
        }

        private void _LatinSqureSub_11( ){
            int c=0;
            int px=PLft[0]%10;
            int rA=(PLft[0]<10)? 0: 3;
            int rB=3-rA;

            for(int r=0; r<3; r++) pSol99[permList[PLft[2],r]+3,c] = U258369[px,r+rA];
            for(int r=0; r<3; r++) pSol99[permList[PLft[3],r]+6,c] = U258369[px,r+rB];
        }
        private void _LatinSqureSub_12( ){
            int nn,rr=0,rs=0;
            int px=PLft[0]%10;
            int rA=(PLft[0]<10)? 0: 3;
            int rB=3-rA;
            //__DBUGprint( );

            //Block4,Column2
            for(int r=0; r<3; r++){
                nn=U258369[px,r+rB];
                pSol99[permList[PLft[4],r]+3,1] = ((nn%3)==2)? p147[PLft[1],rr]: nn;
                if((nn%3)==2) rr++;
            }
            rs=rr;
            //__DBUGprint();

            if(pSol99[3,0]==pSol99[3,2]){ WriteLine("<<< Error >>>"); }

            //Block7,Column2
            for(int r=0; r<3; r++){
                rr%=3;
                nn=U258369[px,r+rA];
                pSol99[permList[PLft[5],r]+6,1] = ((nn%3)==2)? p147[PLft[1],rr]: nn;
                if((nn%3)==2) rr++;
            }
            //__DBUGprint( );

            //Block4,Column3
            for(int r=0; r<3; r++){
                rs%=3;
                nn=U258369[px,r+rB];
                pSol99[permList[PLft[6],r]+3,2] = ((nn%3)==0)? p147[PLft[1],rs]: nn;
                if((nn%3)==0) rs++;
            }
            //__DBUGprint( );

            //Block7,Column3
            for(int r=0; r<3; r++){
                rs%=3;
                nn=U258369[px,r+rA];
                pSol99[permList[PLft[7],r]+6,2] = ((nn%3)==0)? p147[PLft[1],rs]: nn;
                if((nn%3)==0) rs++;
            }
            //__DBUGprint();
        }
        private void __DBUGprint( ){
            string po;
            WriteLine();
            for(int r=0; r<9; r++){
                po = r.ToString("##0:");
                for(int c=0; c<9; c++){
                    int P=pSol99[r,c];
                    po += (P==0)? " .": P.ToString(" #");
                }
                WriteLine(po);
            }
        }

    }

}