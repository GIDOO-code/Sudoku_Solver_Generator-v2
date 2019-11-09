using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace GNPZ_sdk{
    public partial class LatinSqureGen{
        public void  _LatinSqureSub_01R( int[,] Q, int[] PTX ){
            int px, cA;
            for( int s=0; s<56; s++ ){
                int s2=s;
                if(s==0) PTX[0]=PTX[1]=0;
                else if(s<28){ s2+=2; PTX[0]=s2/3; PTX[1]=s2%3; }
                else if(s==28){       PTX[0]=10;   PTX[1]=0; }
                else{ s2+=4;          PTX[0]=s2/3; PTX[1]=s2%3; }

                px=PTX[0]%10;
                cA=(PTX[0]<10)? 0: 3;

                //row1
                for( int c2=0; c2<6; c2++ ){
                    if( !__01Sub(Q,U456789,0,c2,3,px,cA) ) continue;
                    PTX[2]=c2;
                }
                if(PTX[2]<0)  continue;

                for( int c3=0; c3<6; c3++ ){
                    if( !__01Sub(Q,U456789,0,c3,6,px,3-cA) ) continue;
                    PTX[3]=c3;
                }
                if(PTX[3]<0) continue;            

                //block2/row23               
                int cc=0, cw;
                for( int c4=0; c4<6; c4++ ){
                    cc=0;
                    if( !__02Sub(Q,U456789,1,c4,3,px,PTX[1],3-cA,true,ref cc) ) continue;
                    PTX[4]=c4;
                    break;
                }
                if(PTX[4]<0) continue;
                for( int c5=0; c5<6; c5++ ){
                    cw=0;
                    if( !__02Sub(Q,U456789,2,c5,3,px,PTX[1],3-cA,false,ref cw) ) continue;
                    PTX[5]=c5;
                    break;
                }
                if(PTX[5]<0) continue; 
                
                //block3/row23
                for( int c6=0; c6<6; c6++ ){
                    cw=cc;
                    if( !__02Sub(Q,U456789,1,c6,6,px,PTX[1],cA,true,ref cw) ) continue;
                    PTX[6]=c6;
                    break;
                }
                if(PTX[6]<0) continue; 
                for( int c7=0; c7<6; c7++ ){
                    cw=cc;
                    if( !__02Sub(Q,U456789,2,c7,6,px,PTX[1],cA,false,ref cw) ) continue;
                    PTX[7]=c7;
                    break;
                }
                if(PTX[7]>=0) break;
            }
            return;
        }
        private bool __01Sub( int[,] Q, int[,] UX, int r, int c2, int c3,  int px, int cX ){
            for( int c=0; c<3; c++ ) if(Q[r,permList[c2,c]+c3]!=U456789[px,c+cX]) return false;
            return true;
        }
        private bool __02Sub( int[,] Q, int[,] UX, int r, int c2, int c3, int px, int PTop1, int cX, bool LowB, ref int cc ){
            int ca=cc; cc=0;
            for( int c=0; c<3; c++ ){
                ca%=3;
                int nn=UX[px,c+cX];
                if( (LowB && nn<7) || (!LowB && nn>6) ) nn=p123[PTop1,ca];
                if(Q[r,permList[c2,c]+c3]!=nn) return false;
                if( UX[px,c+cX]<7 )  cc++;
                ca++;
            }
            return true;
        }

        public void  _LatinSqureSub_11R( int[,] Q, int[] PTX ){
            int px, rA;
            for( int s=0; s<56; s++ ){
                int s2=s;
                if(s==0) PTX[0]=PTX[1]=0;
                else if(s<28){ s2+=2; PTX[0]=s2/3; PTX[1]=s2%3; }
                else if(s==28){       PTX[0]=10;   PTX[1]=0; }
                else{ s2+=4;          PTX[0]=s2/3; PTX[1]=s2%3; }

                px=PTX[0]%10;
                rA=(PTX[0]<10)? 0: 3;

                //column1
                for( int r2=0; r2<6; r2++ ){
                    if( !__11Sub(Q,U258369,0,r2,3,px,rA) ) continue;
                    PTX[2]=r2;
                }
                if(PTX[2]<0)  continue;

                for( int r3=0; r3<6; r3++ ){
                    if( !__11Sub(Q,U258369,0,r3,6,px,3-rA) ) continue;
                    PTX[3]=r3;
                }
                if(PTX[3]<0) continue;   

                //block4/column2               
                int rr=0, rw=0;
                for( int r4=0; r4<6; r4++ ){
                    rr=0;
                    if( !__12Sub(Q,U258369,1,r4,3,px,PTX[1],3-rA,true,ref rr) ) continue;
                    PTX[4]=r4;
                    break;
                }
                if(PTX[4]<0) continue;
                int rs=rr;

                //block7/column2
                for( int r6=0; r6<6; r6++ ){
                    rw=rr;
                    if( !__12Sub(Q,U258369,1,r6,6,px,PTX[1],rA,true,ref rw) ) continue;
                    PTX[6]=r6;
                    break;
                }
                if(PTX[6]<0) continue; 

                //block4/column3
                for( int r5=0; r5<6; r5++ ){
                    rw=rs;
                    if( !__12Sub(Q,U258369,2,r5,3,px,PTX[1],3-rA,false,ref rw) ) continue;
                    PTX[5]=r5;
                    break;
                }
                if(PTX[5]<0) continue;
                rs=rw;

                //block7/column3
                for( int r7=0; r7<6; r7++ ){
                    rw=rs;
                    if( !__12Sub(Q,U258369,2,r7,6,px,PTX[1],rA,false,ref rw) ) continue;
                    PTX[7]=r7;
                    break;
                }
                if(PTX[7]>=0) break;

            }
            return;
        }
        private bool __11Sub( int[,] Q, int[,] UX, int c, int r2, int r3,  int px, int rX ){
            for( int r=0; r<3; r++ ) if(Q[permList[r2,r]+r3,c]!=U258369[px,r+rX]) return false;
            return true;
        }
        private bool __12Sub( int[,] Q, int[,] UX, int c, int r2, int r3, int px, int PLft1, int cX, bool LowB, ref int rr ){
            int nn, nw;

            for( int r=0; r<3; r++ ){
                rr%=3;
                nn=nw=UX[px,r+cX];
                bool B= (LowB && (nn%3)==2) || (!LowB && (nn%3)==0);
                if(B) nn=p147[PLft1,rr];
                if(Q[permList[r2,r]+r3,c]!=nn) return false;
                if(B)  rr++;
            }
            return true;
        }
  
        private int   RX;
        private int[] URow;
        private int[] UCol;
        private Permutation[] prmLst=new Permutation[9];
        private int[,] LS=new int[9,9];

        public int GetLatSqrID( int[,] Q ){          
            for( int r=0; r<9; r++ ){
                for( int c=0; c<9; c++ )  LS[r,c]=Q[r,c];
            }
            for( int r=3; r<9; r++ ){
                for( int c=3; c<9; c++ ) LS[r,c]=0;
            }

            int ID=0; RX=2;
            while( GenerateLatinSqure() ){
                ID++;
                for( int r=3; r<9; r++ ){
                    for( int c=3; c<9; c++ ) if( LS[r,c]!=Q[r,c] ) goto LUnmatch;
                }
                break;
            LUnmatch:
                continue;
            }
            //__DBUGprint2(LS,"ID:"+ID.ToString()+"  ");
            return ID;
        }
        public bool GenerateLatinSqure( ){
            if( RX<3 ){
                URow=new int[9]; UCol=new int[9];
                for( int r=0; r<3; r++ ){
                    for( int c=3; c<9; c++ ){
                        UCol[c] |= (1<<LS[r,c]);
                        URow[c] |= (1<<LS[c,r]); //(c,r reverse use in URow)
                    }
                }
                RX=3; prmLst[RX] = null;
            }
            do{
              LNxtLevel:
                Permutation prm=prmLst[RX];
                if( prm==null ) prmLst[RX]=prm=new Permutation(9,6);
                
                int[] UCo2 = new int[9];
                int[] UBlk = new int[9];
                for( int c=3; c<9; c++ ) UCo2[c]=UCol[c];
                for( int r=3; r<RX; r++ ){
                    for( int c=3; c<9; c++ ){
                        int no=LS[r,c];
                        UCo2[c] |= (1<<no);
                        UBlk[r/3*3+c/3] |= (1<<no);
                    }
                }
                int nxtX=9;
                while( prm.Successor(nxtX) ){
                    for( int cx=3; cx<9; cx++ ){
                        nxtX=cx-3;
                        int no=prm.Pnum[nxtX]+1;
                        int noB = 1<<(no);
                        if( (UCo2[cx]&noB)>0 ) goto LNxtPrm;
                        if( (URow[RX]&noB)>0 ) goto LNxtPrm;
                        if( (UBlk[RX/3*3+cx/3]&noB)>0 ) goto LNxtPrm;
                        LS[RX,cx] = no;
                    }
                    if( RX==8 ){
                        //__DBUGprint(LS);
                        return true;//<>
                    }
                    prmLst[++RX]=null;
                    goto LNxtLevel;

                  LNxtPrm:
                    continue;
                }
            }while((--RX)>=3);

            return false;
        }   

        private void __DBUGprint2( int[,] pSol99, string st="" ){
            string po;
            WriteLine();
            for( int r=0; r<9; r++ ){
                po = st+r.ToString("##0:");
                for( int c=0; c<9; c++ ){
                    int wk = pSol99[r,c];
                    if( wk==0 ) po += " .";
                    else po += wk.ToString(" #");
                }
                WriteLine(po);
            }
       }
    }

}