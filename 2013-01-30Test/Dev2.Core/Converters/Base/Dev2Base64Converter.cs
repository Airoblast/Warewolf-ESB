﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Interfaces;

namespace Dev2.Converters
{
    internal class Dev2Base64Converter : IBaseConverter, ISpookyLoadable
    {
        public bool IsType(string payload)
        {
            bool result = false;
            try
            {
                Convert.FromBase64String(payload);
                result = true;
            }
            catch
            {
                // if error is thrown we know it is not a valid base64 string
            }
            
            return result;

        }

        public string ConvertToBase(byte[] payload)
        {
            return Convert.ToBase64String(payload);
        }

        public byte[] NeutralizeToCommon(string payload)
        {

            byte[] decoded = Convert.FromBase64String(payload);
            string tmp = Encoding.UTF8.GetString(decoded);

            UTF8Encoding encoder = new UTF8Encoding();
            return (encoder.GetBytes(tmp));

        }

        public Enum HandlesType()
        {
            return enDev2BaseConvertType.Base64;
        }

    }
}
