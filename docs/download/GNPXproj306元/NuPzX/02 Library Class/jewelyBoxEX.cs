using System;
using System.Collections.Generic;
using static System.Console;
using System.Globalization;

using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using System.Resources;

namespace GIDOO_space{
    static public class GNPZExtender{
        static int[] __BC=new int[512];

        static GNPZExtender(){
            for( int n=0; n<512; n++ ) __BC[n] = (n+512).BitCount()-1;
        }

        static public void Swap<T>( ref T a, ref T b ){ T w=b; b=a; a=w; }
        static public void EnumVisual( Visual myVisual, ref List<Visual> Vis ){
            for( int i=0; i<VisualTreeHelper.GetChildrenCount(myVisual); i++){
                // Retrieve child visual at specified index value.
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(myVisual, i);
                Vis.Add(childVisual);
                WriteLine(childVisual.ToString());
                // Do processing of the child visual object.

                // Enumerate children of the child visual object.
                EnumVisual(childVisual, ref Vis);
            }
        }

        static public bool Inner( this MouseButtonEventArgs e, FrameworkElement Cnl ){
            Point pt = e.GetPosition(Cnl);
            if( pt.X<=0.0 || pt.Y<=0.0 )  return false;
            if( pt.X>=Cnl.Width  || pt.Y>=Cnl.Height )  return false;
            return true;
        }

        static public double ToDouble( this string st ){
            double dv;
            if( st==null) return 0.0;
            if( !st.IsNumeric() ){ return double.MaxValue; }

            try{ dv = Convert.ToDouble(st); return dv; }
            catch(Exception){ return double.MinValue; }
        }
        static public float ToFloat( this string st ){
            float dv;
            if( st==null) return 0.0f;
            if( !st.IsNumeric() ){ return float.MaxValue; }
            try{ dv = (float)(Convert.ToDouble(st)); return dv; }
            catch(Exception){ return float.MinValue; }
        }
        static public int ToInt( this string st ){
            int dv;
            if( st==null) return 0;
            if( st==""  ) return 0; 
            if( st=="-" ) return 0; 
            if( st=="+" ) return 0; 

            if( !st.IsNumeric() ){ return int.MaxValue; }

            try{ dv = Convert.ToInt32(st); return dv; }
            catch( Exception ){ return int.MinValue; }
        }
        static public int ToInt( this char ch ){
            int dv;
            string st=ch.ToString();
            if( !st.IsNumeric() ){ return int.MaxValue; }

            try{ dv = st.ToInt(); return dv; }
            catch( Exception ){ return int.MinValue; }
        }

        static public int DifSet( this int A, int B ){ return (int)(A&(B^0xFFFFFFFF)); }

        static public string ToBitString( this int noB, int ncc ){
            string st="";
            for( int n=0; n<ncc; n++ ){
                st += (((noB>>n)&1)!=0)? (n+1).ToString(): ".";
            }
            return st;
        } 

        static public int BitSet( this int X, int n ){
            X |= (1<<n);
            return  X;
        }
        static public int BitReset( this int X, int n ){
            int nR = (1<<n) ^ 0x7FFFFFFF;
            X &= nR;
            return  X;
        }
 
        static public int BitCount( this int nF ){      //by Hacker's Delight
            if( (nF&0x7FFFFE00)==0 )  return __BC[nF]; 
            int x = nF;
            x = (x&0x55555555) + ((x>>1)&0x55555555);
            x = (x&0x33333333) + ((x>>2)&0x33333333);
            x = (x&0x0F0F0F0F) + ((x>>4)&0x0F0F0F0F);
            x = (x&0x00FF00FF) + ((x>>8)&0x00FF00FF);
            x = (x&0x0000FFFF) + ((x>>16)&0x0000FFFF);
            return x;
        }
        static public int BitCount( this uint nF ){     //by Hacker's Delight         
            uint x = nF;
            x = (x&0x55555555) + ((x>>1)&0x55555555);
            x = (x&0x33333333) + ((x>>2)&0x33333333);
            x = (x&0x0F0F0F0F) + ((x>>4)&0x0F0F0F0F);
            x = (x&0x00FF00FF) + ((x>>8)&0x00FF00FF);
            x = (x&0x0000FFFF) + ((x>>16)&0x0000FFFF);
            return (int)x;
        }
        static public bool IsNumeric( this string stTarget ){ //Test whether a string is a number
            int nNullable;
            return int.TryParse( stTarget, System.Globalization.NumberStyles.Any, null, out nNullable );
        }
        static public bool IsNumeric( this char chTarget ){ //Test whether a string is a number
            int nNullable;
            return int.TryParse( chTarget.ToString(), System.Globalization.NumberStyles.Any, null, out nNullable );
        }
        static public List<T> GetControlsCollection<T>(object parent) where T :DependencyObject{
            //WPF - Getting a collection of all controls for a given type on a Page
            //http://stackoverflow.com/questions/7153248/wpf-getting-a-collection-of-all-controls-for-a-given-type-on-a-page
            //List<Button> buttons = GetControlsCollection<Button>(yourPage);
            List<T> logicalCollection = new List<T>();
            GetControlsCollection( parent as DependencyObject, logicalCollection );
            return logicalCollection;
        }
        static private void GetControlsCollection<T>( DependencyObject parent, List<T> logicalCollection ) where T: DependencyObject{
            var children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children){
                if (child is DependencyObject){
                    DependencyObject depChild = child as DependencyObject;
                    if( child is T )logicalCollection.Add(child as T);
                    GetControlsCollection(depChild, logicalCollection);
                }
            }
        }


