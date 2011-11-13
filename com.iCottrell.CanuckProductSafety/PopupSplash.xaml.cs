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

namespace com.iCottrell.CanuckProductSafety
{
    public partial class PopupSplash : UserControl
    {
        public PopupSplash()
        {
            InitializeComponent();
            Visibility v = (Visibility)Resources["PhoneLightThemeVisibility"];
            if (v == System.Windows.Visibility.Visible)
            {
               LayoutRoot.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                LayoutRoot.Background = new SolidColorBrush(Colors.Black);
            }
            this.progressBar1.IsIndeterminate = true;
        }
    }
}
