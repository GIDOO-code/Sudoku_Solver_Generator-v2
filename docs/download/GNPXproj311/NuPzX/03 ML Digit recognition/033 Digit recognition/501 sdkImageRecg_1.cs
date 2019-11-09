using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

using OpenCvSharp;
using GIDOO_Lib;

namespace GIDOOCV{
    /*
    public partial class sdkImageRecg{  //  static public class CvExtensions{
        static public  int regSize=32;
        static public  Point[] Q16;
        static public  int[] link16=new int[16];
        static private int[] drctn={-1,+1,-4,+4 };

        public ProjectiveTrans PT=new ProjectiveTrans( );   //projective transforamtion
        public MLXnumber3 MLn3;

        static sdkImageRecg(){
            //Connected node of 4×4 lattice
            for( int n=0; n<16; n++ ){
                int r0=n/4, c0=n%4, d=0;
                for( int k=0; k<4; k++ ){
                    int n1=n+drctn[k], r1=n1/4, c1=n1%4;
                    if(n1<0 || n1>=16 )  continue;
                    if( (r0==r1 && Math.Abs(c0-c1)==1) || (c0==c1 && Math.Abs(r0-r1)==1) ) d |= (1<<n1);
                }
                link16[n]=d;
                //WriteLine($" n:{n} -> d{d.ToBitStringN(16)}");
            }
        }

        public sdkImageRecg( int MLV, string fName, int MLtype, int MidLSize, bool dropoutB ){
            MLn3 = new MLXnumber3();
            double gamma=dropoutB? 0.7: 1.0;
            MLn3.Set_LayerData(fName,MLtype,MidLSize,gamma,DispB:true);//ver.3  
            MLn3.CreateTeacher_Img_Feature();
        }

    }
    */
}