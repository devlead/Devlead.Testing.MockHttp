using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace Devlead.Testing.MockHttp;

public static class Resources<T>
{
    private static readonly (Type Type, Assembly Assembly, string Namespace) TypeInfo
        = typeof(T) is Type type
        ? (type, type.Assembly, type.Namespace ?? throw new ArgumentNullException(nameof(type.Namespace)))
        : throw new InvalidOperationException("Failed to get type info");

    private static readonly ConcurrentDictionary<string, string> _stringResources = new();

    public static string? GetString(string filename)
        => _stringResources.GetOrAdd(
            filename,
            _ => GetResourceString(filename)
            );

    private static readonly ConcurrentDictionary<string, byte[]> _byteResources = new();

    public static byte[] GetBytes(string filename)
        => _byteResources.GetOrAdd(
            filename,
            _ => GetResourceBytes(filename)
            );


    private static byte[] GetResourceBytes(string filename)
    {
        using var stream = GetResourceStream(filename);
        using var targetStream = new MemoryStream();
        stream.CopyTo(targetStream);
        return targetStream.ToArray();
    }

    private static string GetResourceString(string filename, Encoding? encoding = null)
    {
        using var stream = GetResourceStream(filename);
        using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static Stream GetResourceStream(string filename)
    {
        var resourceStream = TypeInfo
                                .Assembly 
                                .GetManifestResourceStream($"{TypeInfo.Namespace}.Resources.{filename}");

        return resourceStream
            ?? throw new Exception($"Failed to get stream for {filename}.");
    }
}
