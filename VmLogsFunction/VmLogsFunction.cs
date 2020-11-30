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
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                string targetVmName = data?.vmName;

                var credentials = new AzureCredentialsFactory()
                      .FromServicePrincipal(_options.Client, _options.Key, _options.Tenant, AzureEnvironment.AzureGlobalCloud);

                var azure = Azure
                        .Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(credentials)
                        .WithSubscription(_options.SubscriptionId);

                var targetVm = azure.VirtualMachines.GetByResourceGroup("myRessourceGroup", targetVmName);

                string responseMessage = targetVm == null
                    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                    : $"Hello, the VM {targetVmName} is ready. ID: {targetVm.Id}";

                return new OkObjectResult(responseMessage);
            } catch (Exception ex)
            {
                log.LogError(ex, "Error during function run");
                return new BadRequestResult();
            }
        }
    }
}
