using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
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

                await PushToEventGrid(outputObject);

                return new OkObjectResult(responseMessage);
            } catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }

        private async Task PushToEventGrid(VmLogsOutputBody vmObject)
        {
            string topicHostname = new Uri(_options.EventGridTopicEndpointUrl).Host;

            TopicCredentials topicCredentials = new TopicCredentials(_options.EventGridTopicKey);
            EventGridClient client = new EventGridClient(topicCredentials);

            var myEvents = new List<EventGridEvent>()
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "vmAlertEvent",
                    Data = vmObject.Tags,
                    EventTime = DateTime.Now,
                    Subject = vmObject.TargetVmName,
                    DataVersion = "1.0"
                }
            };

            client.PublishEventsAsync(topicHostname, myEvents).GetAwaiter().GetResult();
        }
    }
}
