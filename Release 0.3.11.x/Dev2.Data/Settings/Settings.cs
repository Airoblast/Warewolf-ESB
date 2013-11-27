﻿using System.Collections.Generic;
using Dev2.Data.Settings.Security;
using Newtonsoft.Json;

namespace Dev2.Data.Settings
{
    public class Settings
    {
        public List<WindowsGroupPermission> Security { get; set; }

        public bool HasError { get; set; }
        public string Error { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
