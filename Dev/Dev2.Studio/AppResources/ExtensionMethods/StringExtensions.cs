﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.AppResources.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string GetManagementPayload(this string payload)
        {
            if(payload.Contains("<Dev2System.ManagmentServicePayload>"))
            {
                var startIndx = payload.IndexOf("<Dev2System.ManagmentServicePayload>", StringComparison.Ordinal);
                var length = "<Dev2System.ManagmentServicePayload>".Length;
                var endIndx = payload.IndexOf("</Dev2System.ManagmentServicePayload>", StringComparison.Ordinal);
                var l = endIndx - startIndx - length;
                return payload.Substring(startIndx + length, l);
            }
            return string.Empty;
        }

        public static string ExceptChars(this string str, IEnumerable<char> toExclude)
        {
            StringBuilder sb = new StringBuilder();
            foreach(char c in str)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                if(!toExclude.Contains(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static bool SpaceCaseInsenstiveComparision(this string stringa, string stringb)
        {
            return (stringa == null && stringb == null) || (stringa != null && stringa.ToLower().ExceptChars(new[] { ' ', '\t', '\n', '\r' }).Equals(stringb.ToLower().ExceptChars(new[] { ' ', '\t', '\n', '\r' })));
        }
    }
}
