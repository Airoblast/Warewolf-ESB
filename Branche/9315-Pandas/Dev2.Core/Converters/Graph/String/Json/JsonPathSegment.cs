﻿using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Unlimited.Framework.Converters.Graph.String.Json
{
    public class JsonPathSegment : IPathSegment
    {
        #region Constructors

        internal JsonPathSegment()
        {

        }

        internal JsonPathSegment(string name, bool isEnumarable)
        {
            ActualSegment = name;
            IsEnumarable = isEnumarable;
        }

        #endregion Constructors

        #region Properties

        public string ActualSegment { get; set; }
        public string DisplaySegment { get; set; }
        public bool IsEnumarable { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            if (IsEnumarable)
            {
                return ActualSegment + JsonPath.EnumerableSymbol;
            }
            else
            {
                return ActualSegment;
            }
        }

        public string ToString(bool considerEnumerable)
        {
            if (considerEnumerable && IsEnumarable)
            {
                return ActualSegment + JsonPath.EnumerableSymbol;
            }
            else
            {
                return ActualSegment;
            }
        }

        #endregion Methods
    }
}
