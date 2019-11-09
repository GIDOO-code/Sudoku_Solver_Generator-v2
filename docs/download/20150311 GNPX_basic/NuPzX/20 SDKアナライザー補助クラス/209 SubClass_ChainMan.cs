using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GIDOO_space;

namespace GNPZ_sdk {
    public partial class GNPZ_Analyzer{
        private const int     S=1, W=2;

        public class ChainMan{
            public GNPZ_Analyzer     pSA;
            public List<UCellLink>   CNLst;
            public List<UCellS>      BDLS;
            public Queue<UCellLink>  QueLK;
            private int              NiceLoopMax;
            private int              OrgRC;
            private bool debugOn=true;

            public ChainMan( GNPZ_Analyzer pSA, List<UCell> xBDL, int NiceLoopMax ){
                this.pSA=pSA;
                this.NiceLoopMax=NiceLoopMax;
                BDLS=new List<UCellS>();
                xBDL.ForEach(P=>{ if(P.FreeB>0) BDLS.Add(new UCellS(P)); } );
                QueLK=new Queue<UCellLink>();
                CNLst=new List<UCellLink>();
            }
            
            public void Reset( bool LoopFlg ){
                BDLS.ForEach(P=>{
                    P.TrueB=P.FalseB=0;
                    Array.ForEach(P.SeqNo,p=>{p=999;});
                });
                if(LoopFlg)  pSA.CeLKMan.ResetLoopFlag();
            }

