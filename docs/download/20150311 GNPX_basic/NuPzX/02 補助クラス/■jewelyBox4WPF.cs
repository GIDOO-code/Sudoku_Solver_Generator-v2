using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Threading;
using System.IO;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Interop;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GIDOO_space{
    static public class JKT{

        //===== stopWatchÇÃégÇ¢ï˚ =====
        //  PT=(double)(JKT.stopWatchEnd_TimeSpan(dtSTart).Ticks)/1.0e7;
        //  string st =PT.ToString("0.0") + "ïbåoâﬂÅz\r\n";
        static public DateTime stopWatchStart( ){ return DateTime.Now; }
        static public TimeSpan stopWatchEnd_TimeSpan( DateTime dtSTart ){
            DateTime dt=DateTime.Now;
            TimeSpan diff=dt.Subtract(dtSTart);
            return  diff;
        }
        static public double stopWatchEnd( DateTime dtSTart ){
            DateTime dt=DateTime.Now;
            TimeSpan diff=dt.Subtract(dtSTart);
            return  (double)(diff.Ticks/1.0e7);
        }
        static public Color ReverseColor( Color cr ){  //Åüñ¢âåàÅü
            byte R, G, B;
            /*
                        R=CrSys.R ^ cr.R;
                        G=CrSys.G ^ cr.G;
                        B=CrSys.B ^ cr.B;
            */
            R=(byte)(0xFF - cr.R);
            G=(byte)(0xFF - cr.G);
            B=(byte)(0xFF - cr.B);

            return Color.FromArgb( cr.A, R, G, B);
        }
       
        //===== ê≥ãKóêêî =====
        public static Random sysRndm=new Random(0);
        public static void Å@randomSeedSet( int seed ){
            if( seed==0 ){
                DateTime dt=DateTime.Now;
                seed=(int)dt.Ticks % 10000000;
            }
            sysRndm=new Random( seed );
         }
        public static double _NormalDistribution( double average, double standDev ){
            double rn=_NormalDistribution()*standDev + average;
            return rn;
        }    
        public static double _NormalDistribution(){  //ê≥ãKï™ïzÇ…è]Ç¢ämó¶ïœêîÇî≠ê∂
            double ra=sysRndm.NextDouble();
            double rb=sysRndm.NextDouble();

            //Box-Muller transform
            double rNorm=Math.Sqrt(-2.0*Math.Log(ra)) * Math.Sin( 2.0*Math.PI*rb );
            return rNorm;
        }

    }
}