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

