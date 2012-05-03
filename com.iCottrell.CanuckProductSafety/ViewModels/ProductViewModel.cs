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
using System.Collections.ObjectModel; 
using System.ComponentModel;

namespace com.iCottrell.CanuckProductSafety
{
   
    public class ProductViewModel : INotifyPropertyChanged
    {
        private String _category;
        public String Category { 
            get{
                return _category;
            }
            set{
                if (value != _category)
                {
                    _category = value;
                    NotifyPropertyChanged("Category");
                }
            } 
        }

        private String _href;
        public String Href { 
            get{
                return _href;
            }
            set
            {
                if (value != _href)
                {
                    _href = value;
                    NotifyPropertyChanged("HREF");
                }
            }
        }

        private String _shortDescription;
        public String ShortDescription {
            get
            {
                return _shortDescription;
            }
            set
            {
                if (value != _shortDescription)
                {
                    _shortDescription = value;
                    NotifyPropertyChanged("ShortDescription");
                }
            }
        }

        public DateTime _dateRecall { get; private set; }

        public String RecallDateString
        {
            get{
                if (_dateRecall != null)
                {
                    return _dateRecall.ToString("MMM d, yyyy");
                }
                else {
                    return "";
                }
            }
            set {
                try{
                     String date = value;
                     date = date.Replace("[", "");
                     date = date.Replace("]", "");
                     date = date.Replace(".", "");
                     date = date.Replace(",", "");
                     date = date.Replace("Sept", "Sep");
                     date = date.Replace("y 008", "2008");
                     _dateRecall = DateTime.Parse(date);
                    NotifyPropertyChanged("RecallDate");
                }
                catch(Exception err){}
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
