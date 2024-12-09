using Devlead.Testing.MockHttp.Tests.Model;
using System.Net.Http.Json;

namespace Devlead.Testing.MockHttp.Tests.Services;

public class MyService(HttpClient httpClient)
{
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
