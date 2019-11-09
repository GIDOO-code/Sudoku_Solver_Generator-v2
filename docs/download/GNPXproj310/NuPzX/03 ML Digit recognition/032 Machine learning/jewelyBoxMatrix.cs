using System;
using System.Collections.Generic;
using static System.Console;
using System.Globalization;
using System.Linq;

using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using System.Text.RegularExpressions;
using System.Resources;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GIDOO_space{
    static public class DenseMatrixExtender{
        static private char[] sep={',',' '};

        static DenseMatrixExtender(){ }
        static public string ToString( this DenseMatrix M, string title ){
            string st="";
            if(title!="") st=title+"\r";
            for(int y=0; y<M.RowCount; y++ ){
                for(int x=0; x<M.ColumnCount; x++ ) st+= " "+M[y,x].ToString("G").PadLeft(10);
                st +="\r     ";
            }
            return st;
        }
        static public string ToString( this DenseVector V, string title ){
            string st="";
            if(title!="") st=title;
            for(int x=0; x<V.Count; x++ ) st+= " "+V[x].ToString("G").PadLeft(10);
            st +="\r";
            return st;
        }
    }
}