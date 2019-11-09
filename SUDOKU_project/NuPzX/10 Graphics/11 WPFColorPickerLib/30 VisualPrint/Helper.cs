#region File Info
// File      : Help.cs
// Description: Help class for convert XPS document to flowdocument
// Package   : Visual Print
//
// Authors   : Fred Song
//
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using System.IO;
using System.IO.Packaging;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Xps.Serialization;
using System.Linq;

namespace VisualPrint{
    public static class Helper{
        public static string StipAttributes(string srs, params string[] attributes){
            return System.Text.RegularExpressions.Regex.Replace(srs,
                string.Format(@"{0}(?:\s*=\s*(""[^""]*""|[^\s>]*))?",
                string.Join("|", attributes)),
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        }
        public static string ReplaceAttribute(string srs, string attributeName, string replacementValue ){
            return System.Text.RegularExpressions.Regex.Replace(srs,
                string.Format(@"{0}(?:\s*=\s*(""[^""]*""|[^\s>]*))?", attributeName ),
                string.Format("{0}=\"{1}\"", attributeName, replacementValue ),
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        }
        public static string ReplaceAttribute(string srs, string attributeName, string attributeValue, string replacementValue ){
            return srs.Replace(attributeValue, replacementValue );
        }
        public static string GetFileName( Uri uri ){
            if( !uri.IsAbsoluteUri ){
                string[] chunks = uri.OriginalString.Split('/');
                return chunks[chunks.Length - 1];
            }
            else{
                return uri.Segments[uri.Segments.Length-1];
            }
        }
        public static void SaveToDisk(XpsFont font, string path){
            string folder = System.IO.Path.GetDirectoryName(path);
            if( !Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            using( Stream stm = font.GetStream()){
                using( FileStream fs = new FileStream(path, FileMode.Create )){
                    byte[] dta = new byte[stm.Length];
                    stm.Read(dta, 0, dta.Length);
                    if( font.IsObfuscated ){
                        string guid = new Guid(GetFileName(font.Uri).Split('.')[0]).ToString("N");
                        DeobfuscateData(dta, guid);
                    }
                    fs.Write(dta, 0, dta.Length);
                }
            }
        }
        public static void SaveToDisk(XpsImage image, string path ){
            using( Stream stm = image.GetStream()){
                using( FileStream fs = new FileStream(path, FileMode.Create )){
                    byte[] dta = new byte[stm.Length];
                    stm.Read(dta, 0, dta.Length);
                    fs.Write(dta, 0, dta.Length);
                }
            }
        }

        /// <summary>
        /// Deobfuscate ODTTF
        /// </summary>
        /// <param name="fontData"></param>
        /// <param name="guid"></param>
        private static void DeobfuscateData(byte[] fontData, string guid ){
            byte[] guidBytes = new byte[16];
            for(int i=0; i<guidBytes.Length; i++ ){
                guidBytes[i] = Convert.ToByte(guid.Substring(i*2,2),16);
            }

            for(int i=0; i<32; i++ ){
                int gi = guidBytes.Length - (i%guidBytes.Length) - 1;
                fontData[i] ^= guidBytes[gi];
            }

        }

        public static FlowDocument ConvertXPSDocumentToFlowDocument( Stream stream ){
            FlowDocument fdoc = new FlowDocument();
            fdoc.FlowDirection = FlowDirection.LeftToRight;
            Package pkg = Package.Open(stream, FileMode.Open, FileAccess.Read);
            string pack = "pack://temp.xps";
            Uri uri = new Uri(pack);
            PackageStore.AddPackage(uri, pkg);
            XpsDocument _doc = new XpsDocument(pkg, CompressionOption.Fast, pack);
            DocumentPaginator xpsPaginator = ((IDocumentPaginatorSource )_doc.GetFixedDocumentSequence()).DocumentPaginator;
            DocumentPage fixedpage = xpsPaginator.GetPage(0);
            fdoc.PageHeight = fixedpage.Size.Height;
            fdoc.PageWidth = fixedpage.Size.Width;
            fdoc.ColumnGap = 0;
            fdoc.ColumnWidth = fixedpage.Size.Width;
            fdoc.PagePadding = new Thickness(0, 0, 0, 0);
            DocumentPaginator flowPainator = ((IDocumentPaginatorSource )fdoc).DocumentPaginator;
            flowPainator.PageSize = fixedpage.Size;
            IXpsFixedDocumentSequenceReader fixedDocSeqReader = _doc.FixedDocumentSequenceReader;
            Dictionary<string, string> imageList = new Dictionary<string, string>();
            Dictionary<string, string> fontList = new Dictionary<string, string>();
            foreach( IXpsFixedDocumentReader docReader in fixedDocSeqReader.FixedDocuments){
                foreach( IXpsFixedPageReader fixedPageReader in docReader.FixedPages){
                    while( fixedPageReader.XmlReader.Read()){
                        string page = fixedPageReader.XmlReader.ReadOuterXml();

                        foreach( XpsFont font in fixedPageReader.Fonts ){
                            string name = GetFileName(font.Uri);
                            string guid = new Guid(name.Split('.')[0]).ToString("N");
                            name = System.IO.Path.Combine(guid, System.IO.Path.GetFileNameWithoutExtension(name ) + ".ttf");
                            string path = string.Format(@"{0}\{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), name );
                            if( !fontList.ContainsKey(font.Uri.OriginalString)){
                                SaveToDisk(font, path);
                                fontList.Add(font.Uri.OriginalString, path);
                            }
                        }

                        foreach( XpsImage image in fixedPageReader.Images ){
                            //here to get images
                            string name = Helper.GetFileName(image.Uri);
                            string path = string.Format(@"{0}\{1}", System.IO.Path.GetTempPath(), name );

                            if( !imageList.ContainsKey(image.Uri.OriginalString) ){
                                imageList.Add(image.Uri.OriginalString, path);
                                Helper.SaveToDisk(image, path);
                            }
                        }

                        foreach( KeyValuePair<string,string> val in fontList ){
                            page = Helper.ReplaceAttribute(page, "FontUri", val.Key, val.Value );
                        }
                        foreach( KeyValuePair<string,string> val in imageList){
                            page = Helper.ReplaceAttribute(page, "ImageSource", val.Key, val.Value );
                        }

                        FixedPage fp = XamlReader.Load(new MemoryStream(Encoding.UTF8.GetBytes(page ))) as FixedPage;
                      //FixedPage fp = XamlReader.Load(new MemoryStream(Encoding.Default.GetBytes(page ))) as FixedPage;

                        fp.Children.OfType<Glyphs>().ToList().ForEach(glyph =>{
                               Binding b = new Binding();
                               b.Source = glyph;
                               b.Path = new PropertyPath(Glyphs.UnicodeStringProperty);
                               glyph.SetBinding(TextSearch.TextProperty, b);
                           });

                        BlockUIContainer cont = new BlockUIContainer();
                        cont.Child = fp;
                        ((Block)cont).Margin  = new Thickness(0);
                        ((Block)cont).Padding = new Thickness(0);
                        fdoc.Blocks.Add(cont);
                    }
                }
            }
            pkg.Close();
            PackageStore.RemovePackage(uri);
            return fdoc;
        }
        public static FlowDocument CreateFlowDocument(Visual visual, Size pageSize ){
            FrameworkElement fe = (visual as FrameworkElement);
            fe.Measure(new Size(Int32.MaxValue, Int32.MaxValue ));
            Size visualSize = fe.DesiredSize;
            //Size visualSize = new Size(fe.ActualWidth, fe.ActualHeight);
            fe.Arrange(new Rect(new Point(0, 0), visualSize ));
            MemoryStream stream = new MemoryStream();
            string pack = "pack://temp.xps";
            Uri uri = new Uri(pack);
            DocumentPaginator paginator;
            XpsDocument xpsDoc;
            using( Package container = Package.Open(stream,FileMode.Create ) ){
                PackageStore.AddPackage(uri, container);
                using( xpsDoc = new XpsDocument(container, CompressionOption.Fast, pack) ){
                    XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false );
                    rsm.SaveAsXaml(visual);
                    paginator = ((IDocumentPaginatorSource )xpsDoc.GetFixedDocumentSequence()).DocumentPaginator;
                    paginator.PageSize = visualSize; // new Size(1000, 5000);
                }
                PackageStore.RemovePackage(uri);
            }
            using( Package container = Package.Open(stream,FileMode.Create )){
                using( xpsDoc = new XpsDocument( container, CompressionOption.Fast, pack )){
                    paginator = new VisualDocumentPaginator(paginator, new Size(pageSize.Width, pageSize.Height), new Size(48, 48));
                    XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false );
                    rsm.SaveAsXaml(paginator);
                }
                PackageStore.RemovePackage(uri);
            }

            FlowDocument document = Helper.ConvertXPSDocumentToFlowDocument(stream);
            stream.Close();
            return document;
        }

    }
}
