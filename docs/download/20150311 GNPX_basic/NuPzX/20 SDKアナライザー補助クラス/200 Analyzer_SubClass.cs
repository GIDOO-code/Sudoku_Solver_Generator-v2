using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Linq;

//using CombinationsLib;
using GIDOO_space;

namespace GNPZ_sdk{

  #region Bit81
    public class Bit81{
        public int   ID;
        public readonly int[] _BP;

        public int Count{ get{ return BitCount(); } }

        public Bit81( ){ _BP=new int[3]; }
        public Bit81( int rc ):this(){ BPSet(rc); }
        public Bit81( Bit81 P ):this(){
            this._BP[0]=P._BP[0]; this._BP[1]=P._BP[1]; this._BP[2]=P._BP[2]; 
        }

        public Bit81( List<UCell> X ):this(){
            X.ForEach(P=>{ _BP[P.rc/27] |= (1<<(P.rc%27)); } );
        }
        public Bit81( List<UCell> X, int F, int FreeBC=-1, int MaxSize=-1 ):this(){
            if( MaxSize>0 ){
                X.ForEach(P=>{ if( (P.FreeB&F)>0 && P.FreeBC<=MaxSize ) _BP[P.rc/27] |= (1<<(P.rc%27)); } );
            }
            else{
                if( FreeBC<0 ) X.ForEach(P=>{ if( (P.FreeB&F)>0 ) _BP[P.rc/27] |= (1<<(P.rc%27)); } );
                else X.ForEach(P=>{ if( (P.FreeB&F)>0 && P.FreeBC==FreeBC ) _BP[P.rc/27] |= (1<<(P.rc%27)); } );
            }
        }
/*
        public Bit81( List<UCellS> X, int noB ):this(){
            X.ForEach(P=>{ if( (P.FreeB&noB)>0 ) _BP[P.rc/27] |= (1<<(P.rc%27)); } );
        }
*/
        public Bit81( List<UCell> X, int noB ):this(){
            X.ForEach(P=>{ if( (P.FreeB&noB)>0 ) _BP[P.rc/27] |= (1<<(P.rc%27)); } );
        }


        public void Clear( ){ _BP[0]=_BP[1]=_BP[2]=0; }
        public void BPSet( int rc ){ _BP[rc/27] |= (1<<(rc%27)); }
        public void BPSet( Bit81 sdX ){ for( int nx=0; nx<3; nx++ ) _BP[nx] |= sdX._BP[nx]; }

        public void BPSetRCB27( int rc ){ BPSetR9(rc/9); BPSetC9(rc%9); BPSetB9( rc/27*3+(rc%9)/3 ); }
        public void BPSetR9( int r ){ for( int c=0; c<9; c++ ) BPSet(r*9+c); }
        public void BPSetC9( int c ){ for( int rc=c; rc<81; rc+=9 ) BPSet(rc); }
        public void BPSetB9( int b ){
            for( int nx=0; nx<9; nx++ ){
                int r=(b/3)*3+nx/3;
                int c=(b%3)*3+nx%3;                  
                BPSet(r*9+c);
            }
        }
                      
        public void BPReset( int rc ){ _BP[rc/27] &= ((1<<(rc%27))^0x7FFFFFF); }
        public void BPReset( Bit81 sdk ){ for( int nx=0; nx<3; nx++ ){ _BP[nx] &= (sdk._BP[nx]^0x7FFFFFF); } } 
        public void BPResetRCB27( int rc ){ BPResetR9(rc/9); BPResetC9(rc%9); BPResetB9( rc/27*3+(rc%9)/3 ); }
        public void BPResetR9( int r ){ for( int c=0; c<9; c++ ) BPReset(r*9+c); }
        public void BPResetC9( int c ){ for( int rcS=c; rcS<81; rcS+=9 ) BPReset(rcS); }
        public void BPResetB9( int b ){
            for( int nx=0; nx<9; nx++ ){
                int r=(b/3)*3+nx/3;
                int c=(b%3)*3+nx%3;                  
                BPReset(r*9+c);
            }
        }

