using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GNPXcore{
    public partial class ExtendResultWin: Window{
        private NuPz_Win pGNP00win;

        public ExtendResultWin( NuPz_Win pGNP00win ){
			this.pGNP00win=pGNP00win;
            InitializeComponent();		
            devWin.Width = pGNP00win.Width;
            GNPXGNPX.Content = "GNPXcore "+DateTime.Now.Year;
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();
        }

        private void devWinClose_Click(object sender, RoutedEventArgs e){
            this.Hide();
        }

        public void SetText( string res ){
            ExtRes.Text=res;
        }
    }
}
