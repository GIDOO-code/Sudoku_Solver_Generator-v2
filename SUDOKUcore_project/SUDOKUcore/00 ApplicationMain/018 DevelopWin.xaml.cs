using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace GNPXcore{
    using pRes=Properties.Resources;
    public partial class DevelopWin: Window{
        private NuPz_Win            pGNP00win;
        public  GNPZ_Graphics       SDKGrp;
        private RenderTargetBitmap  bmpGZero;
        private List<UCell>         ppBDL;

        public DevelopWin( NuPz_Win pGNP00win ){
            InitializeComponent();
            GNPXGNPX.Content = "GNPXcore "+DateTime.Now.Year;
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();
            
            SuperLinkMan.devWin = this;
            this.pGNP00win = pGNP00win;           

            SDKGrp = new GNPZ_Graphics(pGNP00win.GNP00);
            bmpGZero = new RenderTargetBitmap((int)dev_GBoard.Width,(int)dev_GBoard.Height, 96,96, PixelFormats.Default);
        }

        private void devWinClose_Click(object sender, RoutedEventArgs e){
            this.Hide();
        }
        public  void Set_dev_GBoard( List<UCell> pBDL, bool dispOn=false ){
            ppBDL = pBDL;
            if(dispOn){
                SDKGrp.GBoardPaint( bmpGZero, pBDL, sNoAssist:true );
                dev_GBoard.Source = bmpGZero;
            }
        }
        private void devWin_IsVisibleChanged( object sender, DependencyPropertyChangedEventArgs e ){
            if(ppBDL==null)  return;
            SDKGrp.GBoardPaint( bmpGZero, ppBDL, sNoAssist:true );
            dev_GBoard.Source = bmpGZero;
        }
		private void SaveBitMap_Click( object sender, RoutedEventArgs e ){
            BitmapEncoder enc = new PngBitmapEncoder(); // JpegBitmapEncoder(); BmpBitmapEncoder();
            BitmapFrame bmf = BitmapFrame.Create(bmpGZero);
            enc.Frames.Add(bmf);
            try{
                Clipboard.SetData(DataFormats.Bitmap,bmf);
            }
            catch(System.Runtime.InteropServices.COMException){ /* NOP */ }

            if( !Directory.Exists(pRes.fldSuDoKuImages) ){ Directory.CreateDirectory(pRes.fldSuDoKuImages); }
            string fName=DateTime.Now.ToString("yyyyMMdd HHmmss Develop")+".png";
            using( Stream stream = File.Create(pRes.fldSuDoKuImages+"/"+fName) ){
                enc.Save(stream);
            }                 
        }
    }
}
