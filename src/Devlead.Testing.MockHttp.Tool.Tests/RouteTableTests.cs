using Microsoft.AspNetCore.Http;

namespace Devlead.Testing.MockHttp.Tool.Tests;

public class RouteTableTests
{
    [Test]
    public void Load_MissingContentResource_Throws()
    {
        using var directory = TemporaryRoutes.Create(
            """
            [
              {
                "Request": {
                  "Methods": [{ "Method": "GET" }],
                  "AbsoluteUri": "https://example.com/x"
                },
                "Responses": [
                  {
                    "ContentResource": "Missing.File.txt",
                    "ContentType": "text/plain",
                    "StatusCode": 200
                  }
                ]
              }
            ]
            """);

        var ex = Assert.Throws<FileNotFoundException>(() => RouteTable.Load(directory.Path, false));
        Assert.That(ex!.Message, Does.Contain("Missing.File.txt"));
        Assert.That(ex.Message, Does.Contain("https://example.com/x"));
    }

    [Test]
    public void Load_ContentResourceOutsideDirectory_Throws()
    {
        using var directory = TemporaryRoutes.Create(
            """
            [
              {
                "Request": {
                  "Methods": [{ "Method": "GET" }],
                  "AbsoluteUri": "https://example.com/x"
                },
                "Responses": [
                  {
                    "ContentResource": "..Outside.txt",
                    "ContentType": "text/plain",
                    "StatusCode": 200
                  }
                ]
              }
            ]
            """);

        var ex = Assert.Throws<InvalidOperationException>(() => RouteTable.Load(directory.Path, false));
        Assert.That(ex!.Message, Does.Contain("..Outside.txt"));
        Assert.That(ex.Message, Does.Contain("https://example.com/x"));
    }

    [Test]
    public void HostMatches_OmittedPortUsesSchemeDefault()
    {
        var httpsDefault = new Uri("https://example.com/index.txt");
        var httpOn443 = new Uri("http://example.com:443/index.txt");
        var host = new HostString("example.com");

        Assert.That(RouteTable.HostMatches(httpsDefault, host), Is.True);
        Assert.That(RouteTable.HostMatches(httpOn443, host), Is.False);
    }
}

file sealed class TemporaryRoutes : IDisposable
{
    private TemporaryRoutes(string path) => Path = path;

    public string Path { get; }

    public static TemporaryRoutes Create(string routesJson)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(path);
        File.WriteAllText(System.IO.Path.Combine(path, "Routes.json"), routesJson);
        return new TemporaryRoutes(path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, true);
        }
    }
}
