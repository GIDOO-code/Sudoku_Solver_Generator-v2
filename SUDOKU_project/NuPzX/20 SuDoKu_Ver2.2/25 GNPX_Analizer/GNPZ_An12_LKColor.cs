using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GIDOO_space;

namespace GNPZ_sdk{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{

        //Coloring is an algorithm that connects the focused digit with a strong link.
        //http://csdenpe.web.fc2.com/page46.html
        public bool Color_Trap( ){
			Prepare();
			CeLKMan.PrepareCellLink(1);                             //Generate StrongLink

            for(int no=0; no<9; no++ ){
                int noB=(1<<no);
                foreach( Bit81[] CRL in _Coloring(no) ){
                    Bit81 HitB=new Bit81();
                    Bit81 ELM = (new Bit81(pBDL,noB))-(CRL[0]|CRL[1]);
                    foreach( var rc in ELM.IEGet_rc() ){
                        Bit81 HB = HouseCells[18+rc.ToBlock()];
                        if( ((ConnectedCells[rc]-HB)&CRL[0]).IsZero() ) continue;
                        if( ((ConnectedCells[rc]-HB)&CRL[1]).IsZero() ) continue;
                        HitB.BPSet(rc);
                    }
                    if(!HitB.IsZero()){
                        Color Cr =_ColorsLst[0];
                        Color Cr1=Cr; Cr1.A=(byte)120;              //Color.FromArgb(120,Cr.R,Cr.G,Cr.B);     
                 
                        foreach( var P in HitB.IEGet_rc().Select(p=>pBDL[p]) )   P.CancelB=noB; 
                        foreach( var P in CRL[0].IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,Cr);
                        foreach( var P in CRL[1].IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,Cr1);
                        
                        SolCode = 2;
                        string SolMsg="Coloring Trap #"+(no+1);
                        Result=SolMsg;
                        if(SolInfoB)  ResultLong=SolMsg;
                        if(__SimpleAnalizerB__)  return true;
                        if(!pAnMan.SnapSaveGP(true))  return true;
                        HitB=new Bit81();
                    }
                }
            }

            return false;
        }
        public bool Color_Wrap( ){
			Prepare();
			CeLKMan.PrepareCellLink(1);    //Generate StrongLink

            for(int no=0; no<9; no++ ){
                int noB=(1<<no);
                foreach(Bit81[] CRL in _Coloring(no)){
                    Bit81 BD0 = new Bit81(pBDL,noB);
                    Bit81 ELM = BD0-(CRL[0]|CRL[1]);
                    if(ELM.Count==0) continue;

                    for(int k=0; k<2; k++ ){
                        for(int dr=0; dr<27; dr++ ){
                            if( (CRL[k]&HouseCells[dr]).Count<2 ) continue;
                            Color CrA = _ColorsLst[1];
                            Color Cr  = _ColorsLst[0];
                            Color Cr1 = Cr;     
                            Color Cr2 = Color.FromArgb(120,Cr.R,Cr.G,Cr.B);
                            foreach( var P in ELM.IEGet_rc().Select(p=>pBDL[p]) )      P.SetNoBBgColor(noB,AttCr,CrA);
                            foreach( var P in CRL[1-k].IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,Cr1);
                            foreach( var P in CRL[k].IEGet_rc().Select(p=>pBDL[p]) ){  P.SetCellBgColor(Cr2); P.CancelB=noB; }
                            
                            SolCode = 2;
                            string SolMsg="Coloring Wrap #"+(no+1);
                            Result=SolMsg;
                            if(SolInfoB)  ResultLong=SolMsg;
                            if(__SimpleAnalizerB__)  return true;
                            if(!pAnMan.SnapSaveGP(true))  return true;
                        }
                    }
                }
            }
            return false;
        }

        private IEnumerable<Bit81[]> _Coloring( int no ){
            Bit81[] CRL=new Bit81[2];
            CRL[0]=new Bit81(); CRL[1]=new Bit81(); 
            Bit81 TBD = new Bit81(pBDL,(1<<no));
            int  rc1=TBD.FindFirstrc();
            while(rc1>=0){
                Queue<int> rcQue=new Queue<int>();
                rcQue.Enqueue(rc1<<1);
                CRL[0].BPSet(rc1);
                TBD.BPReset(rc1);
                while(rcQue.Count>0){
                    rc1 = rcQue.Dequeue();
                    int kx=1-(rc1&1); 
                    rc1 >>= 1;
                    TBD.BPReset(rc1);
                    foreach( var P in CeLKMan.IEGetRcNoType(rc1,no,1) ){
                        int rc2=P.rc2;
                        if(!(CRL[0]|CRL[1]).IsHit(rc2) && TBD.IsHit(rc2)){
                            CRL[kx].BPSet(rc2); rcQue.Enqueue((rc2<<1)|kx);
                        }
                    }
                }
                yield return CRL;

                if( (rc1=TBD.FindFirstrc()) < 0 ) yield break;
                CRL=new Bit81[2];
                CRL[0]=new Bit81(); CRL[1]=new Bit81(); 
            }
            yield break;
        }

