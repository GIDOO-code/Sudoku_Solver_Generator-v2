using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Win32;

using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

//WPF で Zune のようなウィンドウを作るAdd Star
//http://grabacr.net/archives/480

namespace GNPZ_sdk{
    public partial class ToGNPXHP: Window {
        public DispatcherTimer CountDownTmr;
        private int WarpToCC=5;

        public ToGNPXHP( DateTime ExpireDate ) {
            InitializeComponent();
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();

            LblExpireDate.Content = "有効期限："+ExpireDate.ToShortDateString();
            WarpToHP.Content = WarpToCC.ToString()+" 秒後に GNPX-HP にワープ";

            CountDownTmr = new DispatcherTimer(DispatcherPriority.Normal);
            CountDownTmr.Interval = TimeSpan.FromMilliseconds(1000);
            CountDownTmr.Tick += new EventHandler(CountDownTmr_Tick);
            CountDownTmr.Start();
        }

        private void CountDownTmr_Tick( object sender, EventArgs e ){
            if( (--WarpToCC)==0 ){
                Process.Start("http://csdenp.web.fc2.com");
                Environment.Exit(0);
            }
            else{ 
                WarpToHP.Content = (WarpToCC).ToString()+" 秒後に GNPX-HP にワープ";
            }
        }
    }
}