        static public string encapsulate( string st ){
            char[] sep=new Char[]{ ' ', ',', '\t' };
            string[] stL;
            stL=st.Split(sep);
            if( stL[0]!=st) return "\"" + st + "\"";
            else return st;
        }
        static private char[] sepDef=new Char[]{ ' ', ',', '\t' };
        static public string[] SplitEx( this string st, char[] sep=null ){
            if( sep==null ) sep=sepDef;
            int[] sPoint=new int[1024];
            int kk, kkCC, m, n;

            int ndlLL;
            do{
                ndlLL=st.Length;
                st=st.Replace(",,", ",@#,");
                st=st.Replace(", ,", ",@#,");
                st=st.Replace(",  ,", ",@#,");
                st=st.Replace(",   ,", ",@#,");
                st=st.Replace(",    ,", ",@#,");
                st=st.Replace(",     ,", ",@#,");
            } while (ndlLL!=st.Length);

            sPoint[0]=-1;
            for(kk =1, m=0; kk<sPoint.Length; kk++){
                n=st.IndexOf( "\"", m);
               if( n<=0) break;
               sPoint[kk]=n;
                 m=n + 1;
                if( kk==sPoint.Length-1 ){
                    WriteLine($"** system error SplitEx sPoint#={sPoint.Length}");
                    return null;
                }
            }
            kkCC=kk;
            sPoint[kk]=st.Length;

            int     na=0, nb, ix=0;
            string  stw;
            string[]  splitW;
            string[]  split=new string[512];
            for(kk=0;kk<kkCC;kk++){
                if( ix==split.Length-1)             {
                    WriteLine($"** system error elementSeparator2 split#={split.Length}");
                    return null;
                }
                na=sPoint[kk]+1;
                nb=sPoint[kk+1];     
                stw=st.Substring(na,nb-na);      
                if( kk%2==0 ){   
                    splitW=stw.Split(sep);
                    foreach( string s in splitW ){
                        if( s.Length<=0 ) continue;
                        split[ix++]=s.Trim();
                    }
                }
                else{
                    split[ix++]=stw;
                }
            }
            string[] split2=new string[ix];
            for(kk=0; kk < ix; kk++){
                stw=split[kk];
                if( stw=="@#") stw="0";
                split2[kk]=stw.Replace("\"","");
            }
            return split2;
        }

/*
        static public FontStyle GetFontStyle( string fntStyl, FontStyle fsDefault ){
            FontStyle fs=0;
            string[] eLst;

            elementSeparator(fntStyl, out eLst);
            foreach(string st in eLst){
                switch( st ){
                    case "Italic": fs |= FontStyles.Italic; break;
                    case "Normal": fs |= FontStyles.Normal; break;
                    case "Strikeout": fs |= FontStyles.Strikeout; break;
                    case "Underline": fs |= FontStyles.Underline; break;
                }
            }
            if( fs==0 && fsDefault!=0 ) fs=fsDefault;
            return fs;
        }
*/
        static public string replaceSpaceAll( string st ){
            return (st.Trim().Replace(" ", "").Replace("　", ""));
        }
        static public string removePath( string st ){
            int k=st.LastIndexOf("\\");
            if( k>0 ) return st.Remove(0,k+1);
            else return st;
        }
        /*
                //Bitmap -> WPF/control.Source
                static public ImageSource CreateBitmapSourceFromHBitmap( this Bitmap bmp ){
                    ImageSource imgSrc = Imaging.CreateBitmapSourceFromHBitmap( bmp.GetHbitmap(),
                        IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions() );
                    return imgSrc;
                }

                static public PointF DoRotate( this PointF P, double alpha ){
                    double sn = Sin(alpha);
                    double cn = Cos(alpha); 

                    float X = (float)(  P.X*cn + P.Y*sn);
                    float Y = (float)( -P.X*sn + P.Y*cn);

                    return ( new PointF(X,Y) );
                }
                static public PointF DoMove( this PointF P, PointF Pm ){
                    return ( new PointF(P.X+Pm.X, P.Y+Pm.Y) );
                }
                static public PointF DoRotateMove( this PointF P, double alpha,  PointF Pm  ){
                    double sn = Sin(alpha);
                    double cn = Cos(alpha); 

                    float X = (float)(  P.X*cn + P.Y*sn + Pm.X);
                    float Y = (float)( -P.X*sn + P.Y*cn + Pm.Y);

                    return ( new PointF(X,Y) );
                }
        */
    }
}