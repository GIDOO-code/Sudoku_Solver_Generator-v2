using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GIDOO_space;

namespace GNPZ_sdk{

  #region UCell
    public class UCell{
        public object obj;

        public readonly int  rc;
        public readonly int  r;
        public readonly int  c;
        public readonly int  b;
        public int bx{ get{ return ((r%3)*3+(c%3)); } }

        public int      ErrorState; //0:無  1:確定 　8:ルール違反  9:解なし
        public int      No;         //>0:問題  =0:空き  <0:解答
        public int      FreeB0;     //初期状態
        public int      FreeB;
        public int      FreeBC{ get{ return FreeB.BitCount(); } }

        public int      FixedNo;  
        public int      CancelB;

        public List<EColor> ECrLst;
///*999*/        public int  AttNmB;     //着目数字のビット表現
///*999*/        public int  AttCrB;
///*999*/        public int  AttCrB2;
        
        public Color CellBgCr;     
        
        public bool Selected;       //作業用変数
        public int  Fixed=0;        //作業用変数
        public int  nx;             //作業用変数
        public int  FreeBD; //degenerated
        public int  type;   // 1:強いリンク  2:弱いリンク

        public UCell( ){}
        public UCell( int rc, int No=0 ){
            this.rc = rc;
            this.r  = rc/9;
            this.c  = rc%9;
            this.b  = rc/27*3+(rc%9)/3;
            this.No = No;
            this.FreeB = 0x1FF;

            this.ECrLst=null;
        }
        public UCell( int rc, int No, int FreeB ){
            this.rc = rc;
            this.r  = rc/9;
            this.c  = rc%9;
            this.b  = rc/27*3+(rc%9)/3;
            this.No = No;
            this.FreeB = FreeB;

            this.ECrLst=null;
        }
        public UCell( object obj, int rc, int FreeB, int FreeB0=-9 ){
            this.obj     = obj;
            this.rc      = rc;
            this.r       = rc/9;
            this.c       = rc%9;
            this.b       = rc/27*3+(rc%9)/3;
            this.FreeB   = FreeB;
            this.FreeB0  = (FreeB0==-9)? FreeB: FreeB0;
            this.CancelB = 0;
            this.ECrLst=null;
        }

        public void Reset_StepInfo(){
            ErrorState =0;
            CancelB  =0;
            FixedNo  =0;
            Selected =false;
            Fixed    =0;   

            this.ECrLst=null;
            CellBgCr = Colors.Black;       
        }

        public UCell Copy( ){
            UCell UCcpy=(UCell)this.MemberwiseClone();
            if( this.ECrLst!=null ){
                UCcpy.ECrLst=new List<EColor>();
                ECrLst.ForEach(p=>UCcpy.ECrLst.Add(p));
            }
            return UCcpy;
        }

        public void _True( int no ){
            FreeB &= ((1<<no)^0x1FF);
            No = no;
        }
        public void _False( int no ){
            FreeB &= ((1<<no)^0x1FF);
            No = no;
        }

        public void SetCellBgColor( Color CellBgCr ){ 
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(CellBgCr) );
        }

        public void SetNoBColor( int noB, Color cr ){
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(noB,cr) );
        }

        public void SetNoBBgColor( int noB, Color cr, Color crBg ){
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(noB,cr) );
            ECrLst.Add( new EColor(crBg) );
        }
        public override string ToString(){
            string po = " UCell rc:"+rc+"["+((r+1)*10+(c+1)) +"]  no:"+No;
            po +=" FreeB:" + FreeB.ToBitString(9);
            po +=" CancelB:" + CancelB.ToBitString(9);
            return po;
        }
        public void ResetAnalysisResult(){
            CancelB  =0;
            FixedNo  =0;
            Selected =false;
            Fixed    =0; 
            this.ECrLst=null;
        }
    }
  #endregion UCell 

  #region UGrCells
        public class UGrCells: List<UCell>, IComparable{
        public readonly int      tfx;  //house番号
        public readonly int      no;   //UGCellsの要素数字  
        public Bit81             B81;  //セル位置のビット表現
        public int FreeB{
            get{
                int FreeB=0;
                this.ForEach(P=>FreeB|=P.FreeB);
                return FreeB;
            }
        }

        public UGrCells( UCell X, int no ){
            this.tfx=-9; this.no=no;
            B81=new Bit81();
            this.Add(X);
        }
        public UGrCells( int tfx, int no ){
            this.tfx=tfx; this.no=no;
            B81=new Bit81();
        }
        public UGrCells( int tfx, int no, UCell Q ): this(tfx,no){
            Add(Q);
        }
        public UGrCells Copy(){
            UGrCells Pcpy=new UGrCells(tfx,no);
            Pcpy.B81=B81.Copy();
            return Pcpy;
        }

        public void Add( UCell P ){ base.Add(P); B81.BPSet(P.rc); }
        public void Add( List<UCell> PL ){ PL.ForEach(P=>Add(P)); }

        public override bool Equals( object obj ){
            UGrCells Q=obj as UGrCells;
            if( Q==null ) return false;

            if( no!=Q.no ) return false;
            if( Count!=Q.Count ) return false;
            for( int k=0; k<Count; k++ ){
                if( this[k].rc!=Q[k].rc ) return false;
            }
            return true;
        }

        public bool EqualsRC( object obj ){
            UGrCells Q=obj as UGrCells;
            if( Q==null ) return false;

            if( Count!=Q.Count ) return false;
            for( int k=0; k<Count; k++ ){
                if( this[k].rc!=Q[k].rc ) return false;
            }
            return true;
        }
        public int CompareTo( object obj ){
            UGrCells Q = obj as UGrCells;
            if( this.no  !=Q.no   ) return (this.no-Q.no);
            if( this.Count !=Q.Count  ) return (this.Count-Q.Count);
            for( int k=0; k<Count; k++ ){
                if( this[k].rc !=Q[k].rc ) return (this[k].rc-Q[k].rc);
            }
            return 0;
        }

        public override string ToString(){
            string st="";
            if( Count<=0 ) st=".";
            else if( Count==1 ) st = this[0].rc.ToRCString();
            else{
                this.ForEach(P=>{st+=" "+P.rc.ToRCString();});
                st="<"+st.ToString_SameHouseComp()+">";
            }
            return st;
        }

        public string GCToString(){
            string st="";
            if( Count<=0 ) st=".";
            else{
                this.ForEach(P=>{st+=" "+P.rc.ToRCString();});
                st =  "no:"+no+" <"+st.ToString_SameHouseComp()+">";
            }
            return st;
        }
    }
    #endregion GroupedCell
}