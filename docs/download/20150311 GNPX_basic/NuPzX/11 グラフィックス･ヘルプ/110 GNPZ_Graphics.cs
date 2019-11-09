using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GIDOO_space;

//逆引きサンプル コード
//http://msdn.microsoft.com/ja-jp/ff363212#05

//[C#/XAML] WPF でグラフィックスを描画する (Windows フォームから WPF へ)
//http://code.msdn.microsoft.com/windowsdesktop/CVBXAML-WPF-Windows-WPF-0738a600

namespace GNPZ_sdk{
    public class GFont{
        public FontFamily FontFamily;
        public int        FontSize;
        public FontWeight FontWeight;
        public FontStyle  FontStyle;

        public string Name{
            get{ return FontFamily.ToString(); }
        }

        public GFont( string FontName, int FontSize=10 ){
            this.FontFamily =new FontFamily(FontName);
            this.FontSize   = FontSize;
            this.FontWeight = FontWeights.Normal;
            this.FontStyle  = FontStyles.Normal;
        }   

        public GFont( string  FontName, int FontSize, FontWeight FontWeight, FontStyle FontStyle){
            this.FontFamily = new FontFamily(FontName);
            this.FontSize   = FontSize;
            this.FontWeight = FontWeight;
            this.FontStyle  = FontStyle;
        }
    }
    
    public class GFormattedText{
        private CultureInfo CulInfoJpn=CultureInfo.GetCultureInfo("ja-JP");

        private Typeface TFace; //▼
        private GFont    _GFont;
        public GFont gFnt{
            set{
                _GFont = value; 
                TFace  = new Typeface( _GFont.FontFamily, _GFont.FontStyle,
                    _GFont.FontWeight, FontStretches.Medium );
            }
        }

        public GFormattedText( GFont gf ){
            gFnt = gf;
        }

        public FormattedText GFText( string st, Brush br ){
            FormattedText FT = new FormattedText( st, CulInfoJpn, FlowDirection.LeftToRight,
                TFace, _GFont.FontSize, br );                
            return FT;
        }
    }
    
    public class GNPZ_Graphics{
        static private GFont gFnt12 = new GFont("ＭＳ　ゴシック",12,FontWeights.Medium,FontStyles.Normal);
        static private GFormattedText GF8 = new GFormattedText( gFnt12 );
        static private CultureInfo CulInfoJpn=CultureInfo.GetCultureInfo("ja-JP"); 

        private NuPz_Win   pGNP00win;
        private GNumPzl    pGNP00;
        private Dictionary<string,Color> pColorDic;      


        public GNPZ_Graphics( GNumPzl pGNP00 ){
            this.pGNP00 = pGNP00;
            this.pGNP00win = pGNP00.pGNP00win;
            this.pColorDic = GNumPzl.ColorDic;
        }

