using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool GNP00_XYChain(){
            CeLKMan.PrepareCellLink(1);    //strongLink生成

            List<int> LKRec=new List<int>();
            foreach( var CRL in _GetXYChain(LKRec) ){
                int rcS=CRL[0].ID;
                int no=CRL[1].ID, noB=(1<<no);

                Bit81 ELM = ConnectedCells[rcS] - (CRL[0]|CRL[1]);
                if( ELM.IsZero() ) continue;

                Bit81 ELM2=new Bit81();
                bool XYChainF=false;
                foreach( var E in ELM.IEGetUCeNoB(pBDL,noB) ){
                    if( CRL[0].IsHit(ConnectedCells[E.rc]) ){
                        E.CancelB=noB; XYChainF=true;
                        ELM2 |= CRL[0]&ConnectedCells[E.rc];
                    }
                }
                if( !XYChainF )  continue;

                //===== XY-Chain fond =====
                SolCode=2;                    
                Result = "XY Chain";
                if( SolInfoDsp )  ResultLong = Result;
                int rcE;
                foreach( var P in ELM2.IEGetUCeNoB(pBDL,noB) ) P.SetNoBBgColor(noB,AttCr,SolBkCr);
                Bit81 ELM2cpy=ELM2.Copy();
                while( (rcE=ELM2.FindFirstrc())>=0 ){
                    string stR="";
                    Bit81 XYchainB = SelectLink_XYChain(LKRec,rcS,rcE,noB,ref stR)-ELM2cpy; 
                    if( SolInfoDsp )  ResultLong += "\r "+stR;

                    foreach( var P in XYchainB.IEGetUCeNoB(pBDL,0x1FF) ){
                        P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr2);
                    }
                    ELM2.BPReset(rcE);
                }
                pBDL[rcS].SetNoBBgColor(noB,AttCr,SolBkCr);           
                    
                if( !SnapSaveGP(true) )  return true;
                XYChainF=false;
                //foreach( var E in pBDL ) E.CancelB=0;
            }
            return false;
        }
        private Bit81 SelectLink_XYChain( List<int> LinkRecord, int rcS, int rcE, int noB, ref string stRet ){
            //(直接関係する解チェインのみを抽出して表示する）
            Bit81 XYchainB=new Bit81();        
            int rcX=rcE;
            XYchainB.BPSet(rcX);
            List<int> Q=new List<int>();
            if( SolInfoDsp ) Q.Add(rcX);
            while(rcX!=rcS){
                rcX=LinkRecord.Find(p=>(p&0xFF)==rcX);
                if( rcX==0 ) break;
                rcX=(rcX>>8);
                XYchainB.BPSet(rcX);
                if( SolInfoDsp ) Q.Add(rcX);     
            }
            if( SolInfoDsp ){
                Q.Reverse();
                string st="";
                Q.ForEach(p=> st += "-["+(p/9*10+p%9+11)+"]");
                stRet=">"+st.Substring(1);
            }
            return XYchainB;
        }
                
        private IEnumerable<Bit81[]> _GetXYChain( List<int> LKRec ){
            List<UCell> TBDbv = pBDL.FindAll(p=>(p.FreeBC==2));  //BV:bivalue
            foreach( var PS in TBDbv ){
                int rcS=PS.rc;
                foreach( var no in PS.FreeB.IEGet_BtoNo() ){
                    int noB=(1<<no);  
                    Bit81[] CRL=new Bit81[2];
                    CRL[0]=new Bit81(); //連結する着目数字の位置
                    CRL[1]=new Bit81(); //連結するその他数字の位置
                    CRL[0].ID=rcS; CRL[1].ID=no;

                    Bit81 CnctdCs = ConnectedCells[rcS]; //開始セルの関連セル群(行列ブロック関連）
                    Queue<int> rcQue=new Queue<int>();
                    int no0 = pBDL[rcS].FreeB.BitReset(no).BitToNum();//開始セルのもう一方の数字
                    rcQue.Enqueue( (no0<<8)|rcS );

                    LKRec.Clear();
                    while(rcQue.Count>0){
                        int rcX=rcQue.Dequeue();
                        int no1=rcX>>8; 
                        int rc1=rcX&0xFF;
                        foreach( var LK in CeLKMan.IEGetRcNoType(rc1,no1,1) ){ //強リンクで連結
                            int rc2= LK.rc2; 
                            if( pBDL[rc2].FreeBC!=2 ) continue;     //2数字セルのみ
                            if( CRL[0].IsHit(rc2) || CRL[1].IsHit(rc2) ) continue;//ループ除外
                    
                            //開始セルと関連し同じ数字を持つセルは除外
                            if( CnctdCs.IsHit(rc2) && (pBDL[rc2].FreeB&noB)>0 ) continue; 

                            int no2 = (pBDL[rc2].FreeB.BitReset(no1)).BitToNum();//2数字のもう一方
                            int nx=(no2==no)? 0: 1;
                            CRL[nx].BPSet(rc2); 
                            rcQue.Enqueue( (no2<<8)|rc2 ); //次ノードをQueueに入れる
                            LKRec.Add((rc1<<8|rc2));　//リンクを記録
                        }
                    }
                    if( CRL[0].Count>0 || CRL[1].Count>0 ) yield return CRL;
                }
            }
            yield break;
        }
    }
}