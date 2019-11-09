using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

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
        public Bit81( int[] _BPw ):this(){
            if(_BPw.GetLength(0)<3) WriteLine( $"Length Error:{_BP.GetLength(0)}" );
            this._BP[0]=_BPw[0]; this._BP[1]=_BPw[1]; this._BP[2]=_BPw[2]; 
        }
        public Bit81( List<UCell> X ):this(){
            X.ForEach(P=>{ _BP[P.rc/27] |= (1<<(P.rc%27)); } );
        }
        public Bit81( List<UCell> X, int F, int FreeBC=-1　):this(){
            if( FreeBC<0 ) X.ForEach(P=>{ if( (P.FreeB&F)>0 ) _BP[P.rc/27] |= (1<<(P.rc%27)); } );
            else X.ForEach(P=>{ if( (P.FreeB&F)>0 && P.FreeBC==FreeBC ) _BP[P.rc/27] |= (1<<(P.rc%27)); } );
        }
        public Bit81( List<UCell> X, int noB ):this(){
            X.ForEach(P=>{ if( (P.FreeB&noB)>0 ) _BP[P.rc/27] |= (1<<(P.rc%27)); } );
        }
        public Bit81( bool all ):this(){
            if(all) this._BP[0]=this._BP[1]=this._BP[2]= 0x7FFFFFF;
        }

        public void Clear( ){ _BP[0]=_BP[1]=_BP[2]=0; }
        public void BPSet( int rc ){ _BP[rc/27] |= (1<<(rc%27)); }
        public void BPSet( Bit81 sdX ){ for( int nx=0; nx<3; nx++ ) _BP[nx] |= sdX._BP[nx]; }               
        public void BPReset( int rc ){ _BP[rc/27] &= ((1<<(rc%27))^0x7FFFFFF); }
        public void BPReset( Bit81 sdX ){
            for( int nx=0; nx<3; nx++ ) _BP[nx] &= (sdX._BP[nx]^0x7FFFFFF);
        }

        public int  AggregateFreeB( List<UCell> XLst ){
            return this.IEGet_rc().Aggregate(0,(Q,q)=>Q|XLst[q].FreeB);
        }
        public Bit81 Copy(){ Bit81 Scpy=new Bit81(); Scpy.BPSet(this); return Scpy; }

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
                if( !(sdA is Bit81) || !(sdA is Bit81) )  return false;
                for( int nx=0; nx<3; nx++ ){ if( sdA._BP[nx]!=sdB._BP[nx] ) return false; }
                return true;
            }
            catch( NullReferenceException ex ){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
        }
        static public bool operator!=( Bit81 sdA, Bit81 sdB ){
            try{
                if( !(sdA is Bit81) || !(sdA is Bit81) )  return true;
                for( int nx=0; nx<3; nx++ ){ if( sdA._BP[nx]!=sdB._BP[nx] ) return true; }
                return false;
            }
            catch( NullReferenceException ex ){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
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
        public override bool Equals( object obj ){
            Bit81 A = obj as Bit81;
            for( int nx=0; nx<3; nx++ ){ if( A._BP[nx]!=_BP[nx] ) return false; }
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
                    st += ((bp&tmp)>0)? ((k%9)+0).ToString(): "."; //Internal representation
                //  st += ((bp&tmp)>0)? ((k%9)+1).ToString(): "."; //External representation
                    tmp = (tmp<<1);
                    if( k==26 )         st += " /";
                    else if( (k%9)==8 ) st += " ";
                }
            }
            return st;
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

  #region Bit324
    public class Bit324{
        public int   ID;
        static public readonly int sz;
        static public readonly int len=324;
        public readonly uint[] _BP;

        public int Count{ get{ return BitCount(); } }

        static Bit324( ){ sz=(len-1)/32+1; }
        public Bit324( ){ _BP=new uint[sz]; }
        public Bit324( Bit324 P ):this(){ for( int k=0; k<sz; k++ ) this._BP[k]=P._BP[k]; }
        public Bit324( uint[] _BPw ):this(){
            if(_BPw.GetLength(0)<sz) WriteLine( $"Length Error:{_BP.GetLength(0)}" );
            for( int k=0; k<sz; k++ ) this._BP[k]=_BPw[k];
        }

        public Bit324( bool all ):this(){
            if(all) for( int k=0; k<sz; k++ ) this._BP[k]= 0xFFFFFFFF;
        }

        public void Clear( ){ for( int k=0; k<sz; k++ ) this._BP[k]= 0; }
        public void BPSet( int rc ){ _BP[rc/32] |= (uint)(1<<(rc%32)); }
        public void BPSet( Bit324 sdX ){ for( int k=0; k<sz; k++ ) _BP[k] |= sdX._BP[k]; }               
        public void BPReset( int rc ){ _BP[rc/32] &= (uint)((1<<(rc%32))^0xFFFFFFFF); }

        public Bit324 Copy(){ Bit324 Scpy=new Bit324(); Scpy.BPSet(this); return Scpy; }

        static public Bit324 operator|( Bit324 sdA, Bit324 sdB ){
            Bit324 sdC = new Bit324();
            for( int k=0; k<sz; k++ ) sdC._BP[k] = sdA._BP[k] | sdB._BP[k];
            return sdC;
        }
        static public Bit324 operator&( Bit324 sdA, Bit324 sdB ){
            Bit324 sdC = new Bit324();
            for( int k=0; k<sz; k++ ) sdC._BP[k] = sdA._BP[k] & sdB._BP[k];
            return sdC;
        }
        static public Bit324 operator^( Bit324 sdA, Bit324 sdB ){
            Bit324 sdC = new Bit324();
            for( int k=0; k<sz; k++ ) sdC._BP[k] = sdA._BP[k] ^ sdB._BP[k];
            return sdC;
        }
        static public Bit324 operator-( Bit324 sdA, Bit324 sdB ){
            Bit324 sdC = new Bit324();
            for( int k=0; k<sz; k++ ) sdC._BP[k] = sdA._BP[k] & (sdB._BP[k]^0xFFFFFFFF);
            return sdC;
        }

        static public bool operator==( Bit324 sdA, Bit324 sdB ){
            try{
                if(sdB is Bit324){
                    for( int k=0; k<sz; k++ ){ if( sdA._BP[k]!=sdB._BP[k] ) return false; }
                    return true;
                }
                return false;
            }
            catch( NullReferenceException ex ){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
        }
        static public bool operator!=( Bit324 sdA, Bit324 sdB ){
            try{
                if(sdB is Bit324){
                    for( int k=0; k<sz; k++ ){ if(sdA._BP[k]!=sdB._BP[k]) return true; }
                    return false;
                }
                else return true;
            }
            catch( NullReferenceException ex ){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
        }

        public override int GetHashCode(){
            uint hc=0, p=7;
            for( int k=0; k<sz; k++ ){ hc ^= _BP[k]^p; p^=(p*3+997); }
            return (int)hc;
        }

        public uint CompareTo( Bit324 sdB ){
            for( int k=0; k<sz; k++ ) if(this._BP[k]==sdB._BP[k])  return (this._BP[k]-sdB._BP[k]);
            return (this._BP[sz-1]-sdB._BP[sz-1]);
        }

        public bool IsHit( int rc ){ return ((_BP[rc/32]&(1<<(rc%32)))>0); }
        public bool IsHit( Bit324 sdk ){
            for( int k=0; k<sz; k++ ){
                if( (_BP[k]&sdk._BP[k])>0 )  return true;
            }
            return false;
        }
        public bool IsHit( List<UCell> LstP ){ return LstP.Any(P=>(IsHit(P.rc))); }

        public bool IsZero( ){
            for( int k=0; k<sz; k++ ){
                if( _BP[k]>0 )  return false;
            }
            return true;
        }    
        public override bool Equals( object obj ){
            Bit324 A = obj as Bit324;
            if(A==null)  return false;
            for( int k=0; k<sz; k++ ){ if(A._BP[k]!=_BP[k]) return false; }
            return true;
        }       
        public int  BitCount( ){
            int bc=0;
            for( int k=0; k<sz; k++ ) bc+=_BP[k].BitCount();
            return bc;
        } 
        
        public int FindFirstrc(){
            for( int rc=0; rc<81; rc++ ){
                if( this.IsHit(rc) ) return rc;
            }
            return -1;
        }

        public override string ToString(){
            string st="";
            int m=1;
            for( int n=0; n<sz; n++ ){
                uint bp =_BP[n];
                int tmp=1;
              for( int k=0; k<32; k++){
                    st += ((bp&tmp)>0)? ((m%9)+0).ToString(): "."; //Internal representation
                //  st += ((bp&tmp)>0)? ((m%9)+1).ToString(): "."; //External representation
                    tmp = (tmp<<1);
                    if( (m%81)==0 )     st += " /";
                    else if( (m%9)==0 ) st += " ";
                    m++;
                }
            }
            return st;
        }
    }
  #endregion Bit324      
    
  #region Extended Function
    static public class StaticSA{ 
        static public bool rcbHitCheck( this int B, int C ){
            if( B/9==C/9 )  return true;
            if( B%9==C%9 )  return true;
            if( (B/27*3+(B%9)/3)==(C/27*3+(C%9)/3) )  return true;
            return false;
        }
        static public int  sameHouseCheck( this int B, int C ){
            // 0:Completely Different
            // 1:Same Row　2:Same Column　4:Same Block --> Bit Representation
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
                      + " "+(num>>18).ToBitString(9) +" /";
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
            string po="r" + (rc/9+1) + "c" + ((rc%9)+1);
            return po;
        }
        static public string HouseToString( this int HH ){
            string st="";
            if( (HH&0x1FF)>0 ) st += "r"+(HH&0x1FF).ToBitStringN(9)+" ";
            if( ((HH>>=9)&0x1FF)>0 ) st += "c"+(HH&0x1FF).ToBitStringN(9)+" ";
            if( ((HH>>=9)&0x1FF)>0 ) st += "b"+(HH&0x1FF).ToBitStringN(9)+" ";
            return st.Trim();
        }
        static public string tfxToString( this int tfx ){
            string st="";
            if( tfx<9 )            st += "r"+(tfx+1)+" ";
            if( tfx>=9 && tfx<18 ) st += "c"+(tfx-9+1)+" ";
            if( tfx>=18 )          st += "b"+(tfx-18+1)+" ";
            return st.Trim();
        }

        static private char[] sep=new Char[]{ ' ', ',', '\t' };
        static public string ToString_SameHouseComp( this string st ){
            if( st.Length <= 5 ){
                if( st.Length>0 && st[0]==' ' )  st = st.Remove(0,1);
                return st;
            }
            string retSt = "";

            string[] eLst;
            eLst=st.Split(sep,StringSplitOptions.RemoveEmptyEntries);

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
                if( (k%9)==8 )  st += "<>";
                else if( (k%3)==2 )  st += " ";
            }
            return st;
        }

        //========== bit representation→Number  ==========   
        static public int BitToNum( this int FreeB, int sz=9 ){    
            if( FreeB.BitCount()!=1) return -1;
            for( int k=0; k<sz; k++ ){
                if( FreeB==(1<<k) ) return k;
            }
            return -1;
        }

        //========== bit representation→2 Numbers ==========   
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

        static public IEnumerable<UCell> IEGetCellInHouse( this List<UCell> pBDL, int tfx, int FreeB=0x1FF ){
            int r=0, c=0, tp=tfx/9, fx=tfx%9;
            for( int nx=0; nx<9; nx++ ){
                switch(tp){
                    case 0: r=fx; c=nx; break;//row
                    case 1: r=nx; c=fx; break;//column
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//block
                }
                UCell P=pBDL[r*9+c];
                P.nx=nx;
                if( (P.FreeB&FreeB)>0 ) yield return P;
            }
        }
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
        static public IEnumerable<UCell> IEGetFixed_Pivot27( this List<UCell> pBDL, int rc0 ){
            int r0=rc0/9, c0=rc0%9, r=0, c=0;
            for( int tfx=0; tfx<27; tfx++ ){
                int fx=tfx%9;
                switch(tfx/9){
                    case 0: r=r0; c=fx; break; //row   
                    case 1: r=fx; c=c0; break; //Column
                    case 2: int b0=r0/3*3+c0/3; r=(b0/3)*3+fx/3; c=(b0%3)*3+fx%3; break;//block
                }
                if( r==r0 && c==c0 ) continue; //Exclude axis Cell
                int rc=r*9+c;
                if( pBDL[rc].No==0 ) continue; //Exclude unfixed Cell
                yield return pBDL[rc];
            }
        }
        static public IEnumerable<int> IEGet_BtoNo( this int noBin, int sz=9 ){
            for( int no=0; no<sz; no++ ){
                if( (noBin&(1<<no))>0 ) yield return no;
            }
            yield break;
        }
        static public IEnumerable<int> IEGet_rc( this Bit81 X81 ){
            for( int nx=0; nx<3; nx++ ){
                int _BPX=X81._BP[nx];
                for( int m=0; m<27; m++ ){
                    if( (_BPX&(1<<m))>0 ) yield return (nx*27+m);
                }
            }
            yield break;
        }
        static public List<int> GetRowList( this Bit81 X81, int r ) {
            List<int> RowLst=new List<int>();
            int _BPX=X81._BP[r/3];
            for( int c=0; c<9; c++ ){
                int p= (_BPX>>((r%3)*9+c))&1;
                RowLst.Add(p);
            }
            return RowLst;
        }
/*
        static public int Get_Bto1rc( this Bit81 X81 ){
            if(X81.Count!=1)  return -1;
            int bp;
            for( int n=0; n<3; n++ ){
                if( (bp=X81._BP[n])==0) continue;
                for( int k=0; k<27; k++){
                    if( ((bp>>k)&1)!=0 ) return (n*27+k);
                }
            }
            return -1;
        }
*/
        static public int ToBlock( this int rcx ){ return (rcx/27*3 + (rcx%9)/3); }
        static public string Connect<T>(this IEnumerable<T> list, string separator){
    	    return string.Join(separator, list);
        }
    }
  #endregion Extended Function
}