        public void GBoardPaint( RenderTargetBitmap bmp, UProblem pGP, string GSmode, bool sNoAssist=false ){
            if( pGP==null )  return;

            int   LWid  = pGNP00.lineWidth;
            int   CSiz  = pGNP00.cellSize;
            int   CSizP = CSiz+LWid;

            Brush brBoad = new SolidColorBrush(pColorDic["Board"]);
            Brush brFNo  = new SolidColorBrush(pColorDic["CellForeNo"]);
            Brush brPNo  = new SolidColorBrush(pColorDic["CellBkgdPNo"]);    
            Brush brMNo  = new SolidColorBrush(pColorDic["CellBkgdMNo"]);    
            Brush brZNo  = new SolidColorBrush(pColorDic["CellBkgdZNo"]);    
          
            //ＭＳ 明朝 平成明朝 ＭＳ　ゴシック
            GFont gFnt32 = new GFont( "Courier", 32, FontWeights.DemiBold, FontStyles.Normal );
            GFormattedText GF32 = new GFormattedText( gFnt32 );           
            Point pt = new Point();

            var drawVisual = new DrawingVisual();
            using( DrawingContext DC=drawVisual.RenderOpen() ){
                
                //初期状態
                DC.DrawRectangle(brBoad,null,new Rect(0,0,bmp.Width,bmp.Height));

                if( pGP.BDL.Any(p=>p.No!=0) ){ 
                  #region ボードに数字を描く
                    // 問題セル(No>0) 解読済みセル(No<0)
                    Rect Rrct = new Rect(0,0, CSiz,CSiz);
                    foreach( UCell P in pGP.BDL.Where(p=>p.No!=0) ){
                        int r=P.r, c=P.c;
                        pt.X = LWid + c*CSizP + (c/3);
                        pt.Y = LWid + r*CSizP + (r/3);
                                              
                        Brush br = (P.No>0)? brPNo: brMNo;
                        Rrct.X=pt.X+1; Rrct.Y=pt.Y+1;
                        DC.DrawRectangle(br,null,Rrct);
                  
                        string NoStr = Math.Abs(P.No).ToString();
                        pt.X+=10; pt.Y+=2;
                        DC.DrawText(GF32.GFText(NoStr,brFNo),pt);
                    }
                       
                    //未確定セル
                    if( sNoAssist ){
                        foreach( UCell P in pGP.BDL.Where(p=>p.No==0) ){
                            int r=P.r, c=P.c;
                            pt.X = LWid + c*CSizP + (c/3);
                            pt.Y = LWid + r*CSizP + (r/3);
                            Rrct.X=pt.X+1; Rrct.Y=pt.Y+1;

                            RenderTargetBitmap　Rbmp = CreateCellImage(P,true);
                            DC.DrawImage(Rbmp,Rrct);
                        }
                    }
                  #endregion ボードに数字を描く
                }

              #region ボードに枠を描く
                Brush brBL = new SolidColorBrush(pColorDic["BoardLine"]);
                Pen   pen, pen1=new Pen(brBL,1), pen2=new Pen(brBL,2);               
                Point ptS, ptE;
                int hh=1;
                for( int k=0; k<10; k++ ){
                    ptS=new Point(0,hh); ptE=new Point(CSiz*10-2,hh);
                    pen = ((k%3)==0)? pen2: pen1;
                    DC.DrawLine(pen,ptS,ptE);
                    hh += CSizP + (k%3)/2;
                }

                hh=1;
                for( int k=0; k<10; k++ ){
                    ptS=new Point(hh,0); ptE=new Point(hh,CSiz*10-2);
                    pen = ((k%3)==0)? pen2: pen1;
                    DC.DrawLine(pen,ptS,ptE);
                    hh += CSizP + (k%3)/2;
                }
              #endregion ボードに枠を描く

            }    
            bmp.Clear();        
            bmp.Render(drawVisual);
            return;
        }

