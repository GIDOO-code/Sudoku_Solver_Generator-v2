using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

using GIDOO_space;

namespace GNPZ_sdk{
  #region Bit81
    public class Bit81{
    //Bit representation of the entire SUDOKU board.
    // Attributes are ID and digit.
    // Generator functions are from various class entities.
    // Functions are bit operation(arithmetic, logical, string functions).
        public readonly int[] _BP;
        public int   ID;
        public int   no;  // 0-8
        public int Count{ get{ return BitCount(); } }   //count propertie

        //============================================================== Generator
        public Bit81(){ _BP=new int[3]; }
        public Bit81(int rc): this(){ BPSet(rc); }
        public Bit81(Bit81 P): this(){
            this._BP[0]=P._BP[0]; this._BP[1]=P._BP[1]; this._BP[2]=P._BP[2]; 
        }
        public Bit81(int[] _BPw): this(){
            if(_BPw.GetLength(0)<3) WriteLine($"Length Error:{_BP.GetLength(0)}");
            this._BP[0]=_BPw[0]; this._BP[1]=_BPw[1]; this._BP[2]=_BPw[2]; 
        }
        public Bit81(List<UCell> X): this(){
            X.ForEach(P=>{ _BP[P.rc/27] |= (1<<(P.rc%27)); });
        }
        public Bit81(List<UCell> X, int F, int FreeBC=-1):this(){
            if(FreeBC<0) X.ForEach(P=>{ if((P.FreeB&F)>0) _BP[P.rc/27] |= (1<<(P.rc%27)); });
            else X.ForEach(P=>{ if((P.FreeB&F)>0 && P.FreeBC==FreeBC) _BP[P.rc/27] |= (1<<(P.rc%27)); });
        }
        public Bit81(List<UCell> X, int noB):this(){
            X.ForEach(P=>{ if((P.FreeB&noB)>0) _BP[P.rc/27] |= (1<<(P.rc%27)); });
        }
        public Bit81(bool all_1): this(){
            if(all_1) this._BP[0]=this._BP[1]=this._BP[2]= 0x7FFFFFF;
        }

//        public Bit81(List<UCellEx> X, int noB):this(){
//            X.ForEach(P=>{ if((P.FreeB&noB)>0) _BP[P.rc/27] |= (1<<(P.rc%27)); });
//        }
        //--------------------------------------------------------------

        //==============================================================  Functions
        public void Clear(){ _BP[0]=_BP[1]=_BP[2]=0; }
        public void BPSet(int rc){ _BP[rc/27] |= (1<<(rc%27)); }
        public void BPSet(Bit81 sdX){ for(int nx=0; nx<3; nx++) _BP[nx] |= sdX._BP[nx]; }   
        public void BPReset(int rc){ _BP[rc/27] &= ((1<<(rc%27))^0x7FFFFFF); }
        public void BPReset(Bit81 sdX){ for(int nx=0; nx<3; nx++) _BP[nx] &= (sdX._BP[nx]^0x7FFFFFF); }

        public int  AggregateFreeB(List<UCell> XLst){
            return this.IEGet_rc().Aggregate(0,(Q,q)=>Q|XLst[q].FreeB);
        }      

        public Bit81 Copy(){ Bit81 Scpy=new Bit81(); Scpy.BPSet(this); return Scpy; }

        static public Bit81 operator|(Bit81 A, Bit81 B){
            Bit81 C = new Bit81();
            for(int nx=0; nx<3; nx++) C._BP[nx] = A._BP[nx] | B._BP[nx];
            return C;
        }
        static public Bit81 operator&(Bit81 A, Bit81 B){
            Bit81 C = new Bit81();
            for(int nx=0; nx<3; nx++) C._BP[nx] = A._BP[nx]&B._BP[nx];
            return C;
        }
        static public Bit81 operator^(Bit81 A, Bit81 B){
            Bit81 C = new Bit81();
            for(int nx=0; nx<3; nx++) C._BP[nx] = A._BP[nx] ^ B._BP[nx];
            return C;
        }
        static public Bit81 operator^(Bit81 A, int sdbInt){
            Bit81 C = new Bit81();	
            for(int nx=0; nx<3; nx++) C._BP[nx] = A._BP[nx] ^ sdbInt;
            return C;
        }
        static public Bit81 operator-(Bit81 A, Bit81 B){
            Bit81 C = new Bit81();
            for(int nx=0; nx<3; nx++) C._BP[nx] = A._BP[nx] & (B._BP[nx]^0x7FFFFFF);
            return C;
        }

