﻿#pragma checksum "C:\Users\cottrell\dev\windows phone\com.iCottrell.CanuckProductSafety\com.iCottrell.CanuckProductSafety\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "312D8AB2D2DE968F79A2920A9BDFCA21"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace com.iCottrell.CanuckProductSafety {
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Primitives.Popup LoadingScreen;
        
        internal Microsoft.Phone.Controls.Pivot PivotControl;
        
        internal Microsoft.Phone.Controls.PivotItem AllByDate;
        
        internal System.Windows.Controls.ListBox AllListBox;
        
        internal System.Windows.Controls.ListBox AWRListBox;
        
        internal System.Windows.Controls.ListBox FRAListBox;
        
        internal System.Windows.Controls.ListBox CPRListBox;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/com.iCottrell.CanuckProductSafety;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.LoadingScreen = ((System.Windows.Controls.Primitives.Popup)(this.FindName("LoadingScreen")));
            this.PivotControl = ((Microsoft.Phone.Controls.Pivot)(this.FindName("PivotControl")));
            this.AllByDate = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("AllByDate")));
            this.AllListBox = ((System.Windows.Controls.ListBox)(this.FindName("AllListBox")));
            this.AWRListBox = ((System.Windows.Controls.ListBox)(this.FindName("AWRListBox")));
            this.FRAListBox = ((System.Windows.Controls.ListBox)(this.FindName("FRAListBox")));
            this.CPRListBox = ((System.Windows.Controls.ListBox)(this.FindName("CPRListBox")));
        }
    }
}