            public void SetChainNode( UCell P, int no, bool TF=true, bool GenLink=false ){
                int noB=1<<no;
                OrgRC=P.rc;
                UCellS Q0=BDLS.Find(p=>(p.rc==OrgRC));
                Q0.SeqNo[no]=0;
                
                int SW = TF? W: S;
                if(TF){ Q0.TrueB  |= noB; }
                else{   Q0.FalseB |= noB; }

                if(debugOn) Console.WriteLine( "Cell0:"+Q0.ToString() );

                if(GenLink){
                    foreach( var LK in pSA.CeLKMan.IEGetRcNoType(OrgRC,no,SW) ){
                        UCellS Q2=BDLS.Find(p=>(p.rc==LK.rc2));
                        Q2.SeqNo[no]=1;
                                if(debugOn) Console.WriteLine( "    "+LK.ToString() );
                        QueLK.Enqueue(LK); CNLst.Add(LK);
                    }
                }
            }
#if false
            public bool SetChainLinkOld( UCellLink preLK, Stack<UCellLink> SolStack ){
                bool Loop=false;
                int  no=preLK.no;
                int  noB=1<<no;
                int  rc0=preLK.UCe1.rc;
                int  rc1=preLK.UCe2.rc;
                UCellS Q1=BDLS.Find(p=>(p.rc==rc1)); //軸セル(前リンクの終端）
                if(preLK.type==S){ //StrongLink
                    Q1.TrueB |= noB;
                    {// S -> W
                        foreach( var LK in pSA.CeLKMan.IEGetRcNoType(rc1,no,W).Where(P=>P.rc2!=rc0) ){
                            UCellS Q2=BDLS.Find(p=>(p.rc==LK.rc2));
                            if( Q2.SeqNo>=1 || (Q2.UsedB&noB)>0 || Q2.SeqNo>=NiceLoopMax-1 )  continue;
                            if( Q2.SeqNo==0 ){
                                if( Q1.SeqNo<=1 ) continue;
                                Loop=true; LK.LoopFlag=true;
                            }
                            if( Q2.SeqNo<0 )  Q2.SeqNo=Q1.SeqNo+1;
                            QueLK.Enqueue(LK); CNLst.Add(LK);
                                if(debugOn) Console.WriteLine( "S->W "+LK.ToString()+" "+Q2.ToString() );
                        }
                    }

                    {// S -> S
                        int FreeBX=Q1.FreeB.DifSet(noB);
                        foreach( int no2 in FreeBX.IEGet_BtoNo() ){
                            int no2B=1<<no2;
                            Q1.FalseB |= no2B;
                            foreach( var LK in pSA.CeLKMan.IEGetRcNoType(rc1,no2,S).Where(P=>P.rc2!=rc0) ){
                                UCellS Q2=BDLS.Find(p=>(p.rc==LK.rc2));
                                if( Q2.SeqNo>=1 || (Q2.UsedB&noB)>0 || Q2.SeqNo>=NiceLoopMax-1 )  continue;
                                if( Q2.SeqNo==0 ){
                                    if( Q1.SeqNo<=1 ) continue;
                                    Loop=true; LK.LoopFlag=true;
                                }
                                if( Q2.SeqNo<0 )  Q2.SeqNo=Q1.SeqNo+1;
                                QueLK.Enqueue(LK); CNLst.Add(LK);
                                    if(debugOn) Console.WriteLine( "S->S "+LK.ToString()+" "+Q2.ToString() );
                            }
                        }
                    }
                }
                else if(preLK.type==W){ //WeakLink
                    {// W -> S
                        foreach( var LK in pSA.CeLKMan.IEGetRcNoType(rc1,no,S).Where(P=>P.rc2!=rc0) ){
                            UCellS Q2=BDLS.Find(p=>(p.rc==LK.rc2));
                            if( Q2.SeqNo>=1 || (Q2.UsedB&noB)>0 || Q2.SeqNo>=NiceLoopMax-1 )  continue;
                            if( Q2.SeqNo==0 ){
                                if( Q1.SeqNo<=1 ) continue;
                                Loop=true; LK.LoopFlag=true;
                            }
                            if( Q2.SeqNo<0 )  Q2.SeqNo=Q1.SeqNo+1;
                            QueLK.Enqueue(LK); CNLst.Add(LK);
                                if(debugOn) Console.WriteLine( "W->S "+LK.ToString()+" "+Q2.ToString() );
                        }
                    }

                    {// W -> W
                        if( Q1.FreeB.BitCount()==2 ){
                            int no2=Q1.FreeB.DifSet(noB).BitToNum();
                            int no2B=1<<no2;
                            Q1.TrueB |= no2B;
                            foreach( var LK in pSA.CeLKMan.IEGetRcNoType(rc1,no2,W).Where(P=>P.rc2!=rc0) ){
                                UCellS Q2=BDLS.Find(p=>(p.rc==LK.rc2));
                                if( Q2.SeqNo>=1 || (Q2.UsedB&noB)>0 || Q2.SeqNo>=NiceLoopMax-1 )  continue;
                                if( Q2.SeqNo==0 ){
                                    if( Q1.SeqNo<=1 ) continue;
                                    Loop=true; LK.LoopFlag=true;
                                }
                                if( Q2.SeqNo<0 )  Q2.SeqNo=Q1.SeqNo+1;
                                QueLK.Enqueue(LK); CNLst.Add(LK);
                                    if(debugOn) Console.WriteLine( "W->W "+LK.ToString()+" "+Q2.ToString() );                              
                            }
                        }
                    }
                }
                if(Loop){
                    List<UCellLink> SolLst=new List<UCellLink>();
                    var LKX=CNLst.Find(P=>P.LoopFlag);
                    SolLst.Add(LKX);
                    while(LKX!=null){
                        int rcX=LKX.rc1;
                        LKX=CNLst.Find(P=>(P.rc2==rcX));
                        SolLst.Add(LKX);
                        UCellS Q2=BDLS.Find(p=>(p.rc==LKX.rc1));
                        if( Q2.rc==OrgRC ) break;
                    }
                    SolLst.Reverse();
                    SolStack.Clear();
                    SolLst.ForEach(P=>SolStack.Push(P));
                }
                return Loop;
            }
#endif           
            private void SetNLChain0( UCellS QX, UCellLink LKX, Stack<UCellLink> SolStack ){             
                //最初に接続条件チェックが入る
                List<UCellLink> SolLst=new List<UCellLink>();
                SolLst.Add(LKX);
                while(LKX!=null){
                    int rcX=LKX.rc1;
                    LKX=CNLst.Find(P=>(P.rc2==rcX));
                    SolLst.Add(LKX);
                    UCellS Q2=BDLS.Find(p=>(p.rc==LKX.rc1));
                    if( Q2.rc==OrgRC ) break;
                }
                SolLst.Reverse();
/*
                UCellLink LKpre=SolLst.Last();

                UCellLink LKXR=CNLst.Find(P=>(P.rc2==QX.rc));
                if( LKXR==null ){
                    Console.WriteLine();
                }
                UCellLink LKnxt=LKXR.Reverse();
                if( !Check_CellCellSequence(LKpre,LKnxt) ) return;

                SolLst.Add(LKnxt);
                while(LKXR!=null){
                    int rcX=LKXR.rc1;
                    LKXR=CNLst.Find(P=>(P.rc2==rcX));
                    if( LKXR==null ) break;
                    SolLst.Add(LKXR.Reverse());
                    UCellS Q2=BDLS.Find(p=>(p.rc==LKXR.rc1));
                    if( Q2.rc==OrgRC ) break;
                }
*/
                SolLst.ForEach(P=>SolStack.Push(P));
            }

