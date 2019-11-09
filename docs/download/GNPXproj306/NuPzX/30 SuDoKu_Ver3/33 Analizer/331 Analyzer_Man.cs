using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Media;
using System.Threading;
using static System.Console;

using static System.Math;

namespace GNPZ_sdk{
    public class UAlgMethodEx{
        static private int ID0=0;
        public int        ID;
        public string     MethodName;
        public int        DifLevel; 
        public dSolver    Method;
        public bool       GenLogB;
        public int        UsedCC=0;
        public bool       IsChecked=true;
        public bool       IsEnabled=true;

        public UAlgMethodEx( int pid, string MethodName, int DifLevel, dSolver Method, bool GenLogB=false ){
          //this.ID         = (ID<<16)+(ID0++); //stableSort
            this.ID         = pid*1000+(ID0++); //stableSort
            this.MethodName = MethodName;
            this.DifLevel   = DifLevel;
            this.Method     = Method;
            this.GenLogB    = GenLogB;
        }
        public override string ToString(){
            string st=MethodName.PadRight(30)+"["+DifLevel+"]"+" "+UsedCC;
            st += "GeneralLogic:"+GenLogB.ToString();
            return st;
        }
    }

    public class GNPX_AnalyzerManEx{
        public GNPZ_EnginEx pENGN;
        public UProblemEx   pGP{        get{return pENGN.pGP;} }
        public int			GStage=0;

        public bool         Insoluble;
        public List<UCellEx> pBDL{      get{return pENGN.pGP.BDL;} }
        public bool         SolInfoB{   get{return GNPZ_Engin.SolInfoB;} }

        public int          SolCode{    set{pGP.SolCode=value;} get{return pGP.SolCode;} }
        public string       Result{     set{pGP.Sol_Result=value;} }

        public List<UAlgMethod>  SolverLst0;

