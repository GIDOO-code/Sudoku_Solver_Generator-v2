using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using GIDOO_space;

namespace GNPZ_sdk{
    //Sudoku's cell definition.

    public class UCell{ //Basic Cell Class
      //public object   obj;
        public readonly int  rc;    //cell position(0-80)
        public readonly int  r;     //row
        public readonly int  c;     //column
        public readonly int  b;     //block

        public int      ErrorState; //0:-  1:Fixed 　8:Violation  9:No solution
        public int      No;         //>0:Problem  =0:Open  <0:Solution
        public int      FreeB;      //Bit representation of candidate digits
        public int      FreeBC{ get{ return FreeB.BitCount(); } }   //Number of candidate digits.

        public int      FixedNo;    //Fixed digit. Obtaine by an algorithm and reflecte to the board status by management processing
        public int      CancelB;    //Digits to exclude(bit representation). Same process as above.

        public List<EColor> ECrLst; //Display color of cell digits. Obtaine by an algorithm.
        public Color    CellBgCr;   //background color. Obtaine by an algorithm
        
        public bool     Selected;   //(Working variable used in algorithm)
        public int      nx;         //(Working variable used in algorithm)

        public UCell( ){}
        public UCell( int rc, int No=0, int FreeB=0 ){
            this.rc=rc; this.r=rc/9; this.c=rc%9; this.b=rc/27*3+(rc%9)/3;
            this.No=No; this.FreeB=FreeB;
            this.ECrLst=null;
        }

        public void Reset_StepInfo(){
            ErrorState =0;
            CancelB    =0;
            FixedNo    =0;
            Selected   =false;

            this.ECrLst=null;
            CellBgCr = Colors.Black;       
        }

        public UCell Copy( ){
            UCell UCcpy=(UCell)this.MemberwiseClone();
            if(this.ECrLst!=null){
                UCcpy.ECrLst=new List<EColor>();
                ECrLst.ForEach(p=>UCcpy.ECrLst.Add(new EColor(p)));
            }
            return UCcpy;
        }

        public void SetCellBgColor( Color CellBgCr ){ 
            if(ECrLst==null)  ECrLst=new List<EColor>();
            ECrLst.Add(new EColor(CellBgCr));
        }

        public void SetNoBColor( int noB, Color cr ){
            if(ECrLst==null)  ECrLst=new List<EColor>();
            ECrLst.Add(new EColor(noB,cr));
        }
        public void SetNoBColorRev( int noB, Color cr ){
            if(ECrLst==null)  ECrLst=new List<EColor>();
            ECrLst.Add(new EColor(noB,cr,cr));
        }
        public void SetNoBBgColor( int noB, Color cr, Color crBg ){
            if(ECrLst==null)  ECrLst=new List<EColor>();
            ECrLst.Add(new EColor(noB,cr));
            ECrLst.Add(new EColor(crBg));
        }
        public void SetNoBBgColorRev( int noB, Color cr, Color crBg ){
            if(ECrLst==null)  ECrLst=new List<EColor>();
            ECrLst.Add(new EColor(noB,cr,cr));
            ECrLst.Add(new EColor(crBg));
        }
        public override string ToString(){
        //    string po = " UCell rc:"+rc+"["+((r+1)*10+(c+1)) +"]  no:"+No;
            string po = $" UCell rc:{rc}[r{r+1}c{c+1}]  no:{No}";
            po +=" FreeB:" + FreeB.ToBitString(9);
            po +=" CancelB:" + CancelB.ToBitString(9);
            return po;
        }
        public void ResetAnalysisResult(){
            CancelB  =0;
            FixedNo  =0;
            Selected =false;
            this.ECrLst=null;
        }
    }
}