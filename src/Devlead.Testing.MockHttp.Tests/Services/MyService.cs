using Devlead.Testing.MockHttp.Tests.Model;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Devlead.Testing.MockHttp.Tests.Services;

public class MyService(HttpClient httpClient, IOptions<MyServiceSettings> settings)
{
    public string GetServiceName() => settings.Value.ServiceName;

    public async Task<string> GetData()
    {
        var response = await httpClient.GetAsync("https://example.com/index.txt");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<User?> GetSecret()
    {
        return await httpClient.GetFromJsonAsync<User>("https://example.com/login/secret.json");
    }
}
