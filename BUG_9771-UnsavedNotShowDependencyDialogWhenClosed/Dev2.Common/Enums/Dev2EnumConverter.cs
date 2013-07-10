﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.ExtMethods;

namespace Dev2.Common
{
    public static class Dev2EnumConverter
    {
        public static IList<TTEnum> GetEnumsToList<TTEnum>() where TTEnum : struct
        {
            Type type = typeof(TTEnum);
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");
            return Enum.GetValues(type).Cast<TTEnum>().ToList();
        }

        public static IList<string> ConvertEnumsTypeToStringList<tEnum>() where tEnum : struct
        {
           Type enumType = typeof(tEnum);

            IList<string> result = new List<string>();

            foreach (var value in Enum.GetValues(enumType))
            {
               result.Add((value as Enum).GetDescription());
            }

            return result;
        }

        public static string ConvertEnumValueToString(Enum value)
        {
            Type type = value.GetType();
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");


            object[] allAttribs = type.GetCustomAttributes(false);
            return value.GetDescription();
        }    

        public static object GetEnumFromStringDiscription(string discription, Type type)
        {
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");

            foreach (var value in Enum.GetValues(type))
            {
                if ((value as Enum).GetDescription() == discription)
                {
                    return value;
                }
            }
            return null;
        }
    }


}
