#region File Info
// File      : VisualDocumentPaginator.cs
// Description: 
// Package   : VisualPrint
//
// Authors   : Fred Song
//
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;
using System.Windows.Controls;

namespace VisualPrint{
    public class VisualDocumentPaginator: DocumentPaginator{
        Size m_PageSize;
        Size m_Margin;
        DocumentPaginator m_Paginator = null;
        int m_PageCount;
        Size m_ContentSize;
        ContainerVisual m_PageContent;
        ContainerVisual m_SmallerPage;
        ContainerVisual m_SmallerPageContainer;
        ContainerVisual m_NewPage;
        
        public VisualDocumentPaginator(DocumentPaginator paginator, Size pageSize, Size margin ){
            m_PageSize = pageSize;
            m_Margin = margin;
            m_Paginator = paginator;
            m_ContentSize = new Size(pageSize.Width - 2 * margin.Width, pageSize.Height - 2 * margin.Height);
            m_PageCount = (int)Math.Ceiling(m_Paginator.PageSize.Height / m_ContentSize.Height);
            m_Paginator.PageSize = m_ContentSize;
            m_PageContent = new ContainerVisual();
            m_SmallerPage = new ContainerVisual();
            m_NewPage = new ContainerVisual();
            m_SmallerPageContainer = new ContainerVisual();
        }

        Rect Move(Rect rect){
            if( rect.IsEmpty) return rect;
            else{
                return new Rect( rect.Left+m_Margin.Width, rect.Top+m_Margin.Height, rect.Width, rect.Height );
            }
        }

        public override DocumentPage GetPage(int pageNumber){
            m_PageContent.Children.Clear();
            m_SmallerPage.Children.Clear();
            m_NewPage.Children.Clear();
            m_SmallerPageContainer.Children.Clear();
            DrawingVisual title = new DrawingVisual();
            using( DrawingContext ctx = title.RenderOpen() ){
                FontFamily font = new FontFamily("Times New Roman");
                Typeface typeface = new Typeface(font, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                FormattedText text = new FormattedText("Page " + (pageNumber + 1) + " of " + m_PageCount,
                    System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    typeface, 14, Brushes.Black);
                ctx.DrawText(text, new Point(0, 0)); 
            }
            
            DocumentPage page = m_Paginator.GetPage(0);
            m_PageContent.Children.Add(page.Visual);
            RectangleGeometry clip = new RectangleGeometry(new Rect(0, m_ContentSize.Height * pageNumber, m_ContentSize.Width, m_ContentSize.Height));
            m_PageContent.Clip = clip;
            m_PageContent.Transform = new TranslateTransform(0, -m_ContentSize.Height * pageNumber);
            m_SmallerPage.Children.Add(m_PageContent);
            m_SmallerPage.Transform = new ScaleTransform(0.95,0.95);
            m_SmallerPageContainer.Children.Add(m_SmallerPage );
            m_SmallerPageContainer.Transform = new TranslateTransform(0, 24);
            m_NewPage.Children.Add(title );
            m_NewPage.Children.Add(m_SmallerPageContainer);
            m_NewPage.Transform = new TranslateTransform(m_Margin.Width, m_Margin.Height);
            return new DocumentPage(m_NewPage, m_PageSize, Move(page.BleedBox),Move(page.ContentBox));
        }

        public override bool IsPageCountValid{
            get{ return true; }
        }

        public override int PageCount{
            get{ return m_PageCount; }
        }

        public override Size PageSize{
            get{ return m_Paginator.PageSize; }
            set{ m_Paginator.PageSize = value; }
        }

        public override IDocumentPaginatorSource Source{
            get{
                if( m_Paginator!=null ) return m_Paginator.Source;
                return null;
            }
        }
    }
}
  