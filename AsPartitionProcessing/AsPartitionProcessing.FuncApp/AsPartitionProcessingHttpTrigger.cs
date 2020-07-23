using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
      ConfigDatabaseConnectionInfo connectionInfo = new ConfigDatabaseConnectionInfo();

      connectionInfo.Server = ConfigurationManager.AppSettings.Get("ConfigServer");
      connectionInfo.Database = ConfigurationManager.AppSettings.Get("ConfigDatabase");

      //TODO: read from Key Vault
      connectionInfo.UserName = ConfigurationManager.AppSettings.Get("ConfigUserName");
      connectionInfo.Password = ConfigurationManager.AppSettings.Get("ConfigPassword");


      var configModels = ConfigDatabaseHelper.ReadConfig(connectionInfo, null);
      var asUsername = ConfigurationManager.AppSettings.Get("AsUserName");
      var asPassword = ConfigurationManager.AppSettings.Get("AsPassword");

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
