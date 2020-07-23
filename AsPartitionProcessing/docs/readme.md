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

TODO: 
- Reference credentials from Key Vault
- Provide a separate Azure function to add/remove Azure Analysis Service firewall entries on-demand. Intent is to invoke the service to add firewall settings using the <em>AsPartitionProcessing.FuncApp</em> outbound IP address(es). The function will automatically remove the same settings after a configurable period of time, or, on-demand.
