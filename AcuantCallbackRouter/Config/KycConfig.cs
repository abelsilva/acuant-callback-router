namespace AcuantCallbackRouter.Config;

public class KycConfig
{
    internal const string ConfigId = "Kyc";

    public string CallbackUsername { get; set; }
    public string CallbackPassword { get; set; }
    
    public string[] CallbackUrls { get; set; }
}