using System;
using System.Globalization;
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
using System.Collections.ObjectModel;
using Microsoft.Phone.Net.NetworkInformation;
using System.ComponentModel;

namespace com.iCottrell.CanuckProductSafety
{
    public partial class MainPage : PhoneApplicationPage
    {
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            LoadingScreen.Child = new PopupSplash();  
            // Pull down data
            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            App.ViewModel.PropertyChanged += new PropertyChangedEventHandler(NotifyPropertyChanged);
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }


        private void NotifyPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            LoadingScreen.IsOpen = false;
            PivotControl.Visibility = Visibility.Visible;
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (DeviceNetworkInformation.IsNetworkAvailable && !App.ViewModel.IsDataLoaded)
            {
                LoadingScreen.IsOpen = true;
                App.ViewModel.LoadData();
            }
            else if(! DeviceNetworkInformation.IsNetworkAvailable)
            {
                this.NavigationService.Navigate(new Uri("/ErrorPageNoDataConn.xaml", UriKind.Relative));
            }
        
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (DeviceNetworkInformation.IsNetworkAvailable )
            {
                // Set the data context of the listbox control to the sample data
                DataContext = App.ViewModel;
                App.ViewModel.PropertyChanged += new PropertyChangedEventHandler(NotifyPropertyChanged);
                this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            }
            else
            {
                this.NavigationService.Navigate(new Uri("/ErrorPageNoDataConn.xaml", UriKind.Relative));
            }
        }
        private void ProductTap(object sender, GestureEventArgs e)
        {
            StackPanel sp = (StackPanel)sender;
            this.NavigationService.Navigate(new Uri("/ProductReportPage.xaml?href=" + sp.Tag, UriKind.Relative));
        }

        private void openPopup(object sender, ManipulationStartedEventArgs e)
        {
            LoadingScreen.IsOpen = true;
            PivotControl.Visibility = Visibility.Collapsed;
        }

        private void closePopup(object sender, ManipulationCompletedEventArgs e)
        {
            LoadingScreen.IsOpen = false;
            PivotControl.Visibility = Visibility.Visible;
        }    
    }
}