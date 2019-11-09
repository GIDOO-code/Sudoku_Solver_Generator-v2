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
        private Bit81[] BPnoBLst = new Bit81[9];

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
                BPnoBLst[no] = BPnoB;
                for( int tfx=0; tfx<27; tfx++ ){ 
                    Bit81 Q = pHouseCells[tfx]&BPnoB;
                    if( !Q.IsZero() && !HBL.Contains(Q) ){
                        Q.ID=(tfx<<4)|no; UGLLst.Add(new UGLink(Q));
                    }
                }
            }
            foreach( var P in pBDL.Where(p=>p.No==0) )  UGLLst.Add(new UGLink(P));
            UGLink.pBPnoBLst=BPnoBLst;

            return UGLLst.Count;
        }

        public IEnumerable<UBasCov> IEGet_BaseSet( int sz ){
            if(UGLLst==null)  yield break;
            Bit81[] HB819=new Bit81[9]; for(int no=0;no<9;no++)  HB819[no]=new Bit81();
            List<UGLink>  basUGLs=new List<UGLink>();
            Bit324 usedLK=new Bit324();
            bool chkPrnt=false;

            UGLink UGL;
            int nxt=int.MaxValue;
            var prmBas=new Permutation(UGLLst.Count,sz);
            
            Bit81 BPchk=new Bit81();
            while( prmBas.Successor(nxt) ){
                                   ++GeneralLogicGen.ChkBas1;   //*****  
/*
                if(sz>=2) chkPrnt=true;

                if(chkPrnt){
                    Write("\r   -->");
                    for( int k=0; k<sz; k++ ) Write( prmBas.Pnum[k].ToString(" ###0"));
                    for( int k=0; k<sz; k++ ) Write( " "+UGLLst[prmBas.Pnum[k]].ToString2());
                }
*/

                int noBP=0, noC=0;
                BPchk.Clear();
                for( int no=0; no<9; no++ )  HB819[no].Clear();

                for( int k=0; k<sz; k++ ){
                    nxt=k;
                    UGL=UGLLst[prmBas.Pnum[k]];
                    if(UGL.rcB is Bit81){   // RCB
                        int no=UGL.no;
                        if(k>0){
                            if( (noBP&(1<<no))==0 && !BPchk.IsHit(UGL.rcB) )  goto LNextSet; 
                            if( HB819[no].IsHit(UGL.rcB) )   goto LNextSet;  
                        //    if( (HB819[no]&UGL.Conn).IsZero() ) goto LNextSet;
                            if( !UGL.CheckConnected(HB819) ) goto LNextSet;
                        }
                
                        BPchk |= UGL.rcB;
                        HB819[no] |= UGL.rcB;
                        noBP |= 1<<no;
                        noC++;
                    }
                    else{   // Cell
                        UCell uc=UGL.uc;
                        noBP |= uc.FreeB;
                        if(noBP.BitCount()>sz-noC) goto LNextSet; 
                        int rc=uc.rc;

                        if(k>0){
                            if(!UGL.CheckConnected(HB819))   goto LNextSet;
                        }
                        foreach( var no in uc.FreeB.IEGet_BtoNo(9) ){
                            if(k>0 && HB819[no].IsHit(rc) )  goto LNextSet;
                            HB819[no].BPSet(rc);
                        }
                    }
                }
                                        ++GeneralLogicGen.ChkBas2;   //*****
                basUGLs.Clear(); usedLK.Clear();
                for(int k=0; k<sz; k++){
                    basUGLs.Add(UGL=UGLLst[prmBas.Pnum[k]]);
                    usedLK.BPSet(UGL.IDx);
                }
                UBasCov UBC=new UBasCov( noBP, basUGLs, HB819, sz, usedLK );
                
                if(chkPrnt) Write( "#####");

                yield return UBC;

              LNextSet:
                continue;
            }
            yield break;
        }
  
        public IEnumerable<UBasCov> IEGet_CoverSet( UBasCov UBC, int rnk ){ 
            if(UGLLst==null)  yield break;

            int noBP=UBC.noBP;
            int sz=UBC.sz;
            Bit81[] HB819=UBC.HB819;
            Bit324  usedLK=UBC.usedLK;
            int     B;
            List<UGLink> UGLCovLst=new List<UGLink>();  // UGLCovLst:candidate link
            
            foreach( var P in UGLLst.Where(q=>!usedLK.IsHit(q.IDx)) ){
                if(P.rcB is Bit81){
                    if( (noBP&(1<<P.no))==0 ) continue;  //*****
                    B = (HB819[P.no] & P.rcB).BitCount();
                    if(rnk==0){ if(B>=2 ) UGLCovLst.Add(P); }
                    else if(B>0)  UGLCovLst.Add(P);
                }
                else{
                    if( noBP>0 && (noBP&P.uc.FreeB)==0 )  continue;  //*****
                    B=noBP&P.uc.FreeB;
                    if( B==0 )  continue;  //*****
                    int rc=P.uc.rc, cnt=(rnk==0)? 0: 1;
                    foreach( var no in B.IEGet_BtoNo() ){
                        if( !HB819[no].IsHit(rc) ) continue; 
                        if((++cnt)>=2){ UGLCovLst.Add(P); break; }
                    }
                }
            }

            if(UGLCovLst.Count<sz+rnk)  yield break;

            Bit81[] HC819=new Bit81[9];
            Bit81[] HLapB=new Bit81[9];
            for( int no=0; no<9; no++ ){ HC819[no]=new Bit81(); HLapB[no]=new Bit81(); }

            Combination cmbCvr=new Combination(UGLCovLst.Count,sz+rnk);
            int nxt=int.MaxValue;

            while( cmbCvr.Successor(nxt) ){
                                    ++GeneralLogicGen.ChkCov1;

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
                    if( !(HB819[no]-HC819[no]).IsZero() ) goto LNextSet;
                }

                //*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
                int rcCan=-1, noCan=-1, rnkCC=0;
                for( int k=0; k<sz+rnk; k++ ){
                    nxt=k;
                    UGLink P=UGLCovLst[cmbCvr.Cmb[k]];

                    if(P.rcB is Bit81){　//===== row column block =============================
                        int no=P.no;

                        if(rnk>0 && HLapB[no].IsHit(P.rcB) ){      //===== overlap
                            if( rcCan>=0 && noCan!=no ) goto LNextSet;
                            if( (++rnkCC)>rnk ) goto LNextSet;      //Overlap is more than rank number
                            Bit81 Lap=(HLapB[no]&P.rcB)-HB819[no]; //Overlap pattern
                            if( Lap.BitCount()>1 )  goto LNextSet;  //Over 2 cells or more
                            int rcX=Lap.FindFirstrc();             //Overlapping cell position
                            if( rcCan>=0 && rcX!=rcCan ) goto LNextSet; 
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
                        if(cc>1) goto LNextSet;                      //overlap is 2 numbers or more
                        if(cc==1){
                            if( rcCan>=0 && (rc!=rcCan || nn!=noCan) ) goto LNextSet;
                            rcCan=rc; noCan=nn;                     //Overlapping cells, numbers
                            if( (++rnkCC)>rnk ) goto LNextSet;       //Overlap is more than rank number                          
                        }

                        foreach( var no in Q.FreeB.IEGet_BtoNo() ){
                            HC819[no].BPSet(Q.rc);
                            if(!HB819[no].IsHit(Q.rc))  HLapB[no].BPSet(Q.rc);
                        }
                    }　//----------------------------------------------------------------------

                }
                if(rnk==0){ if(rcCan>=0 || noCan>=0 ) goto LNextSet; } //CoverSet conditions
                else{ if(rcCan<0 || noCan<0 ) goto LNextSet; }
                                    ++GeneralLogicGen.ChkCov2;   //*****

                List<UGLink>  covUGLs=new List<UGLink>();
                for( int k=0; k<sz+rnk; k++ ) covUGLs.Add(UGLCovLst[cmbCvr.Cmb[k]]);
                UBC.addCoverSet( covUGLs, HC819, rcCan, noCan, rnk );
                yield return UBC;

              LNextSet:
                continue;
            }
            yield break;
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
        static public Bit81[]   pBPnoBLst;
        static public int ID0;
        public int  sz;
        public int  IDx;

//q     public PropSQ prop;

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
//q         this.prop=new PropSQ(3,uc.rc,uc);
        }
        public UGLink( Bit81 rcB){
            this.rcB=rcB; this.sz=rcB.BitCount(); this.IDx=ID0++;
            Conn=new Bit81();
            foreach(var rc in rcB.IEGet_rc()) Conn |= pConnectedCells[rc];
            int no=rcB.ID&0xF, tfx=rcB.ID>>4;
//q         this.prop=new PropSQ(no,tfx/9,tfx%9,rcB);
        }
        public bool CheckDisconnected( Bit81[] HB819 ){ //
            return !CheckConnected(HB819);
        }   
        public bool CheckConnected( Bit81[] HB819 ){
            if(rcB is Bit81){
                Bit81 B=pBPnoBLst[no]&Conn;
                if( !(HB819[no] & B).IsZero() ) return true;;
                for( int n=0; n<9; n++ ){
                    if(n==no)  continue;
                    if( !(HB819[n] & B).IsZero() ) return true;
                }
                return false;
            }
            else{
                int rc=uc.rc;
                foreach( var no in uc.FreeB.IEGet_BtoNo() ){
                    if( !(HB819[no]&pConnectedCells[rc]).IsZero() )  return false;
                }
                return true;
            }
        }      
        public string ToString( string ttl="" ){
            string st = ttl+" UGLink_";
            if(uc!=null) st+="UCell "+ uc.ToString();
            else         st+="ULink no:"+ (no) + " Bit81 "+rcB.ToString();
            return st;
        }
        public string ToString2(){
            if(rcB is Bit81) return (tfx.tfxToString()+("#"+(no+1))+" ");
            else             return (uc.rc.ToRCString()+" ");
        }
    }

/* //q
    public class PropSQ{ //State quantity
        private int _container;
        public  int type{ get{ return (_container>>28)&0xF; } } //rcb cell
        public  int no{   get{ return (_container>>24)&0xF; } } 
        public  int pos{  get{ return (_container>>16)&0xF; } } //0-8 or 0-80
        public  int attrnB{ get{ return _container&0x1FFF; } }

        public PropSQ( int no, int type, int pos, Bit81 BP){
            _container = BP.GetBitPattern_tfx(type*9+pos);
            _container |= (type<<28) | (no<<24) | (pos<<16);
        }
        public PropSQ( int no, int type, int pos, List<UCell> qBDL ){
            _container = qBDL.IEGetCellInHouse(type*9+pos,1<<no).Aggregate(0,(Q,P)=>Q|(1<<P.nx));
            _container |= (type<<28) | (no<<24) | (pos<<16);
        }
        public PropSQ( int type, int pos, UCell UC ){
            _container |= (type<<28) | (0xF<<24) | (pos<<16) |  UC.FreeB;
        }
    }
*/
}