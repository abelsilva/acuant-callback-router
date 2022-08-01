using System.Text;
using AcuantCallbackRouter.Config;
using AcuantCallbackRouter.Exceptions;
using AcuantCallbackRouter.Middleware;
using Microsoft.AspNetCore.Mvc;

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

        var tasks = CallbackUrls.Select(url => ForwardRequest(url, formData)).ToArray();

        await Task.WhenAll(tasks);

        return tasks.Any(t => t.Result) ? Ok() : NotFound();
    }

    private Task<bool> ForwardRequest(string url, string formData)
    {
        throw new NotImplementedException();
    }
}