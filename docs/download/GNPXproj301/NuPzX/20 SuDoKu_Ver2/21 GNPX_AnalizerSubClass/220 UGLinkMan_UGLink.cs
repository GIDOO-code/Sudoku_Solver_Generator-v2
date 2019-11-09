using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Console;

using GIDOO_space;

namespace GNPZ_sdk {
    public class UGLinkMan{
        public List<UCell>    pBDL;             
        public Bit81[]        pHouseCells;
        public Bit81[]        pConnectedCells;
        private List<Bit81>   HBL=new List<Bit81>();
        private List<UGLink>  UGLLst=null;

        private Bit81[] Colored=new Bit81[9];
        private Bit81[] Processed=new Bit81[9];

        static public bool printSW=false;

        public UGLinkMan( AnalyzerBaseV2 AnB ){
            this.pBDL = AnB.pBDL;
            this.pHouseCells = AnalyzerBaseV2.HouseCells;
            this.pConnectedCells = AnalyzerBaseV2.ConnectedCells;
            UGLink.pConnectedCells = AnalyzerBaseV2.ConnectedCells;
            for( int no=0; no<9; no++ ){
                Colored[no]=new Bit81(); Processed[no]=new Bit81();
            }
            UGLink.pUGLM=this;
        }

        public int PrepareUGLinkMan( ){
            UGLLst=new List<UGLink>();
            UGLink.ID0=0;
            for( int no=0; no<9; no++ ){    
                int noB=1<<no;       
                Bit81 BPnoB=new Bit81(pBDL,noB);
                for( int tfx=0; tfx<27; tfx++ ){ 
                    Bit81 Q = pHouseCells[tfx]&BPnoB;
                    if( !Q.IsZero() && !HBL.Contains(Q) ){ Q.ID=(tfx<<4)|no; UGLLst.Add(new UGLink(Q)); }
                }
            }
            foreach( var P in pBDL.Where(p=>p.No==0) )  UGLLst.Add(new UGLink(P));

            return UGLLst.Count;
        }

        public IEnumerable<UBasCov> IEGet_BaseSet( int sz ){
            if(UGLLst==null)  yield break;
            Bit81[] HB819=new Bit81[9]; for(int no=0;no<9;no++)  HB819[no]=new Bit81();
            List<UGLink>  basUGLs=new List<UGLink>();
            Bit324 usedLK=new Bit324();

            UGLink UGL, UGL0=null;
            int nxt=int.MaxValue;
            Combination cmbBas=new Combination(UGLLst.Count,sz);
            
            Bit81 BasChk=new Bit81();
            while( cmbBas.Successor(nxt) ){
                                        int chk1=++GeneralLogicGen.ChkBas1;   //*****
                int noBP=0;
                BasChk.Clear();
                for( int no=0; no<9; no++ )  HB819[no].Clear();
               // int  RCBcc=0;
                for( int k=0; k<sz; k++ ){
                    nxt=k;
                    UGL=UGLLst[cmbBas.Cmb[k]];
                    if(UGL.rcB is Bit81){   // RCB
                        int no=UGL.no;
                        if( (HB819[no]).IsHit(UGL.rcB) )  goto LNxtCmb;
                        if(sz>=2 && (k==sz-1) ){  //*****
                            if(!BasChk.IsZero() && (BasChk&UGL.Conn).IsZero() )   goto LNxtCmb;
                            if((noBP&(1<<no))>0 && (HB819[no]&UGL.Conn).IsZero()) goto LNxtCmb;
                        }
                        HB819[no] |= UGL.rcB;
                        noBP |= 1<<no; BasChk |= UGL.rcB;
                        if(k==0) UGL0=UGL;
                        if(sz==2 && k==1){      //*****
                            if( UGL0.tfx!=UGL.tfx && UGL0.no!=UGL.no )  goto LNxtCmb;
                        }
                    }
                    else{   // Cell
                        UCell uc=UGL.uc;
                        int rc=uc.rc;
                        if(!BasChk.IsZero() && (BasChk&pConnectedCells[rc]).IsZero() )  goto LNxtCmb;
                        if(noBP>0){
                            if((noBP&uc.FreeB)==0) goto LNxtCmb;
                            int mc=0;
                            if(noBP.BitCount()<2)  goto LNxtCmb;
                            foreach( var no in uc.FreeB.IEGet_BtoNo(9) ){
                                if( HB819[no].IsHit(rc) )  goto LNxtCmb;
                                if( !(HB819[no]&pConnectedCells[rc]).IsZero() )  mc++;
                            }
                            if(mc<2)  goto LNxtCmb;
                        }

                        if(sz>=2 && (k==sz-1)){
                            if(!UGL.CheckConnected(HB819))   goto LNxtCmb;
                        }
                        noBP |= uc.FreeB; 
                        foreach( var no in uc.FreeB.IEGet_BtoNo(9) ){
                            if(k>0 && HB819[no].IsHit(rc) )  goto LNxtCmb;
                            HB819[no].BPSet(rc);
                       }
                    }
                }
                if(sz>=2 && !IsLinked9(HB819) )  goto LNxtCmb;
                                        int chk2=++GeneralLogicGen.ChkBas2;   //*****
                basUGLs.Clear(); usedLK.Clear();
                for(int k=0; k<sz; k++){
                    basUGLs.Add(UGL=UGLLst[cmbBas.Cmb[k]]);
                    usedLK.BPSet(UGL.IDx);
                }
                UBasCov UBC=new UBasCov( noBP, basUGLs, HB819, sz, usedLK );

                yield return UBC;

              LNxtCmb:
                continue;
            }
            yield break;
        }
       
