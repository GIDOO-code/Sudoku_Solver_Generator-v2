using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GIDOO_space;

namespace GNPZ_sdk {
    public partial class AnalyzerBaseV3{
        static public int    __DebugBreak;
        private const int    S=1, W=2;

        public GNPX_AnalyzerManEx pAnMan;
        public List<UCellEx> pBDL{ get{ return pAnMan.pGP.BDL; } }
        public bool          SolInfoB{ get{return pAnMan.SolInfoB;} }
        public int           SolCode{ set{ pAnMan.pGP.SolCode=value;} get{return pAnMan.pGP.SolCode;} }  
        public string        Result{ set{ pAnMan.Result=value;     } }
        
		public CellLinkManEx rCeLKMan;
        public CellLinkManEx CeLKMan{
			get{ 
				if(rCeLKMan==null)	rCeLKMan=new CellLinkManEx(pAnMan);
				return rCeLKMan;
			}
		}
        public ALSLinkManEx rAlsLKMan;
        public ALSLinkManEx ALSMan{
            get{
                if(rCeLKMan==null)	rAlsLKMan=new ALSLinkManEx(pAnMan);
				return rAlsLKMan;
            }
        }

        public Bit81[]       Qtrue;
        public Bit81[]       Qfalse;
        public object[,]     chainDesLK;

        static  AnalyzerBaseV3( ){
            SetConnectedCells();
        }
        public  AnalyzerBaseV3( ){}
        public  AnalyzerBaseV3( GNPX_AnalyzerManEx pAnMan ){ 
            this.pAnMan=pAnMan;
        }

    #region Connected Cells
        static public Bit81[] ConnectedCells;    //Connected Cells
      //static public Bit81[] ConnectedCellsRev; //Connected Cells Reverse (not use in GNPZ_sdk!!)
        static public Bit81[] HouseCells;        //Row(0-8) Collumn(9-17) Block(18-26)
 
        static private void SetConnectedCells(){
            if( ConnectedCells!=null )  return;
            ConnectedCells    = new Bit81[81];
//          ConnectedCellsRev = new Bit81[81];

            for( int rc=0; rc<81; rc++ ){
                Bit81 BS = new Bit81();
                foreach( var q in __IEGetCellsConnectedRC(rc) ) BS.BPSet(q);
                BS.BPReset(rc);
                ConnectedCells[rc]    = BS;
//              ConnectedCellsRev[rc] = BS ^ 0x7FFFFFF;
            }

            HouseCells = new Bit81[27];
            for( int tfx=0; tfx<27; tfx++ ){
                Bit81 tmp=new Bit81();
                foreach( var q in __IEGetCellInHouse(tfx) ) tmp.BPSet(q);
                HouseCells[tfx] = tmp;
            }
        }
        static private IEnumerable<int> __IEGetCellsConnectedRC( int rc ){ 
            int r=0, c=0;
            for( int kx=0; kx<27; kx++ ){
                switch(kx/9){
                    case 0: r=rc/9; c=kx%9; break; //row 
                    case 1: r=kx%9; c=rc%9; break; //collumn
                    case 2: int b=rc/27*3+(rc%9)/3; r=(b/3)*3+(kx%9)/3; c=(b%3)*3+kx%3; break;//block
                }
                yield return r*9+c;
            }
        }
        static private IEnumerable<int> __IEGetCellInHouse( int tfx ){ //nx=0...8
            int r=0, c=0, tp=tfx/9, fx=tfx%9;
            for( int nx=0; nx<9; nx++ ){
                switch(tp){
                    case 0: r=fx; c=nx; break;  //row
                    case 1: r=nx; c=fx; break;  //collumn
                    case 2: r=(fx/3)*3+nx/3; c=(fx%3)*3+nx%3; break;  //block
                }
                yield return (r*9+c);
            }
        }
    #endregion Connected Cells
   
    }
}