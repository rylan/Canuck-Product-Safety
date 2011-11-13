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


namespace com.iCottrell.CanuckProductSafety
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private String Url_HealthCanadaProductRecall = "http://www.hc-sc.gc.ca/ahc-asc/media/advisories-avis/alpha-eng.php";
        private String Url_Prefix = "http://www.hc-sc.gc.ca";

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
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            HtmlWeb webGet = new HtmlWeb();
            webGet.LoadCompleted += parse_RecallList;
            webGet.LoadAsync(Url_HealthCanadaProductRecall);
            
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (propertyName == "WebDataRetrived")
            {
                bacteriaItems = null;
                weightItems = null;
                childSafetyItems = null;
                consumerItems = null;
                dietarySupplementsItems = null;
                drugsItems = null;
                eyeItems = null;
                foodAllergiesItems = null;
                foodSafetyItems = null;
                fraudItems = null;
                impotenceItems = null;
                infantCareItems = null;
                influenzaItems = null;
                jewelleryItems = null;
                labellingItems = null;
                leadItems = null;
                medicalItems = null;
                mercuryItems = null;
                mouldItems = null; 
                naturalItems = null; 
                oralItems = null;
                packagingItems = null;
                sexualItems = null; 
                travelItems = null; 
                veterinaryItems = null;
                Spinnner.ShowSpinner = false;
                
            }
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
            
        }

        private void parse_RecallList(object sender, HtmlDocumentLoadCompleted e)
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
                                po.ShortDescription = li.InnerText;
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

        
       
        private CollectionViewSource childSafetyItems;
        public CollectionViewSource ChildSafetyItems
        {
            get
            {
                if (null == childSafetyItems)
                {
                    childSafetyItems = new CollectionViewSource { Source = Items };
                    childSafetyItems.View.Filter = (x) => FilterItemsByCategory(x, "Child Safety");
                }
                return childSafetyItems;
            }
        }

        private CollectionViewSource consumerItems;
        public CollectionViewSource ConsumerItems
        {
            get
            {
                if (null == consumerItems)
                {
                    consumerItems = new CollectionViewSource { Source = Items };
                    consumerItems.View.Filter = (x) => FilterItemsByCategory(x, "Consumer Product Safety");
                }
                return consumerItems;
            }
        }

        private CollectionViewSource dietarySupplementsItems;
        public CollectionViewSource DietarySupplementsItems
        {
            get
            {
                if (null == dietarySupplementsItems)
                {
                    dietarySupplementsItems = new CollectionViewSource { Source = Items };
                    dietarySupplementsItems.View.Filter = (x) => FilterItemsByCategory(x, "Dietary Supplements");
                }
                return dietarySupplementsItems;
            }
        }

        private CollectionViewSource diseasesItems;
        public CollectionViewSource DiseasesItems
        {
            get
            {
                if (null == diseasesItems)
                {
                    diseasesItems = new CollectionViewSource { Source = Items };
                    diseasesItems.View.Filter = (x) => FilterItemsByCategory(x, "Diseases and Conditions");
                }
                return diseasesItems;
            }
        }

        private CollectionViewSource drugsItems;
        public CollectionViewSource DrugsItems
        {
            get
            {
                if (null == drugsItems)
                {
                    drugsItems = new CollectionViewSource { Source = Items };
                    drugsItems.View.Filter = (x) => FilterItemsByCategory(x, "Drugs and Health Products");
                }
                return drugsItems;
            }
        }

        private CollectionViewSource eyeItems;
        public CollectionViewSource EyeItems
        {
            get
            {
                if (null == eyeItems)
                {
                    eyeItems = new CollectionViewSource { Source = Items };
                    eyeItems.View.Filter = (x) => FilterItemsByCategory(x, "Eye Care");
                }
                return eyeItems;
            }
        }
        
        private CollectionViewSource foodAllergiesItems;
        public CollectionViewSource FoodAllergiesItems
        {
            get
            {
                if (null == foodAllergiesItems)
                {
                    foodAllergiesItems = new CollectionViewSource { Source = Items };
                    foodAllergiesItems.View.Filter = (x) => FilterItemsByCategory(x, "Food Allergies");
                }
                return foodAllergiesItems;
            }
        }    
        
        private CollectionViewSource foodSafetyItems;
        public CollectionViewSource FoodSafetyItems
        {
            get
            {
                if (null == foodSafetyItems)
                {
                    foodSafetyItems = new CollectionViewSource { Source = Items };
                    foodSafetyItems.View.Filter = (x) => FilterItemsByCategory(x, "Food Safety");
                }
                return foodSafetyItems;
            }
        }
           
        private CollectionViewSource fraudItems;
        public CollectionViewSource FraudItems
        {
            get
            {
                if (null == fraudItems)
                {
                    fraudItems = new CollectionViewSource { Source = Items };
                    fraudItems.View.Filter = (x) => FilterItemsByCategory(x, "Fraud");
                }
                return fraudItems;
            }
        }     
             
        private CollectionViewSource impotenceItems;
        public CollectionViewSource ImpotenceItems
        {
            get
            {
                if (null == impotenceItems)
                {
                    impotenceItems = new CollectionViewSource { Source = Items };
                    impotenceItems.View.Filter = (x) => FilterItemsByCategory(x, "Impotence");
                }
                return impotenceItems;
            }
        }
    
        private CollectionViewSource infantCareItems;
        public CollectionViewSource InfantCareItems
        {
            get
            {
                if (null == infantCareItems)
                {
                    infantCareItems = new CollectionViewSource { Source = Items };
                    infantCareItems.View.Filter = (x) => FilterItemsByCategory(x, "Infant Care");
                }
                return infantCareItems;
            }
        }
        
        private CollectionViewSource influenzaItems;
        public CollectionViewSource InfluenzaItems
        {
            get
            {
                if (null == influenzaItems)
                {
                    influenzaItems = new CollectionViewSource { Source = Items };
                    influenzaItems.View.Filter = (x) => FilterItemsByCategory(x, "Influenza and Avian Flu");
                }
                return influenzaItems;
            }
        }

        private CollectionViewSource jewelleryItems;
        public CollectionViewSource JewelleryItems
        {
            get
            {
                if (null == jewelleryItems)
                {
                    jewelleryItems = new CollectionViewSource { Source = Items };
                    jewelleryItems.View.Filter = (x) => FilterItemsByCategory(x, "Jewellery");
                }
                return jewelleryItems;
            }
        }
        
        private CollectionViewSource labellingItems;
        public CollectionViewSource LabellingItems
        {
            get
            {
                if (null == labellingItems)
                {
                    labellingItems = new CollectionViewSource { Source = Items };
                    labellingItems.View.Filter = (x) => FilterItemsByCategory(x, "Labelling");
                }
                return labellingItems;
            }
        }     
              
        private CollectionViewSource leadItems;
        public CollectionViewSource LeadItems
        {
            get
            {
                if (null == leadItems)
                {
                    leadItems = new CollectionViewSource { Source = Items };
                    leadItems.View.Filter = (x) => FilterItemsByCategory(x, "Lead");
                }
                return leadItems;
            }
        }      
        private CollectionViewSource medicalItems; 
        public CollectionViewSource MedicalItems
        {
            get
            {
                if (null == medicalItems)
                {
                    medicalItems = new CollectionViewSource { Source = Items };
                    medicalItems.View.Filter = (x) => FilterItemsByCategory(x, "Medical Devices and Equipment");
                }
                return medicalItems;
            }
        }

        private CollectionViewSource mercuryItems;
        public CollectionViewSource MercuryItems
        {
            get
            {
                if (null == mercuryItems)
                {
                    mercuryItems = new CollectionViewSource { Source = Items };
                    mercuryItems.View.Filter = (x) => FilterItemsByCategory(x, "Mercury");
                }
                return mercuryItems;
            }
        }   
           
        private CollectionViewSource mouldItems; 
        public CollectionViewSource MouldItems
        {
            get
            {
                if (null == mouldItems)
                {
                    mouldItems = new CollectionViewSource { Source = Items };
                    mouldItems.View.Filter = (x) => FilterItemsByCategory(x, "Mould");
                }
                return mouldItems;
            }
        }       
               
        private CollectionViewSource naturalItems; 
        public CollectionViewSource NaturalItems
        {
            get
            {
                if (null == naturalItems)
                {
                    naturalItems = new CollectionViewSource { Source = Items };
                    naturalItems.View.Filter = (x) => FilterItemsByCategory(x, "Natural Health Products");
                }
                return naturalItems;
            }
        }
       
        private CollectionViewSource oralItems; 
        public CollectionViewSource OralItems
        {
            get
            {
                if (null == oralItems)
                {
                    oralItems = new CollectionViewSource { Source = Items };
                    oralItems.View.Filter = (x) => FilterItemsByCategory(x, "Oral Hygiene Products");
                }
                return oralItems;
            }
        }
              
        private CollectionViewSource packagingItems; 
        public CollectionViewSource PackagingItems
        {
            get
            {
                if (null == packagingItems)
                {
                    packagingItems = new CollectionViewSource { Source = Items };
                    packagingItems.View.Filter = (x) => FilterItemsByCategory(x, "Packaging");
                }
                return packagingItems;
            }
        }
        
        private CollectionViewSource sexualItems; 
        public CollectionViewSource SexualItems
        {
            get
            {
                if (null == sexualItems)
                {
                    sexualItems = new CollectionViewSource { Source = Items };
                    sexualItems.View.Filter = (x) => FilterItemsByCategory(x, "Sexual Enhancement Products");
                }
                return sexualItems;
            }
        }
         
        private CollectionViewSource travelItems; 
        public CollectionViewSource TravelItems
        {
            get
            {
                if (null == travelItems)
                {
                    travelItems = new CollectionViewSource { Source = Items };
                    travelItems.View.Filter = (x) => FilterItemsByCategory(x, "Travel Health");
                }
                return travelItems;
            }
        }
         
        private CollectionViewSource veterinaryItems;
        public CollectionViewSource VeterinaryItems
        {
            get
            {
                if (null == veterinaryItems)
                {
                    veterinaryItems = new CollectionViewSource { Source = Items };
                    veterinaryItems.View.Filter = (x) => FilterItemsByCategory(x, "Veterinary Drugs");
                }
                return veterinaryItems;
            }
        }

        private CollectionViewSource weightItems;
        public CollectionViewSource WeightItems
        {
            get
            {
                if (null == weightItems)
                {
                    weightItems = new CollectionViewSource { Source = Items };
                    weightItems.View.Filter = (x) => FilterItemsByCategory(x, "Weight Loss");
                }
                return weightItems;
            }
        }
                 
        private bool FilterItemsByCategory(Object product, String category){
            if (product is ProductViewModel)
            {
                return ((ProductViewModel)product).Category == category;
            }
            return false;
        }

        private CollectionViewSource bacteriaItems;
        public CollectionViewSource BacteriaItems
        {
            get
            {
                if (null == bacteriaItems)
                {
                    bacteriaItems = new CollectionViewSource { Source = Items };
                    bacteriaItems.View.Filter = (x) => FilterItemsByCategory(x, "Bacteria (including E. coli and Salmonella)");
                }
                return bacteriaItems;
            }
        }

    }
}