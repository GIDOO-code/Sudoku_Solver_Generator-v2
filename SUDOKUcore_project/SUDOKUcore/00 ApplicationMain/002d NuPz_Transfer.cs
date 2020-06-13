using System;
using System.Collections.Generic;
using System.Linq;

using static System.Math;
using static System.Console;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GIDOO_space;

namespace GNPXcore{
    using sysWin=System.Windows;

    public partial class NuPz_Win{  //Puzzle Transform
        private PuzzleTrans pPTrans{ get{ return GNP00.PTrans; } }
        private int         noPChg = -1;

       #region Number change
        private void btnNumChange_Click( object sender, RoutedEventArgs e ){
            btnNumChange.IsEnabled=false;
            btnNumChangeRandom.Visibility=Visibility.Hidden;
            TransSolverA("NumChange",true); //display solution

            txNumChange.Text = "1";
            txNumChange.Visibility = Visibility.Visible;
            btnNumChangeDone.Visibility = Visibility.Visible;
            noPChg = 1;            
            PB_GBoard.IsEnabled = true;
            _SetScreenProblem();　//Show free numbers
            _Display_GB_GBoard();

            PB_GBoard.MouseDown += new MouseButtonEventHandler(PTrans_PB_GBoard_MouseLeftButtonDown);
        }
        private void btnNumChangeDone_Click( object sender, RoutedEventArgs e ){
            btnNumChange.IsEnabled=true;
            btnNumChangeRandom.Visibility=Visibility.Visible;
            GNP00.GSmode = "tabACreate";
            txNumChange.Visibility = Visibility.Hidden;
            btnNumChangeDone.Visibility = Visibility.Hidden;
            noPChg = -1;
            _Display_GB_GBoard();
            PB_GBoard.MouseDown -= new MouseButtonEventHandler(PTrans_PB_GBoard_MouseLeftButtonDown);
        }
        private void PTrans_PB_GBoard_MouseLeftButtonDown( object sender, MouseButtonEventArgs e ){  
            int rcX = _Get_PB_GBoardRCNum();
            if(rcX<0) btnNumChangeDone_Click(this,new RoutedEventArgs());
            else{
                _Change_PB_GBoardNum(rcX);
                _Display_GB_GBoard();
            }
        }
        private int  _Get_PB_GBoardRCNum( ){
            int cSz=GNP00.cellSize, LWid=GNP00.lineWidth;
            sysWin.Point pt = Mouse.GetPosition(PB_GBoard);
            int cn=(int)pt.X-2, rn=(int)pt.Y-2;

            cn = cn - cn/cSz - cn/(cSz*3+LWid)*LWid;
            cn /= cSz;
            if(cn<0 || cn>=9) return -1;
            
            rn = rn - rn/cSz - rn/(cSz*3+LWid)*LWid;
            rn /= cSz;
            if(rn<0 || rn>=9) return -1;
            return (rn*9+cn);
        }