        //=================== MultiColoring ==================================
        public bool MultiColor_Type1( ){  
			Prepare();
			CeLKMan.PrepareCellLink(1);    //Generate StrongLink

            for(int no=0; no<9; no++ ){
                int noB=(1<<no);
                List<Bit81[]> MCRL = _Coloring(no).ToList();
                if( MCRL==null || MCRL.Count<2 ) continue;
                var cmb=new Combination(MCRL.Count,2);
                while( cmb.Successor() ){
                    Bit81[] CRLa = MCRL[cmb.Index[0]];
                    Bit81[] CRLb = MCRL[cmb.Index[1]];
                    for(int na=0; na<2; na++ ){
                        Bit81 HCRLa = new Bit81();
                        foreach(var rc in CRLa[na].IEGet_rc()) HCRLa |= ConnectedCells[rc];
                        for(int nb=0; nb<2; nb++ ){
                            if((HCRLa&CRLb[nb]).IsZero()) continue;

                            Bit81 BD0=new Bit81(pBDL,noB);
                            Bit81 ELMtry = BD0-(CRLa[na]|CRLb[nb]|CRLa[1-na]|CRLb[1-nb]);
                            if(ELMtry.Count==0) continue;
                            
                            bool solF=false;
                            Bit81 ELM=new Bit81();
                            foreach( var rc in ELMtry.IEGet_rc() ){
                                if( !ConnectedCells[rc].IsHit(CRLa[1-na]) ) continue;
                                if( !ConnectedCells[rc].IsHit(CRLb[1-nb]) ) continue;
                                pBDL[rc].CancelB=noB; ELM.BPSet(rc); solF=true;
                            }
                            if(solF){
                                Color CrA = _ColorsLst[0];
                                foreach( var P in ELM.IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,CrA);
                                for(int k=0; k<2; k++){
                                    Bit81[] CRLX = MCRL[cmb.Index[k]];
                                    Color Cr1=_ColorsLst[k];
                                    Color Cr2=Cr1; Cr2.A=(byte)120;          
                                    foreach( var P in CRLX[1-k].IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,Cr1); 
                                    foreach( var P in CRLX[k].IEGet_rc().Select(p=>pBDL[p]) )   P.SetNoBBgColor(noB,AttCr,Cr2);
                                }

                                SolCode = 2;
                                string SolMsg="MultiColoring Type1 #"+(no+1);
                                Result=SolMsg;
                                if(SolInfoB)  ResultLong=SolMsg;
                                if(__SimpleAnalizerB__)  return true;
                                if(!pAnMan.SnapSaveGP(true))  return true;
                                solF=false;
                            }
                        }
                    }
                }
            }
            return false;    
        }

        public bool MultiColor_Type2( ){  
			Prepare();
			CeLKMan.PrepareCellLink(1);    //Generate StrongLink
            for(int no=0; no<9; no++ ){
                int noB=(1<<no);
                List<Bit81[]> MCRL = _Coloring(no).ToList();
                if(MCRL==null || MCRL.Count<2)  continue;

                var prm=new Permutation(MCRL.Count,2);
                while(prm.Successor()){
                    if( prm.Index[0]<prm.Index[1] )  continue;
                    Bit81[] CRLa = MCRL[prm.Index[0]];
                    Bit81[] CRLb = MCRL[prm.Index[1]];
                    if(CRLa[0].IsZero() || CRLa[1].IsZero()) continue;
                    if(CRLb[0].IsZero() || CRLb[1].IsZero()) continue;

                    Bit81 HCRL0=new Bit81(), HCRL1=new Bit81();
                    foreach( var rc in CRLa[0].IEGet_rc() ) HCRL0 |= ConnectedCells[rc];
                    foreach( var rc in CRLa[1].IEGet_rc() ) HCRL1 |= ConnectedCells[rc];

                    for(int nb=0; nb<2; nb++ ){
                        if((CRLb[nb]&HCRL0).IsZero()) continue;
                        if((CRLb[nb]&HCRL1).IsZero()) continue;

                        Color Cr1 = _ColorsLst[0];   
                        Color Cr2 = Color.FromArgb( 100, Cr1.R, Cr1.G, Cr1.B );
                        foreach( var P in CRLa[0].IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,Cr1);
                        foreach( var P in CRLa[1].IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,Cr2);

                        Cr1 = _ColorsLst[1];   
                        Cr2 = Color.FromArgb( 100, Cr1.R, Cr1.G, Cr1.B );
                        foreach( var P in CRLb[1-nb].IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(noB,AttCr,Cr2);
                        foreach( var P in CRLb[nb].IEGet_rc().Select(p=>pBDL[p]) ){ P.SetCellBgColor(Cr1); P.CancelB=noB; }

                        SolCode = 2;
                        string SolMsg="MultiColoring Type2 #"+(no+1);
                        Result=SolMsg;
                        if(SolInfoB)  ResultLong=SolMsg;
                        if(__SimpleAnalizerB__)  return true;
                        if(!pAnMan.SnapSaveGP(true))  return true;
                    }
                }
            }
            return false;    
        }    
    }
}
