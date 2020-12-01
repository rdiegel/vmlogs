using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using VmLogsFunction.JSON;

namespace VmLogsFunction
{
    public class VmLogsFunction
    {
        private readonly AzureTenantOptions _options;

        public VmLogsFunction(IOptions<AzureTenantOptions> options)
        {
            _options = options.Value;
        }

        [FunctionName("VmLogsFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var requestObject = JsonConvert.DeserializeObject<VmLogsRequestBody>(requestBody);

                var credentials = new AzureCredentialsFactory()
                      .FromServicePrincipal(_options.Client, _options.Key, _options.Tenant, AzureEnvironment.AzureGlobalCloud);

                var azure = Azure
                        .Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(credentials)
                        .WithSubscription(_options.SubscriptionId);

                var targetVm = azure.VirtualMachines.GetByResourceGroup(requestObject.ResourceGroup, requestObject.TargetVmName);

                var outputObject = new VmLogsOutputBody(requestObject, targetVm.Tags);

                string responseMessage = targetVm == null
                    ? "No VM found for request."
                    : outputObject.GetJsonString();

                return new OkObjectResult(responseMessage);
            } catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestResult();
            }
        }
    }
}
