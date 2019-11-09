using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Console;

using GIDOO_Lib;

using OpenCvSharp;

namespace GIDOOCV{
    public class LearningDataNumber{    
        public  ProjectiveTrans     PT;
        public  string              fName;
        public  int UDataLstCount{ get{ return UDataLst.Count; } }
        public  List<ULeData>       UDataLst;

        public LearningDataNumber( string fName ){
            this.fName=fName;
            PT = new ProjectiveTrans();
        }

        public ULeData GetULeData( int nx ){
            if(nx<UDataLst.Count)  return UDataLst[nx];
            else return null;
        }
    }
}