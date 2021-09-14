using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GIDOO_space;

namespace GNPXcore {
    public partial class AnalyzerBaseV2{
        static public bool   __SimpleAnalyzerB__;
        static public int    __DebugBreak;
        private const int    S=1, W=2;

        public GNPX_AnalyzerMan pAnMan;
        public List<UCell>   pBDL{ get{ return pAnMan.pGP.BDL; } }
        public bool          SolInfoB{ get{return pAnMan.SolInfoB;} }
        public bool          chbConfirmMultipleCells{   get{return pAnMan.chbConfirmMultipleCells;} }
        public int           SolCode{    set{pAnMan.pGP.SolCode=value;} get{return pAnMan.pGP.SolCode;} }  
        public string        Result{     set{pAnMan.Result=value;} }
        public string        ResultLong{ set{pAnMan.ResultLong=value;} }
		public string		 extRes{     set{pAnMan.pGP.extRes=value;} get{ return pAnMan.pGP.extRes; } }
         
		public SuperLinkMan	 pSprLKsMan{ get{return pAnMan.SprLKsMan;} }
        public CellLinkMan   CeLKMan{    get{return pAnMan.SprLKsMan.CeLKMan;} }
        public ALSLinkMan    ALSMan{     get{return pAnMan.SprLKsMan.ALSMan;} }

        public Bit81[]       Qtrue;
        public Bit81[]       Qfalse;
        public object[,]     chainDesLK;

        static  AnalyzerBaseV2( ){
            Create_ConnectedCells();
            __SimpleAnalyzerB__=false;
        }
        public  AnalyzerBaseV2( ){}
        public  AnalyzerBaseV2( GNPX_AnalyzerMan pAnMan ){ 
            this.pAnMan=pAnMan;
        }

    #region Display Control
        static public readonly string[]    rcbStr=new string[]{ "r", "c", "b" };

        static public readonly Color[] _ColorsLst=new Color[]{
            Colors.LightGreen, Colors.Yellow,
            Colors.Aqua, Colors.MediumSpringGreen, Colors.Moccasin, Colors.YellowGreen, 
            Colors.Pink, Colors.ForestGreen, Colors.Aquamarine, Colors.Beige,
            Colors.Lavender, Colors.Magenta, Colors.Olive, Colors.SlateBlue };

        static public readonly Color AttCr    = Colors.Red;
        static public readonly Color AttCr2   = Colors.Blue;
        static public readonly Color AttCr3   = Colors.Green;
        static public readonly Color SolBkCr  = Colors.Yellow;
        static public readonly Color SolBkCr2 = Colors.LightGreen;//Aqua;//SpringGreen//Colors.CornflowerBlue;  //FIn
        static public readonly Color SolBkCr3 = Colors.Aqua;　　  //Colors.CornflowerBlue;
        static public readonly Color SolBkCr4 = Colors.CornflowerBlue;
    #endregion Display Control  

    #region Connected Cells
        static public Bit81[] ConnectedCells;    //Connected Cells
      //static public Bit81[] ConnectedCellsRev; //Connected Cells Reverse (not use in GNPXcore!!)
        static public Bit81[] HouseCells;        //Row(0-8) Collumn(9-17) Block(18-26)
 
        static private void Create_ConnectedCells(){
            if(ConnectedCells!=null)  return;
            ConnectedCells    = new Bit81[81];
          //ConnectedCellsRev = new Bit81[81];

            for(int rc=0; rc<81; rc++ ){
                Bit81 BS = new Bit81();
                foreach( var q in __IEGetCellsConnectedRC(rc).Where(q=>q!=rc)) BS.BPSet(q);
                BS.BPReset(rc);

                ConnectedCells[rc] = BS;
              //ConnectedCellsRev[rc] = BS ^ 0x7FFFFFF;
            }

            HouseCells = new Bit81[27];
            for(int tfx=0; tfx<27; tfx++ ){
                Bit81 tmp=new Bit81();
                foreach( var q in __IEGetCellInHouse(tfx) ) tmp.BPSet(q);
                HouseCells[tfx] = tmp;
            }
        }
        static private IEnumerable<int> __IEGetCellsConnectedRC( int rc ){ 
            int r=0, c=0;
            for(int kx=0; kx<27; kx++ ){
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
            for(int nx=0; nx<9; nx++ ){
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