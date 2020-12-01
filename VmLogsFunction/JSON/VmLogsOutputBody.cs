using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;

namespace VmLogsFunction.JSON
{
    public class VmLogsOutputBody : VmLogsRequestBody
    {
        public VmLogsOutputBody(VmLogsRequestBody requestObject, IReadOnlyDictionary<string, string> tags)
        {
            this.ResourceGroup = requestObject.ResourceGroup;
            this.TargetVmName = requestObject.TargetVmName;
            this.Tags = tags;
        }

        public IReadOnlyDictionary<string, string> Tags;

        public string GetJsonString()
        {
            JObject jsonObject = new JObject();

            jsonObject.Add("ReourceGroup", this.ResourceGroup);
            jsonObject.Add("VmName", this.TargetVmName);

            foreach (var kvp in this.Tags)
            {
                jsonObject.Add(kvp.Key, kvp.Value);
            }

            return jsonObject.ToString();
        }
    }
}
