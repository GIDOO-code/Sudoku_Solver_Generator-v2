using System;
using System.Collections.Generic;
using static System.Math;

using GIDOO_space;

namespace GNPZ_sdk{
    public class UProbS{
        public int        IDmp1{ get; set; }
        public int        DifLevel{ get; set; }
        public string     Sol_Result{ get; set; }
        public string     Sol_ResultLong{ get; set; }
        public UProbS( int IDmp1, int DifLevel, string Sol_Result, string Sol_ResultLong ){
            this.IDmp1 = IDmp1; this.DifLevel = DifLevel;
            this.Sol_Result = string.Copy(Sol_Result);
            this.Sol_ResultLong = string.Copy(Sol_ResultLong);
        }
        public UProbS(UProblem P){
            this.IDmp1=P.IDmp1; this.DifLevel=P.DifLevel;
            this.Sol_Result=P.Sol_Result; this.Sol_ResultLong=P.Sol_ResultLong;
        }
    }

    public class UProbMan{
        static public GNPZ_Engin pGNPX_Eng;   
        public List<UProblem> MltUProbLst=null;
        public int        stageNo;
        public UProblem   pGPsel=null;
        public UProbMan   GPMpre=null;
        public UProbMan   GPMnxt=null; //(next version)

        public UProbMan( int stageNo ){
            this.stageNo=stageNo;
            MltUProbLst = new List<UProblem>();
        }

        public bool CreateNextStage(){
            if(pGPsel==null)  return true;
            UProbMan Q=new UProbMan(stageNo+1);
            Q.GPMpre=this; this.GPMnxt=Q;
            pGNPX_Eng.pGP=pGPsel.Copy(stageNo+1,0);
            SDK_Ctrl.UGPMan=Q;
            return false;
        }
    }

    public class UProblem{
        public int         IDm;
        public int         IDmp1{get{return (IDm+1);}}
        public int         ID;
        public List<UCell> BDL;
        public int[]       AnsNum;
        public bool?       IsSelected;

        public long        HTicks;
        public string      Name; 
        public string      TimeStamp;

        public int         DifLevel{ get; set; }    //-1:InitialState　0:Manual
        public bool        Insoluble;   //No solution

        public int         stageNo;
        public UAlgMethod  pMethod=null;
        public string      solMessage;
        public string      Sol_Result{ get; set; }
        public string      Sol_ResultLong;
        public string      __SolRes;
        public string      GNPX_AnalyzerMessage;
		public string      extRes{ get; set; }
        public int         SolCode;
   
        public UProblem( ){
            ID=-1;
            BDL = new List<UCell>();
            for( int rc=0; rc<81; rc++ ) BDL.Add(new UCell(rc));
            this.DifLevel = 0;
            HTicks=DateTime.Now.Ticks;
        }
        public UProblem( string Name ): this(){ this.Name=Name; }

        public UProblem( List<UCell> BDL ){
            this.BDL      = BDL;
            this.DifLevel = 0;
            HTicks=DateTime.Now.Ticks;
        }
        public UProblem( UProblemEx UPex ): this(){
            for( int rc=0; rc<81; rc++ ) BDL[rc].No = UPex.BDL[rc].No;
            this.DifLevel = UPex.DifLevel;
            this.Sol_Result = UPex.Sol_Result;
            this.Name = UPex.Name;
            HTicks=DateTime.Now.Ticks;
        }
        public UProblem( int ID, List<UCell> BDL, string Name="", int DifLvl=0, string TimeStamp="" ){
            this.ID       = ID;
            this.BDL      = BDL;
            this.Name     = Name;
            this.DifLevel = DifLvl;
            this.TimeStamp = TimeStamp;
            HTicks=DateTime.Now.Ticks;
        }

        public UProblem Copy( int stageNo, int IDm ){
            UProblem P = (UProblem)this.MemberwiseClone();
            P.BDL = new List<UCell>();
            foreach( var q in BDL ) P.BDL.Add(q.Copy());
            P.HTicks=DateTime.Now.Ticks;;
            P.stageNo=this.stageNo+1;
            P.IDm=IDm;
            return P;
        }

        public string ToLineString(){
            string st = BDL.ConvertAll(q=>Max(q.No,0)).Connect("").Replace("0",".");
            st += ", " + (ID+1) + "  ,\"" + Name + "\"";
            st += ", " + DifLevel.ToString();
            st += ", \"" + TimeStamp +  "\"";
            return st;
        }
        public string CopyToBuffer(){
            string st = BDL.ConvertAll(q=>Max(q.No,0)).Connect("").Replace("0",".");
            return st;
        }
        public string ToGridString( bool SolSet ){
            string st="";
            BDL.ForEach( P =>{
                st+=(SolSet? P.No: Max(P.No,0));
                if( P.c==8 ) st+="\r";
                else if( P.rc!=80 ) st+=",";
            } );
            return st;
        }
    }
}