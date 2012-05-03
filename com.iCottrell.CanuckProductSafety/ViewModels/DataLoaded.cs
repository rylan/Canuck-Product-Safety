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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace com.iCottrell.CanuckProductSafety
{
    public class DataLoaded : INotifyPropertyChanged
    {
        private Boolean _dataLoaded;

        public Boolean ShowSpinner
        {
            get
            {
                return !_dataLoaded;
            }
            set
            {
                if (value != !_dataLoaded)
                {
                    _dataLoaded = !value;
                    NotifyPropertyChanged("DataLoaded");
                }
            }
        }

        public Boolean IsDataLoaded
        {
            get
            {
                return _dataLoaded;
            }
            set
            {
                if (value != _dataLoaded)
                {
                    _dataLoaded = value;
                    NotifyPropertyChanged("DataLoaded");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    

}