        public int  AggregateFreeB( List<UCell> XLst ){
            return this.IEGet_rc().Aggregate(0,(Q,q)=>Q|XLst[q].FreeB);
        }

        public int[] GetBPRow( ){
            int[] BPR=new int[9];
            for( int r=0; r<9; r++ ){
                int _BPX = _BP[r/3]>>((r%3)*9);
                BPR[r] = (_BPX&0x1FF);
            }
            //    for( int k=0; k<9; k++ )  Console.WriteLine(" {0} {1}", k, BPR[k].ToBitString(9) );
            return BPR;
        }
        public int[] GetBPColumn( ){
            int[] BPC=new int[9];
            int bit=1;
            for( int nx=0; nx<3; nx++ ){
                int BPX=_BP[nx];
                for( int kx=0; kx<27; kx++ ){
                    if( (BPX&1)>0 )  BPC[kx%9] |= bit;
                    BPX>>=1;
                    if( (kx%9)==8 ) bit<<=1;
                }
            }
            //  for( int k=0; k<9; k++ )  Console.WriteLine(" {0} {1}", k, BPC[k].ToBitString(9) );
            return BPC;
        }

        public Bit81 Copy(){ Bit81 Scpy = new Bit81(); Scpy.BPSet(this); return Scpy; }

        static public Bit81 operator|( Bit81 sdA, Bit81 sdB ){
            Bit81 sdC = new Bit81();
            for( int nx=0; nx<3; nx++ ) sdC._BP[nx] = sdA._BP[nx] | sdB._BP[nx];
            return sdC;
        }
        static public Bit81 operator&( Bit81 sdA, Bit81 sdB ){
            Bit81 sdC = new Bit81();
            for( int nx=0; nx<3; nx++ ) sdC._BP[nx] = sdA._BP[nx]&sdB._BP[nx];
            return sdC;
        }
        static public Bit81 operator^( Bit81 sdA, Bit81 sdB ){
            Bit81 sdC = new Bit81();
            for( int nx=0; nx<3; nx++ ) sdC._BP[nx] = sdA._BP[nx] ^ sdB._BP[nx];
            return sdC;
        }
        static public Bit81 operator^( Bit81 sdA, int sdbInt ){
            Bit81 sdC = new Bit81();
            for( int nx=0; nx<3; nx++ ) sdC._BP[nx] = sdA._BP[nx] ^ sdbInt;
            return sdC;
        }
        static public Bit81 operator-( Bit81 sdA, Bit81 sdB ){
            Bit81 sdC = new Bit81();
            for( int nx=0; nx<3; nx++ ) sdC._BP[nx] = sdA._BP[nx] & (sdB._BP[nx]^0x7FFFFFF);
            return sdC;
        }

        static public bool operator==( Bit81 sdA, Bit81 sdB ){
            try{
                for( int nx=0; nx<3; nx++ ){ if( sdA._BP[nx]!=sdB._BP[nx] ) return false; }
                return true;
            }
            catch( NullReferenceException ){ return true; }
        }
        static public bool operator!=( Bit81 sdA, Bit81 sdB ){
            try{
                for( int nx=0; nx<3; nx++ ){ if( sdA._BP[nx]!=sdB._BP[nx] ) return true; }
                return false;
            }
            catch( NullReferenceException ){ return false; }
        }

        public override int GetHashCode(){ return ( _BP[0]^ (_BP[1]*1301)^ (_BP[2]*6577) ); }
        public int CompareTo( Bit81 sdB ){
            if( this._BP[0]==sdB._BP[0] )  return (this._BP[0]-sdB._BP[0]);
            if( this._BP[1]==sdB._BP[1] )  return (this._BP[1]-sdB._BP[1]);
            return (this._BP[2]-sdB._BP[2]);
        }


