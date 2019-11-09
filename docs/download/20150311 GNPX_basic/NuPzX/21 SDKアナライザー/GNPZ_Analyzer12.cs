using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool GNP00_SueDeCoq( ){
            ALSLinkMan  fALS=new ALSLinkMan(this);  //(houseに属するセル群を扱うクラスとしてALSを利用)
            if( fALS.ALS_Search(2)<=3 ) return false;   //+1と+2のfakeALSを生成

            foreach( var ISPB in fALS.ALSLst.Where(p=> p.tfx>=18 && p.Size>=3) ){//ブロックfALS選択
                if( ISPB.rcbRow.BitCount()<=1 || ISPB.rcbCol.BitCount()<=1 ) continue;　//ブロック枡は複数行・列

                //▼行(列)fALS選択
                foreach( var ISPR in fALS.ALSLst.Where(p=> p.tfx<18 && p.Size>=3) ){　//行fALS選択
                    if( (ISPR.rcbBlk&ISPB.rcbBlk)==0 ) continue; //ブロックb0と交差あり
                    if( ISPR.rcbBlk.BitCount()<2 )     continue; //行(列)fALSは複数ブロック

                    //交差部のセル構成は同じか
                    if( (ISPB.B81&HouseCells[ISPR.tfx]) != (ISPR.B81&HouseCells[ISPB.tfx]) ) continue;

                    Bit81 IS = ISPB.B81&ISPR.B81;                //◆交差部(Bit81表現)
                    if( IS.Count<2 ) continue; 　                //交差部は2セル以上
                    if( (ISPR.B81-IS).Count==0 ) continue;       //行(列)ALSに交差部以外の部分がある                    

                    Bit81 PB = ISPB.B81-IS;                      //(ISPBのIS外)
                    Bit81 PR = ISPR.B81-IS;                      //(ISPRのIS外)
                    int IS_FreeB = IS.AggregateFreeB(pBDL);      //(交差部数字)
                    int PB_FreeB = PB.AggregateFreeB(pBDL);      //(ISPBのIS外の数字)
                    int PR_FreeB = PR.AggregateFreeB(pBDL);      //(ISPRのIS外の数字)
                    if( (IS_FreeB&PB_FreeB&PR_FreeB)>0 ) continue;

                    //A.DifSet(B)=A-B=A&(B^0x1FF)
                    int PB_FreeBn = PB_FreeB.DifSet(IS_FreeB);   //ブロックの交差部に無い数字
                    int PR_FreeBn = PR_FreeB.DifSet(IS_FreeB);   //行(列)の交差部に無い数字

                    int sdqNC = PB_FreeBn.BitCount()+PR_FreeBn.BitCount();  //交差部外確定の数字数
                    if( (IS_FreeB.BitCount()-IS.Count) != (PB.Count+PR.Count-sdqNC) ) continue;

                    int elmB = PB_FreeB | IS_FreeB.DifSet(PR_FreeB); //ブロックの除外数字 
                    int elmR = PR_FreeB | IS_FreeB.DifSet(PB_FreeB); //行(列)の除外数字                
                    if( elmB==0 && elmR==0 ) continue;

                    bool ElmF=false;
                    foreach( var P in ISPB.GetRestCells(elmB) ){ P.CancelB|=P.FreeB&elmB; ElmF=true; }
                    foreach( var P in ISPR.GetRestCells(elmR) ){ P.CancelB|=P.FreeB&elmR; ElmF=true; }

                    if( !ElmF ) continue;
                   
                    //--- SueDeCoq fond ----------------------------------------------
                    SolCode=2;
                    SuDoQueEx_SolResult( ISPB, ISPR );
                    if( ISPB.Level>=3 || ISPB.Level>=3 ) Console.WriteLine("Level-3");
                    if( !SnapSaveGP(true) )  return true;
                    //foreach( var E in pBDL ) E.CancelB=0;
                }
            }
            return false;
        }

        private void SuDoQueEx_SolResult( UALS ISPB, UALS ISPR ){
            Result = ResultLong = "SueDeCoq";

            if( SolInfoDsp ){
                ISPB.UCellLst.ForEach(P=> P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr) );
                ISPR.UCellLst.ForEach(P=> P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr) );

                string ptmp = "";
                ISPB.UCellLst.ForEach(p=>{ ptmp+=" r"+(p.r+1)+"c" + (p.c+1); } );

                string po = "\r Cells";
                if( ISPB.Level==1 ) po += "(block)  ";
                else{ po += "-"+ISPB.Level+"(block)"; }
                po += ": "+ISPB.ToStringRCN();

                po += "\r Cells" + ((ISPR.Level==1)? "": "-2");
                po += ((ISPR.tfx<9)? "(row)":"(col)");
                po += ((ISPR.Level==1)? "    ": "  ");
                po += ": "+ISPR.ToStringRCN();

                ResultLong += po;
            }
        }
    }
}