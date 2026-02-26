using System.Collections.Concurrent;
using System.IO;

using Microsoft.Web.WebView2.Core;

namespace HTWind.Services;

public sealed class WebViewEnvironmentProvider : IWebViewEnvironmentProvider
{
    private readonly ConcurrentDictionary<string, Task<CoreWebView2Environment>> _environments =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<CoreWebView2Environment> GetWidgetsEnvironmentAsync()
    {
        return GetEnvironmentAsync("Widgets");
    }

    public Task<CoreWebView2Environment> GetEditorEnvironmentAsync()
    {
        return GetEnvironmentAsync("Editor");
    }

    private Task<CoreWebView2Environment> GetEnvironmentAsync(string profileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileName);

        return _environments.GetOrAdd(profileName, CreateEnvironmentWithRetryOnFailureAsync);
    }

    private async Task<CoreWebView2Environment> CreateEnvironmentWithRetryOnFailureAsync(
        string profileName
    )
    {
        try
        {
            var userDataFolder = BuildUserDataFolder(profileName);
            return await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        }
        catch
        {
            _environments.TryRemove(profileName, out _);
            throw;
        }
    }

    private static string BuildUserDataFolder(string profileName)
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HTWind",
            "WebView2",
            profileName
        );

        Directory.CreateDirectory(path);
        return path;
    }
}
