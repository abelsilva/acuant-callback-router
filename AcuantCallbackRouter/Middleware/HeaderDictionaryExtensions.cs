using System.Net;
using AcuantCallbackRouter.Exceptions;

namespace AcuantCallbackRouter.Middleware;

public static class HeaderDictionaryExtensions
{
    private const string HTTP_AUTHORIZATION_HEADER = "Authorization";
    private const string BASIC_AUTHENTICATION_SCHEME = "Basic";
    
    public static string GetBasicAuthHash(this IHeaderDictionary headers)
    {
        return GetToken(headers, BASIC_AUTHENTICATION_SCHEME);
    }

    private static string GetToken(IHeaderDictionary headers, string scheme = null)
    {
        if (headers == null)
            throw new HttpResponseException(HttpStatusCode.Unauthorized);

        if (!headers.ContainsKey(HTTP_AUTHORIZATION_HEADER))
            throw new HttpResponseException(HttpStatusCode.Unauthorized);

        if (headers[HTTP_AUTHORIZATION_HEADER].Count != 1)
            throw new HttpResponseException(HttpStatusCode.Unauthorized);

        var authTokenHeader = headers[HTTP_AUTHORIZATION_HEADER][0];

        if (string.IsNullOrWhiteSpace(authTokenHeader))
            throw new HttpResponseException(HttpStatusCode.Unauthorized);

        var authToken = authTokenHeader;
        if (string.IsNullOrEmpty(scheme))
            return authToken;

        if (!authTokenHeader.StartsWith($"{scheme} ") || authTokenHeader.Length <= (scheme.Length + 1))
            throw new HttpResponseException(HttpStatusCode.Unauthorized);

        authToken = authToken.Substring(scheme.Length + 1);
        if (string.IsNullOrWhiteSpace(authToken))
            throw new HttpResponseException(HttpStatusCode.Unauthorized);

        return authToken;
    }
}