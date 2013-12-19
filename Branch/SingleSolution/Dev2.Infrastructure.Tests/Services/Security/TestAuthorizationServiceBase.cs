using System;
using System.Security.Principal;
using Dev2.Services.Security;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    public class TestAuthorizationServiceBase : AuthorizationServiceBase
    {
        public TestAuthorizationServiceBase(ISecurityService securityService)
            : base(securityService)
        {
        }

        public int RaisePermissionsChangedHitCount { get; private set; }

        public IPrincipal User { get; set; }

        protected override void RaisePermissionsChanged()
        {
            RaisePermissionsChangedHitCount++;
            base.RaisePermissionsChanged();
        }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            return IsAuthorized(User, context, resource);
        }
    }
}