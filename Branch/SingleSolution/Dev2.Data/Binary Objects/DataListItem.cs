﻿using System.ComponentModel;
using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract.Binary_Objects
{

    // Not sure this belongs here, should be in the studio?
    public class DataListItem : IDataListItem, INotifyPropertyChanged
    {
        #region Fields

        private string _field;
        private string _recordset;
        private string _displayValue;
        private bool _isRecordset;
        private string _recordsetIndex;
        private string _value;
        private string _description;
        private enRecordsetIndexType _recordsetIndexType;

        #endregion Fields

        #region Properties

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }

        public string Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;
                OnNotifyPropertyChange("Field");
            }
        }

        public string Recordset
        {
            get
            {
                return _recordset;
            }
            set
            {
                _recordset = value;
                OnNotifyPropertyChange("Recordset");
            }
        }

        public string RecordsetIndex
        {
            get
            {
                return _recordsetIndex;
            }
            set
            {
                _recordsetIndex = value;
                OnNotifyPropertyChange("RecordsetIndex");
            }
        }

        public enRecordsetIndexType RecordsetIndexType
        {
            get
            {
                return _recordsetIndexType;
            }
            set
            {
                _recordsetIndexType = value;
                OnNotifyPropertyChange("RecordsetIndexType");
            }
        }

        public bool IsRecordset
        {
            get
            {
                return _isRecordset;
            }
            set
            {
                _isRecordset = value;
                OnNotifyPropertyChange("IsRecordset");
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnNotifyPropertyChange("Value");
            }
        }

        public string DisplayValue
        {
            get
            {
                return _displayValue;
            }
            set
            {
                _displayValue = value;
                OnNotifyPropertyChange("DisplayValue");
            }
        }

        #endregion Properties

        #region Ctor

        public DataListItem()
        {

        }

        #endregion Ctor

        #region Methods



        #endregion Methods

        #region Private Methods



        #endregion Private Methods

        #region INotifyPropertyChanged Implementaion


        protected void OnNotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged Implementaion
    }
}
