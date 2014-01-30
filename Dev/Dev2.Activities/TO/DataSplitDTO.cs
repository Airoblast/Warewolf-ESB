﻿using System.Collections.Generic;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Util;
using Dev2.Validation;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    // ReSharper disable InconsistentNaming
    public class DataSplitDTO : ValidatedObject, IDev2TOFn, IOutputTOConvert
    // ReSharper restore InconsistentNaming
    {
        private string _outputVariable;
        private string _splitType;
        private string _at;
        private int _indexNum;
        private bool _enableAt;
        private bool _include;
        string _escapeChar;
        bool _isEscapeCharFocused;
        bool _isOutputVariableFocused;
        bool _isAtFocused;

        public DataSplitDTO()
        {
        }

        public DataSplitDTO(string outputVariable, string splitType, string at, int indexNum, bool include = false, bool inserted = false)
        {
            Inserted = inserted;
            OutputVariable = outputVariable;
            SplitType = string.IsNullOrEmpty(splitType) ? "Index" : splitType;
            At = string.IsNullOrEmpty(at) ? string.Empty : at;
            IndexNumber = indexNum;
            Include = include;
            _enableAt = true;
            OutList = new List<string>();
        }

        public string WatermarkTextVariable { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public bool EnableAt
        {
            get
            {
                return _enableAt;
            }
            set
            {
                _enableAt = value;
                OnPropertyChanged();
            }
        }

        public int IndexNumber
        {
            get
            {
                return _indexNum;
            }
            set
            {
                _indexNum = value;
                OnPropertyChanged();
            }
        }

        public List<string> OutList { get; set; }

        public bool Include
        {
            get
            {
                return _include;
            }
            set
            {
                _include = value;
                OnPropertyChanged();
            }
        }

        [FindMissing]
        public string EscapeChar { get { return _escapeChar; } set { OnPropertyChanged(ref _escapeChar, value); } }

        public bool IsEscapeCharFocused { get { return _isEscapeCharFocused; } set { OnPropertyChanged(ref _isEscapeCharFocused, value); } }

        [FindMissing]
        public string OutputVariable
        {
            get
            {
                return _outputVariable;
            }
            set
            {
                _outputVariable = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsOutputVariableFocused { get { return _isOutputVariableFocused; } set { OnPropertyChanged(ref _isOutputVariableFocused, value); } }

        public string SplitType
        {
            get
            {
                return _splitType;
            }
            set
            {
                if(value != null)
                {
                    _splitType = value;
                    OnPropertyChanged();
                    RaiseCanAddRemoveChanged();
                }
            }
        }

        [FindMissing]
        public string At
        {
            get
            {
                return _at;
            }
            set
            {

                _at = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsAtFocused { get { return _isAtFocused; } set { OnPropertyChanged(ref _isAtFocused, value); } }

        public bool CanRemove()
        {
            if(SplitType == "Index" || SplitType == "Chars")
            {
                if(string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(At))
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        public bool CanAdd()
        {
            bool result = true;
            if(SplitType == "Index" || SplitType == "Chars")
            {
                if(string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(At))
                {
                    result = false;
                }
            }
            return result;
        }

        public void ClearRow()
        {
            OutputVariable = string.Empty;
            SplitType = "Char";
            At = string.Empty;
            Include = false;
            EscapeChar = string.Empty;
        }

        public bool Inserted { get; set; }

        public OutputTO ConvertToOutputTO()
        {
            return DataListFactory.CreateOutputTO(OutputVariable, OutList);
        }

        public bool IsEmpty()
        {
            return OutputVariable == string.Empty && SplitType == "Index" && string.IsNullOrEmpty(At)
                   || OutputVariable == string.Empty && SplitType == "Chars" && string.IsNullOrEmpty(At)
                   || OutputVariable == string.Empty && SplitType == "None" && string.IsNullOrEmpty(At);
        }

        public override void Validate()
        {
            Validate("OutputVariable");
            Validate("At");
            Validate("EscapeChar");
        }

        protected override RuleSet GetRuleSet(string propertyName)
        {
            var ruleSet = new RuleSet();
            if(IsEmpty())
            {
                return ruleSet;
            }
            switch(propertyName)
            {
                case "OutputVariable":
                    ruleSet.Add(new StringCannotBeEmptyOrNullRule(OutputVariable, () => IsOutputVariableFocused = true));
                    ruleSet.Add(new StringCannotBeInvalidExpressionRule(OutputVariable, () => IsOutputVariableFocused = true));
                    break;
                case "At":
                    if(SplitType == "Index")
                    {
                        ruleSet.Add(new StringCannotBeInvalidExpressionRule(At, () => IsAtFocused = true));
                        ruleSet.Add(new IsNumericRule(At, () => IsAtFocused = true));
                    }
                    break;
                case "EscapeChar":
                    break;
            }
            return ruleSet;
        }
    }
}
