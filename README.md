# vmlogs

Azure Function for enriching Log Analytics information with tags from target ressources (VMs).

Scenario:
1. Function triggered by HTTP POST with JSON body, containing information about the target resource
1. Accessing the target resource via Azure Fluent SDK
1. Reading tags from target resource
1. Add tags to JSON from request
1. Send output JSON to some storage or other processing service (e.g. Event Hub)

Conditions:
- The access to the resource needs to be handled via a Service Principal that has read permissions on the target resources.
