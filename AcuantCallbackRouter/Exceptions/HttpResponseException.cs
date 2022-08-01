using System.Net;
using AcuantCallbackRouter.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AcuantCallbackRouter.Exceptions;

public class HttpResponseException : Exception
{
    public HttpStatusCode HttpStatusCode { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }

    public HttpResponseException(HttpStatusCode statusCode, string errorCode = null, string errorMessage = null)
    {
        HttpStatusCode = statusCode;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    internal IActionResult ConvertToHttpResponse()
    {
        if (ErrorCode != null)
        {
            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode,
                Content = JsonConvert.SerializeObject(new Error(ErrorCode, ErrorMessage)),
                ContentType = "application/json"
            };
        }
        else
            return new StatusCodeResult((int)HttpStatusCode);
    }
}