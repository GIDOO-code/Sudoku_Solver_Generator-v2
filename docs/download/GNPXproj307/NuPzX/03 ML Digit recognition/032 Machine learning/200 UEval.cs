using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GIDOOCV{
    public class UEval{
        public string      name;
        public int         type; //0,1,2,3,4 
        public double      val0;
        public double[]    val1;
        public double[,]   val2;
        public double[,,]  val3;
        public double[,,,] val4;
        public double[]    PDiff;

        public int         Size;
        public int         Indx0;
        public Func<int[],int> FIndex;

        public List<string> StrLst;

        private UEval( string name, int type ){
            this.name = name;
            this.type = type;
            this.PDiff = null;
            this.Size  = 0;
            for(int k=0; k<Size; k++ ) PDiff[k]=0.001;
        }
        public UEval( string name, double val0 ): this(name, 0 ){
            this.val0 = val0;
            Size = 1;
        }
        public UEval( string name, double[] val1 ): this(name, 1 ){
            this.val1 = val1;
            Size = val1.Length;
            this.PDiff = new double[Size];
            Size = 1;
        }
        public UEval( string name, double[,] val2 ): this(name, 2 ){
            this.val2 = val2;
            Size = val2.Length;
            this.PDiff = new double[Size];
        }
        public UEval( string name, double[,,] val3 ): this(name, 3 ){
            this.val3 = val3;
            Size = val3.Length;
            this.PDiff = new double[Size];
        }
        public UEval( string name, double[,,,] val4 ): this(name, 4 ){
            this.val4 = val4;
            Size = val4.Length;
            this.PDiff = new double[Size];
        }
        public UEval( string name, List<string> StrLst ){
            this.name = name;
            this.type = 10;
            
            List<string> StrLst2 = new List<string>();
            StrLst.ForEach(p=>StrLst2.Add(p.Replace(" ","")));
            this.StrLst = StrLst2;
        }

        public UEval( string name, int type/*20*/, double[] val1, Func<int[],int> FIndex ){
            this.name = name;
            this.type = type;
            this.val1 = val1;
            this.FIndex = FIndex;
        }
 
        public String ToStringB( bool sPrint ){
            try{
                StringBuilder stb = new StringBuilder();
                stb.Append(name+", "+type.ToString());
                int sz0, sz1, sz2, sz3, nc=0;
                double  tt=0.0;
                double  keyZero = Convert.ToDouble("1.00011");
                switch(type){
                    case 0:
                        stb.Append(", "+ val0.ToString("0.00"));
                        break;

                    case 1:
                        sz0 = val1.Length;
                        stb.Append(", "+ sz0);                    
                        for(int k=0; k<sz0; k++ ){
                            if( val1[k]==keyZero ) val1[k]=0.0;
                            else stb.Append(", "+ val1[k].ToString("0.00"));
                            tt+=val1[k];
                        }
                        Size = sz0;
                        if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                        break;

                    case 2:
                        sz0 = val2.GetLength(0);
                        sz1 = val2.GetLength(1);
                        
                        stb.Append(", "+ sz0+","+ sz1);
                        for(int k=0; k<sz0; k++ ){
                            for(int m=0; m<sz1; m++ ){ 
                                if( val2[k,m]==keyZero ) val2[k,m]=0.0;
                                stb.Append(", "+ val2[k,m].ToString("0.00"));
                                tt+=val2[k,m];
                                if(sPrint && ++nc>16 ) goto L2;
                            }
                        }
                    L2:
                        Size = sz0*sz1;
                        if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                        break;

                    case 3:
                        sz0 = val3.GetLength(0);
                        sz1 = val3.GetLength(1);
                        sz2 = val3.GetLength(2);
                        stb.Append(", "+ sz0+","+ sz1+","+ sz2);
                        for(int k=0; k<sz0; k++ ){
                            for(int m=0; m<sz1; m++ ){
                                for(int n=0; n<sz2; n++ ){
                                    if( val3[k,m,n]==keyZero ) val3[k,m,n]=0.0;
                                    stb.Append(", "+ val3[k,m,n].ToString("0.00"));
                                    tt+=val3[k,m,n];
                                    if(sPrint && ++nc>16 ) goto L3;
                                }
                            }
                            if( k<sz0-1 ) stb.Append("&\r");
                        }
                    L3:
                        Size = sz0*sz1*sz2;
                        if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                        break;
                
                    case 4:
                        sz0 = val4.GetLength(0);
                        sz1 = val4.GetLength(1);
                        sz2 = val4.GetLength(2);
                        sz3 = val4.GetLength(3);
                        stb.Append(", "+ sz0+","+ sz1+","+ sz2+","+ sz3);
                        for(int k=0; k<sz0; k++ ){
                            for(int m=0; m<sz1; m++ ){
                                for(int n=0; n<sz2; n++ ){
                                    for(int r=0; r<sz3; r++ ){
                                        if( val4[k,m,n,r]==keyZero ) val4[k,m,n,r]=0.0;
                                        stb.Append(", "+ val4[k,m,n,r].ToString("0.00"));
                                        tt+=val4[k,m,n,r];
                                        if(sPrint && ++nc>16 ) goto L4;
                                    }
                                }
                                if( m<sz1-1 ) stb.Append("&\r");
                            }
                        }
                    L4:
                        Size = sz0*sz1*sz2*sz3;
                        if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                        break;

                    case 10:
                        StrLst.ForEach( p => stb.Append(", "+p) );
                        break;
                }

                return stb.ToString();
            }
            catch( Exception ex ){
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return "error.....";
        }
        public String ToStringInt( bool sPrint ){
            try{
                StringBuilder stb = new StringBuilder();
                stb.Append(name+" type:"+type.ToString());
                int sz0, sz1, sz2, sz3, nc=0;
                double  tt=0.0;
                double  keyZero = Convert.ToDouble("1.00011");
                switch(type){
                    case 0:
                        stb.Append(" "+ val0.ToString("0.00"));
                        break;

                    case 1:
                        sz0 = val1.Length;
                        stb.Append(" size:"+ sz0+" /");                    
                        for(int k=0; k<sz0; k++ ){
                            if( val1[k]==keyZero ) val1[k]=0.0;
                            else stb.Append(" "+ val1[k].ToString("0"));
                            tt+=val1[k];
                        }
                        Size = sz0;
                        if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                        break;

                    case 2:
                        sz0 = val2.GetLength(0);
                        sz1 = val2.GetLength(1);
                        
                        stb.Append(" size:"+ sz0+" "+ sz1);
                        for(int k=0; k<sz0; k++ ){
                            for(int m=0; m<sz1; m++ ){ 
                                if( val2[k,m]==keyZero ) val2[k,m]=0.0;
                                stb.Append(" "+ val2[k,m].ToString("0"));
                                tt+=val2[k,m];
                                if(sPrint && ++nc>16 ) goto L2;
                            }
                        }
                    L2:
                        Size = sz0*sz1;
                        if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                        break;

                    case 3:
                        sz0 = val3.GetLength(0);
                        sz1 = val3.GetLength(1);
                        sz2 = val3.GetLength(2);
                        stb.Append(" size:"+ sz0+""+ sz1+" "+ sz2);
                        for(int k=0; k<sz0; k++ ){
                            for(int m=0; m<sz1; m++ ){
                                for(int n=0; n<sz2; n++ ){
                                    if( val3[k,m,n]==keyZero ) val3[k,m,n]=0.0;
                                    stb.Append(" "+ val3[k,m,n].ToString("0"));
                                    tt+=val3[k,m,n];
                                    if(sPrint && ++nc>16 ) goto L3;
                                }
                            }
                            if( k<sz0-1 ) stb.Append("&\r");
                        }
                    L3:
                        Size = sz0*sz1*sz2;
                        if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                        break;
                
                    case 4:
                        sz0 = val4.GetLength(0);
                        sz1 = val4.GetLength(1);
                        sz2 = val4.GetLength(2);
                        sz3 = val4.GetLength(3);
                        stb.Append(" size:"+ sz0+" "+ sz1+" "+ sz2+" "+ sz3);
                        for(int k=0; k<sz0; k++ ){
                            for(int m=0; m<sz1; m++ ){
                                for(int n=0; n<sz2; n++ ){
                                    for(int r=0; r<sz3; r++ ){
                                        if( val4[k,m,n,r]==keyZero ) val4[k,m,n,r]=0.0;
                                        stb.Append(" "+ val4[k,m,n,r].ToString("0"));
                                        tt+=val4[k,m,n,r];
                                        if(sPrint && ++nc>16 ) goto L4;
                                    }
                                }
                                if( m<sz1-1 ) stb.Append("&\r");
                            }
                        }
                    L4:
                        Size = sz0*sz1*sz2*sz3;
                        if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                        break;

                    case 10:
                        StrLst.ForEach( p => stb.Append(", "+p) );
                        break;
                }

                return stb.ToString();
            }
            catch( Exception ex ){
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return "error.....";
        }
       
        public String ToStringDiff(){
            try{
                StringBuilder stb = new StringBuilder();
                if( type>=1 && type<=4 && PDiff!=null ){
                    stb.Append(name+"_Diff");
                    double diff;
                    for(int k=0; k<Size; k++ ){
                        diff = PDiff[k];
                        stb.Append(", "+ diff.ToString("0.000"));
                        if( (k%128)==127 && k<Size-1 ) stb.Append("&\r");
                    }
                }

                return stb.ToString();
            }
            catch( Exception ex ){
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return "error.....";
        }
        public override string ToString(){
            string st = type.ToString();
            int sz0, sz1, sz2, sz3;
            double  tt=0.0;
            switch(type){
                case 0:
                    st += ", "+ val0.ToString("0.00");
                    break;

                case 1:
                    sz0 = val1.Length;
                    st += ", "+ sz0;                    
                    for(int k=0; k<sz0; k++ ){ st+=", "+ val1[k].ToString("0.00"); tt+=val1[k]; }

                    if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                    break;

                case 2:
                    sz0 = val2.GetLength(0);
                    sz1 = val2.GetLength(1);
                    st += ", "+ sz0+","+ sz1;
                    for(int k=0; k<sz0; k++ ){
                        for(int m=0; m<sz1; m++ ){ st+=", "+ val2[k,m].ToString("0.00");  tt+=val2[k,m]; }
                    }
                    // if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                    break;

                case 3:
                    sz0 = val3.GetLength(0);
                    sz1 = val3.GetLength(1);
                    sz2 = val3.GetLength(2);
                    st += ", "+ sz0+","+ sz1+","+ sz2;
                    for(int k=0; k<sz0; k++ ){
                        for(int m=0; m<sz1; m++ ){
                            for(int n=0; n<sz2; n++ ){st+=", "+ val3[k,m,n].ToString("0.00"); tt+=val3[k,m,n]; }
                        }
                        st += "&\r";
                    }
                    // if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                    break;
                
                case 4:
                    sz0 = val4.GetLength(0);
                    sz1 = val4.GetLength(1);
                    sz2 = val4.GetLength(2);
                    sz3 = val4.GetLength(3);
                    st += ", "+ sz0+","+ sz1+","+ sz2+","+ sz3;
                    for(int k=0; k<sz0; k++ ){
                        for(int m=0; m<sz1; m++ ){
                            for(int n=0; n<sz2; n++ ){
                                for(int r=0; r<sz3; r++ ){ st+=", "+ val4[k,m,n,r].ToString("0.00"); tt+=val4[k,m,n,r]; }
                            }
                            st += "&\r";
                        }
                    }
                    // if( tt==0.0 ) _WriteDevelopInfo( name+" total="+tt );
                    break;

                case 10:
                    StrLst.ForEach( p => st+=", "+p);
                    break;
            }
            return st;
        }

        static private bool __first__=true;
        private void _WriteDevelopInfo( string st ){
            using( StreamWriter fp=new StreamWriter("◆DevelopInfo.txt",true) ){
                if( __first__ ) fp.WriteLine("\r ===== "+DateTime.Now.ToString("yy/MM/dd HH-mm-ss")+ " =====");
                __first__ = false;
                fp.WriteLine(st);
            }
        }
    }
}