        public IEnumerable<UBasCov> IEGet_CoverSet( UBasCov UBC, int rnk ){ 
            if(UGLLst==null)  yield break;

            int noBP=UBC.noBP;
            int sz=UBC.sz;
            Bit81[] HB819=UBC.HB819;

            Bit81 Z0, Zw;
            Bit81 Z1=new Bit81();   // Cell containing one BaseSet element
            Bit81 Zn=new Bit81();   // Cell that contains two or more BaseSet elements
            foreach( var P in UBC.basUGLs.Where(q=>(q.rcB is Bit81) ) ){
                Z0=P.rcB; Zw=Z0&Z1;
                Zn|=Zw; Z1 =(Z1-Zw)|(Z0-(Z1|Zn));
            }

            Bit324  usedLK=UBC.usedLK;
            List<UGLink> UGLCovLst=new List<UGLink>();  // UGLCovLst:candidate link
            foreach( var P in UGLLst.Where(q=>!usedLK.IsHit(q.IDx)) ){
                if(P.rcB is Bit81){
                    if( noBP>0 && (noBP&(1<<P.no))==0 ) continue;  //*****
                    if( HB819[P.no].IsHit(P.rcB) )  UGLCovLst.Add(P);
                }
                else{
                    if( noBP>0 && (noBP&P.uc.FreeB)==0 )  continue;  //*****
                    if(!Zn.IsZero() && Zn.IsHit(P.uc.rc) ) UGLCovLst.Add(P);
                }
            }
            if(UGLCovLst.Count<sz+rnk)  yield break;

            Bit81[] HC819=new Bit81[9];
            Bit81[] HLapB=new Bit81[9];
            for( int no=0; no<9; no++ ){
                HC819[no]=new Bit81();
                HLapB[no]=new Bit81();
            }

            Combination cmbCvr=new Combination(UGLCovLst.Count,sz+rnk);
            int nxt=int.MaxValue;

            while( cmbCvr.Successor(nxt) ){
                                    int chk1=++GeneralLogicGen.ChkCov1;

                for( int no=0; no<9; no++ ){ HC819[no].Clear(); HLapB[no].Clear(); }
                for( int k=0; k<sz+rnk; k++ ){
                    UGLink P=UGLCovLst[cmbCvr.Cmb[k]];

                    if(P.rcB is Bit81) HC819[P.no] |= P.rcB;
                    else{
                        UCell Q=P.uc;
                        foreach( var no in Q.FreeB.IEGet_BtoNo() ) HC819[no].BPSet(Q.rc);
                    }

                }
                for( int no=0; no<9; no++ ){
                    if( !(HB819[no]-HC819[no]).IsZero() ) goto LNxtCmb;
                }

                //*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
                int rcCan=-1, noCan=-1, rnkCC=0;
                for( int k=0; k<sz+rnk; k++ ){
                    nxt=k;
                    UGLink P=UGLCovLst[cmbCvr.Cmb[k]];

                    if(P.rcB is Bit81){　//===== row column block =============================
                        int no=P.no;

                        if(rnk>0 && HLapB[no].IsHit(P.rcB) ){      //===== overlap
                            if( rcCan>=0 && noCan!=no ) goto LNxtCmb;
                            if( (++rnkCC)>rnk ) goto LNxtCmb;      //Overlap is more than rank number
                            Bit81 Lap=(HLapB[no]&P.rcB)-HB819[no]; //Overlap pattern
                            if( Lap.BitCount()>1 )  goto LNxtCmb;  //Over 2 cells or more
                            int rcX=Lap.FindFirstrc();             //Overlapping cell position
                            if( rcCan>=0 && rcX!=rcCan ) goto LNxtCmb; 
                            rcCan=rcX; noCan=no;　　               //Overlapping cells, numbers
                        }
                        HC819[no] |= P.rcB;                        
                        HLapB[no] |= (P.rcB-HB819[no]); 
                    }
                    else{　             //===== cell ==========================================
                        UCell Q=P.uc;
                        int rc=Q.rc, cc=0, nn=-1;
                        foreach( var no in Q.FreeB.IEGet_BtoNo() ){ //===== Overlap? 
                            if( HB819[no].IsHit(rc) )  continue;
                            if( HLapB[no].IsHit(rc) ){ nn=no; cc++; }
                        }
                        if(cc>1) goto LNxtCmb;                      //overlap is 2 numbers or more
                        if(cc==1){
                            if( rcCan>=0 && (rc!=rcCan || nn!=noCan) ) goto LNxtCmb;
                            rcCan=rc; noCan=nn;                     //Overlapping cells, numbers
                            if( (++rnkCC)>rnk ) goto LNxtCmb;       //Overlap is more than rank number                          
                        }

                        foreach( var no in Q.FreeB.IEGet_BtoNo() ){
                            HC819[no].BPSet(Q.rc);
                            if(!HB819[no].IsHit(Q.rc))  HLapB[no].BPSet(Q.rc);
                        }
                    }　//----------------------------------------------------------------------

                }
                if(rnk==0){ if(rcCan>=0 || noCan>=0 ) goto LNxtCmb; } //CoverSet conditions
                else{ if(rcCan<0 || noCan<0 ) goto LNxtCmb; }
                                    int chk2=++GeneralLogicGen.ChkCov2;   //*****

                List<UGLink>  covUGLs=new List<UGLink>();
                for( int k=0; k<sz+rnk; k++ ) covUGLs.Add(UGLCovLst[cmbCvr.Cmb[k]]);
                UBC.addCoverSet( covUGLs, HC819, rcCan, noCan, rnk );
                yield return UBC;

              LNxtCmb:
                continue;
            }
            yield break;
        }
        public bool IsLinked9( Bit81[] P9 ){ //Check connection status of BaseSet element
            int noS=-1, noB=0, cc=0;
            for( int no=0; no<9; no++ ){
                if( !P9[no].IsZero() ){
                    Colored[no].Clear();
                    Processed[no].Clear();
                    if(noS<0)  noS=no;
                    noB |= 1<<no;
                    cc++;
                }
            }
            if(cc==1)  return true;

            int rc0 = P9[noS].FindFirstrc();
            Colored[noS].BPSet(rc0);
            while(true){
                noS=-1;
                foreach( var no in noB.IEGet_BtoNo() ){
                    Bit81 T = Colored[no]-Processed[no];
                    noS=no;
                    if( (rc0=T.FindFirstrc())>=0 ) break;
                }
                if(rc0<0)  break;

                Processed[noS].BPSet(rc0);
                Colored[noS] |= P9[noS]&pConnectedCells[rc0];
                foreach( var no in noB.IEGet_BtoNo() ){
                    if( P9[no].IsHit(rc0) ) Colored[no].BPSet(rc0);
                } 
            }

            foreach( var no in noB.IEGet_BtoNo() ){
                if( !(P9[no]-Colored[no]).IsZero() ) return false;
            }
            return true;
        }
    }

