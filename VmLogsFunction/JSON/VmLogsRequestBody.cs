using System;
using System.Collections.Generic;
using System.Text;

namespace VmLogsFunction.JSON
{
    public class VmLogsRequestBody
    {
        public string TargetVmName { get; set; }

        public string ResourceGroup { get; set; }
    }
}
