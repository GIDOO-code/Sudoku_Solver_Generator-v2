using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;

using GIDOO_space;

namespace GNPXcore{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{

        //X-Chain is an algorithm using Locked which occurs when concatenating strong and weak links.
        //http://csdenpe.web.fc2.com/page48.html
        public bool XChain(){
			Prepare();
			CeLKMan.PrepareCellLink(1);                              //Generate StrongLink

            for(int no=0; no<9; no++ ){
                int noB=(1<<no);   

                List<int> LKRec=new List<int>();
                foreach( var CRL in _GetXChain(no,LKRec) ){
                    int rcS=CRL[0].ID;                              //Bit81.ID is used for information exchange(irregular use)
                    
                    Bit81 ELM=(ConnectedCells[rcS]&CRL[1])-CRL[2];  //CRL[2]:exclude chain of only two links(trivial solution).
                    if( ELM.IsZero() ) continue;                    //ELM:eliminatable cells

                    //===== X-Chain found =====
                    SolCode = 2;   
                    foreach( var P in ELM.IEGetUCeNoB(pBDL,noB) ) P.CancelB=noB;
                    string SolMsg=$"X-Chain #{(no+1)}";
                    Result=SolMsg;
                    if(SolInfoB){  
                        Bit81 LKRecB=_SelectLink_XChain(LKRec,rcS,ELM,noB); //Extract related-Link            
                        CRL[0]&=LKRecB; CRL[1]&=LKRecB;

                        Color Cr  = _ColorsLst[0];
                        Color Cr1 = Color.FromArgb(255,Cr.R,Cr.G,Cr.B); 
                        Color Cr2 = Color.FromArgb(120,Cr.R,Cr.G,Cr.B);    //(Lightness adjustment)
                        foreach( var P in CRL[0].IEGetUCeNoB(pBDL,noB) ) P.SetNoBBgColor(noB,AttCr,Cr2);
                        foreach( var P in CRL[1].IEGetUCeNoB(pBDL,noB) ) P.SetNoBBgColor(noB,AttCr,Cr1);
                        pBDL[rcS].SetNoBBgColor(noB,AttCr,SolBkCr);
                        ResultLong=SolMsg;;
                    }
                    if(__SimpleAnalizerB__)  return true;
                    if(!pAnMan.SnapSaveGP(true))  return true;
                }
            }
            return false;
        }
        private Bit81 _SelectLink_XChain( List<int> LKRec, int rcS, Bit81 ELM, int noB ){
            Bit81 LKRecB=new Bit81();
            foreach( var P in ELM.IEGetUCeNoB(pBDL,noB) ){
                int rcX=P.rc;
                LKRecB.BPSet(rcX);
                while(rcX!=rcS){
                    rcX=LKRec.Find(p=>(p&0xFF)==rcX);
                    if(rcX==0) break;
                    rcX=(rcX>>8);
                    LKRecB.BPSet(rcX);
                }
            }
            return LKRecB;
        }
               
        private IEnumerable<Bit81[]> _GetXChain( int no, List<int> LKRec ){
            Bit81 TBD = new Bit81(pBDL,(1<<no));

            int rcS;
            while( (rcS=TBD.FindFirstrc())>=0 ){                    //rcS:Set the origin cell.
                TBD.BPReset(rcS);                                   //Reset TBD to Processed.

                //===== Repeatedly coloring processing. initialize =====
                Bit81[] CRL=new Bit81[3];                           //Coloring 2 groups(CRL[0] and CRL[1]).
                CRL[0]=new Bit81(); CRL[1]=new Bit81(rcS); CRL[2]=new Bit81();
                CRL[0].ID=rcS;
                Queue<int> rcQue=new Queue<int>();
                rcQue.Enqueue( (rcS<<1)|1 );                        //(First StrongLink) 

                //===== Repeatedly coloring processing. start =====
                LKRec.Clear();                                      //clear chain recorder.
                bool firstLK=true;
                while(rcQue.Count>0){
                    int rcX = rcQue.Dequeue();                      //recorded [cell and color]
                    int swF = 1-(rcX&1);                            //next color(inversion S-W)
                    int rc1 = (rcX>>1);                             //next cell

                    foreach( var LKx in CeLKMan.IEGetRcNoType(rc1,no,(swF+1)) ){//LKx:link connected to cell rc1
                        int rc2=LKx.rc2;    //anather cell of LKx
                        if( (CRL[0]|CRL[1]).IsHit(rc2) ) continue;  //already colored
                        CRL[swF].BPSet(rc2);                        //coloring
                        rcQue.Enqueue( (rc2<<1)|swF );              //enqueue(next cell and color)
                        LKRec.Add( rc1<<8|rc2 );                    //chain record
                        if(firstLK) CRL[2].BPSet(rc2);              //record colored cells from rcS(source cell)
                    }
                    firstLK=false;
                }
                if(CRL[1].Count>0) yield return CRL;
                //----- Repeatedly coloring processing. end ----- 
            }
            yield break;
        }
    }
}