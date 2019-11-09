using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool GNP00_RemotePair( ){     //RemotePairs
            foreach( Bit81[] CRL in _RPColoring( ) ){
                int FreeB=CRL[0].ID;
                bool RPfond=false;
                foreach( var P in pBDL.Where(p=>(p.FreeB&FreeB)>0) ){
                    if( (CRL[0]&ConnectedCells[P.rc]).IsZero() )  continue;
                    if( (CRL[1]&ConnectedCells[P.rc]).IsZero() )  continue;                  
                    P.CancelB=P.FreeB&FreeB; RPfond=true;
                }
                if( RPfond ){
                    SolCode = 2;
                    Result = "Remote Pair #"+FreeB.ToBitStringN(9);
                    if( !SolInfoDsp ) return true;
                    ResultLong = Result;

                    Color Cr  = _ColorsLst[0];
                    Color Cr1 = Color.FromArgb(255,Cr.R,Cr.G,Cr.B);   
                    Color Cr2 = Color.FromArgb(150,Cr.R,Cr.G,Cr.B);
                    foreach( var P in CRL[0].IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(FreeB,AttCr,Cr1);
                    foreach( var P in CRL[1].IEGet_rc().Select(p=>pBDL[p]) ) P.SetNoBBgColor(FreeB,AttCr,Cr2);

                    if( !SnapSaveGP(true) )  return true;
                    //foreach( var E in pBDL ) E.CancelB=0;
                    RPfond=false;
                }
            }
            return false;
        }
           
        private IEnumerable<Bit81[]> _RPColoring( ){
            List<UCell>  BV2Lst = pBDL.FindAll(p=>p.FreeBC==2);
            if( BV2Lst.Count<4 )  yield break;
          
            Bit81 TBD = new Bit81();  BV2Lst.ForEach(p=>TBD.BPSet(p.rc));
            int  rc1;
            while( (rc1=TBD.FindFirstrc())>=0 ){
                Bit81[] CRL=new Bit81[2]; 
                CRL[0]=new Bit81(); CRL[1]=new Bit81(); 
                Queue<int> rcQue = new Queue<int>();
                rcQue.Enqueue(rc1<<1);
                CRL[0].BPSet(rc1);
                int FreeB = pBDL[rc1].FreeB;
                CRL[0].ID=FreeB;

                while( rcQue.Count>0 ){
                    int rcX=rcQue.Dequeue();
                    int kx = 1-(rcX&1);
                    rc1 = rcX>>1;
                    TBD.BPReset(rc1);

                    Bit81 Chain = TBD&ConnectedCells[rc1];
                    foreach( var rc2 in Chain.IEGet_rc() ){
                        if( pBDL[rc2].FreeB==FreeB ){
                            rcQue.Enqueue( (rc2<<1)|kx );
                            CRL[kx].BPSet(rc2);
                            TBD.BPReset(rc2);
                        }
                    }
                }
                yield return CRL;
            }
            yield break;
        }
    }
}