        public bool IsHit( int rc ){ return ((_BP[rc/27]&(1<<(rc%27)))>0); }
        public bool IsHit( Bit81 sdk ){
            for( int nx=0; nx<3; nx++ ){
                if( (_BP[nx]&sdk._BP[nx])>0 )  return true;
            }
            return false;
        }
        public bool IsHit( List<UCell> LstP ){ return LstP.Any(P=>(IsHit(P.rc))); }

        public bool IsZero( ){
            for( int nx=0; nx<3; nx++ ){
                if( _BP[nx]>0 )  return false;
            }
            return true;
        }
        
        public int  BitCount( ){
            int bc = _BP[0].BitCount() + _BP[1].BitCount() + _BP[2].BitCount();
            return bc;
        } 
        
        public int FindFirstrc(){
            for( int rc=0; rc<81; rc++ ){
                if( this.IsHit(rc) ) return rc;
            }
            return -1;
        }
        public List<int> ToList(){
            List<int> rcList = new List<int>();
            for( int n=0; n<3; n++ ){
                int bp = _BP[n];
                for( int k=0; k<27; k++){
                    if( (bp&(1<<k)) > 0 ) rcList.Add(n*27+k);
                }
            }
            return rcList;
        }
        public void CompressRow3( out int r9c3, out int c9r3 ){
            int r, c, b;
            r9c3=0;
            c9r3=0;

            for( int n=0; n<3; n++ ){
                int bp = _BP[n];
                for( int k=0; k<27; k++){
                    if( ((bp>>k)&1)==0 )  continue;
                    r = k/9 + n*3;
                    c = k%9;
                    b = (r/3*3+c/3);
                    r9c3 |= 1<<(b*3+c%3);
                    c9r3 |= 1<<(b*3+r%3);
                }
            }
        }
        public override string ToString(){
            string st="";
            for( int n=0; n<3; n++ ){
                int bp =_BP[n];
                int tmp=1;
                for( int k=0; k<27; k++){
                    st += ((bp&tmp)>0)? ((k%9)+0).ToString(): "."; //内部表現
                //  st += ((bp&tmp)>0)? ((k%9)+1).ToString(): "."; //外部表現
                    tmp = (tmp<<1);
                    if( k==26 )         st += "■";
                    else if( (k%9)==8 ) st += " ";
                }
            }
            return st;
        }
        public override bool Equals( object obj ){
            Bit81 A = obj as Bit81;
            for( int nx=0; nx<3; nx++ ){ if( A._BP[nx]!=_BP[nx] ) return false; }
            return true;
        }
        public string ToRCString(){
            string st="";
            for( int n=0; n<3; n++ ){
                int bp=_BP[n];
                for( int k=0; k<27; k++){
                    if( (bp&(1<<k))==0 )  continue;
                    int rc = n*27+k;
                    st += " ["+(rc/9*10+rc%9+11)+"]";
                }
            }
            return st;
        }
    }

  #endregion Bit81      
    
  #region BaseSet
    public class BaseSet{
        private int no; 
        private int tfx;
        public  int tp{ get{return tfx/9;} }
        public  int fx{ get{return tfx%9;} }

        public  int nxB;
        public  List<int> rUCellLst;
        public  List<int> rcbBitList;

        public BaseSet( int no, int tfx, int nxB ){
            this.no  = no;
            this.tfx = tfx;
            this.nxB = nxB;

            rUCellLst = new List<int>();
            rcbBitList = new List<int>();
            int r=0, c=0, b=0, rcbB=0;
            for( int nx=0; nx<9; nx++ ){
                if( (nxB&(1<<nx))!=0 ){
                    switch(tp){
                        case 0: r=fx; c=nx; b=r/3*3+c/3; rcbB=(1<<(c+9))+(1<<b); break;
                        case 1: r=nx; c=fx; b=r/3*3+c/3; rcbB=(1<<(r+18))+(1<<b); break;
                        case 2: b=fx; r=b/3*3+nx/3; c=(b%3)*3+nx%3; rcbB=(1<<(r+18))+(1<<(c+9)); break;
                    }
                    rUCellLst.Add(r*9+c);
                    rcbBitList.Add(rcbB);
                }
            }
        }
        public override string ToString(){
            string st = " no:" + (no+1);
            st += " tp:" + tp;
            st += "-" + (fx+1) + " " + nxB.ToBitString(9);
            rUCellLst.ForEach( x =>{st+=(" ["+(x/9*10+(x%9)+11)+"]");} );
            return st;
        }
    }   
  #endregion BaseSet

