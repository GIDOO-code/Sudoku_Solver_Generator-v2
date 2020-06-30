using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VisualPrint;
using static System.Diagnostics.Debug;

namespace GNPXcore{
    public partial class SDK_PuzzlePrinter: Window{
        private NuPz_Win pGNP00win;
        public SDK_PuzzlePrinter(){
            InitializeComponent();
        }

        public void SDK_PrintDocument( GNPXApp000 GNP00, int mLow, int mHigh, int mStart, int mEnd, bool SortF/*, colorList crList*/ ){
            this.pGNP00win = GNP00.pGNP00win;
            GNPZ_Graphics  SDKGrp=new GNPZ_Graphics(GNP00);       
            int lvl;

            List<UPuzzle> SDKPList = new List<UPuzzle>();
            foreach( var P in GNP00.SDKProbLst ){
                lvl = P.DifLevel;
                int n = P.ID;
                if( lvl<mLow || lvl>mHigh || n<mStart || n>mEnd )    continue;
                SDKPList.Add(P);
            }
            if( SortF ) SDKPList.Sort( (pa,pb)=>(pa.DifLevel-pb.DifLevel) );

            WrapPanel WrapPan = new WrapPanel();
            WrapPan.Width = 800;
            
            //【TBD】Printing of multiple pages can not be controlled。
            var m_PrtDlg = new PrintDialog();
            double GrdWidth  = (m_PrtDlg.PrintableAreaWidth-48.0*2)/2.0;
            double GrdHeight = (m_PrtDlg.PrintableAreaHeight-48.0*2)/2.0;
            
            foreach( UPuzzle P in SDKPList ){
                try{                
                    Grid Grd = new Grid( );
                    Grd.Width=GrdWidth;
                    Grd.Height=GrdHeight;

                    Label Lblname=new Label();
                    Lblname.Content = "["+P.ID+ "]  " + P.Name;
                    Lblname.FontSize=16;
                    Lblname.Margin=new Thickness();
                    Grd.Children.Add(Lblname);

                    Label Lbldif=new Label();
                    Lbldif.Content = "Dif:"+P.DifLevel;
                    Lbldif.FontSize=14;
                    Lbldif.HorizontalAlignment=HorizontalAlignment.Right;
                    Lbldif.VerticalAlignment  =VerticalAlignment.Top;
                    Lbldif.Margin=new Thickness(0,10,8,0);
                    Grd.Children.Add(Lbldif);

                    var drwVis = new RenderTargetBitmap(338,338, 96,96, PixelFormats.Default); //338
                    SDKGrp.GBoardPaintPrint( drwVis, P );
                    Image Img = new Image();
                    Img.Source = drwVis;
                    Img.Margin=new Thickness(0,35,10,0);
                    Img.HorizontalAlignment=HorizontalAlignment.Left;
                    Img.VerticalAlignment=VerticalAlignment.Top;
                    Grd.Children.Add(Img);

                    WrapPan.Children.Add(Grd);
                }
                catch( Exception ex ){
                    WriteLine( ex.Message );
                    WriteLine( ex.StackTrace );
                }
            }
                        
            VisualPrintDialog printDlg = new VisualPrintDialog( WrapPan );
            printDlg.ShowDialog();
        }
    }
}
