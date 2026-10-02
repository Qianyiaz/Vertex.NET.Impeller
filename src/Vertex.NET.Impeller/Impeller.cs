using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Vertex.NET.Impeller;

public partial class Impeller
{
    static Impeller() => NativeLibrary.SetDllImportResolver(typeof(Impeller).Assembly, ResolveDllImport);

    public static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        string resolveNameError;
        try
        {
            var handle = NativeLibrary.Load(libraryName, assembly, searchPath);
            return handle;
        }
        catch (Exception ex)
        {
            resolveNameError = ex.Message;
        }

        string resolvePathError;
        var libraryPath = GetLibraryPath(libraryName);
        try
        {
            var handle = NativeLibrary.Load(libraryPath);
            return handle;
        }
        catch (Exception ex)
        {
            resolvePathError = ex.Message;
        }

        throw new DllNotFoundException(
            string.Join("\n", string.Empty, resolveNameError, resolvePathError)
        );
    }

    public static string GetLibraryPath(string libraryName)
    {
        var appBase = AppContext.BaseDirectory;
        var rid = GetNormalizedRuntimeIdentifier();

        var path = Path.Combine(appBase, "runtimes", rid, "native");

        var fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"{libraryName}.dll"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? $"lib{libraryName}.dylib"
                : $"lib{libraryName}.so";

        return Path.Combine(path, fileName);
    }

    /// <summary>
    ///     Gets a normalized runtime identifier that's consistent across different installation methods.
    ///     https://github.com/dotnet/runtime/issues/114156#issuecomment-2773234611
    /// </summary>
    public static string GetNormalizedRuntimeIdentifier()
    {
        var rid = RuntimeInformation.RuntimeIdentifier;

        // If already in the expected format, return as is
        if (rid is "win-x64" or "win-arm64" or "linux-x64" or "linux-arm64" or "linux-musl-x64" or "linux-musl-arm64"
            or "osx-x64" or "osx-arm64") return rid;

        // Handle OS-specific RIDs from native repositories

        // Extract architecture (should be the part after the last dash)
        var architecture = "x64"; // Default
        if (rid.Contains('-')) architecture = rid[(rid.LastIndexOf('-') + 1)..];

        // Determine OS and variant
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return $"win-{architecture}";

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? $"osx-{architecture}"
                :
                // Fallback to the original RID if we can't normalize it
                rid;
        // Check if it's Alpine Linux (musl-based)
        if (rid.Contains("alpine") || IsAlpineLinux()) return $"linux-musl-{architecture}";
        return $"linux-{architecture}";
    }

    /// <summary>
    ///     Checks if the current Linux distribution is Alpine (musl-based).
    /// </summary>
    private static bool IsAlpineLinux()
    {
        try
        {
            // Check for /etc/os-release file which contains distribution info
            if (!File.Exists("/etc/os-release")) return File.Exists("/etc/alpine-release");
            var content = File.ReadAllText("/etc/os-release");
            return content.Contains("ID=alpine") || content.Contains("ID=\"alpine\"");
            // Alternative check for /etc/alpine-release
        }
        catch
        {
            return false;
        }
    }
}