        public RenderTargetBitmap CreateCellImage( UCell P, bool candDisp ){
            Color crFore  = pColorDic["CellForeNo"];
            Color crFix   = pColorDic["CellFixed"];
            Color crBgFix = pColorDic["CellBkgdFix"];
            Brush br;
            var bmp = new RenderTargetBitmap(35,35, 96,96, PixelFormats.Default);

            if( P.ECrLst==null )  P.ECrLst=new List<EColor>(); 
       //     if( P.ECrLst!=null && P.ECrLst.Any(p=>p==null) ){
       //         Console.WriteLine(P.ToString()); //△
       //     }

            EColor EC;
            var drawVisual=new DrawingVisual();
            using( var DC=drawVisual.RenderOpen() ){
                Color bgcr=Colors.Black;

                if( P.ECrLst!=null && (EC=P.ECrLst.FindLast(p=>(p.CellBgCr!=Colors.Black)))!=null ){ 
                    bgcr=EC.CellBgCr;
                }

                if( P.FixedNo>0 ){
                    if( P.ECrLst==null ) P.ECrLst=new List<EColor>();
                    P.ECrLst.Add( new EColor((1<<(P.FixedNo-1)),crFix) );
                    bgcr=crBgFix;
                }
                else if( P.CancelB>0 ){
                    if( P.ECrLst==null ) P.ECrLst=new List<EColor>();
                    P.ECrLst.Add( new EColor(P.CancelB,Colors.White,crFix) ); //反転表示
                }

                if( bgcr!=Colors.Black ){
                    br = new SolidColorBrush(bgcr);
                    DC.DrawRectangle( br, null, new Rect( 0,0, bmp.Width, bmp.Height) );
                }

                int dspB=0;
                foreach( int no in P.FreeB.IEGet_BtoNo() ){
                    int    noB=(1<<no), noP=no+1, x=(no%3)*12, y=(no/3)*12;
                    Point  pt = new Point(x+3,y);                   
                    
                    List<EColor> ECrLst=P.ECrLst;
                    if( ECrLst==null ) continue;
                    if( ECrLst.Any(p=>p==null) ){
                        Console.WriteLine(P.ToString());
                    }

                    EC=ECrLst.FindLast(p=>(p.noB&noB)>0);
                    if( EC!=null ){
                        Color crF=crFore;
                        if( EC.Nbgcr!=Colors.Black ){
                            Brush brBg=new SolidColorBrush(EC.Nbgcr);
                            Rect   re =new Rect(x+1,y,12,12); //
                            DC.DrawRectangle( brBg, null,re );
                            crF=Colors.White;
                        }
                        else{ crF=EC.Ncr; }
                        br = new SolidColorBrush(crF);
                        DC.DrawText( GF8.GFText( noP.ToString(), br ), pt );
                        dspB|=(1<<no);
                    }
                }

                if( (dspB=(P.FreeB).DifSet(dspB))>0 ){
                    br = new SolidColorBrush(crFore);
                    foreach( int no in dspB.IEGet_BtoNo() ){
                        int  noB=(1<<no), x=(no%3)*12, y=(no/3)*12;
                        Point pt = new Point(x+3,y);
                        int noP=no+1;
                        DC.DrawText( GF8.GFText( noP.ToString(), br ), pt );
                    }
                }
            }
            bmp.Render(drawVisual);

            return bmp;
        }
        public RenderTargetBitmap CreateCellImageLight( UCell P, int noX ){
            Color crFix   = pColorDic["CellFixed"];
            Color crFree  = pColorDic["CellForeNo"];
            Brush br      = new SolidColorBrush(pColorDic["CellBkgdZNo2"]);
            var bmp = new RenderTargetBitmap(35,35, 96,96, PixelFormats.Default);

            var drawVisual=new DrawingVisual();
            using( var DC=drawVisual.RenderOpen() ){
                DC.DrawRectangle( br, null, new Rect( 0,0, bmp.Width, bmp.Height) );
                foreach( int no in P.FreeB.IEGet_BtoNo() ){
                    int noP=no+1, x=(no%3)*12, y=(no/3)*12;
                    Color cr=(noP==noX)? crFix: crFree; 
                    Point pt = new Point(x+3,y);
                    br = new SolidColorBrush(cr);
                    DC.DrawText( GF8.GFText( noP.ToString(), br ), pt );
                }
            }
            
            bmp.Render(drawVisual);

            return bmp;
        }

        public void GBPatternPaint( Canvas can, /*colorListWPF CRL,*/ int[,] GPat ){
            int   csz  = pGNP00.cellSize/2;
            int   cszP = csz+1;
            Point ptS, ptE;
            Brush brBoad = new SolidColorBrush(pColorDic["Board"]);
            Brush br = new SolidColorBrush(pColorDic["CellBkgdPNo"]);

            can.Children.Clear();
            Rectangle rct = new Rectangle();
            rct.Fill=brBoad; rct.Height=can.Width; rct.Width=can.Height;
            can.Children.Add(rct);

            for( int r=0; r<9; r++ ){
                for( int c=0; c<9; c++ ){
                    if( GPat[r,c]==0 ) continue;
                    Rectangle rcPatt = new Rectangle();
                    rcPatt.Fill=br;
                    ptS= new Point( c*cszP+c/3+2, r*cszP+r/3+2 );
                    rcPatt.Margin=new Thickness(ptS.X, ptS.Y,0.0,0.0);
                    rcPatt.Height=rcPatt.Width=csz;
                    can.Children.Add(rcPatt);
                }
            }

            //===== ボードに枠を描く =====
            Brush brBL = new SolidColorBrush(pColorDic["BoardLine"]);
            int wd, hh;

            hh=1;
            for( int k=0; k<10; k++ ){
                ptS=new Point(0,hh); ptE=new Point(csz*10-4,hh);
                wd = ((k%3)==0)? 2: 1;
                Line Ln=LinePlotter(ptS,ptE,brBL,wd,0);
                can.Children.Add(Ln);
                hh += cszP+(k%3)/2;
            }

            hh=1;
            for( int k=0; k<10; k++ ){
                ptS=new Point(hh,0); ptE=new Point(hh,csz*10-4);
                wd = ((k%3)==0)? 2: 1;
                Line Ln=LinePlotter(ptS,ptE,brBL,wd,0);
                can.Children.Add(Ln);
                hh += cszP+(k%3)/2;
            }

            return;
        }
        public Line LinePlotter( Point ptS, Point ptE, Brush br, int wd/*線の幅*/, int sCode/*線種*/ ){
            try{  
                Line ln = new Line();
                ln.Stroke=br;
                ln.X1=ptS.X; ln.Y1=ptS.Y; ln.X2=ptE.X;  ln.Y2=ptE.Y;
                ln.StrokeThickness = wd;

			    switch(sCode){
				    case 0: break; //直線
				    case 1: ln.StrokeDashArray = new DoubleCollection{4,4}; break;	//点線
				    case 2: ln.StrokeDashArray = new DoubleCollection{4,2,2,2}; break;	//一点鎖線
				    case 3: ln.StrokeDashArray = new DoubleCollection{4,2,2,2,2,2}; break;	//二点鎖線
                    case 4: ln.StrokeDashArray = new DoubleCollection{4,2}; break;	//点線
			    }
                return ln;
            }
            catch( Exception ex ){
                Console.WriteLine( ex.Message );
                Console.WriteLine( ex.StackTrace );
            }
            return null;
		}
    
