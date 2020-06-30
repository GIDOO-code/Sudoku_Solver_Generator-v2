using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Diagnostics.Debug;
using System.Windows.Media;

using GIDOO_space;

namespace GNPXcore {
    public partial class SuperLinkMan{
        public int      SolCode{ set{ pAnMan.pGP.SolCode=value;} get{return pAnMan.pGP.SolCode;} }  
        private Bit81[] ConnectedCells = AnalyzerBaseV2.ConnectedCells;
        private int     _dbCC=0, chkX=0;

        public string __Debug_ChainPrint( GroupedLink P, bool PrintOn=true ){
            GroupedLink Q=P;
            string st="";
            if(Q==null) st+="null";
            else{
                int nc=0;
                do{
                    st = Q.GrLKToString()+st;
                    Q=Q.preGrpedLink as GroupedLink;
                    if(Q==null) break;
                    st = " Å° "+st;
                    if(++nc>20) break;
                }while(true);
            }
            st = $"\tÅü{++_dbCC}Åü{st}";
            if(P.UGCellsB.B81.Count==1) st += "ÅúÅúÅú";
            if(PrintOn)  WriteLine(st);
            return st;
        }

        public  List<GroupedLink> Convert_ChainToList_GNL( USuperLink GNL_Result ){
            if(GNL_Result==null)  return null;

            var SolLst=new List<GroupedLink>();
            GroupedLink GLKnxt = GNL_Result.resultGLK; 

            //===================== convert chain to list ===========================           
            GroupedLink X=GLKnxt;
            while(X!=null && SolLst.Count<50){
                SolLst.Add(X);
                X=X.preGrpedLink as GroupedLink;
            } 
            SolLst.Reverse();

            return SolLst;
        }          

        private bool _IsLooped(GroupedLink P){
            UGrCells PB=P.UGCellsB;
            var Q=P.preGrpedLink as GroupedLink;
            int nc=0;
            while(Q!=null){
                UGrCells QA=Q.UGCellsA;
                if(QA.Equals(PB)) return true;
                Q=Q.preGrpedLink as GroupedLink;
                if(++nc>20)  return true;           //Å•Å•Å•Å• ÉoÉOÇ†ÇË Å•Å•Å•Å•
            }
            return false;
        }

		public  void developDisp( int rc0, int no0, USuperLink USLK, bool DevelopB ){
            if( USLK==null || USLK.Qtrue==null )  return;
            List<UCell> qBDL=new List<UCell>();
            pBDL.ForEach(p=>qBDL.Add(p.Copy()));

            var pQtrue =USLK.Qtrue;
            var pQfalse=USLK.Qfalse;
            var pchainDesLKT=USLK.chainDesLKT;
            var pchainDesLKF=USLK.chainDesLKF;

            foreach( var P in qBDL ){
                if(P.No!=0)  continue;
                int noT=0, noF=0;
                foreach( var no in P.FreeB.IEGet_BtoNo()){
                    if( pQtrue[no].IsHit(P.rc) )  noT|=(1<<no);
                    if( pQfalse[no].IsHit(P.rc) ) noF|=(1<<no);
                }
                if(noT>0) P.SetNoBBgColor(noT, Colors.Red, Colors.LemonChiffon  );
                if(noF>0) P.SetNoBBgColorRev(noF, Colors.Red, Colors.LemonChiffon  );
                if((noT&noF)>0){
                    P.SetNoBBgColor(noT&noF, Colors.White , Colors.PowderBlue );
                    P.SetNoBColorRev(noT&noF,Colors.Blue );
                }
            }
            qBDL[rc0].SetNoBBgColor(1<<no0, Colors.Red, Colors.Yellow);

            devWin.Set_dev_GBoard( qBDL, dispOn:false );

            if(DevelopB){
				string stMsg="";
                foreach( var P in pBDL ){
                    if(P.No!=0) continue;
                    foreach( var no in P.FreeB.IEGet_BtoNo() ){
                        if( pQfalse[no].IsHit(P.rc) && pQtrue[no].IsHit(P.rc) ){
                            WriteLine("------------error");
                        }
						string st= _GenMessage( USLK, P, no);
                        if(st.Length>4){
							WriteLine(st);
							stMsg += st;
						}
                    }
                }
				USLK.stMsg=stMsg;
            }
        }
		private string _GenMessage( USuperLink USLK, UCell P, int no ){
			string st="";
            var pQtrue =USLK.Qtrue;
            var pQfalse=USLK.Qfalse;
            var pchainDesLKT=USLK.chainDesLKT;
            var pchainDesLKF=USLK.chainDesLKF;

            if( pQfalse[no].IsHit(P.rc) ){
                GroupedLink Pdes=(GroupedLink)pchainDesLKF[P.rc,no];
                st += _chainToString(P,Pdes,-(no+1))+"\r";
            }
            if( pQtrue[no].IsHit(P.rc) ){
                GroupedLink Pdes=(GroupedLink)pchainDesLKT[P.rc,no];
                st += _chainToString(P,Pdes,no+1)+"\r";
            }
			return st;
		}
		private string _chainToString( UCell U, GroupedLink Gdes, int noRem ){
            string st="";
            if(Gdes==null)  return st;
            var Qlst=new List<GroupedLink>();

            var X=Gdes;
            while(X!=null){
                Qlst.Add(X);
                X=X.preGrpedLink as GroupedLink;
                if(Qlst.Count>10) break; //error
            }
            Qlst.Reverse();
            string pm=((noRem>0)? "+":"")+noRem;
            st = "r"+(U.r+1)+"c"+(U.c+1)+"("+U.rc+")/"+pm+" ";
            foreach( var R in Qlst ){ st += R.GrLKToString()+ " => "; };
            st = st.Substring(0,st.Length-4);

            if(Qlst.Count>20) st=" ##loop? error##"+st;
            return st;
        }
    }
}