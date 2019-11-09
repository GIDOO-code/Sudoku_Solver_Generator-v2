using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading;
using System.Diagnostics;
using static System.Console;

using static System.Math;

namespace GNPZ_sdk{
    public partial class GNPZ_EnginEx{
      //# main control
        private GNPXApp000       pGNP00;

      //# puzzle
        public UProblemEx        pGP=null;

      //# analizer(methods) 
        public  GNPX_AnalyzerManEx AnManEx;
        private List<UMthdChked> pGMthdLst{ get{ return pGNP00.SolverLst2; } }
        public  List<UAlgMethod> MethodLst_Run=new List<UAlgMethod>();

      //# result
        public int               retCode;               

      //# for debug                  
        //private bool             __ChkPrint__=false;
      //----------------------------------------------------------------------

        public GNPZ_EnginEx( GNPXApp000 pGNP00 ){
            this.pGNP00=pGNP00;
            AnManEx=new GNPX_AnalyzerManEx(this);
        }

        public void SetGP( UProblemEx pGP ){
            this.pGP=pGP;
            AnManEx.Set_CellFreeB();
        }

        public int Set_MethodLst_Run( bool AllMthd=false ){
            MethodLst_Run.Clear(); 
            foreach( var S in pGMthdLst ){
                if( !AllMthd && !S.IsChecked )  continue;
                var Sobj = AnManEx.SolverLst0.Find(P=>P.MethodName==S.Name );
                if(Sobj!=null)  MethodLst_Run.Add(Sobj);
            }
            return MethodLst_Run.Count;
        }

        public void sudokuSolver_AutoEx( ){
            retCode=0;
            AnManEx.Set_CellFreeB();
            pGP.DifLevel=-1;
            while(true){
                if( !Analyzer_1stage_Ex() ) break;
                if( !AnManEx.FixOrEliminate_SuDoKu() ){ retCode=-998; return; }
            }
     
            int  nP=0, nZ=0, nM=0;
            AnManEx.AggregateCellsPZM(ref nP,ref nZ,ref nM);
            retCode=nZ;
        }

        public bool Analyzer_1stage_Ex( ){
            try{
                int   lvlLow=SDK_Ctrl.lvlLow, lvlHgh=SDK_Ctrl.lvlHgh;
                AnManEx.SolversInitialize();

                //int  mCC=0;            
                do{
                    if( !AnManEx.VerifyRoule_SuDoKu() ) return false;
					if( AnManEx.pBDL.All(p=>(p.FreeB==0)) ) break;

                    pGP.SolCode=-1;
                    foreach( var P in MethodLst_Run ){
                        int lvl=P.DifLevel; 
                        if( Abs(lvl)>lvlHgh || lvl<0 )  continue;
                        //if( __ChkPrint__ ) WriteLine( $"---> method{(mCC++)} :{P.MethodName}");

                        if( P.Method() ){
                            if(pGP.DifLevel<=P.DifLevel){ 
                                pGP.DifLevel=P.DifLevel; pGP.Name=P.MethodName;
                            }
                            //if( __ChkPrint__ ) WriteLine( $"========================> solved {P.MethodName}" );
                            return true; //step solved
                        }
                     }
                    return false;

                }while(false);
            }
            catch( Exception ex ){
                WriteLine( ex.Message+"\r"+ex.StackTrace );
            }        
            return false;  
        }
    }
}