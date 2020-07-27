using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;

using GIDOO_space;

namespace GNPXcore{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{

        //XY-Chain is an algorithm using Locked which occurs in the concatenation of bivalues.
        //http://csdenpe.web.fc2.com/page49.html
        public bool XYChain(){
			Prepare();
			CeLKMan.PrepareCellLink(1);    //Generate StrongLink

            List<int> LKRec=new List<int>();
            foreach( var CRL in _GetXYChain(LKRec) ){
                int rcS=CRL[0].ID, no=CRL[1].ID, noB=(1<<no);

                Bit81 ELM = ConnectedCells[rcS] - (CRL[0]|CRL[1]);
                if( ELM.IsZero() ) continue;

                Bit81 ELM2=new Bit81();
                bool XYChainF=false;
                foreach( var E in ELM.IEGetUCeNoB(pBDL,noB) ){
                    if( CRL[0].IsHit(ConnectedCells[E.rc]) ){
                        E.CancelB=noB; XYChainF=true;
                        ELM2 |= CRL[0]&ConnectedCells[E.rc];
                        break;
                    }
                }
                if(!XYChainF)  continue;

                //===== XY-Chain found =====
                SolCode=2;                    
                String SolMsg="XY Chain";
                Result=SolMsg;

                int rcE;
                foreach( var P in ELM2.IEGetUCeNoB(pBDL,noB) ) P.SetNoBBgColor(noB,AttCr,SolBkCr);
                Bit81 ELM2cpy = new Bit81(ELM2);
                while( (rcE=ELM2.FindFirstrc())>=0 ){
                    string stR="";
                    Bit81 XYchainB = _SelectLink_XYChain(LKRec,rcS,rcE,noB,ref stR)-ELM2cpy; 
                    if(SolInfoB)  SolMsg+="\r "+stR;

                    foreach( var P in XYchainB.IEGetUCeNoB(pBDL,0x1FF) ){
                        P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr2);
                    }
                    ELM2.BPReset(rcE);
                }
                if(SolInfoB)  ResultLong = SolMsg;
                pBDL[rcS].SetNoBBgColor(noB,AttCr,SolBkCr);           
                    
                if(__SimpleAnalizerB__)  return true;
                if(!pAnMan.SnapSaveGP(true))  return true;
                XYChainF=false;
            }
            return false;
        }
        private Bit81 _SelectLink_XYChain( List<int> LinkRecord, int rcS, int rcE, int noB, ref string stRet ){
            //(Extract and display directly related solution chain）
            Bit81 XYchainB=new Bit81();        
            int rcX=rcE;
            XYchainB.BPSet(rcX);
            List<int> Q=new List<int>();
            if(SolInfoB) Q.Add(rcX);
            while(rcX!=rcS){
                rcX=LinkRecord.Find(p=>(p&0xFF)==rcX);
                if(rcX==0) break;
                rcX=(rcX>>8);
                XYchainB.BPSet(rcX);
                if(SolInfoB) Q.Add(rcX);     
            }
            if(SolInfoB){
                Q.Reverse();
                string st="";
                Q.ForEach(p=> st += "-["+(p/9*10+p%9+11)+"]");
                stRet=">"+st.Substring(1);
            }
            return XYchainB;
        }
                
        private IEnumerable<Bit81[]> _GetXYChain( List<int> LKRec ){
            List<UCell> TBDbv = pBDL.FindAll(p=>(p.FreeBC==2));                     //Extract BV_Cell(BV:bivalue).
            foreach ( var PS in TBDbv ){                                            //Choose one BV_Cell(=>PS)
                int rcS=PS.rc;
                foreach( var no in PS.FreeB.IEGet_BtoNo() ){                        //Choose one digit(in PS)
                    int noB=(1<<no);  
                    Bit81[] CRL=new Bit81[2];
                    CRL[0]=new Bit81();                                             //Position of the target digit to be concatenated
                    CRL[1]=new Bit81();                                             //Position of other digits to be connected
                    CRL[0].ID=rcS; CRL[1].ID=no;

                    Bit81 CnctdCs = ConnectedCells[rcS];                            //Associated cells group of starting cell
                    Queue<int> rcQue=new Queue<int>();
                    int no0 = pBDL[rcS].FreeB.BitReset(no).BitToNum();              //The other digit of the starting cell
                    rcQue.Enqueue( (no0<<8)|rcS );

                    LKRec.Clear();
                    while(rcQue.Count>0){                                           //Extend the chain step by step
                        int rcX=rcQue.Dequeue();
                        int no1=rcX>>8, rc1=rcX&0xFF;
                        foreach( var LK in CeLKMan.IEGetRcNoType(rc1,no1,1) ){      //strongLink connected with rc1, #no
                            int rc2= LK.rc2; 
                            if(pBDL[rc2].FreeBC!=2) continue;                       //bivalue?
                            if(CRL[0].IsHit(rc2) || CRL[1].IsHit(rc2)) continue;    //Different from colored cells?

                            //Exclude cells associated with the starting cell and having the same digit
                            if(CnctdCs.IsHit(rc2) && (pBDL[rc2].FreeB&noB)>0) continue; 

                            int no2 = (pBDL[rc2].FreeB.BitReset(no1)).BitToNum();   //other digit
                            int nx=(no2==no)? 0: 1;
                            CRL[nx].BPSet(rc2); 
                            rcQue.Enqueue( (no2<<8)|rc2 );                          //Put the next [Digit&Cell] in Queue
                            LKRec.Add((rc1<<8|rc2));　                              //Record Link
                        }
                    }
                    if( CRL[0].Count>0 || CRL[1].Count>0 ) yield return CRL;
                }
            }
            yield break;
        }
    }
}