        public void GBoardPaintPrint( RenderTargetBitmap bmp, UProblem pGP ){
            if( pGP==null )  return;

            int   LWid = pGNP00.lineWidth;
            int   CSiz  = pGNP00.cellSize;
            int   CSizP = CSiz+LWid;
            
            Brush brBoad   = new SolidColorBrush(Colors.White);
            Point ptS, ptE;

            //RenderTargetBitmap Rbmp;
            Rect Rrct = new Rect(0,0, CSiz,CSiz);

          
            //ＭＳ 明朝 平成明朝 ＭＳ　ゴシック
            GFont gFnt32 = new GFont( "Courier", 32, FontWeights.DemiBold, FontStyles.Normal );
            GFormattedText GF32 = new GFormattedText( gFnt32 );

            var drawVisual = new DrawingVisual();
            using( DrawingContext DC=drawVisual.RenderOpen() ){
            
                DC.DrawRectangle( brBoad, null, new Rect(0,0, bmp.Width,bmp.Height) );

                Brush br   = new SolidColorBrush(Colors.Black);
                 Point pt = new Point();

                foreach( UCell BDX in pGP.BDL ){
                    int r = BDX.r;
                    int c = BDX.c;
                     
                    pt.X = c*CSizP + LWid/2 + (c/3);
                    pt.Y = r*CSizP + LWid/2 + (r/3);
                    Rrct.X=pt.X;　Rrct.Y=pt.Y;

                    int No = BDX.No;
                    if( No!=0 ){
                        #region 問題/解読済みセル
                        pt.X += 10; pt.Y += 2;
                        string NoStr = Math.Abs(No).ToString();
                        DC.DrawText( GF32.GFText(NoStr,br), pt );
                        #endregion 問題数字/解読済み数字
                    }
                }

                #region ボードに枠を描く
                Pen pen1 = new Pen(br,1);
                Pen pen2 = new Pen(br,2);
                Pen pen;

                int hh=1;
                for( int k=0; k<10; k++ ){
                    ptS = new Point(0,hh);
                    ptE = new Point(CSiz*10-2,hh);
                    pen = ((k%3)==0)? pen2: pen1;
                    DC.DrawLine(pen,ptS,ptE);
                    hh += CSizP + (k%3)/2;
                }

                hh=1;
                for( int k=0; k<10; k++ ){
                    ptS = new Point(hh,0);
                    ptE = new Point(hh,CSiz*10-2);
                    pen = ((k%3)==0)? pen2: pen1;
                    DC.DrawLine(pen,ptS,ptE);
                    hh += CSizP + (k%3)/2;
                }
                #endregion ボードに枠を描く
            }                  
            
            bmp.Render(drawVisual);

            return;
        }
    }

}
