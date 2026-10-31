using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace AutosPA.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;

    public AuthorizationMessageHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _js.InvokeAsync<string?>(
            "localStorage.getItem",
            cancellationToken,
            "flexicar_token");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}