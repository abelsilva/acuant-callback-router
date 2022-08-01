using Newtonsoft.Json;

namespace AcuantCallbackRouter.Data;

public class Error
{
    public Error()
    {
    }

    public Error(string id, string description)
    {
        Id = id;
        Description = description;
    }

    public string Id { get; set; }

    [JsonProperty("error")]
    public string ErrorType { get; set; }

    public string Description { get; set; }
}