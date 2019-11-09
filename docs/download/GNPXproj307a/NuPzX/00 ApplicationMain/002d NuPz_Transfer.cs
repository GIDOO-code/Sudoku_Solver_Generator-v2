using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Globalization;

using static System.Math;
using static System.Console;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

using Microsoft.Win32;

using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;

using GIDOOCV;

using GIDOO_space;

namespace GNPZ_sdk{
    using pRes=Properties.Resources;
    using sysWin=System.Windows;

    public partial class NuPz_Win{
    #region Puzzle Transform
        private PuzzleTrans pPTrans{ get{ return GNP00.PTrans; } }
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
            for(int r=0; r<9; r++ )  ranNum.Add( rnd.Next(0,9)*10+r );
            ranNum.Sort((x,y)=>(x-y));
            for(int r=0; r<9; r++) ranNum[r] %= 10;

            foreach( var q in pGP.BDL.Where(p=>p.No!=0) ){
                int nm=q.No, nm2=ranNum[Abs(nm)-1]+1;
                q.No = (nm>0)? nm2: -nm2;
            }
            _Display_GB_GBoard();
        }
    #endregion Puzzle Transform
    }
}