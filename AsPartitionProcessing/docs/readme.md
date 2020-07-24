## Why was this forked?

### Azure Functions V1
Added an Azure Functions V1 project, <em>AsPartitionProcessing.FuncApp</em>, with an HTTP trigger to process partitions. This was authored in V1 as the Microsoft Analysis libraries are written for the full .NET Framework. At the time of writing, the .NET core version is still in preview and is not recommended for production. Refer to [Analysis Services client libraries](https://docs.microsoft.com/en-us/analysis-services/client-libraries?view=asallproducts-allversions) for further details.

### Modifications
The existing ...SampleClient and AsParitionProcessing projects have been upgraded to target .NET Framework 4.7.2, and, adulterated to reference <em>Microsoft.Extensions.Logging</em>. This was the most straightforward way of logging <em>AsPartitionProcessing</em> operations and making the logs readily available outside of the database logging functionality implemented within <em>AsPartitionProcessing.ConfigurationLogging</em>.

### Deploying Functions App
The functions app can be deployed to Azure using Visual Studio, or, a DevOps tool of your choice. The application requires the following settings:
```
    "ConfigServer": "[replace-with-your-sql-server]database.windows.net",
    "ConfigDatabase": "[replace-with-your-config-database-name]",
    "ConfigUserName": "[replace-with-your-config-database-username]",
    "ConfigPassword": "[replace-with-your-config-database-password]",
    "AsUserName": "[replace-with-your-analysis-server-username]",
    "AsPassword": "[replace-with-your-analysis-server-password]"
```
A sample settings file is provided in AsPartitionProcessing.FuncApp/sample.settings.json. To run the function app locally, create a local.settings.json file and provide the pertintent configuration values. Note that the "AsUserName" must be an Azure AD UPN - e.g. someuser@somedomain.onmicrosoft.com.

#### Key Vaul Integration
Typically, the easiest way to integration web apps and function apps with key vault is using Visual Studio 2017 (or later) Connected Services feature. Unfortunately, since this is a V1 function app, this must be done manually by following the steps outlined in [Use Key Vault from App Service with Managed Service Identity](https://docs.microsoft.com/en-us/samples/azure-samples/app-service-msi-keyvault-dotnet/keyvault-msi-appservice-sample/). The value of the app settings will be the URI of the secret; ex.
```json
"ConfigUserName": "https://[keyvault-instance-name].vault.azure.net/secrets/ConfigUserName/[secret-version]"
```
When running locally, you must be signed in (using Azure CLI) to the subscription that contains the key vault instance with a user that has <em>get</em> access to secrets.

In order to avoid conflicts libraries/nuget packages, the Microsoft Analysis Services reference was updated to use Microsoft.AnalysisServices.retail.amd64 V19.6.0 in both the function app and the <em>AsPartitionProcessing</em> projects.

## TODO: 
- Provide a separate Azure function to add/remove Azure Analysis Service firewall entries on-demand. Intent is to invoke the service to add firewall settings using the <em>AsPartitionProcessing.FuncApp</em> outbound IP address(es). The function will automatically remove the same settings after a configurable period of time, or, on-demand.