  #region 拡張関数
    static public class StaticSA{ 
        static public bool rcbHitCheck( this int B, int C ){
            if( B/9==C/9 )  return true;
            if( B%9==C%9 )  return true;
            if( (B/27*3+(B%9)/3)==(C/27*3+(C%9)/3) )  return true;
            return false;
        }
        static public int  sameHouseCheck( this int B, int C ){
            // 0:まったく異なる
            // 1:行が同じ　2:列が同じ　4:ブロックが同じ  ◆ビット表現
            int ret = 0;
            if( B/9==C/9 ) ret = 1;
            if( B%9==C%9 ) ret |= 2;
            if( (B/27*3+(B%9)/3)==(C/27*3+(C%9)/3) ) ret |= 4;
            return ret;
        }
        static public string ToBitString( this int num, int ncc ){
            int numW = num;
            string st="";
            for( int k=0; k<ncc; k++ ){
                st += ((numW&1)!=0)? (k+1).ToString(): ".";
                numW >>= 1;
            }
            return st;
        }

        static public string ToBitString27( this int num ){
            string st = (num&0x1FF).ToBitString(9)
                      + " "+((num>>9)&0x1FF).ToBitString(9)
                      + " "+(num>>18).ToBitString(9) +"■";
            return st;
        }

        static public string ToBitStringN( this int num, int ncc ){
            int numW = num;
            string st="";
            for( int k=0; k<ncc; k++ ){
                if( (numW&1)!=0 ) st += (k+1).ToString();
                numW >>= 1;
            }
            if( st=="" )  st = "-";

            return st;
        }
        static public string ToBitStringNor( this int num, int ncc ){
            int numW = num;
            string st="";
            for( int k=0; k<ncc; k++ ){
                if( (numW&1)!=0 ){
                    if( st=="" ) st = (k+1).ToString();
                    else st += " or "+(k+1).ToString();
                }
                numW >>= 1;
            }
            if( st=="" )  st = "*";

            return st;
        }
        static public string ToBitStringNZ( this int num, int ncc ){
            int numW = num;
            string st="";
            for( int k=0; k<ncc; k++ ){
                if( (numW&1)!=0 ) st += (k+1).ToString();
                numW >>= 1;
            }
            return st;
        } 

        static public string ToRCString( this int rc ){
          //string po="r" + (rc/9+1).ToString() + "c" + ((rc%9)+1).ToString();
            string po="r" + (rc/9+1) + "c" + ((rc%9)+1);
            return po;
        }
        static public string ToString_SameHouseComp( this string st ){
            if( st.Length <= 5 ){
                if( st.Length>0 && st[0]==' ' )  st = st.Remove(0,1);
                return st;
            }
            string retSt = "";

            string[] eLst;
            st.elementSeparator( out eLst );

            int[,] rcX = new int[2,9];
            Array.ForEach( eLst, s =>{
                int r = s.Substring(1,1).ToInt()-1;
                int c = s.Substring(3,1).ToInt()-1;
                rcX[0,r] |= (1<<c);
                rcX[1,c] |= (1<<r);
            } );

            bool hitSW = false;
            for( int r=0; r<9; r++ ){
                if( rcX[0,r].BitCount()>1 )  hitSW = true;
            }

            if( hitSW ){
                for( int r=0; r<9; r++ ){
                    if( rcX[0,r]==0 )  continue;
                    retSt += " r" + (r+1) + "c";
                    for( int c=0; c<9; c++ ){
                        if( (rcX[0,r]&(1<<c))==0 )  continue;
                        retSt += (c+1).ToString();
                        rcX[1,c] ^= (1<<r);
                    }
                }
            }

            for( int c=0; c<9; c++ ){
                if( rcX[1,c]==0 )  continue;
                retSt += " r";
                for( int r=0; r<9; r++ ){
                    if( (rcX[1,c]&(1<<r))>0 )  retSt += (r+1).ToString();
                }
                retSt += "c" + (c+1);
            }

            return (retSt.Remove(0,1));
        }