    public class UBasCov{
        public Bit324       usedLK;
        public List<UGLink> basUGLs; //
        public List<UGLink> covUGLs; //
        public Bit81[] HB819;
        public Bit81[] HC819;
        public int noBP;
        public int rcCan;
        public int noCan;
        public int sz;
        public int rnk;

        public UBasCov( int noBP, List<UGLink> basUGLs, Bit81[] HB819, int sz, Bit324 usedLK ){
            this.noBP=noBP; this.basUGLs=basUGLs; this.HB819=HB819; this.sz=sz; this.usedLK=usedLK;
        }
        public void addCoverSet( List<UGLink> covUGLs, Bit81[] HC819, int rcCan, int noCan, int rnk ) {
            this.covUGLs=covUGLs; this.HC819=HC819; this.rcCan=rcCan; this.noCan=noCan; this.rnk=rnk;
        }
    }
    public class UGLink{
        static public UGLinkMan pUGLM;
        static public Bit81[]   pConnectedCells;
        static public int ID0;
        public int  sz;
        public int  IDx;

        public UCell uc=null;
        public Bit81 rcB=null;
        public int no{
            get{ return( (rcB is Bit81)? (rcB.ID&0xF): -1); }
            set{ if(rcB is Bit81) rcB.ID=(rcB.ID&0xF)|value; }
        }
        public int tfx{
            get{ return( (rcB is Bit81)? (rcB.ID>>4): -1); }
            set{ if(rcB is Bit81) rcB.ID=(rcB.ID&0xF)|(value<<4); }
        }
        public Bit81 Conn=null;

