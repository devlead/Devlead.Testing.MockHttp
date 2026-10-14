using Devlead.Testing.MockHttp.Tool.Commands;

namespace Devlead.Testing.MockHttp.Tool.Tests;

public class ServeSettingsTests
{
    [Test]
    public void ResolveListenUrl_DefaultPort()
    {
        var url = ServeCommand.ResolveListenUrl(new ServeSettings());

        Assert.That(url, Is.EqualTo("http://127.0.0.1:5080"));
    }

    [Test]
    public void ResolveListenUrl_CustomPort()
    {
        var url = ServeCommand.ResolveListenUrl(new ServeSettings { Port = 6001 });

        Assert.That(url, Is.EqualTo("http://127.0.0.1:6001"));
    }

    [Test]
    public void ResolveListenUrl_UrlOverridesPort()
    {
        var url = ServeCommand.ResolveListenUrl(
            new ServeSettings
            {
                Port = 6001,
                Url = "http://127.0.0.1:7000/ignored"
            });

        Assert.That(url, Is.EqualTo("http://127.0.0.1:7000"));
    }

    [Test]
    public void ResolveListenUrl_InvalidUrlThrows()
    {
        Assert.Throws<ArgumentException>(
            () => ServeCommand.ResolveListenUrl(new ServeSettings { Url = "not-a-url" }));
    }
}
