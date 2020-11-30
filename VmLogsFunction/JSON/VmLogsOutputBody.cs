using System.Collections.Generic;

namespace VmLogsFunction.JSON
{
    public class VmLogsOutputBody : VmLogsRequestBody
    {
        public VmLogsOutputBody(VmLogsRequestBody requestObject, IReadOnlyDictionary<string, string> tags)
        {
            this.ResourceGroup = requestObject.ResourceGroup;
            this.TargetVmName = requestObject.TargetVmName;
            this.VmTags = string.Join(", ", tags);
        }

        public string VmTags { get; set; }
    }
}