        static public int ToRCBitPat( this int rc ){
            int r=rc/9, c=rc%9, b=r/3*3+c/3;
            int rcbBP = (1<<(b+18)) | (1<<(c+9)) | (1<<r);
            return rcbBP;
        }
            
        static public string Row3Col3ToString( this int rcX3 ){
            string st="";
            for( int k=0; k<27; k++ ){
                if( (rcX3&(1<<k)) > 0 )  st += (k%3+1).ToString();
                else st+=".";
                if( (k%9)==8 )  st += "■";
                else if( (k%3)==2 )  st += " ";
            }
            return st;
        }

        //========== ビット→数字（ビット数が１のとき）   ==========   
        static public int BitToNum( this int FreeB, int sz=9 ){    
            if( FreeB.BitCount()!=1) return -1;
            for( int k=0; k<sz; k++ ){
                if( FreeB==(1<<k) ) return k;
            }
            return -1;
        }

        //========== ビット→2数字（ビット数が2のとき） ==========   
        static public bool BitTo2Nums( this int noB, ref int na, ref int nb  ){
            na=nb=-1;
            if( noB.BitCount()!=2) return false;
             for( int k=0; k<9; k++ ){
                if( (noB&1)>0 ) nb=k;
                if( na<0 ) na=nb;
                noB >>= 1;
            }
            return true;
        }   
/*
        static public IEnumerable<UCell> IEGet( this List<UCell> BDL, int tp, int fx, int FreeB=0x1FF, int blk=-1 ){ //nx=0...8
            int r=0, c=0, rc;
            for( int nx=0; nx<9; nx++ ){
                switch(tp){
                    case 0: r=fx; c=nx; break;//行
                    case 1: r=nx; c=fx; break;//列
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//ブロック
                }
                if( blk>=0 && (rc=r*9+c).ToBlock()!=blk ) continue;
                UCell P=BDL[r*9+c];
                if( (P.FreeB&FreeB)>0 ) yield return P;
            }
        }
*/
        static public IEnumerable<UCell> IEGetUCeNoB( this Bit81 BX, List<UCell> pBDL, int noBX ){ //nx=0...8        
            for( int n=0; n<3; n++ ){
                int bp = BX._BP[n];
                for( int k=0; k<27; k++){
                    if( ((bp>>k)&1)==0 ) continue;
                    UCell P=pBDL[n*27+k];
                    if( (P.FreeB&noBX)>0 )  yield return P;
                }
            }
        }

        static public IEnumerable<UCell> IEGet( this List<UCell> BDL, int tfx, int FreeB ){
            int r=0, c=0, tp=tfx/9, fx=tfx%9;
            for( int nx=0; nx<9; nx++ ){
                switch(tp){
                    case 0: r=fx; c=nx; break;//行
                    case 1: r=nx; c=fx; break;//列
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//ブロック
                }
                UCell P=BDL[r*9+c];
                P.nx=nx;
                if( (P.FreeB&FreeB)>0 ) yield return P;
            }
        }
        static public IEnumerable<UCell> IEGet( this List<UCell> BDL, int noB ){
            foreach( var P in BDL.Where(p=>(p.FreeB&noB)>0) ) yield return P;
        }

