﻿/*******************************************************************************
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
using Microsoft.Phone.Tasks;

namespace com.iCottrell.CanuckProductSafety
{
    public partial class MainPage : PhoneApplicationPage
    {
        MarketplaceDetailTask _marketPlaceDetailTask = new MarketplaceDetailTask();
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            LoadingScreen.Child = new PopupSplash();
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
                if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back && !App.ViewModel.IsDataLoaded)
                {
                    this.NavigationService.GoBack();
                }
                else
                {
                    App.ViewModel.IsDataLoaded = false;
                    this.NavigationService.Navigate(new Uri("/ErrorPageNoDataConn.xaml", UriKind.Relative));
                }
            }
        }
        private void ProductTap(object sender, GestureEventArgs e)
        {
            StackPanel sp = (StackPanel)sender;
            //will need to clean up consumer report 
            String href = (String)sp.Tag;
            this.NavigationService.Navigate(new Uri("/ProductReportPage.xaml?href=" + Uri.EscapeDataString(href), UriKind.Relative));
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