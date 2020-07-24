using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AsPartitionProcessing.FuncApp
{
  public static class AsPartitionProcessingHttpTrigger
  {
    [FunctionName(nameof(ProcessPartitions))]
    public static async Task<HttpResponseMessage> ProcessPartitions(
      [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
      ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");
        
      var models = InitializeFromDatabase();
      foreach (ModelConfiguration modelConfig in models)
      {
        PartitionProcessor.PerformProcessing(log, modelConfig, LogMessage);
      }
      return req.CreateResponse(HttpStatusCode.OK);
    }

    private static List<ModelConfiguration> InitializeFromDatabase()
    {
      AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

      var keyVaultClient = new KeyVaultClient(
            new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

      ConfigDatabaseConnectionInfo connectionInfo = new ConfigDatabaseConnectionInfo();

      connectionInfo.Server = ConfigurationManager.AppSettings.Get("ConfigServer");
      connectionInfo.Database = ConfigurationManager.AppSettings.Get("ConfigDatabase");

      connectionInfo.UserName = keyVaultClient.GetSecretAsync(ConfigurationManager.AppSettings.Get("ConfigUserName")).GetAwaiter().GetResult().Value;
      connectionInfo.Password = keyVaultClient.GetSecretAsync(ConfigurationManager.AppSettings.Get("ConfigPassword")).GetAwaiter().GetResult().Value;


      var configModels = ConfigDatabaseHelper.ReadConfig(connectionInfo, null);
      var asUsername = keyVaultClient.GetSecretAsync(ConfigurationManager.AppSettings.Get("AsUserName")).GetAwaiter().GetResult().Value;
      var asPassword = keyVaultClient.GetSecretAsync(ConfigurationManager.AppSettings.Get("AsPassword")).GetAwaiter().GetResult().Value;

      foreach (var configModel in configModels)
      {
        configModel.UserName = asUsername;
        configModel.Password = asPassword;
      }

      return configModels;
    }

    private static void LogMessage(ILogger log, string message, MessageType messageType, ModelConfiguration partitionedModel)
    {
      log.LogInformation(message);
      try
      {
        ConfigDatabaseHelper.LogMessage(message, messageType, partitionedModel);
      }
      catch (Exception exc)
      {
        log.LogError(exc.Message, exc);
      }
    }
  }
}