        static public IEnumerable<UCell> IEGet_SelTFBRC( this List<UCell> BDL, int tfx, int FreeB=-1, int block=-1, int rcSel=-1 ){ 
            int r=0, c=0, rc, tp=tfx/9, fx=tfx%9;
            for( int nx=0; nx<9; nx++ ){
                if( rcSel>0 && (rcSel&(1<<nx))==0 ) continue; 
                switch(tp){
                    case 0: r=fx; c=nx; break;//行
                    case 1: r=nx; c=fx; break;//列
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//ブロック
                }
                if( block>=0 && (rc=r*9+c).ToBlock()!=block ) continue;
                UCell P=BDL[r*9+c];
                P.nx=nx;
                if( (P.FreeB&FreeB)>0 ) yield return P;
            }
        }
/*       
        static public IEnumerable<UCell> IEGet_Sel( this List<UCell> BDL, int tp, int fx, int noB=0x1FF ){
            int r=0, c=0;
            for( int nx=0; nx<9; nx++ ){
                switch(tp){
                    case 0: r=fx; c=nx; break;//行
                    case 1: r=nx; c=fx; break;//列
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//ブロック
                }
                UCell P = BDL[r*9+c];
                if( P.No!=0 )  continue;
                if( (P.FreeB&noB)==0 )  continue;
                P.nx=nx;
                yield return P;
            }
        }
*/
        static public IEnumerable<UCell> IEGetFree_Pivot( this List<UCell> BDL, int tp, int rc0 ){
            int r=0, c=0, r0=rc0/9, c0=rc0%9;
            for( int nx=0; nx<9; nx++ ){
                switch(tp){
                    case 0: r=r0; c=nx; break; //行   
                    case 1: r=nx; c=c0; break; //列
                    case 2: int b0=r0/3*3+c0/3; r=(b0/3)*3+nx/3; c=(b0%3)*3+nx%3; break;//ブロック
                }
                if( r==r0 && c==c0 ) continue;
                int rc=r*9+c;
                if( BDL[rc].No!=0 ) continue;
                yield return BDL[rc];
            }
        }
        static public IEnumerable<UCell> IEGetFixed_Pivot27( this List<UCell> BDL, int rc0 ){
            int r0=rc0/9, c0=rc0%9, r=0, c=0;
            for( int tfx=0; tfx<27; tfx++ ){
                int fx=tfx%9;
                switch(tfx/9){
                    case 0: r=r0; c=fx; break; //行   
                    case 1: r=fx; c=c0; break; //列
                    case 2: int b0=r0/3*3+c0/3; r=(b0/3)*3+fx/3; c=(b0%3)*3+fx%3; break;//ブロック
                }
                if( r==r0 && c==c0 ) continue; //軸セルは除外
                int rc=r*9+c;
                if( BDL[rc].No==0 ) continue; //未確定セルは除外
                yield return BDL[rc];
            }
        }
   
        static public IEnumerable<int> IEGet_BtoNo( this int noBin, int sz=9 ){
            for( int no=0; no<sz; no++ ){
                if( (noBin&(1<<no))>0 ) yield return no;
            }
            yield break;
        }
        static public IEnumerable<int> IEGet_rc( this Bit81 X81 ){
        //    for( int rc=0; rc<81; rc++ ){
        //        if( X81.IsHit(rc) ) yield return rc;
        //    }
            for( int nx=0; nx<3; nx++ ){
                int _BPX=X81._BP[nx];
                for( int m=0; m<27; m++ ){
                    if( (_BPX&(1<<m))>0 ) yield return (nx*27+m);
                }
            }
            yield break;
        }
 
        static public int ToBlock( this int rcx ){ return (rcx/27*3 + (rcx%9)/3); }
        static public int BlockNToRc( this int b, int n ){ return (((b/3)*3+n/3)*9 + ((b%3)*3+n%3)); }

        static public string Connect<T>(this IEnumerable<T> list, string separator) {
    	    return string.Join(separator, list);
        }
    }
  #endregion 拡張関数

}