using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace com.iCottrell.CanuckProductSafety
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private String Url_HealthCanadaProductRecall_2012 = "http://www.hc-sc.gc.ca/ahc-asc/media/advisories-avis/_2012/index-eng.php";
        private String Url_HealthCanadaProductRecall_2011 = "http://www.hc-sc.gc.ca/ahc-asc/media/advisories-avis/_2011/index-eng.php";
        private String Url_HealthCanadaProductRecall_f2011 = "http://www.hc-sc.gc.ca/ahc-asc/media/advisories-avis/_fpa-ape_2011/index-eng.php";
        private String Url_HealthCanadaConsumer = "http://cpsr-rspc.hc-sc.gc.ca/PR-RP/results-resultats-eng.jsp?searchstring=&searchyear=2012&searchyear=2011&searchcategory=";
        private String Url_HealthCanadaConsumerPrefix = "http://cpsr-rspc.hc-sc.gc.ca";
        //private String Url_HealthCanadaProductRecallCategory = "http://www.hc-sc.gc.ca/ahc-asc/media/advisories-avis/alpha-eng.php";
        //private String Url_Prefix = "http://www.hc-sc.gc.ca";

        public MainViewModel()
        {
            this.Spinnner = new DataLoaded();
            this.Spinnner.ShowSpinner = true;
            this.Items = new ObservableCollection<ProductViewModel>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ProductViewModel> Items { get; private set; }

        public DataLoaded Spinnner { get; private set; }

        private string _sampleProperty = "Sample Runtime Property Value";
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string SampleProperty
        {
            get
            {
                return _sampleProperty;
            }
            set
            {
                if (value != _sampleProperty)
                {
                    _sampleProperty = value;
                    NotifyPropertyChanged("SampleProperty");
                }
            }
        }

        public bool IsDataLoaded
        {
            get;
            set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            HtmlWeb webGet = new HtmlWeb();
            webGet.LoadCompleted += parse_RecallListCurrent;
            webGet.LoadAsync(Url_HealthCanadaProductRecall_2012);
            webGet.LoadAsync(Url_HealthCanadaProductRecall_2011);

            HtmlWeb fGet = new HtmlWeb();
            fGet.LoadCompleted += parse_RecallListForegin;
            fGet.LoadAsync(Url_HealthCanadaProductRecall_f2011);

            HtmlWeb cGet = new HtmlWeb();
            cGet.LoadCompleted += load_ConsumerProduct;
            cGet.LoadAsync(Url_HealthCanadaConsumer);
            
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (propertyName == "WebDataRetrived")
            {
                IList<ProductViewModel> tmpItems = new List<ProductViewModel>(Items);
                Items.Clear();
                foreach (ProductViewModel p in tmpItems.OrderByDescending(x => x._dateRecall))
                {
                    this.Items.Add(p);

                }
                tmpItems.Clear();
                foreignProductAlerts = null;
                advisoriesWarningsRecalls = null;
                Spinnner.ShowSpinner = false;
                
            }
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
            
        }

        //Because current contains uncategorized items
        private void parse_RecallListCurrent(object sender, HtmlDocumentLoadCompleted e)
        {
            IList<ProductViewModel> tmpItems = new List<ProductViewModel>();
            IList<HtmlNode> hnc = e.Document.DocumentNode.DescendantNodes().ToList();
            String currentCategory = "Advisories, Warnings and Recalls";
  
            foreach (HtmlNode node in hnc)
            {
                if (node.Name.ToLower() == "div")
                {
                    if (node.Attributes.Count == 1 && node.Attributes[0].Name.ToLower() == "class"
                        && node.Attributes[0].Value.ToLower() == "date")
                    {
                        foreach (HtmlNode child in node.DescendantNodes().ToList())
                        {
                             if (child.Name.ToLower() == "li")
                             {
                                if (child.ParentNode.Name == "ul" && child.ParentNode.Attributes.Count == 0)
                                {
                                    ProductViewModel po = new ProductViewModel();
                                    po.Category = currentCategory;
                                    foreach (HtmlNode li in child.DescendantNodes().ToList())
                                    {
                                        if (li.Name.ToLower() == "a")
                                        {
                                            foreach (HtmlAttribute at in li.Attributes)
                                            {
                                                if (at.Name.ToLower() == "href")
                                                {
                                                    po.Href = at.Value;
                                                }
                                            }
                                            po.ShortDescription = ConvertWhitespacesToSingleSpaces(li.InnerText.Replace("�", "'"));
                                        }
                                        else if (li.Name.ToLower() == "span")
                                        {
                                            po.RecallDateString = li.InnerText;
                                        }
                                        else if (li.Name.ToLower() == "#text")
                                        {
                                            if (li.ParentNode.Name.ToLower() == "li" && li.InnerText.Contains("20"))
                                            {
                                                String date = "";
                                                if (li.PreviousSibling != null && li.PreviousSibling.Name.ToLower() == "abbr")
                                                {
                                                    date += li.PreviousSibling.InnerText + " ";
                                                }
                                                date += li.InnerText;
                                                po.RecallDateString = date;

                                            }
                                        }
                                    }
                                    //if (po.RecallDateString == "Jun 7, 2011")
                                    //{
                                    //    goto EndParse;
                                    //}
                                    tmpItems.Add(po);
                                }
                             }
                        }
                    }        
                }
            } //EndParse:

            foreach (ProductViewModel p in tmpItems.OrderByDescending(x => x._dateRecall))
            {
                this.Items.Add(p);

            }
            tmpItems.Clear();
            this.IsDataLoaded = true;
            NotifyPropertyChanged("WebDataRetrived");
           
           
        }

        //Because current contains uncategorized items
        private void parse_RecallListForegin(object sender, HtmlDocumentLoadCompleted e)
        {
            IList<ProductViewModel> tmpItems = new List<ProductViewModel>();
            IList<HtmlNode> hnc = e.Document.DocumentNode.DescendantNodes().ToList();
            String currentCategory = "Foreign Product Alerts";

            foreach (HtmlNode node in hnc)
            {
                if (node.Name.ToLower() == "div")
                {
                    if (node.Attributes.Count == 1 && node.Attributes[0].Name.ToLower() == "class"
                        && node.Attributes[0].Value.ToLower() == "date")
                    {
                        foreach (HtmlNode child in node.DescendantNodes().ToList())
                        {
                            if (child.Name.ToLower() == "li")
                            {
                                if (child.ParentNode.Name == "ul" && child.ParentNode.Attributes.Count == 0)
                                {
                                    ProductViewModel po = new ProductViewModel();
                                    po.Category = currentCategory;
                                    foreach (HtmlNode li in child.DescendantNodes().ToList())
                                    {
                                        if (li.Name.ToLower() == "a")
                                        {
                                            foreach (HtmlAttribute at in li.Attributes)
                                            {
                                                if (at.Name.ToLower() == "href")
                                                {
                                                    po.Href = at.Value;
                                                }
                                            }
                                            po.ShortDescription = ConvertWhitespacesToSingleSpaces(li.InnerText.Replace("�", "'"));
                                        }
                                        else if (li.Name.ToLower() == "span")
                                        {
                                            po.RecallDateString = li.InnerText;
                                        }
                                        else if (li.Name.ToLower() == "#text" )
                                        {
                                            if (li.ParentNode.Name.ToLower() == "li" && li.InnerText.Contains("20"))
                                            {
                                                String date = "";
                                                if (li.PreviousSibling != null && li.PreviousSibling.Name.ToLower() == "abbr")
                                                {
                                                    date += li.PreviousSibling.InnerText + " ";
                                                }
                                                date += li.InnerText;
                                                po.RecallDateString = date;

                                            }
                                        }
                                    }
                                    tmpItems.Add(po);
                                }
                            }
                        }
                    }
                }
            }

            foreach (ProductViewModel p in tmpItems.OrderByDescending(x => x._dateRecall))
            {
                this.Items.Add(p);
            }
            tmpItems.Clear();
            this.IsDataLoaded = true;
            NotifyPropertyChanged("WebDataRetrived");
        }


        private void load_ConsumerProduct(object sender, HtmlDocumentLoadCompleted e)
        {
            IList<ProductViewModel> tmpItems = new List<ProductViewModel>();
            IList<HtmlNode> hnc = e.Document.DocumentNode.DescendantNodes().ToList();

            foreach (HtmlNode node in hnc)
            {
                if (node.Name.ToLower() == "h2")
                {
                    if (node.InnerText.Contains("Search Results:"))
                    {
                        String[] strs = node.InnerText.Split();
                        int max = int.MinValue;
                        foreach (String s in strs)
                        {
                            int tmp;
                            if (int.TryParse(s, out tmp) )
                            {
                                max = Math.Max(max, tmp);
                            }
                        }
                        if (max != int.MinValue)
                        {
                            if (max % 15 > 1)
                            {
                                max+=15;
                                
                                HtmlWeb webGet = new HtmlWeb();
                                webGet.LoadCompleted += parse_ConsumerList;
                                webGet.LoadAsync(Url_HealthCanadaConsumer);
                                for (int i = 1; i < max; i += 15)
                                {
                                    String url = Url_HealthCanadaConsumer + "&next=t&StartIndex="+ i;
                                    webGet.LoadAsync( url );
                                }
                            }
                        }
                    }
                }
            }
        }

        private void parse_ConsumerList(object sender, HtmlDocumentLoadCompleted e)
        {
            IList < ProductViewModel >  tmpItems = new List<ProductViewModel>();
            IList<HtmlNode> hnc = e.Document.DocumentNode.DescendantNodes().ToList();
            String currentCategory = "Consumer Product Recalls";
            foreach (HtmlNode node in hnc)
            {
                if (node.Name.ToLower() == "table")
                {
                    foreach (HtmlAttribute att in node.Attributes)
                    {
                        if (att.Name.ToLower() == "summary" && att.Value == "Search results")
                        {
                            foreach (HtmlNode table in node.DescendantNodes().ToList())
                            {
                                if (table.Name.ToLower() == "tr")
                                {
                                    ProductViewModel po = new ProductViewModel();
                                    po.Category = currentCategory;

                                    foreach (HtmlNode tr in table.DescendantNodes().ToList())
                                    {
                                        if (tr.Name.ToLower() == "th")
                                        {
                                            break;
                                        }
                                        else if(tr.Name.ToLower() == "td")
                                        {
                                            String datetest = tr.InnerText;    
                                            DateTime test;
                                            datetest = datetest.Replace(",", "");
                                            datetest = datetest.Replace("Sept", "Sep");
                                            datetest = datetest.Replace("y 008", "2008");
                                            if (DateTime.TryParse(datetest, out test))
                                            {
                                                po.RecallDateString = datetest;
                                            }
                                            else
                                            {
                                                po.ShortDescription = ConvertWhitespacesToSingleSpaces( this.getTitleFromURLNode(tr) ).Trim();
                                                po.Href = Url_HealthCanadaConsumerPrefix + this.getURLFromNode(tr);
                                                break;
                                            }
                                            
                                        }    
                                    }
                                    if (po.Href != null && po.Href != "")
                                    {
                                        tmpItems.Add(po);
                                    } 
                                }
                            }
                        }
                    }
                }
            }
            foreach (ProductViewModel p in tmpItems.OrderByDescending(x => x._dateRecall))
            {
                this.Items.Add(p);
            }
            tmpItems.Clear();
            this.IsDataLoaded = true;
            NotifyPropertyChanged("WebDataRetrived");
        }

        public String getURLFromNode(HtmlNode n)
        {
            String url = "";
            if (n.Name.ToLower() == "a")
            {
                foreach (HtmlAttribute att in n.Attributes)
                {
                    if (att.Name.ToLower() == "href")
                    {
                        url = att.Value;
                        break;
                    }
                }
            }
            else
            {
                foreach (HtmlNode i in n.DescendantNodes().ToList())
                {
                    String u = getURLFromNode(i);
                    if (u != "")
                    {
                        url = u;
                        break;
                    }
                }
            }
            return url;
        }

        private String getTitleFromURLNode(HtmlNode n)
        {
            String title = "";
            if (n.Name.ToLower() == "a")
            {
                title = n.InnerText;

            }else
            {
                foreach (HtmlNode i in n.DescendantNodes().ToList())
                {
                    String u = getTitleFromURLNode(i);
                    if (u != "")
                    {
                        title = u;
                        break;
                    }
                }
            }
            return title;
        }
        private void parse_RecallListByCategory(object sender, HtmlDocumentLoadCompleted e)
        {
            IList < ProductViewModel >  tmpItems = new List<ProductViewModel>();
            IList<HtmlNode> hnc = e.Document.DocumentNode.DescendantNodes().ToList();
            String currentCategory = null;
            Boolean flag_comment = false;
            foreach (HtmlNode node in hnc)
            {
                if (!flag_comment && node.Name.ToLower() == "#comment")
                {
                    flag_comment = true;
                }
                else if (flag_comment && node.PreviousSibling != null && node.PreviousSibling.Name == "#comment")
                {
                    flag_comment = false;
                }

                if (!flag_comment && node.Name.ToLower() == "h3")
                {
                    currentCategory = node.InnerText;
                }
                else if (!flag_comment && node.Name.ToLower() == "li")
                {
                    if (node.ParentNode.Name == "ul" && node.ParentNode.Attributes.Count == 0)
                    {
                        ProductViewModel po = new ProductViewModel();
                        po.Category = currentCategory;
                        foreach (HtmlNode li in node.DescendantNodes().ToList())
                        {
                            if (li.Name.ToLower() == "a")
                            {
                                foreach (HtmlAttribute at in li.Attributes)
                                {
                                    if (at.Name.ToLower() == "href")
                                    {
                                        po.Href = at.Value;
                                    }
                                }
                                po.ShortDescription = ConvertWhitespacesToSingleSpaces(li.InnerText.Replace("�", "'")); 
                            }
                            else if (li.Name.ToLower() == "span")
                            {
                                po.RecallDateString = li.InnerText;
                            }
                            else if (li.Name.ToLower() == "#text")
                            {
                                if (li.ParentNode.Name.ToLower() == "li" && li.InnerText.Contains("20"))
                                {
                                    String date = "";
                                    if (li.PreviousSibling != null && li.PreviousSibling.Name.ToLower() == "abbr")
                                    {
                                        date += li.PreviousSibling.InnerText + " ";
                                    }
                                    date += li.InnerText;
                                    po.RecallDateString = date;
                                }
                            }
                        }
                      tmpItems.Add(po);
                    }
                }
            }

            foreach (ProductViewModel p in tmpItems.OrderByDescending(x => x._dateRecall))
            {
                this.Items.Add(p);
            }
            this.IsDataLoaded = true;
            NotifyPropertyChanged("WebDataRetrived"); 
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

        private bool FilterItemsByCategory(Object product, String category)
        {
            if (product is ProductViewModel)
            {
                return ((ProductViewModel)product).Category == category;
            }
            return false;
        }

        private CollectionViewSource foreignProductAlerts;
        public CollectionViewSource ForeignProductAlerts
        {
            get
            {
                if (null == foreignProductAlerts)
                {
                    foreignProductAlerts = new CollectionViewSource { Source = Items };
                    foreignProductAlerts.View.Filter = (x) => FilterItemsByCategory(x, "Foreign Product Alerts");
                }
                return foreignProductAlerts;
            }
        }

        
        private CollectionViewSource advisoriesWarningsRecalls;
        public CollectionViewSource AdvisoriesWarningsRecalls
        {
            get
            {
                if (null == advisoriesWarningsRecalls)
                {
                    advisoriesWarningsRecalls = new CollectionViewSource { Source = Items };
                    advisoriesWarningsRecalls.View.Filter = (x) => FilterItemsByCategory(x, "Advisories, Warnings and Recalls");
                }
                return advisoriesWarningsRecalls;
            }
        }

        private CollectionViewSource consumerProductRecalls;
        public CollectionViewSource ConsumerProductRecalls
        {
            get
            {
                if (null == consumerProductRecalls)
                {
                    consumerProductRecalls = new CollectionViewSource { Source = Items };
                    consumerProductRecalls.View.Filter = (x) => FilterItemsByCategory(x, "Consumer Product Recalls");
                }
                return consumerProductRecalls;
            }
        }

    }
}