        public  GNPX_AnalyzerManEx( GNPZ_EnginEx pENGN ){
            SolverLst0 = new List<UAlgMethod>();
            this.pENGN=pENGN;

            var SSingle=new SimpleSingleGenEx(this);
            SolverLst0.Add( new UAlgMethod( 1, "LastDigit",    1, SSingle.LastDigitEx ) );
            SolverLst0.Add( new UAlgMethod( 2, "NakedSingle",  1, SSingle.NakedSingleEx ) );
            SolverLst0.Add( new UAlgMethod( 3, "HiddenSingle", 1, SSingle.HiddenSingleEx ) );

            var LockedCand=new LockedCandidateGenEx(this);
            SolverLst0.Add( new UAlgMethod( 5, "LockedCandidate", 2, LockedCand.LockedCandidate ) );
                
            var LockedSet=new LockedSetGenEx(this);
            SolverLst0.Add( new UAlgMethod( 10, "LockedSet(2D)",        2, LockedSet.LockedSet2 ) );
            SolverLst0.Add( new UAlgMethod( 12, "LockedSet(3D)",        3, LockedSet.LockedSet3 ) );
            SolverLst0.Add( new UAlgMethod( 14, "LockedSet(4D)",        4, LockedSet.LockedSet4 ) );
            SolverLst0.Add( new UAlgMethod( 16, "LockedSet(5D)",       -5, LockedSet.LockedSet5 ) );
            SolverLst0.Add( new UAlgMethod( 18, "LockedSet(6D)",       -5, LockedSet.LockedSet6 ) );
            SolverLst0.Add( new UAlgMethod( 20, "LockedSet(7D)",       -5, LockedSet.LockedSet7 ) );           
            SolverLst0.Add( new UAlgMethod( 11, "LockedSet(2D)Hidden",  2, LockedSet.LockedSet2Hidden ) );           
            SolverLst0.Add( new UAlgMethod( 13, "LockedSet(3D)Hidden",  3, LockedSet.LockedSet3Hidden ) );          
            SolverLst0.Add( new UAlgMethod( 15, "LockedSet(4D)Hidden",  4, LockedSet.LockedSet4Hidden ) );
            SolverLst0.Add( new UAlgMethod( 17, "LockedSet(5D)Hidden", -5, LockedSet.LockedSet5Hidden ) );
            SolverLst0.Add( new UAlgMethod( 19, "LockedSet(6D)Hidden", -5, LockedSet.LockedSet6Hidden ) );            
            SolverLst0.Add( new UAlgMethod( 21, "LockedSet(7D)Hidden", -5, LockedSet.LockedSet7Hidden ) );

            var Fish=new FishGenEx(this);
            SolverLst0.Add( new UAlgMethod( 30, "XWing",            3, Fish.XWing ) );
            SolverLst0.Add( new UAlgMethod( 31, "SwordFish",        4, Fish.SwordFish ) );
            SolverLst0.Add( new UAlgMethod( 32, "JellyFish",        5, Fish.JellyFish ) );
            SolverLst0.Add( new UAlgMethod( 33, "Squirmbag",       -5, Fish.Squirmbag ) );
            SolverLst0.Add( new UAlgMethod( 34, "Whale",           -5, Fish.Whale ) );
            SolverLst0.Add( new UAlgMethod( 35, "Leviathan",       -5, Fish.Leviathan ) );

            SolverLst0.Add( new UAlgMethod( 40, "Finned XWing",     4, Fish.FinnedXWing ) );
            SolverLst0.Add( new UAlgMethod( 41, "Finned SwordFish", 5, Fish.FinnedSwordFish ) );
            SolverLst0.Add( new UAlgMethod( 42, "Finned JellyFish", 5, Fish.FinnedJellyFish ) );
            SolverLst0.Add( new UAlgMethod( 43, "Finned Squirmbag", 6, Fish.FinnedSquirmbag ) );
            SolverLst0.Add( new UAlgMethod( 44, "Finned Whale",     6, Fish.FinnedWhale ) );
            SolverLst0.Add( new UAlgMethod( 45, "Finned Leviathan", 6, Fish.FinnedLeviathan ) );

            SolverLst0.Add( new UAlgMethod( 90, "Franken/MutantFish",         7, Fish.FrankenMutantFish ) );
            SolverLst0.Add( new UAlgMethod( 91, "Finned Franken/Mutant Fish", 7, Fish.FinnedFrankenMutantFish ) );

            var CellLink=new CellLinkGenEx(this);
            SolverLst0.Add( new UAlgMethod( 50, "Skyscraper",       4, CellLink.Skyscraper ) );
            SolverLst0.Add( new UAlgMethod( 51, "EmptyRectangle",   4, CellLink.EmptyRectangle ) );
            SolverLst0.Add( new UAlgMethod( 52, "XY-Wing",          5, CellLink.XYwing ) );
            SolverLst0.Add( new UAlgMethod( 53, "W-Wing",           6, CellLink.Wwing ) );

            SolverLst0.Add( new UAlgMethod( 55, "RemotePair",       5, CellLink.RemotePair ) );    
            SolverLst0.Add( new UAlgMethod( 56, "XChain",           6, CellLink.XChain ) );
            SolverLst0.Add( new UAlgMethod( 57, "XYChain",          6, CellLink.XYChain ) ); 
            
            SolverLst0.Add( new UAlgMethod( 60, "Color-Trap",       5, CellLink.Color_Trap ) );
            SolverLst0.Add( new UAlgMethod( 61, "Color-Wrap",       5, CellLink.Color_Wrap ) );
            SolverLst0.Add( new UAlgMethod( 62, "MultiColor-Type1", 6, CellLink.MultiColor_Type1 ) );
            SolverLst0.Add( new UAlgMethod( 63, "MultiColor-Type2", 6, CellLink.MultiColor_Type2 ) );

            var ALSTechP=new AALSTechGenEx(this);  //fakeALS(2ŽŸALS)
            SolverLst0.Add( new UAlgMethod( 59, "SueDeCoq",         5, ALSTechP.SueDeCoq ) );  

            var ALSTech=new ALSTechGenEx(this);
            SolverLst0.Add( new UAlgMethod( 75, "XYZ-WingALS",         7, ALSTech.XYZwingALS ) );
            SolverLst0.Add( new UAlgMethod( 80, "ALS-XZ",              7, ALSTech.ALS_XZ ) );
            SolverLst0.Add( new UAlgMethod( 81, "ALS-XY-Wing",         8, ALSTech.ALS_XY_Wing ) );
            SolverLst0.Add( new UAlgMethod( 82, "ALS-Chain",           9, ALSTech.ALS_Chain ) );
            SolverLst0.Add( new UAlgMethod( 83, "ALS-DeathBlossom",    9, ALSTech.ALS_DeathBlossom ) );
            SolverLst0.Add( new UAlgMethod( 83, "ALS-DeathBlossomExt", 9, ALSTech.ALS_DeathBlossomExt ) );

            SolverLst0.Sort((a,b)=>(a.ID-b.ID));
        }

