﻿using Dev2.Common;
using Dev2.DataList.Contract;

namespace Dev2.Data.TO
{
    // ReSharper disable InconsistentNaming
    public class FormatNumberTO
    // ReSharper restore InconsistentNaming
    {
        #region Properties

        public string Number { get; set; }
        public string RoundingType { get; set; }
        public int RoundingDecimalPlaces { get; set; }
        public bool AdjustDecimalPlaces { get; set; }
        public int DecimalPlacesToShow { get; set; }

        #endregion Properties

        #region Constructor

        public FormatNumberTO()
        {
        }

        public FormatNumberTO(string number, enRoundingType roundingType, int roundingDecimalPlaces,
                              bool adjustDecimalPlaces, int decimalPlacesToShow)
        {
            Number = number;
            RoundingType = Dev2EnumConverter.ConvertEnumValueToString(roundingType);
            RoundingDecimalPlaces = roundingDecimalPlaces;
            AdjustDecimalPlaces = adjustDecimalPlaces;
            DecimalPlacesToShow = decimalPlacesToShow;
        }

        public FormatNumberTO(string number, string roundingType, int roundingDecimalPlaces, bool adjustDecimalPlaces,
                              int decimalPlacesToShow)
        {
            Number = number;
            RoundingType = roundingType;
            RoundingDecimalPlaces = roundingDecimalPlaces;
            AdjustDecimalPlaces = adjustDecimalPlaces;
            DecimalPlacesToShow = decimalPlacesToShow;
        }

        #endregion Constructor

        #region Methods

        public enRoundingType GetRoundingTypeEnum()
        {
            return Dev2EnumConverter.GetEnumFromStringValue<enRoundingType>(RoundingType);
        }

        #endregion Methods
    }
}