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
    #region File I/O, keyDown 
        private string    fNameSDK;
        private void btnOpenPuzzleFile_Click( object sender, RoutedEventArgs e ){
            var OpenFDlog = new OpenFileDialog();
            OpenFDlog.Multiselect = false;

            OpenFDlog.Title  = pRes.filePuzzleFile;
            OpenFDlog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if( (bool)OpenFDlog.ShowDialog() ){
                fNameSDK = OpenFDlog.FileName;
                GNP00.SDK_FileInput( fNameSDK, (bool)chbInitialState.IsChecked );
                txtFileName.Text = fNameSDK;

                _SetScreenProblem();
                GNP00._SDK_Ctrl_Initialize();

                btnProbPre.IsEnabled = (GNP00.CurrentPrbNo>=1);
                btnProbNxt.IsEnabled = (GNP00.CurrentPrbNo<GNP00.SDKProbLst.Count-1);
            }
        }
        private void btnSavePuzzle_Click( object sender, RoutedEventArgs e ){
            var SaveFDlog = new SaveFileDialog();
            SaveFDlog.Title  =  pRes.filePuzzleFile;
            SaveFDlog.FileName = fNameSDK;
            SaveFDlog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            
            GNPXApp000.SlvMtdCList[0] = true;
            if( !(bool)SaveFDlog.ShowDialog() ) return;
            fNameSDK = SaveFDlog.FileName;
            bool append  = (bool)chbAdditionalSave.IsChecked;
            bool fType81 = (bool)chbFile81Nocsv.IsChecked;
            bool SolSort = (bool)chbSolutionSort.IsChecked;
            bool SolSet  = (bool)cbxProbSolSetOutput.IsChecked;
            bool SolSet2 = (bool)chbAddAlgorithmList.IsChecked;

            if( GNP00.SDKProbLst.Count==0 ){
                if( pGP.BDL.All(p=>p.No==0) ) return;
                pGP.ID = GNP00.SDKProbLst.Count;
                GNP00.SDKProbLst.Add(pGP);
                GNP00.CurrentPrbNo=0;
                _SetScreenProblem();
            }
            GNP00.GNPX_Eng.Set_MethodLst_Run(AllMthd:false);  //true:All Method 
            GNP00.SDK_FileOutput( fNameSDK, append, fType81, SolSort, SolSet, SolSet2 );
        }
        private void btnSaveToFavorites_Click( object sender, RoutedEventArgs e ){
            GNP00.btnFavfileOutput(true,SolSet:true,SolSet2:true);
        }
        private void cbxProbSolSetOutput_Checked( object sender, RoutedEventArgs e ){
            chbAddAlgorithmList.IsEnabled = (bool)cbxProbSolSetOutput.IsChecked;
            Color cr = chbAddAlgorithmList.IsEnabled? Colors.White: Colors.Gray;
            chbAddAlgorithmList.Foreground = new SolidColorBrush(cr); 
        }

       //Copy/Paste Puzzle(board<-->clipboard)
        private void Grid_PreviewKeyDown( object sender, KeyEventArgs e ){
            bool KeySft  = (Keyboard.Modifiers&ModifierKeys.Shift)>0;
            bool KeyCtrl = (Keyboard.Modifiers&ModifierKeys.Control)>0;

            if( e.Key==Key.C && KeyCtrl ){
                string st=pGP.CopyToBuffer();
                try{
                    Clipboard.Clear();
                    Clipboard.SetData(DataFormats.Text, st);
                }
                catch(System.Runtime.InteropServices.COMException){ /* NOP */ }
            }
            else if( e.Key==Key.F && KeyCtrl ){
                string st=pGP.ToGridString(KeySft);   
                try{
                    Clipboard.Clear();
                    Clipboard.SetData(DataFormats.Text, st);
                }
                catch(System.Runtime.InteropServices.COMException){ /* NOP */ }
            }
            else if( e.Key==Key.V && KeyCtrl ){
                string st=(string)Clipboard.GetData(DataFormats.Text);
                Clipboard.Clear();
                if( st==null || st.Length<81 ) return ;
                var UP=GNP00.SDK_ToUProblem(st,saveF:true); 
                if( UP==null) return;
                GNP00.CurrentPrbNo=999999999;//GNP00.SDKProbLst.Count-1
                _SetScreenProblem();
                _ResetAnalizer(false); //Clear analysis result

            }
        }
    #endregion File I/O, keyDown 
    }
}