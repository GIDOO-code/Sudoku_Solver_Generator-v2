using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Math;
using static System.Console;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using GIDOO_space;

namespace GNPZ_sdk{
    public class UCellEx{ //Basic Cell Class
        public readonly int  rc;
        public readonly int  r;
        public readonly int  c;
        public readonly int  b;

        public int      ErrorState; //0:-  1:Fixed @8:Violation  9:No solution
        public int      No;         //>0:Problem  =0:Open  <0:Solution
        public int      FreeB;
        public int      FreeBC{ get{ return FreeB.BitCount(); } }

        public int      FixedNo;  
        public int      CancelB;
        
        public bool     Selected;
        public int      nx;

        public UCellEx( ){}
        public UCellEx( int rc, int No=0, int FreeB=0 ){
            this.rc = rc;
            this.r  = rc/9;
            this.c  = rc%9;
            this.b  = rc/27*3+(rc%9)/3;
            this.No = No;
            this.FreeB = FreeB;
        }

        public UCellEx Copy( ){
            UCellEx UCcpy=(UCellEx)this.MemberwiseClone();
            return UCcpy;
        }
        public override string ToString(){
            string po = " UCell rc:"+rc+"["+((r+1)*10+(c+1)) +"]  no:"+No;
            po +=" FreeB:" + FreeB.ToBitString(9);
            po +=" CancelB:" + CancelB.ToBitString(9);
            return po;
        }
    }
}