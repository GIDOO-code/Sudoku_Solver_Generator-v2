using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{     
        public bool  GNP00_EmptyRectangle( ){
            CeLKMan.PrepareCellLink(1);    //strongLink生成
            
            UCell PSel1=null, PSel2=null, PElm=null;
            for( int no=0; no<9; no++ ){
                int noB = 1<<no;
                for( int bx=0; bx<9; bx++ ){
                    int erB = GetTupleList(pBDL,2,bx,noB).Aggregate(0,(Q,P)=>Q|(1<<P.nx));

                    for( int er=0; er<9; er++ ){
                        int Lr=er/3, Lc=er%3;   //ブロックローカル行・列
                        int rxF = 7<<(Lr*3);    //7=1+2+4
                        int cxF = 73<<Lc;       //73=1+8+64
          
                        if( (erB&rxF)==0 || erB.DifSet(rxF)==0 )  continue; //Lr行
                        if( (erB&cxF)==0 || erB.DifSet(cxF)==0 )  continue; //Lc列          
                        if( erB.DifSet(rxF|cxF)>0 )               continue; //Lr行+Lc列                      
                        
                        int r1 = bx/3*3+Lr;     //絶対行に換算
                        int c1 = (bx%3)*3+Lc;   //絶対列に換算
                        //(EmptyRectangle候補がある) block:bx - [r1,c1]

                        for( int r2=0; r2<9; r2++ ){        //第２の行
                            if( r2/3==r1/3 ) continue;      //着目ブロックの行は除外
                            int rc21=r2*9+c1;
                            if( (pBDL[rc21].FreeB&noB)==0 ) continue;

                            for( int c2=0; c2<9; c2++ ){    //第２の列
                                if( c2/3==c1/3 )  continue; //着目ブロックの列は除外
                                int rc12=r1*9+c2;
                                if( (pBDL[rc12].FreeB&noB)==0 ) continue;

                                int rc22=r2*9+c2;
                                if( (pBDL[rc12].FreeB&noB)>0 ){ //除外できる数字がある
                                    var LK = new UCellLink(0,1,no,rc21,rc22);
                                    if( CeLKMan.ContainsLink(LK) ){ //強いリンクがあるか
                                        PSel1=pBDL[rc21]; PSel2=pBDL[rc22]; PElm=pBDL[rc12]; 
                                        EmptyRectangle_SolResult( no, bx, PSel1, PSel2, PElm );
                                        if( !SnapSaveGP(true) )  return true;
                                    }
                                }

                                if( (pBDL[rc21].FreeB&noB)>0 ){ //除外できる数字がある
                                    var LK = new UCellLink(1,1,no,rc12,rc22);
                                    if( CeLKMan.ContainsLink(LK) ){ //強いリンクがある
                                        PSel1=pBDL[rc12]; PSel2=pBDL[rc22]; PElm=pBDL[rc21];
                                        EmptyRectangle_SolResult( no, bx, PSel1, PSel2, PElm );
                                        if( !SnapSaveGP(true) )  return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void EmptyRectangle_SolResult(  int no, int bx, UCell PSel1, UCell PSel2, UCell PElm ){
            int noB=(1<<no);
            SolCode = 2;
            Result = "EmptyRectangl #"+(no+1) +" in B"+(bx+1);
            PElm.CancelB = noB;    //キャンセル数字設定 
            if( !SolInfoDsp ) return;
              
            ResultLong = "EmptyRectangl";
            PSel1.SetNoBBgColor( noB, AttCr, SolBkCr2 );  //強いリンクをマーク
            PSel2.SetNoBBgColor( noB, AttCr, SolBkCr2 );  //強いリンクをマーク
               
            string st=""; 
            foreach( var Q in GetTupleList(pBDL,2,bx,noB) ){
                Q.SetNoBBgColor(noB,AttCr,SolBkCr);   //empty rectangle
                st += " "+Q.rc.ToRCString();
            }
            string msg="\r         digit: #"+(no+1) +"\r            ER: B"+(bx+1)+"("+st.ToString_SameHouseComp()+")";
            msg +="\r        S-Link: "+PSel2.rc.ToRCString()+"-"+PSel1.rc.ToRCString();
            msg +="\rEliminatedCell: "+PElm.rc.ToRCString();
            ResultLong = "EmptyRectangl"+msg;
        }

#if false
        public bool  GNP00_EmptyRectangle_old( ){     //Empty Rectangle  //▼▼▼スマートさに欠ける▼▼▼

            int[,,] rcPosNoB = new int[2,9,9];

            for(int tfx=0; tfx<18; tfx++){
                int tp = tfx/9;
                int fx = tfx%9;

                foreach( var P in GetTupleList(pBDL,tp,fx,0x1FF) ){
                    int FreeB = P.FreeB;
                    for( int no=0; no<9; no++){
                        if( (FreeB&(1<<no))==0 ) continue;
                        rcPosNoB[tp,fx,no] |= (1<<P.nx);    //[0,row位置,数字]=col位置(=nx)
                    }                                       //[1,col位置,数字]=row位置(=nx)
                }
            }
            for(int tfx=0; tfx<18; tfx++){
                for( int no=0; no<9; no++ ){
                    if( rcPosNoB[tfx/9,tfx%9,no].BitCount()==2 )  continue;
                    rcPosNoB[tfx/9,tfx%9,no] = 0;         //弱いリンクは除外する
                }
            }

            for( int no=0; no<9; no++ ){
                int noB = 1<<no;
                for( int bx=0; bx<9; bx++ ){
                    int ERbit = 0;
                    foreach( var P in GetTupleList(pBDL,2,bx,noB) ) ERbit |= (1<<P.nx);

                    bool ERSolFond=false;
                    for( int Lr=0; Lr<3; Lr++ ){//ブロックローカルのLr行
                        int rxF = 7<<(Lr*3);
                        if( (ERbit&rxF)==0 )  continue;             //Lr行にヒットあり?
                        if( (ERbit&(rxF^0x1FF))==0 )  continue;     //Lr行以外にもあり?

                        for( int Lc=0; Lc<3; Lc++ ){//ブロックローカルのLc列
                            int cxF = 73<<Lc;   //73=1+8+64
                            if( (ERbit&cxF)==0 )  continue;         //Lc列にヒットあり?
                            if( (ERbit&(cxF^0x1FF))==0 )  continue; //Lc列以外にもあり?

                            int rcxF = (rxF|cxF)^0x1FF;             
                            if( (ERbit&rcxF)>0 )  continue;         //Lr行とLc列で全て消えるか?

                            //--------------------------------------
                            int r1 = bx/3*3+Lr;                     //絶対行に換算
                            int c1 = (bx%3)*3+Lc;                   //絶対列に換算
                         //   Console.WriteLine( "ER-1 no:{0} bx:{1} row:{2} col:{3}",
                         //       (no+1), (bx+1), (r1+1), (c1+1) );

                            //===== step2 =====
                            int r1B = 1<<r1;
                            int chk;
                            for( int cx=0; cx<9; cx++ ){   //cx=col
                                if( cx/3==c1/3 )  continue;                 //セル[r1,cx]はbxブロックか?
                                if( pBDL[r1*9+cx].No!=0 )  continue;        //セル[r1,cx]は確定済みか?
                                if( (chk=rcPosNoB[1,cx,no])==0 )  continue; //cx列には強いリンクがあるか?
                                if( (chk&r1B)==0 )  continue;               //cx列の強いリンクはr1にあるか?
                                int r2 = (chk&(r1B^0x1FF)).BitToNum();      //r2=cx列の強いリンクの他端行
                                if( r2/3==r1/3 )  continue;                 //セル[r2,cx]は異なるブロックか?
                                int FreeB = pBDL[r2*9+c1].FreeB;            
                                if( (FreeB&noB)==0 )  continue;             //セル[r2,c1]は未確定か?

                                pBDL[r1*9+cx].AttNmB = noB;                 //強いリンクのセル[r1,cx]をマーク
                                pBDL[r2*9+cx].AttNmB = noB;                 //強いリンクのセル[r2,cx]をマーク
                                pBDL[r1*9+cx].CellBgCr = Colors.Lavender;
                                pBDL[r2*9+cx].CellBgCr = Colors.Lavender;
                                pBDL[r2*9+c1].CancelB = FreeB&noB;          //セル[r2,c1]の数字noにキャンセル設定
                              
                                foreach( var Q in GetTupleList(pBDL,2,bx,noB) ){
                                    Q.AttNmB=noB; Q.CellBgCr=SolBkCr;       //empty rectangle成立セルにマーキング
                                } 
                                ERSolFond = true;
                                goto solutionFound;
                            }

                            int colB = 1<<c1;
                            for( int rx=0; rx<9; rx++ ){   //rx=row
                                if( rx/3==r1/3 )  continue;                 //セル[rx,c1]はbxブロックか?
                                if( pBDL[rx*9+c1].No!=0 )  continue;        //セル[rx,c1]は確定済みか?
                                if( (chk=rcPosNoB[0,rx,no])==0 )  continue; //rx行には強いリンクがあるか?
                                if( (chk&colB)==0 )  continue;              //rx行の強いリンクはc1にあるか?
                                int c2 = (chk&(colB^0x1FF)).BitToNum();     //c2=rx行の強いリンクの他端行
                                if( c2/3==c1/3 )  continue;                 //セル[rx,c2]は異なるブロックか?
                                int FreeB = pBDL[r1*9+c2].FreeB;            
                                if( (FreeB&noB)==0 )  continue;             //セル[r1,c2]は未確定か?

                                pBDL[rx*9+c1].AttNmB = noB;                 //強いリンクのセル[rx,c1]をマーク
                                pBDL[rx*9+c2].AttNmB = noB;                 //強いリンクのセル[rx,c2]をマーク
                                pBDL[rx*9+c1].CellBgCr = Colors.Lavender;
                                pBDL[rx*9+c2].CellBgCr = Colors.Lavender;
                                pBDL[r1*9+c2].CancelB = FreeB&noB;          //セル[r1,c2]の数字noにキャンセル設定            

                                foreach( var Q in GetTupleList(pBDL,2,bx,noB) ){
                                    Q.AttNmB=noB; Q.CellBgCr=SolBkCr;       //empty rectangle成立セルにマーキング
                                } 
                                SolCode = 2;
                                ERSolFond = true;
                                goto solutionFound;
                            }
                        }
                    }
                }
            }
            return false;

        solutionFound:
            SolCode = 2;
 
            if( SolInfoDsp ) ResultLong = "EmptyRectangl";
            Result = "EmptyRectangl";
            return true;           
        }
#endif
    }
}