        static public bool operator==(Bit81 A, Bit81 B){
            try{
                if(!(A is Bit81) || !(B is Bit81))  return false;
                for(int nx=0; nx<3; nx++){ if(A._BP[nx]!=B._BP[nx]) return false; }
                return true;
            }
            catch(NullReferenceException ex){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
        }
        static public bool operator!=(Bit81 A, Bit81 B){
            try{
                if(!(A is Bit81) || !(B is Bit81))  return true;
                for(int nx=0; nx<3; nx++){ if(A._BP[nx]!=B._BP[nx]) return true; }
                return false;
            }
            catch(NullReferenceException ex){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
        }

        public override int GetHashCode(){ return (_BP[0]^ (_BP[1]*1301)^ (_BP[2]*6577)); }
        public int CompareTo(Bit81 B){
            if(this._BP[0]==B._BP[0])  return (this._BP[0]-B._BP[0]);
            if(this._BP[1]==B._BP[1])  return (this._BP[1]-B._BP[1]);
            return (this._BP[2]-B._BP[2]);
        }

        public bool IsHit(int rc){ return ((_BP[rc/27]&(1<<(rc%27)))>0); }
        public bool IsHit(Bit81 sdk){
            for(int nx=0; nx<3; nx++){ if((_BP[nx]&sdk._BP[nx])>0)  return true; }
            return false;
        }
        public bool IsHit(List<UCell> LstP){ return LstP.Any(P=>(IsHit(P.rc))); }

        public bool IsZero(){
            for(int nx=0; nx<3; nx++){ if(_BP[nx]>0)  return false; }
            return true;
        }    
        public override bool Equals(object obj){
            Bit81 A = obj as Bit81;
            for(int nx=0; nx<3; nx++){ if(A._BP[nx]!=_BP[nx]) return false; }
            return true;
        }       
        public int  BitCount(){
            int bc = _BP[0].BitCount() + _BP[1].BitCount() + _BP[2].BitCount();
            return bc;
        } 
        
        public int FindFirstrc(){
            for(int rc=0; rc<81; rc++){ if(this.IsHit(rc)) return rc; }
            return -1;
        }

        public int GetBitPattern_tfx(int tfx){
            int r=0, c=0, tp=tfx/9, fx=tfx%9, bp=0;
            for(int nx=0; nx<9; nx++){
                switch(tp){
                    case 0: r=fx; c=nx; break;//row
                    case 1: r=nx; c=fx; break;//column
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//block
                }
                if(IsHit(r*9+c)) bp|=1<<nx;
            }
            return  bp;
        }
         
        public IEnumerable<int> IEGetRC(){
            int rc=0, B;
            for(int k=0; k<3; k++){
                rc=k*27;
                if((B=_BP[k])==0) continue;
                for(int m=0; m<27; m++){
                    if((B&1)==1) yield return (rc+m);
                    B=B>>1;
                }
            }
        }
        public List<int> ToList(){
            List<int> rcList = new List<int>();
            for(int n=0; n<3; n++){
                int bp = _BP[n];
                for(int k=0; k<27; k++){ if((bp&(1<<k))>0) rcList.Add(n*27+k); }
            }
            return rcList;
        }

        static private int sft9=7<<9, sft18=7<<18;

        public int Get_RowBitPatten(int rx){
            int bp = this._BP[rx/3]>>((rx%3)*9);
            return (bp&0x1FF);
        }
        public int GetColumnPattern(int cx){
            int bp=0, k=0;
            for(int rc=cx; rc<81; rc+=9){
                if(IsHit(rc)) bp.BitSet(k);
                k++;
            }
            return bp;
        }
        public int Get_blockBitPattern(int bx){
            //Arrange the cells in order of block
            int bp = this._BP[bx/3]>>((bx%3)*3);
            int bPat = (bp&7) | ((bp&sft9)>>6) | ((bp&sft18)>>12);
            return bPat;
        }
        public int Get_RowColumnBlock(){
            int rcb=0;
            foreach(var rc in IEGetRC()){
                rcb |= 1<<(rc/9) | 1<<((rc%9)+9) | 1<<(rc/27*3+(rc%9)/3+18);
            }
            return rcb;
        }

        public void CompressRow3(out int r9c3, out int c9r3){
            int r, c, b;
            r9c3=0;
            c9r3=0;

            for(int n=0; n<3; n++){
                int bp = _BP[n];
                for(int k=0; k<27; k++){
                    if(((bp>>k)&1)==0)  continue;
                    r=k/9+n*3; c=k%9; b=(r/3*3+c/3);
                    r9c3 |= 1<<(b*3+c%3);
                    c9r3 |= 1<<(b*3+r%3);
                }
            }
        }

        public override string ToString(){
            string st="";
            for(int n=0; n<3; n++){
                int bp =_BP[n];
                int tmp=1;
              for(int k=0; k<27; k++){
                    st += ((bp&tmp)>0)? ((k%9)+0).ToString(): "."; //Internal representation
                //  st += ((bp&tmp)>0)? ((k%9)+1).ToString(): "."; //External representation
                    tmp = (tmp<<1);
                    if(k==26)         st += " /";
                    else if((k%9)==8) st += " ";
                }
            }
            return st;
        }
        public string ToRCString(){
            string st="";
            for(int n=0; n<3; n++){
                int bp=_BP[n];
                for(int k=0; k<27; k++){
                    if((bp&(1<<k))==0)  continue;
                    int rc=n*27+k;
                    st += " ["+(rc/9*10+rc%9+11)+"]";
                }
            }
            return st;
        }

    }
  #endregion Bit81    
    
  #region Bit981
    public class Bit981{
        //bit81×9 digits
        static private int[] tfbPat;
        static readonly int   sz=9;
        public  int     ID;
        public  Bit81[] _BQ;
        public  int[]   tfx27Lst;
        public  int     nzBit=0;

        static Bit981(){
            tfbPat=new int[81];
            for(int rc=0; rc<81; rc++){
                tfbPat[rc] =(1<<(rc/9)) | (1<<(rc%9+9)) | (1<<((rc/27*3+(rc%9)/3)+18));
            }
        }
        public int Count{ get{ return BitCount(); } }

        public Bit981(){
            _BQ=new Bit81[9]; tfx27Lst=new int[9];
            for(int n=0; n<sz; n++) _BQ[n]=new Bit81();
        }
        public Bit981(Bit981 Q): this(){
            this.nzBit=Q.nzBit;
            for(int n=0; n<sz; n++) this._BQ[n]=Q._BQ[n];
        }
        public Bit981(Bit81 P): this(){
            int no=P.no;
            this._BQ[no]=P;
            if(!P.IsZero()) nzBit |= (1<<no);
        }
         
        public Bit981(UGLink UGL): this(){
            Bit981 B=new Bit981();

            if(UGL.rcBit81 is Bit81){
                int no=UGL.rcBit81.no;
                B._BQ[no] = UGL.rcBit81;
                foreach(int rc in UGL.rcBit81.IEGet_rc())  B._BQ[no].BPSet(rc);
                if(!UGL.rcBit81.IsZero()) nzBit |= (1<<no);
            }
            else{ 
                UCell uc=UGL.UC as UCell;
                foreach(var n in uc.FreeB.IEGet_BtoNo()){
                    B._BQ[n].BPSet(uc.rc);
                    nzBit |= (1<<n);
                }
            }
        }

        public void Clear(){
            for(int n=0; n<sz; n++){ this._BQ[n].Clear(); tfx27Lst[n]=0; }
            nzBit=0;
        }
        public void BPSet(int no, int rc, bool tfbSet=false){
            _BQ[no].BPSet(rc); nzBit |= (1<<no);
            if(tfbSet) tfxSet(no,rc);
        }
        public void BPSet(int no, Bit81 sdX, bool tfbSet=false){
            _BQ[no] |= sdX; nzBit |= (1<<no);
            if(tfbSet){
                foreach(var rc in sdX.IEGetRC())  tfxSet(no,rc);
            }
        }
        public void BPReset(int no, int rc){
            _BQ[no].BPReset(rc);
            if(_BQ[no].IsZero()) nzBit.BitReset(no);
        }
        public void tfxSet(int no, int rc){
            //(1<<(rc/9)) | (1<<(rc%9+9)) | (1<<((rc/27*3+(rc%9)/3)+18));
            tfx27Lst[no] |= tfbPat[rc];

        }
      //public Bit81 Get_BP81A2(int n0, int n1){ return _BQ[n0]&_BQ[n1]; }
        public void tfxReset(int n, int tfx){ tfx27Lst[n] = tfx27Lst[n].BitReset(tfx); }

        public Bit981 Copy(){ 
            Bit981 Scpy=new Bit981();
            for(int n=0; n<sz; n++) Scpy._BQ[n] = _BQ[n].Copy();
            Scpy.nzBit = this.nzBit;
            return Scpy;
        }

        static public Bit981 operator|(Bit981 A, Bit981 B){
            Bit981 C = new Bit981();
            for(int n=0; n<sz; n++){ C._BQ[n] = A._BQ[n] | B._BQ[n]; }
            __Set_nzBit(C);
            return C;
        }
        static public Bit981 operator&(Bit981 A, Bit981 B){
            Bit981 C = new Bit981();
            for(int n=0; n<sz; n++) C._BQ[n] = A._BQ[n] & B._BQ[n];
            __Set_nzBit(C);
            return C;
        }
        static public Bit981 operator^(Bit981 A, Bit981 B){
            Bit981 C = new Bit981();
            for(int n=0; n<sz; n++) C._BQ[n] = A._BQ[n] ^ B._BQ[n];
            __Set_nzBit(C);
            return C;
        }
        static public Bit981 operator-(Bit981 A, Bit981 B){
            Bit981 C = new Bit981();
            for(int n=0; n<sz; n++) C._BQ[n] = A._BQ[n] - B._BQ[n];
            __Set_nzBit(C);
            return C;
        }
        static private void __Set_nzBit(Bit981 C){
            int nzBit=0;
            for(int n=0; n<sz; n++){
                if(C._BQ[n].IsZero()) nzBit &= ((1<<n)^0x7FFFFFFF);
                else nzBit |= (1<<n);
            }
            C.nzBit=nzBit;
        }

        static public bool operator==(Bit981 A, Bit981 B){
            try{
                if(A.nzBit!=B.nzBit)  return false;
                if(B is Bit981){
                    for(int k=0; k<sz; k++){ if(A._BQ[k]!=B._BQ[k]) return false; }
                    return true;
                }
                return false;
            }
            catch(NullReferenceException ex){ WriteLine(ex.Message+"\r"+ex.StackTrace); return false; }
        }
        static public bool operator!=(Bit981 A, Bit981 B){
            try{
                if(A.nzBit!=B.nzBit)  return true;
                if(B is Bit981){
                    for(int k=0; k<sz; k++){ if(A._BQ[k]!=B._BQ[k]) return true; }
                    return false;
                }
                else return true;
            }
            catch(NullReferenceException ex){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
        }

        public override int GetHashCode(){
            uint hc=0;
            uint P = (uint)_BQ[0].GetHashCode();
            for(int k=1; k<sz; k++){ hc ^= (uint)_BQ[k].GetHashCode()^P^(uint)(3<<k); }
            return (int)hc;
        }

        public uint CompareTo(Bit981 B){
            for(int n=0; n<sz-1; n++){
                if(this._BQ[n]==B._BQ[n])  return (uint)(this._BQ[n].CompareTo(B._BQ[n]));
            }
            return (uint)(this._BQ[sz-1].CompareTo(B._BQ[sz-1]));
        }
/*
        public int  _SetNonZeroB(){
            NonZeroB=0;
            for(int n=0; n<sz; n++){ if(!_BQ[n].IsZero()) NonZeroB |= (1<<n); }
            return NonZeroB;
        }
*/
        public int IsHit(int rc){
            int H=0;
            for(int n=0; n<9; n++){ if(IsHit(n,rc)) H |= (1<<n); }
            return H;
        }
        public bool IsHit(int no, int rc){
            return ((_BQ[no]._BP[rc/27]&(1<<(rc%27)))>0);
        }
        public bool IsHit(int no, Bit81 A){
            if(_BQ[no].IsHit(A))  return true;
            return false;
        }
        public Bit81 CompressToHitCells(){
            Bit81 Q=new Bit81();
            foreach(var n in nzBit.IEGet_BtoNo()) Q |= _BQ[n];
            return Q;
        }

        public bool IsZero(){
            if(nzBit==0)  return true;
            for(int k=0; k<sz; k++){ if(!_BQ[k].IsZero())  return false; }
            return true;
        }    
        public override bool Equals(object obj){
            Bit981 A = obj as Bit981;
            if(A==null)  return false;
            for(int k=0; k<sz; k++){ if(_BQ[k]!=A._BQ[k]) return false; }
            return true;
        }       
        public int  BitCount(){
            int bc=0;
            foreach(int n in nzBit.IEGet_BtoNo()) bc += _BQ[n].BitCount();
            // for(int k=0; k<sz; k++) bc+=_BQ[k].BitCount();
            return bc;
        } 
        
        public int GetBitPattern_tfnx(int n, int tfx){
            Bit81 P=_BQ[n];        
            int r=0, c=0, tp=tfx/9, fx=tfx%9, bp=0;
            for(int nx=0; nx<9; nx++){
                switch(tp){
                    case 0: r=fx; c=nx; break;//row
                    case 1: r=nx; c=fx; break;//column
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;//block
                }
                if(P.IsHit(r*9+c)) bp|=1<<nx;
            }
            return  bp;
        }
        public int GetBitPattern_rcN(int rc){
            int bp=0;
            foreach(var n in nzBit.IEGet_BtoNo()){ if(_BQ[n].IsHit(rc))  bp|=1<<n; }
            return bp;
        }

        public override string ToString(){
            string st="nonZero:"+nzBit.ToBitString(9)+"\r";
            for(int no=0; no<sz; no++){
                st += string.Format("no:{0} {1}", no, _BQ[no]) + "\r";
            }
            return st;
        }
    }
  #endregion Bit981

  #region Bit324
    public class Bit324{    //324=81*4
      //Bit representations of arbitrary length
        public int   ID;
        static public readonly int len=324;
        static public readonly int sz;
        public readonly uint[] _BP;

        public int Count{ get{ return BitCount(); } }

        static Bit324(){ sz=(len-1)/32+1; }
        public Bit324(){ _BP=new uint[sz]; }
        public Bit324(Bit324 P):this(){ for(int k=0; k<sz; k++) this._BP[k]=P._BP[k]; }
        public Bit324(uint[] _BPw):this(){
            if(_BPw.GetLength(0)<sz) WriteLine($"Length Error:{_BP.GetLength(0)}");
            for(int k=0; k<sz; k++) this._BP[k]=_BPw[k];
        }

        public Bit324(bool all):this(){
            if(all) for(int k=0; k<sz; k++) this._BP[k]= 0xFFFFFFFF;
        }
         
        public Bit324(UGLink UGL){
            Bit324 B=new Bit324();
            if(UGL.rcBit81 is Bit81){
                int no81=UGL.rcBit81.no*81;
                foreach(int k in UGL.rcBit81.IEGet_rc()) B.BPSet(no81+k);
            }
            else{ 
                UCell uc=UGL.UC as UCell;
                foreach(var no in uc.FreeB.IEGet_BtoNo()) B.BPSet(no*81+uc.rc);
            }
        }

        public void Clear(){ for(int k=0; k<sz; k++) this._BP[k]= 0; }
//        public void BPSet(int rc){ _BP[rc/32] |= (uint)(1<<(rc%32)); }
        public void BPSet(int rc){
            try{
                _BP[rc/32] |= (uint)(1<<(rc%32));
            }
            catch(Exception e){
                WriteLine(e.Message+"\r"+e.StackTrace);
            }      
        }

        public void BPSet(Bit324 sdX){ for(int k=0; k<sz; k++) _BP[k] |= sdX._BP[k]; }               
        public void BPReset(int rc){ _BP[rc/32] &= (uint)((1<<(rc%32))^0xFFFFFFFF); }

        public Bit324 Copy(){ Bit324 Scpy=new Bit324(); Scpy.BPSet(this); return Scpy; }

        static public Bit324 operator|(Bit324 A, Bit324 B){
            Bit324 C = new Bit324();
            for(int k=0; k<sz; k++) C._BP[k] = A._BP[k] | B._BP[k];
            return C;
        }
        static public Bit324 operator&(Bit324 A, Bit324 B){
            Bit324 C = new Bit324();
            for(int k=0; k<sz; k++) C._BP[k] = A._BP[k] & B._BP[k];
            return C;
        }
        static public Bit324 operator^(Bit324 A, Bit324 B){
            Bit324 C = new Bit324();
            for(int k=0; k<sz; k++) C._BP[k] = A._BP[k] ^ B._BP[k];
            return C;
        }
        static public Bit324 operator-(Bit324 A, Bit324 B){
            Bit324 C = new Bit324();
            for(int k=0; k<sz; k++) C._BP[k] = A._BP[k] & (B._BP[k]^0xFFFFFFFF);
            return C;
        }

        static public bool operator==(Bit324 A, Bit324 B){
            try{
                if(B is Bit324){
                    for(int k=0; k<sz; k++){ if(A._BP[k]!=B._BP[k]) return false; }
                    return true;
                }
                return false;
            }
            catch(NullReferenceException ex){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
        }
        static public bool operator!=(Bit324 A, Bit324 B){
            try{
                if(B is Bit324){
                    for(int k=0; k<sz; k++){ if(A._BP[k]!=B._BP[k]) return true; }
                    return false;
                }
                else return true;
            }
            catch(NullReferenceException ex){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
        }

        public override int GetHashCode(){
            uint hc=0, p=7;
            for(int k=0; k<sz; k++){ hc ^= _BP[k]^p; p^=(p*3+997); }
            return (int)hc;
        }

        public uint CompareTo(Bit324 B){
            for(int k=0; k<sz; k++) if(this._BP[k]==B._BP[k])  return (this._BP[k]-B._BP[k]);
            return (this._BP[sz-1]-B._BP[sz-1]);
        }

        public bool IsHit(int rc){ return ((_BP[rc/32]&(1<<(rc%32)))>0); }
        public bool IsHit(Bit324 sdk){
            for(int k=0; k<sz; k++){ if((_BP[k]&sdk._BP[k])>0)  return true; }
            return false;
        }
        public bool IsHit(List<UCell> LstP){ return LstP.Any(P=>(IsHit(P.rc))); }

        public bool IsZero(){
            for(int k=0; k<sz; k++){ if(_BP[k]>0)  return false; }
            return true;
        }    
        public override bool Equals(object obj){
            Bit324 A = obj as Bit324;
            if(A==null)  return false;
            for(int k=0; k<sz; k++){ if(A._BP[k]!=_BP[k]) return false; }
            return true;
        }       
        public int  BitCount(){
            int bc=0;
            for(int k=0; k<sz; k++) bc+=_BP[k].BitCount();
            return bc;
        } 
        
        public int FindFirstrc(){
            for(int rc=0; rc<81; rc++){ if(this.IsHit(rc)) return rc; }
            return -1;
        }

        public override string ToString(){
            string st="";
            int m=1;
            for(int n=0; n<sz; n++){
                uint bp =_BP[n];
                int tmp=1;
              for(int k=0; k<32; k++){
                    st += ((bp&tmp)>0)? ((m%9)+0).ToString(): "."; //Internal representation
                //  st += ((bp&tmp)>0)? ((m%9)+1).ToString(): "."; //External representation
                    tmp = (tmp<<1);
                    if((m%81)==0)     st += " /";
                    else if((m%9)==0) st += " ";
                    m++;
                }
            }
            return st;
        }
    }
  #endregion Bit324      
}