﻿﻿/*******************************************************************************
 * Copyright (c) 2011 Rylan Cottrell. iCottrell.com.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Rylan Cottrell - initial API and implementation and/or initial documentation
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using HtmlAgilityPack;
using Microsoft.Phone.Shell;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Net.NetworkInformation;
using System.Windows.Media.Imaging;

namespace com.iCottrell.CanuckProductSafety
{
    public partial class ProductReportPage : PhoneApplicationPage
    {
        private String Url_HealthCanadaConsumerPrefix = "http://cpsr-rspc.hc-sc.gc.ca";

        MarketplaceDetailTask _marketPlaceDetailTask = new MarketplaceDetailTask();
        private String CurrentPage;
        public static String CndHealthSite = "http://www.hc-sc.gc.ca";
        public ProductReportPage()
        {
            InitializeComponent();
            LoadingProductScreen.Child = new PopupSplash();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string href = "";
            
            if (DeviceNetworkInformation.IsNetworkAvailable)
            {
                if (NavigationContext.QueryString.TryGetValue("external", out href))
                {
                    if (NavigationContext.QueryString.TryGetValue("href", out href))
                    {
                        if (!href.Contains("http://"))
                        {
                            href = CndHealthSite + href;
                        }
                        WebBrowserTask task = new WebBrowserTask();
                        task.Uri = new Uri(href);
                        task.Show();
                    }
                }
                else if (NavigationContext.QueryString.TryGetValue("href", out href))
                {
                    loadPage(href);
                }
            }
            else
            {
                this.NavigationService.Navigate(new Uri("/ErrorPageNoDataConn.xaml", UriKind.Relative));
            }
        }

        public void loadPage(String url)
        {
            if(url.Contains("http://") ) {
                CurrentPage = url;
            }else {
                CurrentPage = CndHealthSite + url;
            }

            if (CurrentPage != "")
            {
                if (CurrentPage.Contains("cpsr-rspc"))
                {
                    LoadingProductScreen.IsOpen = true;
                    HtmlWeb webGet = new HtmlWeb();
                    webGet.LoadCompleted += parse_ConsumerReportsPageCompleted;
                    webGet.LoadAsync(CurrentPage, Encoding.UTF8);
                }
                else
                {
                    LoadingProductScreen.IsOpen = true;
                    HtmlWeb webGet = new HtmlWeb();
                    webGet.LoadCompleted += parse_DownloadProductReportPageCompleted;
                    webGet.LoadAsync(CurrentPage, Encoding.UTF8);
                }
            }
        }

        public static string ConvertWhitespacesToSingleSpaces(string value)
        {
            if (value != null)
            {
                value = Regex.Replace(value, @"\s+", " ");
                return value;
            }
            else
            {
                return " ";
            }
        }

        public void parse_ConsumerReportsPageCompleted(Object sender, HtmlDocumentLoadCompleted e)
        {
            if (e != null && e.Document != null && e.Document.DocumentNode != null)
            {
                IList<Block> pageBody = new List<Block>();
                IList<HtmlNode> hnc = e.Document.DocumentNode.DescendantNodes().ToList();
                Paragraph paragraph = new Paragraph();
                Boolean content_flag = false;
                Boolean title_flag = false;
                foreach (HtmlNode node in hnc)
                {
                    if (node.Name.ToLower() == "#comment")
                    {
                        if (node.InnerText.Contains("CONTENT TITLE BEGINS"))
                        {
                            content_flag = true;
                        }
                        else if (content_flag && node.InnerText.Contains("CONTENT ENDS"))
                        {
                            content_flag = false;
                        }

                    }else if(content_flag && node.Name.ToLower() == "h2")
                    {
                        if (node.InnerText.Contains("Product Name"))
                        {
                            title_flag = true;
                        }
                        else
                        {
                            title_flag = false;
                            if (paragraph.Inlines.Count > 0)
                            {
                                pageBody.Add(paragraph);
                                paragraph = new Paragraph();
                            }
                            Paragraph h2 = new Paragraph();
                            h2.FontSize = 25.333;
                            h2.Inlines.Add(ConvertWhitespacesToSingleSpaces(node.InnerText.Trim()));
                            pageBody.Add(h2);
                        }
                    }
                    else if (content_flag && node.Name.ToLower() == "img")
                    {
                        if (paragraph.Inlines.Count > 0)
                        {
                            pageBody.Add(paragraph);
                            paragraph = new Paragraph();
                        }

                        String href = "";
                        foreach (HtmlAttribute att in node.Attributes)
                        {
                            if (att.Name.ToLower() == "src")
                            {
                                href = att.Value;
                            }
                        }
                        if (href != "")
                        {
                            InlineUIContainer con = new InlineUIContainer();
                            Image i = new Image();
                            i.Source = new BitmapImage(new Uri(Url_HealthCanadaConsumerPrefix + href, UriKind.Absolute));
                            con.Child = i;
                            paragraph.Inlines.Add(con);
                        }
                    }
                    else if (content_flag && node.Name.ToLower() == "p")
                    {
                        if (ConvertWhitespacesToSingleSpaces(node.InnerText) == " ")
                        {
                            continue;
                        }
                        if (title_flag)
                        {
                            PageTitle.Text = ConvertWhitespacesToSingleSpaces(node.InnerText.Replace("�", "'").Trim());
                            title_flag = false;
                        }
                        else
                        {
                            if (paragraph.Inlines.Count > 0)
                            {
                                pageBody.Add(paragraph);
                                paragraph = new Paragraph();
                            }

                            Paragraph np = new Paragraph();

                            foreach (HtmlNode pc in node.DescendantNodes().ToList())
                            {
                                if (content_flag && node.Name.ToLower() == "ul")
                                {
                                    foreach (HtmlNode a in node.DescendantNodes().ToList())
                                    {
                                        if (a.Name.ToLower() == "a")
                                        {
                                            Boolean err_flag = false;
                                            Hyperlink hl = new Hyperlink();

                                            hl.Inlines.Add(ConvertWhitespacesToSingleSpaces(a.InnerText.Replace("&nbsp;", "")));
                                            foreach (HtmlAttribute att1 in a.Attributes)
                                            {
                                                if (att1.Name.ToLower() == "href")
                                                {
                                                    try
                                                    {
                                                        if (att1.Value.ToCharArray()[0] != '#')
                                                        {
                                                            hl.NavigateUri = new Uri("/ProductReportPage.xaml?external=true&href=" + att1.Value, UriKind.Relative);

                                                        }
                                                        else
                                                        {
                                                            err_flag = true;
                                                        }
                                                    }
                                                    catch (Exception err)
                                                    {
                                                        err_flag = true;
                                                    }
                                                    break;
                                                }
                                            }
                                            if (!err_flag)
                                            {
                                                np.Inlines.Add(hl);
                                            }
                                            else
                                            {
                                                Run r = new Run();
                                                r.Text = ConvertWhitespacesToSingleSpaces(a.InnerText.Replace("&nbsp;", ""));
                                                np.Inlines.Add(r);
                                            }
                                        }
                                        else if (a.Name.ToLower() == "li")
                                        {
                                            Bold b = new Bold();
                                            b.Inlines.Add("- ");
                                            np.Inlines.Add(b);
                                        }
                                        else if (a.Name.ToLower() == "b" || a.ParentNode.Name.ToLower() == "strong")
                                        {
                                            Bold b = new Bold();
                                            b.Inlines.Add(ConvertWhitespacesToSingleSpaces(a.InnerText.Replace("&nbsp;", "")));
                                            np.Inlines.Add(b);
                                        }
                                        else if (a.Name.ToLower() == "i")
                                        {
                                            Italic i = new Italic();
                                            i.Inlines.Add(ConvertWhitespacesToSingleSpaces(a.InnerText.Replace("&nbsp;", "")));
                                            np.Inlines.Add(i);
                                        }
                                        else if (a.Name.ToLower() == "#text")
                                        {
                                            if (a.ParentNode.Name.ToLower() != "a" && a.ParentNode.Name.ToLower() != "b" && a.ParentNode.Name.ToLower() != "strong" && a.ParentNode.Name.ToLower() != "i")
                                            {

                                                String str = ConvertWhitespacesToSingleSpaces(a.InnerText);
                                                if (str != " ")
                                                {
                                                    str = str.Replace("&quot;", "\"");
                                                    Run run = new Run();
                                                    run.Text = str.Replace("&nbsp;", "");
                                                    np.Inlines.Add(run);
                                                }
                                                if (a.NextSibling != null && a.NextSibling.Name.ToLower() == "li")
                                                {
                                                    np.Inlines.Add(new LineBreak());
                                                }
                                            }

                                        }
                                    }
                                    np.Inlines.Add(new LineBreak());
                                }
                                else if (pc.Name.ToLower() == "a")
                                {
                                    if (pc.ParentNode.Name.ToLower() != "i" && pc.ParentNode.Name.ToLower() != "b" && pc.ParentNode.Name.ToLower() != "strong")
                                    {
                                        Boolean image_flag = false;
                                        Boolean flag_local = false;
                                        Hyperlink hl = new Hyperlink();

                                        hl.Inlines.Add(ConvertWhitespacesToSingleSpaces(pc.InnerText.Replace("&nbsp;", "")));

                                        foreach (HtmlAttribute att1 in pc.Attributes)
                                        {
                                            if (att1.Name.ToLower() == "href")
                                            {
                                                try
                                                {
                                                    if (att1.Value.ToCharArray()[0] != '#')
                                                    {
                                                        hl.NavigateUri = new Uri("/ProductReportPage.xaml?external=true&href=" + att1.Value, UriKind.Relative);
                                                    }
                                                    else
                                                    {
                                                        flag_local = true;
                                                    }
                                                }
                                                catch (Exception err)
                                                {
                                                    image_flag = true;
                                                }
                                            }
                                            else if (att1.Name.ToLower() == "class" && att1.Value == "image")
                                            {
                                                image_flag = true;
                                            }
                                        }
                                        if (!image_flag && !flag_local)
                                        {
                                            np.Inlines.Add(hl);
                                        }
                                        else if (flag_local)
                                        {
                                            Run r = new Run();
                                            r.Text = ConvertWhitespacesToSingleSpaces(pc.InnerText.Replace("&nbsp;", ""));
                                            np.Inlines.Add(r);
                                        }
                                    }
                                }
                                else if (pc.Name.ToLower() == "#text")
                                {
                                    if (pc.ParentNode.Name.ToLower() != "a" && pc.ParentNode.Name.ToLower() != "b" && pc.ParentNode.Name.ToLower() != "strong" && pc.ParentNode.Name.ToLower() != "i")
                                    {
                                        String str = ConvertWhitespacesToSingleSpaces(pc.InnerText);
                                        if (str != " ")
                                        {
                                            str = str.Replace("&quot;", "\"");
                                            Run run = new Run();
                                            run.Text = str.Replace("&nbsp;", "");
                                            np.Inlines.Add(run);
                                        }
                                    }
                                }
                                else if (pc.Name.ToLower() == "b" || pc.Name.ToLower() == "strong")
                                {
                                    Bold b = new Bold();
                                    foreach (HtmlNode n in pc.DescendantNodes().ToList())
                                    {
                                        if (n.Name.ToLower() == "#text" && n.ParentNode.Name.ToLower() != "a")
                                        {
                                            b.Inlines.Add(ConvertWhitespacesToSingleSpaces(n.InnerText.Replace("&nbsp;", "")));
                                        }
                                        else if (n.Name.ToLower() == "a")
                                        {
                                            Boolean image_flag = false;
                                            Boolean flag_local = false;
                                            Hyperlink hl = new Hyperlink();

                                            hl.Inlines.Add(ConvertWhitespacesToSingleSpaces(n.InnerText.Replace("&nbsp;", "")));

                                            foreach (HtmlAttribute att1 in n.Attributes)
                                            {
                                                if (att1.Name.ToLower() == "href")
                                                {
                                                    try
                                                    {
                                                        if (att1.Value.ToCharArray()[0] != '#')
                                                        {
                                                            hl.NavigateUri = new Uri("/ProductReportPage.xaml?external=true&href=" + att1.Value, UriKind.Relative);
                                                        }
                                                        else
                                                        {
                                                            flag_local = true;
                                                        }
                                                    }
                                                    catch (Exception err)
                                                    {
                                                        image_flag = true;
                                                    }
                                                }
                                                else if (att1.Name.ToLower() == "class" && att1.Value == "image")
                                                {
                                                    image_flag = true;
                                                }
                                            }
                                            if (!image_flag && !flag_local)
                                            {
                                                b.Inlines.Add(hl);
                                            }
                                            else
                                            {
                                                b.Inlines.Add(ConvertWhitespacesToSingleSpaces(n.InnerText.Replace("&nbsp;", "")));
                                            }
                                        }
                                    }

                                    np.Inlines.Add(b);
                                }
                                else if (pc.Name.ToLower() == "br")
                                {
                                    // Run r = new Run();
                                    // r.Text = "\n";
                                    //  np.Inlines.Add(r);
                                }
                                else if(pc.Name.ToLower() == "img" )
                                {
                                    String href = "";
                                    foreach (HtmlAttribute att in pc.Attributes)
                                    {
                                        if (att.Name.ToLower() == "src")
                                        {
                                            href = att.Value;
                                        }
                                    }
                                    if (href != "")
                                    {
                                        InlineUIContainer con = new InlineUIContainer();
                                        Image i = new Image();
                                        i.Source = new BitmapImage(new Uri(Url_HealthCanadaConsumerPrefix+href, UriKind.Absolute));
                                        con.Child = i;
                                        np.Inlines.Add(con);
                                    }
                                }
                                else if (pc.Name.ToLower() == "i")
                                {
                                    Italic i = new Italic();
                                    foreach (HtmlNode n in pc.DescendantNodes().ToList())
                                    {
                                        if (n.Name.ToLower() == "#text" && n.ParentNode.Name.ToLower() != "a")
                                        {
                                            i.Inlines.Add(ConvertWhitespacesToSingleSpaces(n.InnerText.Replace("&nbsp;", "")));
                                        }
                                        else if (n.Name.ToLower() == "a")
                                        {
                                            Boolean image_flag = false;
                                            Boolean flag_local = false;
                                            Hyperlink hl = new Hyperlink();

                                            hl.Inlines.Add(ConvertWhitespacesToSingleSpaces(n.InnerText.Replace("&nbsp;", "")));

                                            foreach (HtmlAttribute att1 in n.Attributes)
                                            {
                                                if (att1.Name.ToLower() == "href")
                                                {
                                                    try
                                                    {
                                                        if (att1.Value.ToCharArray()[0] != '#')
                                                        {
                                                            hl.NavigateUri = new Uri("/ProductReportPage.xaml?external=true&href=" + att1.Value, UriKind.Relative);
                                                        }
                                                        else
                                                        {
                                                            flag_local = true;
                                                        }
                                                    }
                                                    catch (Exception err)
                                                    {
                                                        image_flag = true;
                                                    }
                                                }
                                                else if (att1.Name.ToLower() == "class" && att1.Value == "image")
                                                {
                                                    image_flag = true;
                                                }
                                            }
                                            if (!image_flag && !flag_local)
                                            {
                                                i.Inlines.Add(hl);
                                            }
                                            else
                                            {
                                                i.Inlines.Add(n.InnerText.Replace("&nbsp;", ""));
                                            }
                                        }
                                    }

                                    np.Inlines.Add(i);
                                }
                            }
                            np.Inlines.Add(new LineBreak());
                            pageBody.Add(np);

                        }
                    }
                }
                if (paragraph != null && paragraph.Inlines.Count > 0)
                {
                    pageBody.Add(paragraph);
                }
                foreach (Block b in pageBody)
                {
                    RichTextBox rtb = new RichTextBox();
                    rtb.IsReadOnly = true;
                    rtb.VerticalAlignment = VerticalAlignment.Top;
                    rtb.Blocks.Add(b);
                    PageBody.Children.Add(rtb);

                }
                LoadingProductScreen.IsOpen = false;
                PageBody.InvalidateArrange();
                PageBody.InvalidateMeasure();
                scrollerViewer.InvalidateArrange();
                scrollerViewer.InvalidateMeasure();
                scrollerViewer.InvalidateScrollInfo();
                Content.InvalidateArrange();
                Content.InvalidateMeasure();
            }
        }

        public void parse_DownloadProductReportPageCompleted(Object sender, HtmlDocumentLoadCompleted e)
        {
            if (e != null && e.Document != null && e.Document.DocumentNode != null)
            {
                IList<Block> pageBody = new List<Block>(); 
                IList<HtmlNode> hnc = e.Document.DocumentNode.DescendantNodes().ToList();
                
                Paragraph paragraph = new Paragraph();
                Boolean content_flag = false;

                foreach (HtmlNode htmlNode in hnc)
                {
                    if (htmlNode.Name.ToLower() == "h1" && htmlNode.Attributes.Count == 0)
                    {
                        PageTitle.Text = htmlNode.InnerText.Replace("�", "'");
                        content_flag = true;

                    }
                    else if (content_flag && htmlNode.Name.ToLower() == "h2")
                    {
                        if (paragraph.Inlines.Count > 0)
                        {
                            pageBody.Add(paragraph);
                            paragraph = new Paragraph();
                        }
                        if (htmlNode.InnerText == "Related Health Canada Web content:")
                        {
                            content_flag = false;
                        }
                        else
                        {
                            Paragraph h2 = new Paragraph();
                            h2.FontSize = 25.333;
                            h2.Inlines.Add(htmlNode.InnerText.Trim());
                            pageBody.Add(h2);
                        }
                    }
                    else if (content_flag && htmlNode.Name.ToLower() == "ul")
                    {
                        if (htmlNode.ParentNode.Name.ToLower() != "p")
                        {
                            if (paragraph.Inlines.Count > 0)
                            {
                                pageBody.Add(paragraph);
                                paragraph = new Paragraph();
                            }

                            Paragraph np = new Paragraph();
                            foreach (HtmlNode a in htmlNode.DescendantNodes().ToList())
                            {
                                if (a.Name.ToLower() == "a")
                                {
                                    Boolean err_flag = false;
                                    Hyperlink hl = new Hyperlink();

                                    hl.Inlines.Add(a.InnerText);
                                    foreach (HtmlAttribute att1 in a.Attributes)
                                    {
                                        if (att1.Name.ToLower() == "href")
                                        {
                                            try
                                            {
                                                if (att1.Value.ToCharArray()[0] != '#')
                                                {
                                                    hl.NavigateUri = new Uri("/ProductReportPage.xaml?external=true&href=" + att1.Value, UriKind.Relative);

                                                }
                                                else
                                                {
                                                    err_flag = true;
                                                }
                                            }
                                            catch (Exception err)
                                            {
                                                err_flag = true;
                                            }
                                            break;
                                        }
                                    }
                                    if (!err_flag)
                                    {
                                        np.Inlines.Add(hl);
                                    }
                                    else
                                    {
                                        Run r = new Run();
                                        r.Text = a.InnerText.Replace("&nbsp;", "");
                                        np.Inlines.Add(r);
                                    }
                                }
                                else if (a.Name.ToLower() == "li")
                                {
                                    Bold b = new Bold();
                                    b.Inlines.Add("- ");
                                    np.Inlines.Add(b);
                                }
                                else if (a.Name.ToLower() == "b" || a.ParentNode.Name.ToLower() == "strong")
                                {
                                    Bold b = new Bold();
                                    b.Inlines.Add(a.InnerText.Replace("&nbsp;", ""));
                                    np.Inlines.Add(b);
                                }
                                else if (a.Name.ToLower() == "i")
                                {
                                    Italic i = new Italic();
                                    i.Inlines.Add(a.InnerText.Replace("&nbsp;", ""));
                                    np.Inlines.Add(i);
                                }
                                else if (a.Name.ToLower() == "#text")
                                {
                                    if (a.ParentNode.Name.ToLower() != "a" && a.ParentNode.Name.ToLower() != "b" && a.ParentNode.Name.ToLower() != "strong" && a.ParentNode.Name.ToLower() != "i")
                                    {

                                        String str = ConvertWhitespacesToSingleSpaces(a.InnerText);
                                        if (str != " ")
                                        {
                                            str = str.Replace("&quot;", "\"");
                                            Run run = new Run();
                                            run.Text = str.Replace("&nbsp;", "");
                                            np.Inlines.Add(run);
                                        }
                                        if (a.NextSibling != null && a.NextSibling.Name.ToLower() == "li")
                                        {
                                            np.Inlines.Add(new LineBreak());
                                        }
                                    }

                                }
                            }
                            np.Inlines.Add(new LineBreak());
                            pageBody.Add(np);
                        }
                    }
                    else if (content_flag && htmlNode.Name.ToLower() == "p")
                    {
                        if (paragraph.Inlines.Count > 0)
                        {
                            pageBody.Add(paragraph);
                            paragraph = new Paragraph();
                        }

                        Paragraph np = new Paragraph();

                        foreach (HtmlNode pc in htmlNode.DescendantNodes().ToList())
                        {
                            if (content_flag && htmlNode.Name.ToLower() == "ul")
                            {
                                foreach (HtmlNode a in htmlNode.DescendantNodes().ToList())
                                {
                                    if (a.Name.ToLower() == "a")
                                    {
                                        Boolean err_flag = false;
                                        Hyperlink hl = new Hyperlink();

                                        hl.Inlines.Add(a.InnerText.Replace("&nbsp;", ""));
                                        foreach (HtmlAttribute att1 in a.Attributes)
                                        {
                                            if (att1.Name.ToLower() == "href")
                                            {
                                                try
                                                {
                                                    if (att1.Value.ToCharArray()[0] != '#')
                                                    {
                                                        hl.NavigateUri = new Uri("/ProductReportPage.xaml?external=true&href=" + att1.Value, UriKind.Relative);

                                                    }
                                                    else
                                                    {
                                                        err_flag = true;
                                                    }
                                                }
                                                catch (Exception err)
                                                {
                                                    err_flag = true;
                                                }
                                                break;
                                            }
                                        }
                                        if (!err_flag)
                                        {
                                            np.Inlines.Add(hl);
                                        }
                                        else
                                        {
                                            Run r = new Run();
                                            r.Text = a.InnerText.Replace("&nbsp;", "");
                                            np.Inlines.Add(r);
                                        }
                                    }
                                    else if (a.Name.ToLower() == "li")
                                    {
                                        Bold b = new Bold();
                                        b.Inlines.Add("- ");
                                        np.Inlines.Add(b);
                                    }
                                    else if (a.Name.ToLower() == "b" || a.ParentNode.Name.ToLower() == "strong")
                                    {
                                        Bold b = new Bold();
                                        b.Inlines.Add(a.InnerText.Replace("&nbsp;", ""));
                                        np.Inlines.Add(b);
                                    }
                                    else if (a.Name.ToLower() == "i")
                                    {
                                        Italic i = new Italic();
                                        i.Inlines.Add(a.InnerText.Replace("&nbsp;", ""));
                                        np.Inlines.Add(i);
                                    }
                                    else if (a.Name.ToLower() == "#text")
                                    {
                                        if (a.ParentNode.Name.ToLower() != "a" && a.ParentNode.Name.ToLower() != "b" && a.ParentNode.Name.ToLower() != "strong" && a.ParentNode.Name.ToLower() != "i")
                                        {

                                            String str = ConvertWhitespacesToSingleSpaces(a.InnerText);
                                            if (str != " ")
                                            {
                                                str = str.Replace("&quot;", "\"");
                                                Run run = new Run();
                                                run.Text = str.Replace("&nbsp;", "");
                                                np.Inlines.Add(run);
                                            }
                                            if (a.NextSibling != null && a.NextSibling.Name.ToLower() == "li")
                                            {
                                                np.Inlines.Add(new LineBreak());
                                            }
                                        }

                                    }
                                }
                                np.Inlines.Add(new LineBreak());
                            } else if (pc.Name.ToLower() == "a")
                            {
                                if (pc.ParentNode.Name.ToLower() != "i" && pc.ParentNode.Name.ToLower() != "b" && pc.ParentNode.Name.ToLower() != "strong")
                                {
                                    Boolean image_flag = false;
                                    Boolean flag_local = false;
                                    Hyperlink hl = new Hyperlink();

                                    hl.Inlines.Add(pc.InnerText.Replace("&nbsp;", ""));

                                    foreach (HtmlAttribute att1 in pc.Attributes)
                                    {
                                        if (att1.Name.ToLower() == "href")
                                        {
                                            try
                                            {
                                                if (att1.Value.ToCharArray()[0] != '#')
                                                {
                                                    hl.NavigateUri = new Uri("/ProductReportPage.xaml?external=true&href=" + att1.Value, UriKind.Relative);
                                                }
                                                else
                                                {
                                                    flag_local = true;
                                                }
                                            }
                                            catch (Exception err)
                                            {
                                                image_flag = true;
                                            }
                                        }
                                        else if (att1.Name.ToLower() == "class" && att1.Value == "image")
                                        {
                                            image_flag = true;
                                        }
                                    }
                                    if (!image_flag && !flag_local)
                                    {
                                        np.Inlines.Add(hl);
                                    }
                                    else if (flag_local)
                                    {
                                        Run r = new Run();
                                        r.Text = pc.InnerText.Replace("&nbsp;", "");
                                        np.Inlines.Add(r);
                                    }
                                }
                            }
                            else if (pc.Name.ToLower() == "#text")
                            {
                                if (pc.ParentNode.Name.ToLower() != "a" && pc.ParentNode.Name.ToLower() != "b" && pc.ParentNode.Name.ToLower() != "strong" && pc.ParentNode.Name.ToLower() != "i")
                                {
                                    String str = ConvertWhitespacesToSingleSpaces(pc.InnerText);
                                    if (str != " ")
                                    {
                                        str = str.Replace("&quot;", "\"");
                                        Run run = new Run();
                                        run.Text = str.Replace("&nbsp;", "");
                                        np.Inlines.Add(run);
                                    }
                                }
                            }
                            else if (pc.Name.ToLower() == "b" || pc.Name.ToLower() == "strong")
                            {
                                Bold b = new Bold();
                                foreach (HtmlNode n in pc.DescendantNodes().ToList())
                                {
                                    if (n.Name.ToLower() == "#text" && n.ParentNode.Name.ToLower() != "a")
                                    {
                                        b.Inlines.Add(n.InnerText.Replace("&nbsp;", ""));
                                    }
                                    else if (n.Name.ToLower() == "a")
                                    {
                                        Boolean image_flag = false;
                                        Boolean flag_local = false;
                                        Hyperlink hl = new Hyperlink();

                                        hl.Inlines.Add(n.InnerText.Replace("&nbsp;", ""));

                                        foreach (HtmlAttribute att1 in n.Attributes)
                                        {
                                            if (att1.Name.ToLower() == "href")
                                            {
                                                try
                                                {
                                                    if (att1.Value.ToCharArray()[0] != '#')
                                                    {
                                                        hl.NavigateUri = new Uri("/ProductReportPage.xaml?external=true&href=" + att1.Value, UriKind.Relative);
                                                    }
                                                    else
                                                    {
                                                        flag_local = true;
                                                    }
                                                }
                                                catch (Exception err)
                                                {
                                                    image_flag = true;
                                                }
                                            }
                                            else if (att1.Name.ToLower() == "class" && att1.Value == "image")
                                            {
                                                image_flag = true;
                                            }
                                        }
                                        if (!image_flag && !flag_local)
                                        {
                                            b.Inlines.Add(hl);
                                        }
                                        else
                                        {
                                            b.Inlines.Add(n.InnerText.Replace("&nbsp;", ""));
                                        }
                                    }
                                }

                                np.Inlines.Add(b);
                            }
                            else if (pc.Name.ToLower() == "br")
                            {
                               // Run r = new Run();
                               // r.Text = "\n";
                              //  np.Inlines.Add(r);
                            }
                            else if (pc.Name.ToLower() == "i")
                            {
                                Italic i = new Italic();
                                foreach (HtmlNode n in pc.DescendantNodes().ToList())
                                {
                                    if (n.Name.ToLower() == "#text" && n.ParentNode.Name.ToLower() != "a")
                                    {
                                        i.Inlines.Add(n.InnerText.Replace("&nbsp;", ""));
                                    }
                                    else if (n.Name.ToLower() == "a")
                                    {
                                        Boolean image_flag = false;
                                        Boolean flag_local = false;
                                        Hyperlink hl = new Hyperlink();

                                        hl.Inlines.Add(n.InnerText.Replace("&nbsp;", ""));

                                        foreach (HtmlAttribute att1 in n.Attributes)
                                        {
                                            if (att1.Name.ToLower() == "href")
                                            {
                                                try
                                                {
                                                    if (att1.Value.ToCharArray()[0] != '#')
                                                    {
                                                        hl.NavigateUri = new Uri("/ProductReportPage.xaml?external=true&href=" + att1.Value, UriKind.Relative);
                                                    }
                                                    else
                                                    {
                                                        flag_local = true;
                                                    }
                                                }
                                                catch (Exception err)
                                                {
                                                    image_flag = true;
                                                }
                                            }
                                            else if (att1.Name.ToLower() == "class" && att1.Value == "image")
                                            {
                                                image_flag = true;
                                            }
                                        }
                                        if (!image_flag && !flag_local)
                                        {
                                            i.Inlines.Add(hl);
                                        }
                                        else
                                        {
                                            i.Inlines.Add(n.InnerText.Replace("&nbsp;", ""));
                                        }
                                    }
                                }

                                np.Inlines.Add(i);
                            }
                        }
                        np.Inlines.Add(new LineBreak());
                        pageBody.Add(np);

                    }
                   
                }
                if (paragraph != null && paragraph.Inlines.Count > 0)
                {
                    pageBody.Add(paragraph);
                }
                foreach (Block b in pageBody)
                {
                    RichTextBox rtb = new RichTextBox();
                    rtb.IsReadOnly = true;
                    rtb.VerticalAlignment = VerticalAlignment.Top;
                    rtb.Blocks.Add(b);
                    PageBody.Children.Add(rtb);

                }
                LoadingProductScreen.IsOpen = false;
                PageBody.InvalidateArrange();
                PageBody.InvalidateMeasure();
                scrollerViewer.InvalidateArrange();
                scrollerViewer.InvalidateMeasure();
                scrollerViewer.InvalidateScrollInfo();
                Content.InvalidateArrange();
                Content.InvalidateMeasure();
            }
        }

        private void OpenAbout(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void ShareProductPage(object sender, EventArgs e)
        {
            ShareLinkTask slt = new ShareLinkTask();
            slt.LinkUri = new Uri(CurrentPage);
            slt.Title = "Important - Product Safety Information";
            slt.Message = PageTitle.Text;
            slt.Show();

        }
        private void EmailProductPage(object sender, EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "";
            emailComposeTask.Body = "Health Canada has issued a safety notice for <a href=\"" + CurrentPage + "\">" + PageTitle.Text + "</a> that may be of concern to you.";
            emailComposeTask.Subject = "Important - Product Safety Information";
            emailComposeTask.Show(); 
        }
        private void EmailBug(object sender, EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "dev@icottrell.com";
            emailComposeTask.Body = "There is a bug with the product page "+CurrentPage+"\n <------Comments-------->\n";
            emailComposeTask.Subject = "Bug Report - Canuck Product Safety";
            emailComposeTask.Show(); 
        }
    }
}