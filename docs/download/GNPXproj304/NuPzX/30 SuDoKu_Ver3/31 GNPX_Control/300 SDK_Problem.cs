using System;
using System.Collections.Generic;
using static System.Math;

using GIDOO_space;

namespace GNPZ_sdk{
    public class UProblemEx{
        public int         IDm;
        public int         IDmp1{get{return (IDm+1);}}
        public int         ID;
        public List<UCellEx> BDL;

        public int        DifLevel{ get; set; }    //-1:InitialState　0:Manual
        public string     Name{ get; set; }
        public string     Sol_Result{ get; set; }
        public int        SolCode;
   
        public UProblemEx( ){ }

        public UProblemEx( List<UCellEx> BDL ){
            this.BDL=BDL; this.DifLevel=0;
        }

        public string ToLineString(){
            string st = BDL.ConvertAll(q=>Max(q.No,0)).Connect("").Replace("0",".");
            st += ", " + (ID+1)+" difLvl:"+DifLevel.ToString();
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