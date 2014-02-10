﻿using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Providers.Validation.Rules
{
    public class IsValidEmailAddressRule : IsValidCollectionRule
    {
        public IsValidEmailAddressRule(Func<string> getValue, char splitToken = ';')
            : base(getValue, "email address", splitToken)
        {
        }

        protected override bool IsValid(string item)
        {
            return item.IsEmail();
        }
    }
}