        private void _Change_PB_GBoardNum(int rcX){
            if(rcX<0) return;
            int noP=Abs(pGP.BDL[rcX].No);
            if(noP==0)  return;
            if(noP>noPChg){
                foreach(var q in pGP.BDL.Where(r=>r.No!=0)){                 
                    int nm=q.No, nmAbs=Abs(nm), nmSgn=Sign(nm);
                    if(nmAbs<noPChg) continue;
                    else if(nmAbs==noP) q.No = nmSgn * noPChg;
                    else if(nmAbs<noP)  q.No = nmSgn * (nmAbs+1);
                }
            }
            else if(noP<noPChg){          
                foreach(var q in pGP.BDL.Where(r=>r.No!=0)){                 
                    int nm=q.No, nmAbs=Abs(nm), nmSgn=Sign(nm);
                    if(nmAbs<noP) continue;
                    else if(nmAbs==noP)    q.No = nmSgn * noPChg;
                    else if(nmAbs<=noPChg) q.No = nmSgn * (nmAbs-1);
                }           
            }
/*
            if(noP!=noPChg){
                foreach(var q in pGP.BDL.Where(r=>r.No!=0)){                 
                    int nm=q.No, nmAbs=Abs(nm), nmSgn=Sign(nm);
                    if(noP>=noPChg){
                        if(nmAbs<noPChg) continue;
                        else if(nmAbs==noP) q.No = nmSgn * noPChg;
                        else if(nmAbs<noP)  q.No = nmSgn * (nmAbs+1);
                      //else if(nmAbs==noP) q.No = (nm>0)? noPChg:  -noPChg;
                      //else if(nmAbs<noP)  q.No = (nm>0)? nmAbs+1: -(nmAbs+1);
                    }
                    else{
                        if(nmAbs<noP) continue;
                        else if(nmAbs==noP)    q.No = nmSgn * noPChg;
                        else if(nmAbs<=noPChg) q.No = nmSgn * (nmAbs-1);
                      //else if(nmAbs==noP)    q.No = (nm>0)? noPChg:  -noPChg;
                      //else if(nmAbs<=noPChg) q.No = (nm>0)? nmAbs-1: -(nmAbs-1);
                    }
                }
            }
*/
            _SetScreenProblem();
            noPChg++;
            txNumChange.Text = noPChg.ToString();
            if(noPChg>9) btnNumChangeDone_Click(this,new RoutedEventArgs());
            return;
        }
      #endregion Number change  

        private void btnPatCVRCg_Click( object sender, RoutedEventArgs e ){
            Button btn = sender as Button;
            TransSolverA(btn.Name,(bool)chbShowSolution.IsChecked);
            _SetScreenProblem();
            _Display_Develop();
        }
        private void TransSolverA( string Name, bool DspSol ){
            SDK_Ctrl.MltProblem = 1;
            SDK_Ctrl.lvlLow = 0;
            SDK_Ctrl.lvlHgh = 999;
            GNP00.SDKCntrl.CbxDspNumRandmize=false;
            GNP00.SDKCntrl.GenLStyp = 1;
            GNPXApp000.chbConfirmMultipleCells = (bool)chbConfirmMultipleCells.IsChecked;
            GNPZ_Engin.SolInfoB = true;
            pPTrans.SDK_TransProbG(Name,DspSol);
        }
        private void chbShowSolution_Checked( object sender, RoutedEventArgs e ){
            if(pGP.AnsNum==null)  TransSolverA("Checked",true);
            pPTrans.SDK_TransProbG("Checked",(bool)chbShowSolution.IsChecked);
            _Display_GB_GBoard(DevelopB:true);
        }
        private void btnTransEst_Click( object sender, RoutedEventArgs e ){
            pPTrans.btnTransEst();
            _Display_GB_GBoard(DevelopB:true);
        }
        private void btnTransRes_Click(object sender, RoutedEventArgs e ){
            pPTrans.btnTransRes();
            if(!(bool)chbShowSolution.IsChecked) pGP.BDL.ForEach(P=>{P.No=Max(P.No,0);});
            _Display_GB_GBoard(DevelopB:true);
        }

        private void btnNomalize_Click( object sender, RoutedEventArgs e ){
            if(pGP.AnsNum==null)  TransSolverA("Checked",true);
            string st=pPTrans.SDK_Nomalize( (bool)chbShowSolution.IsChecked, (bool)chbNrmlNum.IsChecked );
            tbxTransReport.Text=st;
            _Display_GB_GBoard(DevelopB:true);
        }
        
        private void btnNumChangeRandom_Click(object sender,RoutedEventArgs e){
            TransSolverA("random",true);
            var rnd=new Random();
            List<int> ranNum = new List<int>();
            for(int r=0; r<9; r++)  ranNum.Add( rnd.Next(0,9)*10+r );
            ranNum.Sort((x,y)=>(x-y));
            for(int r=0; r<9; r++) ranNum[r] %= 10;

            foreach( var q in pGP.BDL.Where(p=>p.No!=0) ){
                int nm=q.No, nm2=ranNum[Abs(nm)-1]+1;
                q.No = (nm>0)? nm2: -nm2;
            }
            _Display_GB_GBoard();
        }
    }
}