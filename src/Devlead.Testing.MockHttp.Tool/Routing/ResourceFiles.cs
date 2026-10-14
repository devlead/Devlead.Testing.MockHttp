namespace Devlead.Testing.MockHttp.Tool.Routing;

internal static class ResourceFiles
{
    public static byte[] GetBytes(string resourcesDirectory, string contentResource, string? absoluteUri = null)
    {
        var fullPath = ResolvePath(resourcesDirectory, contentResource, absoluteUri);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(Describe(contentResource, absoluteUri, "was not found"), fullPath);
        }

        return File.ReadAllBytes(fullPath);
    }

    internal static string ResolvePath(string resourcesDirectory, string contentResource, string? absoluteUri = null)
    {
        if (Path.IsPathRooted(contentResource))
        {
            throw new InvalidOperationException(Describe(contentResource, absoluteUri, "must be relative to the resources directory"));
        }

        var relativePath = ToRelativePath(contentResource);
        if (Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException(Describe(contentResource, absoluteUri, "must be relative to the resources directory"));
        }

        var root = Path.GetFullPath(resourcesDirectory);
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                     + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(prefix, comparison) && !string.Equals(fullPath, root, comparison))
        {
            throw new InvalidOperationException(Describe(contentResource, absoluteUri, "must stay inside the resources directory"));
        }

        return fullPath;
    }

    internal static string ToRelativePath(string contentResource)
    {
        var lastDot = contentResource.LastIndexOf('.');
        if (lastDot <= 0)
        {
            return contentResource.Replace('.', Path.DirectorySeparatorChar);
        }

        return contentResource[..lastDot].Replace('.', Path.DirectorySeparatorChar) + contentResource[lastDot..];
    }

    private static string Describe(string contentResource, string? absoluteUri, string suffix)
    {
        return absoluteUri is { Length: > 0 }
            ? $"ContentResource {contentResource} for {absoluteUri} {suffix}."
            : $"ContentResource {contentResource} {suffix}.";
    }
}
