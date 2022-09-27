using Newtonsoft.Json;

namespace AcuantCallbackRouter.Data;

public class NewEvaluationResponse
{
    [JsonProperty("state")]
    public string State { get; set; }

    public KycState GetState()
        => State switch
        {
            "A" => KycState.ACCEPTED,
            "R" => KycState.UNDER_REVIEW,
            _ => KycState.REJECTED
        };

    [JsonProperty("tid")]
    public string Tid { get; set; }

    public enum KycState
    {
        ACCEPTED,
        UNDER_REVIEW,
        REJECTED
    }
}