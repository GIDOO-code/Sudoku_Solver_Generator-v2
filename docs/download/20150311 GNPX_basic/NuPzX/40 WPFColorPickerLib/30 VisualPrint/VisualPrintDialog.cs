#region File Info
// File      : VisualPrintDialog.cs
// Description: 
// Package   : VisualPrint
//
// Authors   : Fred Song
//
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Printing;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;

//WPF Visual Print Component
//http://www.codeproject.com/Articles/164033/WPF-Visual-Print-Component

namespace VisualPrint{
    public class VisualPrintDialog{
        //private PrintWindow m_Window;
        private Visual m_Visual;
        private PrintDialog m_PrtDlg;

        public VisualPrintDialog( Visual visual ){
            //m_Window = new PrintWindow(visual);
            m_Visual = visual;
            m_PrtDlg = new PrintDialog();
            m_PrtDlg.UserPageRangeEnabled = false;
        }

        #region 'Public Properties'

        public Visual Visual{
            get{ return m_Visual; }
            set{ m_Visual = value; }
        }

        #endregion

        #region 'Public Methods'
        public void ShowDialog(){
            try{
                bool? result = m_PrtDlg.ShowDialog();
                FlowDocument flowDocument = null;
                if( (bool)result ){
                    var OverrideCursorMemo = System.Windows.Input.Mouse.OverrideCursor;
                    System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    try{
                        flowDocument = Helper.CreateFlowDocument(m_Visual, new Size(m_PrtDlg.PrintableAreaWidth, m_PrtDlg.PrintableAreaHeight));
                    }
                    finally{
                        System.Windows.Input.Mouse.OverrideCursor = OverrideCursorMemo;
                    }
                    if( flowDocument!=null){
                        PrintPreview(flowDocument);
                    }
                }
            }
            catch( Exception ex ){
                Console.WriteLine( ex.Message );
                Console.WriteLine( ex.StackTrace );
            }
        }
        #endregion

        #region 'Private Methods/Events'
        
        private void OnPrint( object sender, RoutedEventArgs e ){
            FlowDocument document = (e.Source as PreviewWindow).Document;
            if( document!=null)
                Print(document);
        }

        private void Print(FlowDocument document){
            DocumentPaginator paginator = ((IDocumentPaginatorSource )document).DocumentPaginator;
            m_PrtDlg.PrintDocument(paginator, "Printing");
        }

        private void PrintPreview(FlowDocument document ){
            PreviewWindow win = new PreviewWindow(document);
            win.Print += new RoutedEventHandler(OnPrint);
            win.ShowDialog();
        }
 
        #endregion
    }
}
