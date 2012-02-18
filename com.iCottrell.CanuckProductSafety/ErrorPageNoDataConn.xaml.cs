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
using Microsoft.Phone.Net.NetworkInformation;

namespace com.iCottrell.CanuckProductSafety
{
    public partial class ErrorPageNoDataConn : PhoneApplicationPage
    {
        public ErrorPageNoDataConn()
        {
            InitializeComponent();
        }

        private void reload_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceNetworkInformation.IsNetworkAvailable)
            {
                if (this.NavigationService.CanGoBack)
                {
                    this.NavigationService.GoBack();
                }
                else
                {
                    this.NavigationService.Navigate(new Uri("/MainPage.xaml?reload=true", UriKind.Relative));
                }
            }
        }
    }

}