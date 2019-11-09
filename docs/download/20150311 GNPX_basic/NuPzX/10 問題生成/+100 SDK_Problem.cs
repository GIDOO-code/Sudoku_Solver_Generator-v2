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
  #region UProblem(問題class)

    public class UProblem{
        public int     ID;
        public long    HTicks;
        public string  Name; 
        public string  TimeStamp;
        public string  solMessage;

        public int     DifLevelT;    //-1:初期状態　0:手動作成
        public bool    Insoluble;    //解なし

        public List<UCell> BDL;

        public int     difLevel;
        public string  GNPZ_Result;           //可能解探索用に追加 20140730
        public string  GNPZ_ResultLong;       //可能解探索用に追加 20140730
        public string  GNPZ_AnalyzerMessage;  //可能解探索用に追加 20140730
        public int     SolCode;
   
        public UProblem( ){
            ID=-1;
            BDL = new List<UCell>();
            for( int rc=0; rc<81; rc++ ) BDL.Add(new UCell(rc));
            this.DifLevelT = 0;
            HTicks=DateTime.Now.Ticks;
        }
        public UProblem( string Name ): this(){ this.Name=Name; }

        public UProblem( List<UCell> BDL ){
            this.BDL  =  BDL;
            this.DifLevelT = 0;
            HTicks=DateTime.Now.Ticks;
        }
        public UProblem( int ID, List<UCell> BDL, string Name="", int DifLvl=0 ){
            this.ID = ID;
            this.BDL = BDL;
            this.Name = Name;
            this.DifLevelT = DifLvl;
            HTicks=DateTime.Now.Ticks;
        }

        public UProblem Copy( ){
            UProblem P = (UProblem)this.MemberwiseClone();
            P.BDL = new List<UCell>();
            foreach( var q in BDL ) P.BDL.Add(q.Copy());
            P.HTicks=DateTime.Now.Ticks;;
            return P;
        }

        public string ToLineString(){
            string st = BDL.ConvertAll(q=>Math.Max(q.No,0)).Connect("").Replace("0",".");
            st += ", " + (ID+1) + "  ,\"" + Name + "\"";
            st += ", " + DifLevelT.ToString();
            st += ", \"" + TimeStamp +  "\"";
            return st;
        }
        public string ToGridString( bool SolSet ){
            string st = (ID+1).ToString() + ", \"" + Name + "\"";
            st += ", " + DifLevelT.ToString();
            st += ","+ solMessage+"\r";

            BDL.ForEach( P =>{
                st+=(SolSet? P.No: Math.Max(P.No,0)); //+Shift：全数字(+-)
                if( P.c==8 ) st+="\r";
                else if( P.rc!=80 ) st+=",";
            } );    
            return st;
        }
    }
  #endregion UProblem(問題class)
    public class EColor{
        public Color CellBgCr=Colors.Black;
        public int   noB;
        public Color Ncr=Colors.Black;
        public Color Nbgcr=Colors.Black;
        public EColor( Color CellBgCr ){ this.CellBgCr=CellBgCr; }
        public EColor( int noB, Color Ncr ){ this.noB=noB; this.Ncr=Ncr;  }
        public EColor( int noB, Color Ncr, Color Nbgcr ){ this.noB=noB; this.Ncr=Ncr; this.Nbgcr=Nbgcr; }
    }
  #region MethodInfo
    public class MethodInfo{
        public string methodName;
        public int    methodCode;
        public int    lvlValue;

        public MethodInfo( int mCode, string mthdName, int lV ){
            methodCode = mCode;
            methodName = mthdName;
            lvlValue   = lV;
        }
    }

    public class methodCouter{
        public int    counter;
        public double DifLevelT;
        public methodCouter( ){
            counter   = 0;
            DifLevelT = 0.0;
        }
        public int counterPlus(int np ){
            counter += np;
            return counter;
        }
        public void counterReset( ){
            counter = 0;
            DifLevelT = 0.0;
        }
    }
  #endregion MethodInfo

}