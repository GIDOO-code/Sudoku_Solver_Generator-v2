using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Console;
using GIDOO_Lib;

//using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenCvSharp;

namespace GIDOOCV{
    public partial class ULDataBase{
        static private double IniVal=0.0;
        static private int  _pSize=-1;
        public int  pSize{ 
            get{
                if(_pSize<0) _pSize=GetSize();
                return _pSize;
            }
        }
        public int  ID;
        public Mat  pUData;

        public bool selB;
        public int  ans;
        public int  est;
        public Dictionary<string,UEval> EvalValDic;
        public double[]  gPara;

        public ULDataBase( Mat UData, int ans ){
            this.pUData=UData; this.ans=ans;
            EvalValDic = new Dictionary<string,UEval>();  
        }
        private int GetSize(){
            int s=0;
            foreach( var p in EvalValDic ){
                UEval ue = p.Value;
                ue.Indx0=s;
                switch(ue.type){
                    case 0: s++; break;
                    case 1: s += ue.val1.Length; break;
                    case 2: s += ue.val2.Length; break;
                    case 3: s += ue.val3.Length; break;
                    case 4: s += ue.val4.Length; break;
                }
            }
            WriteLine( "pSize:"+s );
            return s;
        }

      #region Evaluation value initial setting
        public UEval _GenInitialEvalVal( string name ){
            return new UEval(name,IniVal);
        }
        public UEval _GenInitialEvalVal( string name, int sz0 ){
            double[] val1 = new double[sz0];
            for( int k=0; k<sz0; k++ )  val1[k] = IniVal;
            return new UEval(name,val1);
        }
        public UEval _GenInitialEvalVal( string name, int sz0, int sz ){
            double[,] val2 = new double[sz0,sz];
            for( int k=0; k<sz0; k++ ){
                for( int m=0; m<sz; m++ )  val2[k,m]=IniVal;
            }
            return new UEval(name,val2);
        }
        public UEval _GenInitialEvalVal( string name, int sz0, int sz1, int sz2 ){
            double[,,] val3 = new double[sz0,sz1,sz2];
            for( int k=0; k<sz0; k++ ){
                for( int m=0; m<sz1; m++ ){
                    for( int n=0; n<sz2; n++ )  val3[k,m,n]=IniVal;
                }
            }
            return new UEval(name,val3);
        }
        public UEval _GenInitialEvalVal( string name, int sz0, int sz1, int sz2, int sz3 ){
            double[,,,] val4 = new double[sz0,sz1,sz2,sz3];
            for( int k=0; k<sz0; k++ ){
                for( int m=0; m<sz1; m++ ){
                    for( int n=0; n<sz2; n++ ){
                        for( int r=0; r<sz3; n++ ) val4[k,m,n,r]=IniVal;
                    }
                }
            }
            return new UEval(name,val4);
        }
      #endregion Evaluation value initial setting    
        
      #region Evaluation value setting
        public void _SeteVal( string vName, double val ){
            UEval UE = EvalValDic[vName];
            UE.val0=val;
        }
        public void _SeteVal( string vName, int kx0, double val ){
            UEval UE = EvalValDic[vName];
            UE.val1[kx0]=val;
        }
        public void _SeteVal( string vName, int kx0, int kx1, double val ){
            if( kx1<0 )  return;
            UEval UE = EvalValDic[vName];
            double V=UE.val2[kx0,kx1]=val;
        }     
        public void _SeteVal( string vName, int kx0, int kx1, int kx2, double val ){
            if( kx1<0 || kx2<0 )  return;
            UEval UE = EvalValDic[vName];
            UE.val3[kx0,kx1,kx2]=val;
        }
        public void _SeteVal( string vName, int kx0, int kx1, int kx2, int kx3, double val ){
            if( kx1<0 || kx2<0 || kx3<0 )  return;
            UEval UE = EvalValDic[vName];
            UE.val4[kx0,kx1,kx2,kx3]=val;
        }
      #endregion Evaluation value setting   
        
