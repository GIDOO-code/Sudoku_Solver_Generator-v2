using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Threading;
using System.Diagnostics;

namespace GNPZ_sdk{
    public partial class ToGNPXHP: Window {
        public DispatcherTimer CountDownTimer;
        private int WarpToCC=8;

        public ToGNPXHP( DateTime ExpireDate ){
            InitializeComponent();
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();

            LblExpireDate.Content = "Available date:"+ExpireDate.ToShortDateString();
            WarpToHP.Content = "Warp to GNPX-HP after "+WarpToCC.ToString()+" seconds";

            CountDownTimer = new DispatcherTimer(DispatcherPriority.Normal);
            CountDownTimer.Interval = TimeSpan.FromMilliseconds(1000);
            CountDownTimer.Tick += new EventHandler(CountDownTimer_Tick);
            CountDownTimer.Start();
        }

        private void CountDownTimer_Tick( object sender, EventArgs e ){
            if( (--WarpToCC)==0 ){
                Process.Start("http://csdenpe.web.fc2.com");
                Environment.Exit(0);
            }
            else{ 
                WarpToHP.Content = "Warp to GNPX-HP after "+WarpToCC.ToString()+" seconds";
            }
        }
    }
}