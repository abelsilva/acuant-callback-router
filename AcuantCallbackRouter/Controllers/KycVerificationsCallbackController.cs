using System.Net;
using System.Text;
using AcuantCallbackRouter.Config;
using AcuantCallbackRouter.Data;
using AcuantCallbackRouter.Exceptions;
using AcuantCallbackRouter.Middleware;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AcuantCallbackRouter.Controllers;

[ApiController]
[Route("kyc")]
public class KycVerificationsCallbackController : ControllerBase
{
    public KycVerificationsCallbackController(IConfiguration configuration, ILogger<KycVerificationsCallbackController> logger)
    {
        Logger = logger;
        var kycConfig = configuration.GetSection(KycConfig.ConfigId).Get<KycConfig>();
        if (kycConfig == null)
        {
            throw new ConfigurationException("KycConfig was not found");
        }

        var auth = $"{kycConfig.CallbackUsername}:{kycConfig.CallbackPassword}";
        BasicAuthValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));

        CallbackUrls = kycConfig.CallbackUrls;
        if (CallbackUrls == null || !CallbackUrls.Any())
            throw new ConfigurationException("CallbackUrls missing from configuration");
    }

    private ILogger<KycVerificationsCallbackController> Logger { get; }
    private string BasicAuthValue { get; }
    private string[] CallbackUrls { get; set; }

    [HttpPost("acuant")]
    public async Task<ActionResult> AcuantCallback()
    {
        var basicAuthHash = Request.Headers.GetBasicAuthHash();
        if (string.IsNullOrWhiteSpace(basicAuthHash))
        {
            Logger.LogWarning("Request received with empty username/password");
            return NotFound();
        }

        if (basicAuthHash != BasicAuthValue)
        {
            Logger.LogWarning("Request received with invalid username/password");
            return NotFound();
        }

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var formData = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(formData))
            return BadRequest();

        Logger.LogInformation("New callback from Acuant (data length: {DataLength})", formData.Length);

        try
        {
            var newEval = JsonConvert.DeserializeObject<NewEvaluationResponse>(formData);
            if (newEval != null)
                Logger.LogInformation("ID: {AcuantId} - State: {AcuantState}", newEval.Tid, newEval.GetState());
            else
                Logger.LogInformation("Couldn't process data: {Data}", formData);
        }
        catch
        {
            Logger.LogInformation("Couldn't process data: {FormData}", formData);
        }

        var tasks = CallbackUrls.Select(url => ForwardRequest(url, formData)).ToArray();

        await Task.WhenAll(tasks);

        return tasks.Any(t => t.Result) ? Ok() : NotFound();
    }

    private async Task<bool> ForwardRequest(string url, string formData)
    {
        var resp = await url
            .AllowAnyHttpStatus()
            .WithHeader("Authorization", $"Basic {BasicAuthValue}")
            .WithHeader("Accept", "application/json")
            .WithHeader("Content-Type", "application/json; charset=UTF-8")
            .WithHeader("Content-Length", Encoding.UTF8.GetBytes(formData).Length)
            .PostStringAsync(formData);

        if (resp.StatusCode == (int) HttpStatusCode.OK)
        {
            Logger.LogInformation("Accepted - {Url}", url);
            return true;
        }

        Logger.LogInformation("Rejected - {Url}", url);
        return false;
    }
}