        public void SolversInitialize(){ }
 
//==========================================================

        public bool AggregateCellsPZM( ref int nP, ref int nZ,ref int nM ){
            int P=0, Z=0, M=0;
            if( pBDL==null )  return false;
            pBDL.ForEach( q =>{
                if(q.No>0)      P++;
                else if(q.No<0) M++;
                else            Z++;
            } );
            nP=P; nZ=Z; nM=M;
            return pBDL.Any(q=>q.FreeB>0);
        }

        private int[] NChk=new int[27];
        public bool FixOrEliminate_SuDoKu( ){//Confirmation process
            if( pBDL.Any(p=>p.FixedNo>0) ){
                foreach( var P in pBDL.Where(p=>p.No==0) ){
                    int No=P.FixedNo;
                    if(No<1 || No>9) continue;
                    P.FixedNo=0; P.No=-No;
                }
                
                Set_CellFreeB(false);
                foreach( var P in pBDL.Where(p=>(p.No==0 && p.FreeBC==0)) )  P.ErrorState=9;

                for( int h=0; h<27; h++ ) NChk[h]=0;
                foreach( var P in pBDL ){
                    int no=(P.No<0)? -P.No: P.No;
                    int H=(no>0)? (1<<(no-1)): P.FreeB;
                    NChk[P.r]|=H; NChk[P.c+9]|=H; NChk[P.b+18]|=H;
                }
                for( int h=0; h<27; h++ ){
                    if(NChk[h]!=0x1FF){ SolCode=-9119; return false; }
                }
            }
            else if( pBDL.Any(p=>p.CancelB>0) ){
                foreach( var P in pBDL.Where(p=>p.CancelB>0) ){
                    int CancelB=P.CancelB^0x1FF;
                    P.FreeB &= CancelB; P.CancelB=0;       
                }
            }
            else{ return false; }  //No solution

            SolCode=-1;
            return true;
        }

        public void Set_CellFreeB( bool allFlag=true ){
            Insoluble=false;
            foreach( var P in pBDL ){
                int freeB=0;
                if( P.No==0 ){
                    foreach( var Q in pBDL.IEGetFixed_Pivot27(P.rc) ) freeB |= (1<<Abs(Q.No));
                    freeB=(freeB>>=1)^0x1FF; //internal representation with 1 right bit shift
                    if( !allFlag ) freeB &= P.FreeB;
                    if( freeB==0 ){ Insoluble=true; P.ErrorState=1; }//No solution
                }
                P.FreeB=freeB;
            }
        }
        public bool VerifyRoule_SuDoKu( ){
            bool    ret=true;

            if( Insoluble==true ){ SolCode=9; return false; }

            for( int tfx=0; tfx<27; tfx++ ){
                int usedB=0, errB=0;
                foreach( var P in pBDL.IEGetCellInHouse(tfx).Where(Q=>Q.No!=0) ){
                    int no=Abs(P.No);
                    if( (usedB&(1<<no))!=0 ) errB |= (1<<no);
                    usedB |= (1<<no);
                }

                if(errB==0) continue;
                foreach( var P in pBDL.IEGetCellInHouse(tfx).Where(Q=>Q.No!=0) ){
                    int no=Abs(P.No);
                    if( (errB&(1<<no))!=0 ){ P.ErrorState=8; ret=false; }
                }
            }
            SolCode = ret? 0: 9; //99:anti-rule
            return ret;
        }
    }
}