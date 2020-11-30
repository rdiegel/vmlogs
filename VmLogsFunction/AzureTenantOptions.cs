using System;
using System.Collections.Generic;
using System.Text;

namespace VmLogsFunction
{
    public class AzureTenantOptions
    {
        public string Client { get; set; }

        public string Key { get; set; }

        public string Tenant { get; set; }

        public string SubscriptionId { get; set; }
    }
}