            public bool Check_CellCellSequence( UCellLink LKpre, UCellLink LKnxt ){ 
                int noP=LKpre.no, noN=LKnxt.no;
                List<UCell>   qBDL=pSA.pGP.BDL;
                UCell UCX=qBDL[LKpre.rc2];
                switch(LKpre.type){
                    case 1:
                        switch(LKnxt.type){
                            case 1: return (noP!=noN);  //S->S
                            case 2: return (noP==noN);  //S->W
                        }
                        break;
                    case 2:
                        switch(LKnxt.type){
                            case 1: return (noP==noN);  //W->S
                            case 2: return ((noP!=noN)&&(UCX.FreeBC==2)); //W->W
                        }
                        break;
                }
                return false;
            }

            public IEnumerable<UCellLink> SetChainLink( UCellLink preLK, Stack<UCellLink> SolStack ){
                bool Loop=false;
                int  no=preLK.no;
                int  noB=1<<no;
                int  rc0=preLK.UCe1.rc;
                int  rc1=preLK.UCe2.rc;
                UCellS Q1=BDLS.Find(p=>(p.rc==rc1)); //軸セル(前リンクの終端）

                SolStack.Clear();
                if(preLK.type==S){ //StrongLink
                    Q1.TrueB |= noB;
                    {// S -> W
                        foreach( var LK in pSA.CeLKMan.IEGetRcNoType(rc1,no,W).Where(P=>P.rc2!=rc0) ){
                            UCellS Q2=BDLS.Find(p=>(p.rc==LK.rc2));
                            if( (Q2.UsedB&noB)>0 || Q2.SeqNo[no]>=NiceLoopMax-1 )  continue;
                                           
                            if(debugOn) Console.WriteLine( "S->W "+LK.ToString()+" "+Q2.ToString() );
                            if( Q2.rc==OrgRC ){
                                SetNLChain0( Q2, LK, SolStack);
                                LK.LoopFlag=true;
                                yield return LK;      
                                SolStack.Clear();
                            }
                            else{
                                Q2.SeqNo[no]=Math.Min(Q2.SeqNo[no],Q1.SeqNo[no]+1);
                                QueLK.Enqueue(LK); CNLst.Add(LK);
                            }
                        }
                    }

                    {// S -> S
                        int FreeBX=Q1.FreeB.DifSet(noB);
                        foreach( int no2 in FreeBX.IEGet_BtoNo() ){
                            int no2B=1<<no2;
                            Q1.FalseB |= no2B;
                            foreach( var LK in pSA.CeLKMan.IEGetRcNoType(rc1,no2,S).Where(P=>P.rc2!=rc0) ){
                                UCellS Q2=BDLS.Find(p=>(p.rc==LK.rc2));
                                if( (Q2.UsedB&noB)>0 || Q2.SeqNo[no]>=NiceLoopMax-1 )  continue;

                                if(debugOn) Console.WriteLine( "S->S "+LK.ToString()+" "+Q2.ToString() );
                                SolStack.Clear();
                                if( Q2.rc==OrgRC ){
                                    SetNLChain0( Q2, LK, SolStack);
                                    LK.LoopFlag=true;
                                    yield return LK;      
                                    SolStack.Clear();
                                }
                                else{
                                    Q2.SeqNo[no]=Math.Min(Q2.SeqNo[no],Q1.SeqNo[no]+1);
                                    QueLK.Enqueue(LK); CNLst.Add(LK);
                                }
                            }
                        }
                    }
                }
                else if(preLK.type==W){ //WeakLink
                    {// W -> S
                        foreach( var LK in pSA.CeLKMan.IEGetRcNoType(rc1,no,S).Where(P=>P.rc2!=rc0) ){
                            UCellS Q2=BDLS.Find(p=>(p.rc==LK.rc2));

                            if(debugOn) Console.WriteLine( "W->S "+LK.ToString()+" "+Q2.ToString() );
                            if( Q2.rc==OrgRC ){
                                SetNLChain0( Q2, LK, SolStack);
                                LK.LoopFlag=true;
                                yield return LK;      
                                SolStack.Clear();
                            }
                            else{
                                Q2.SeqNo[no]=Math.Min(Q2.SeqNo[no],Q1.SeqNo[no]+1);
                                QueLK.Enqueue(LK); CNLst.Add(LK);
                            }
                        }
                    }

                    {// W -> W
                        if( Q1.FreeB.BitCount()==2 ){
                            int no2=Q1.FreeB.DifSet(noB).BitToNum();
                            int no2B=1<<no2;
                            Q1.TrueB |= no2B;
                            foreach( var LK in pSA.CeLKMan.IEGetRcNoType(rc1,no2,W).Where(P=>P.rc2!=rc0) ){
                                UCellS Q2=BDLS.Find(p=>(p.rc==LK.rc2));

                                if(debugOn) Console.WriteLine( "W->W "+LK.ToString()+" "+Q2.ToString() ); 
                                if( Q2.rc==OrgRC ){
                                    SetNLChain0( Q2, LK, SolStack);
                                    LK.LoopFlag=true;
                                    yield return LK;      
                                    SolStack.Clear();                                    
                                }
                                else{
                                    Q2.SeqNo[no]=Math.Min(Q2.SeqNo[no],Q1.SeqNo[no]+1);
                                    QueLK.Enqueue(LK); CNLst.Add(LK);
                                }
                            }
                        }
                    }
                }
                yield break;
            }
        }
        
        public class UCellS{
            public readonly int rc;
            public readonly int FreeB;
            public int[]        SeqNo=new int[9];
            public int          TrueB=0;
            public int          FalseB=0;
            public int          UsedB{ get{ return (TrueB|FalseB);} }
/*        
            public int  r{ get{ return rc/9; } }
            public int  c{ get{ return rc%9; } }
            public int  b{ get{ return rc/27*3+(rc%9)/3; } }
*/
            public UCellS( UCell P ){
                this.rc=P.rc;
                this.FreeB=P.FreeB;
                Array.ForEach(SeqNo,p=>{p=999;});
            }

            public override string ToString(){
                string st="UcellS: rc:"+rc.ToString().PadLeft(2) +"["+SeqNo+"]";
                st +=" FreeB:"+FreeB.ToBitString(9);
                st +=" TrueB:"+TrueB.ToBitString(9) +" FalseB:"+FalseB.ToBitString(9);
                return st;
            }

        }

    }
}