        public UGLink( UCell uc ){
            this.uc=uc; this.sz=uc.FreeBC; this.IDx=ID0++;

        }
        public UGLink( Bit81 rcB ){
            this.rcB=rcB; this.sz=rcB.BitCount(); this.IDx=ID0++;
            Conn=new Bit81();
            foreach(var rc in rcB.IEGet_rc()) Conn |= pConnectedCells[rc];
        }
        public bool CheckDisconnected( Bit81[] HB819 ){ //
            if(rcB is Bit81){ return (HB819[no]&Conn).IsZero(); }
            else{
                int rc=uc.rc;
                foreach( var no in uc.FreeB.IEGet_BtoNo() ){
                    if( !(HB819[no]&pConnectedCells[rc]).IsZero() )  return false;
                }
                return true;
            }
        }   
        public bool CheckConnected( Bit81[] HB819 ){
            if(rcB is Bit81){ return !(HB819[no]&Conn).IsZero(); }
            else{
                int rc=uc.rc;
                foreach( var no in uc.FreeB.IEGet_BtoNo() ){
                    if( !(HB819[no]&pConnectedCells[rc]).IsZero() )  return true;
                }
                return false;
            }
        }      
        public string ToString( string ttl="" ){
            string st = ttl+" UGLink_";
            if(uc!=null) st+="UCell "+ uc.ToString();
            else         st+="ULink no:"+ (no) + " Bit81 "+rcB.ToString();
            return st;
        }
    }
}