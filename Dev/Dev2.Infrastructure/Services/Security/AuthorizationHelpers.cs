using System;

namespace Dev2.Services.Security
{
    public static class AuthorizationHelpers
    {
        public static string ToReason(this AuthorizationContext value, bool isAuthorized = false)
        {
            //
            // MUST return null and NOT empty string as the result is used as TargetNullValue in bindings!
            //
            if(isAuthorized)
            {
                return null;
            }

            var field = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(ReasonAttribute)) as ReasonAttribute;

            return attribute == null || String.IsNullOrEmpty(attribute.Reason) ? null : attribute.Reason;
        }

        public static Permissions ToPermissions(this AuthorizationContext context)
        {
            switch(context)
            {
                case AuthorizationContext.View:
                    return Permissions.Administrator | Permissions.Contribute | Permissions.View;

                case AuthorizationContext.Execute:
                    return Permissions.Administrator | Permissions.Contribute | Permissions.Execute;

                case AuthorizationContext.Contribute:
                    return Permissions.Administrator | Permissions.Contribute;

                case AuthorizationContext.DeployTo:
                    return Permissions.Administrator | Permissions.DeployTo;

                case AuthorizationContext.DeployFrom:
                    return Permissions.Administrator | Permissions.DeployFrom;
            }
            return Permissions.None;
        }

        public static bool Matches(this WindowsGroupPermission permission, string resource)
        {
            if(permission.IsServer || String.IsNullOrEmpty(resource))
            {
                return true;
            }

            Guid resourceID;
            if(Guid.TryParse(resource, out resourceID))
            {
                return permission.ResourceID == resourceID;
            }

            // ResourceName is in the format: {categoryName}\{resourceName}
            return permission.ResourceName.EndsWith("\\" + resource);
        }
    }
}