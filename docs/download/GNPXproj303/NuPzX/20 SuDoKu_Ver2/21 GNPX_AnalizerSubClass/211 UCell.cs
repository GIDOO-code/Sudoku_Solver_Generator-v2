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
    public class UCell{ //Basic Cell Class
        public object obj;

        public readonly int  rc;
        public readonly int  r;
        public readonly int  c;
        public readonly int  b;
        public int bx{ get{ return ((r%3)*3+(c%3)); } }

        public int      ErrorState; //0:-  1:Fixed 　8:Violation  9:No solution
        public int      No;         //>0:Problem  =0:Open  <0:Solution
        public int      FreeB;
        public int      FreeBC{ get{ return FreeB.BitCount(); } }

        public int      FixedNo;  
        public int      CancelB;

        public List<EColor> ECrLst;     
        public Color CellBgCr;     
        
        public bool Selected;
        public int  Fixed=0;
        public int  nx;

        public UCell( ){}
        public UCell( int rc, int No=0, int FreeB=0 ){
            this.rc = rc;
            this.r  = rc/9;
            this.c  = rc%9;
            this.b  = rc/27*3+(rc%9)/3;
            this.No = No;
            this.FreeB = FreeB;

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
                ECrLst.ForEach(p=>UCcpy.ECrLst.Add(new EColor(p)));
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
        public void SetNoBColorRev( int noB, Color cr ){
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(noB,cr,cr) );
        }
        public void SetNoBBgColor( int noB, Color cr, Color crBg ){
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(noB,cr) );
            ECrLst.Add( new EColor(crBg) );
        }
        public void SetNoBBgColorRev( int noB, Color cr, Color crBg ){
            if( ECrLst==null )  ECrLst=new List<EColor>();
            ECrLst.Add( new EColor(noB,cr,cr) );
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
}