      #region Parameter conversion                        
        public void CopyParameter_DicToArray( ){ //Dic -> Array
            if(gPara==null)  gPara=new double[pSize];

            int kx=0;
            foreach( var p in EvalValDic ){
                UEval ue = p.Value;
                int sz0, sz1, sz2, sz3;//, dx=0;

                switch(ue.type){
                    case 0:
                        gPara[kx++] = ue.val0;
                        break;

                    case 1:
                        sz0=ue.val1.GetLength(0);
                        for( int k=0; k<sz0; k++ ){
                            gPara[kx++]=ue.val1[k];
                        }
                        break;

                    case 2:
                        sz0=ue.val2.GetLength(0);
                        sz1=ue.val2.GetLength(1);
                        for( int k=0; k<sz0; k++ ){
                            for( int m=0; m<sz1; m++ ){
                                gPara[kx++]=ue.val2[k,m];
                            }
                        }
                        break;

                    case 3:
                        sz0=ue.val3.GetLength(0);
                        sz1=ue.val3.GetLength(1);
                        sz2=ue.val3.GetLength(2);
                        for( int k=0; k<sz0; k++ ){
                            for( int m=0; m<sz1; m++ ){
                                for( int n=0; n<sz2; n++ ){
                                    gPara[kx++]=ue.val3[k,m,n];
                                }
                            }
                        }
                        break;

                    case 4:
                        sz0=ue.val4.GetLength(0);
                        sz1=ue.val4.GetLength(1);
                        sz2=ue.val4.GetLength(2);
                        sz3=ue.val4.GetLength(3);
                        for( int k=0; k<sz0; k++ ){
                            for( int m=0; m<sz1; m++ ){
                                for( int n=0; n<sz2; n++ ){
                                    for( int r=0; r<sz3; r++ ){
                                        gPara[kx++]=ue.val4[k,m,n,r];
                                    }
                                }
                            }
                        }
                        break;
                    default: break;
                }
               // for(int k=0; k<pSize; k++) gPara[k]=(gPara[k]>0)? 1.0: 0.0;
            }
        }
        public void CopyParameter_ArrayToDic( ){ //Array -> Dic
            int kx=0;
            int sz0, sz1, sz2, sz3;
            foreach( var p in EvalValDic ){
                UEval ue = p.Value;
                try{
                    switch(ue.type){
                        case 0:
                            ue.val0=gPara[kx++];
                            break;
                        
                        case 1:
                            sz0=ue.val1.GetLength(0);
                            if( ue.PDiff==null ) 
                            for( int k=0; k<sz0; k++ ){
                                ue.val1[k]=gPara[kx++]; 
                            }
                            break;

                        case 2:
                            sz0=ue.val2.GetLength(0);
                            sz1=ue.val2.GetLength(1);
                            for( int k=0; k<sz0; k++ ){
                                for( int m=0; m<sz1; m++ ){
                                    ue.val2[k,m]=gPara[kx++];
                                }
                            }
                            break;

                        case 3:
                            sz0=ue.val3.GetLength(0);
                            sz1=ue.val3.GetLength(1);
                            sz2=ue.val3.GetLength(2);
                            for( int k=0; k<sz0; k++ ){
                                for( int m=0; m<sz1; m++ ){
                                    for( int n=0; n<sz2; n++ ){
                                        ue.val3[k,m,n]=gPara[kx++];
                                    }
                                }
                            }
                            break;
                        
                        case 4:
                            sz0=ue.val4.GetLength(0);
                            sz1=ue.val4.GetLength(1);
                            sz2=ue.val4.GetLength(2);
                            sz3=ue.val4.GetLength(3);
                            for( int k=0; k<sz0; k++ ){
                                for( int m=0; m<sz1; m++ ){
                                    for( int n=0; n<sz2; n++ ){
                                        for( int r=0; r<sz3; r++ ){
                                            ue.val4[k,m,n,r]=gPara[kx++];
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
                catch( Exception ex ){
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
      #endregion Parameter conversion 

      #region Evaluation value file IO
        public void ReadParameter( string fName, Dictionary<string,UEval> EvalValDicX=null ){
            Dictionary<string,UEval> pEvalValDic=EvalValDicX;
            if( pEvalValDic==null ) pEvalValDic=EvalValDic;

            int sz0, sz1, sz2, sz3;
            string fLine, fLine2;
            try{          
                using( var fp=new StreamReader(fName, Encoding.Unicode) ){                   
                    while( (fLine=fp.ReadLine())!=null ){
                        if( fLine=="" ) continue;
                        if( fLine.First()=='!' )  continue;

//z                     Console.WriteLine("++"+fLine);
                        if( fLine.Last()=='&' ){
                            while( (fLine2=fp.ReadLine())!=null ){
                                if( fLine2.Length<1 ) break;
                                fLine += fLine2;
                                if( fLine2.Last()!='&' ) break;
                               
                            }
                        }
                        fLine = fLine.Replace("&","");
                        string[] eLst=fLine.Split(G._sepC);

                        int elLng = eLst.Length;
                        int nx=0;
                        string name = eLst[nx++];
                        //Console.WriteLine(name);
                                              
                        //*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
                        if( fLine.Substring(0,3)!="sys" ){
                            for( int k=nx; k<elLng; k++ ){
                                if( eLst[k]==" 0.00" ) eLst[k]="1.00011";
                            }
                        }
                        //*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*

                        UEval ue;
                        if( name.EndsWith("_Diff") ){
                            name = name.Replace("_Diff","");
                            //Console.WriteLine(name);
                            ue = pEvalValDic[name];
                            double[] pDiff = pEvalValDic[name].PDiff;
                            nx=1;
                            //方向vectorは1000倍されている。
                            for( int k=0; k<ue.Size; k++ ) pDiff[k] =  Convert.ToDouble(eLst[nx++]);
                            continue;
                        }

                        int typ  = Convert.ToInt32(eLst[nx++]);
                        switch(typ){
                            case 0:
                                double par0 = Convert.ToDouble(eLst[nx++]);
                                ue = new UEval( name, par0 );
                                pEvalValDic[name] = ue;
                                break;
                            
                            case 1:
                                sz0 = Convert.ToInt32(eLst[nx++]);
                                double[] par1 = new double[sz0];
                                for( int k=0; k<sz0; k++ ) par1[k] = Convert.ToDouble(eLst[nx++]);
                                ue = new UEval( name, par1 );
                                pEvalValDic[name] = ue;
                                break;

                            case 2:
                                sz0 = Convert.ToInt32(eLst[nx++]);
                                sz1 = Convert.ToInt32(eLst[nx++]);
                                double[,] par2 = new double[sz0,sz1];
                                for( int k=0; k<sz0; k++ ){
                                    for( int m=0; m<sz1; m++ ) par2[k,m]=Convert.ToDouble(eLst[nx++]);
                                }
                                ue = new UEval( name, par2 );
                                pEvalValDic[name] = ue;
                                break;

                            case 3:
                                sz0 = Convert.ToInt32(eLst[nx++]);
                                sz1 = Convert.ToInt32(eLst[nx++]);
                                sz2 = Convert.ToInt32(eLst[nx++]);
                                double[,,] par3 = new double[sz0,sz1,sz2];
                                for( int k=0; k<sz0; k++ ){
                                    for( int m=0; m<sz1; m++ ){
                                        for( int n=0; n<sz2; n++ )  par3[k,m,n] = Convert.ToDouble(eLst[nx++]);
                                    }
                                }
                                ue = new UEval( name, par3 );
                                pEvalValDic[name] = ue;
                                break;

                            case 4:
                                sz0 = Convert.ToInt32(eLst[nx++]);
                                sz1 = Convert.ToInt32(eLst[nx++]);
                                sz2 = Convert.ToInt32(eLst[nx++]);
                                sz3 = Convert.ToInt32(eLst[nx++]);
                                double[,,,] par4 = new double[sz0,sz1,sz2,sz3];
                                for( int k=0; k<sz0; k++ ){
                                    for( int m=0; m<sz1; m++ ){
                                        for( int n=0; n<sz2; n++ ){
                                            for( int r=0; r<sz3; r++ ) par4[k,m,n,r] = Convert.ToDouble(eLst[nx++]);
                                        }
                                    }
                                }
                                ue = new UEval( name, par4 );
                                pEvalValDic[name] = ue;
                                break;

                            case 10:
                                List<string> stL = eLst.Skip(2).ToList();
                                ue = new UEval( name, stL );
                                pEvalValDic[name] = ue;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch( Exception e ){
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public void WriteParameter( string fName, bool moveOn=false ){
            if( !Directory.Exists(G._backupDir) ) Directory.CreateDirectory(G._backupDir);
            if( File.Exists(fName) && moveOn ){
                string dstFName = DateTime.Now.ToString().Replace("/","-").Replace(":","-");
                try{
                    File.Move(fName,G._backupDir+"/"+dstFName+fName);
                }
                catch(Exception e ){
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            using( var fp=new StreamWriter(fName,false,Encoding.Unicode,4096) ){
                foreach( var p in EvalValDic ){
                    string st = p.Value.ToStringB(false); //常に全データ出力
                    if( p.Key.Substring(0,2)!="sy" ){
                        fp.WriteLine("!---------------------------------------------------------------");
                    }
                    fp.WriteLine(st);
                    if( p.Key.Substring(0,2)!="sy" ) fp.WriteLine(p.Value.ToStringDiff());
                }
            }
        }
        public string ToStringInt( bool sPrint ){
            string st="\r";
            foreach( var p in EvalValDic ) st += p.Value.ToStringInt(sPrint)+"\r";
            return st;
        }
        public override string ToString( ){
            string st="";
            foreach( var p in EvalValDic ) st += p.Value.ToStringB(false)+"\r";
            return st;
        }
      #endregion Evaluation value file IO
    }
}