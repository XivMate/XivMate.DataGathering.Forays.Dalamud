using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Plugin;
using XivMate.DataGathering.Forays.Dalamud.Models;

namespace XivMate.DataGathering.Forays.Dalamud.Services;

public class ApiService(IDalamudPluginInterface dalamudPluginInterface)
{
    private async Task PostRequest<T>(T obj, string endpoint)
    {
        var config = dalamudPluginInterface.GetPluginConfig() as Configuration;
        var baseUrl = config.SystemConfiguration.ApiUrl ?? throw new Exception($"Api URL not set");
        if (baseUrl.EndsWith("/"))
            baseUrl += "/";
        var url = $"{config.SystemConfiguration.ApiUrl}{endpoint}";
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add($"X-API-Key", config.SystemConfiguration.ApiKey);
        var result = await client.PostAsJsonAsync(url, obj);
        result.EnsureSuccessStatusCode();
    }

    public async Task UploadFate(Fate fate)
    {
        await PostRequest(fate, $"fateended");
    }
}
