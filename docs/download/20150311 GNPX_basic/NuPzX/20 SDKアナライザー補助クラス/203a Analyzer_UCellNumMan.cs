using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GIDOO_space;

namespace GNPZ_sdk{
  
#region UCellNumMan
    public class UCellNum{//: IComparable{
        public object obj;

        public readonly int  rc;
        public readonly int  r;
        public readonly int  c;
        public readonly int  b;
        public readonly int  No;
        public int bx{ get{ return ((r%3)*3+(c%3)); } }

        public object        preLink;

        public UCellNum( ){}
        public UCellNum( int rc, int No ){
            this.rc = rc;
            this.r  = rc/9;
            this.c  = rc%9;
            this.b  = rc/27*3+(rc%9)/3;
            this.No = No;    
        }

        public  override bool Equals(object obj){
            UCellNum U=obj as UCellNum;
            return (U.rc==this.rc && U.No==this.No );
        }
 
        public UCellNum Copy( ){
            UCellNum UCNcpy=(UCellNum)this.MemberwiseClone();
            return UCNcpy;
        }
    }
#endregion UCellNumMan

}