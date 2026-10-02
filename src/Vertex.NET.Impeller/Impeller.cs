using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Vertex.NET.Impeller;

public partial class Impeller
{
    static Impeller() =>
        NativeLibrary.SetDllImportResolver(typeof(Impeller).Assembly, ResolveDllImport);

    private static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var handle) ||
            NativeLibrary.TryLoad(GetLibraryPath(libraryName), out handle))
            return handle;

        throw new DllNotFoundException(
            $"Unable to load native library '{libraryName}' (RID: {GetNormalizedRuntimeIdentifier()}).");
    }

    private static string GetLibraryPath(string libraryName) =>
        Path.Combine(AppContext.BaseDirectory, "runtimes", GetNormalizedRuntimeIdentifier(), "native",
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{libraryName}.dll"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"lib{libraryName}.dylib"
            : $"lib{libraryName}.so");

    /// <summary>
    ///     Gets a normalized runtime identifier that's consistent across different installation methods.
    ///     https://github.com/dotnet/runtime/issues/114156#issuecomment-2773234611
    /// </summary>
    private static string GetNormalizedRuntimeIdentifier()
    {
        var rid = RuntimeInformation.RuntimeIdentifier;

        // Already in the expected format
        if (rid is "win-x64" or "win-arm64"
            or "linux-x64" or "linux-arm64"
            or "linux-musl-x64" or "linux-musl-arm64"
            or "osx-x64" or "osx-arm64")
            return rid;

        // Extract architecture (defaults to x64 if not present)
        var arch = rid.Contains('-') ? rid[(rid.LastIndexOf('-') + 1)..] : "x64";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return $"win-{arch}";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return $"osx-{arch}";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return rid.Contains("alpine") || IsAlpineLinux()
                ? $"linux-musl-{arch}"
                : $"linux-{arch}";

        // Fallback to the original RID if we can't normalize it
        return rid;
    }

    /// <summary>Checks if the current Linux distribution is Alpine (musl-based).</summary>
    private static bool IsAlpineLinux()
    {
        try
        {
            if (File.Exists("/etc/alpine-release"))
                return true;

            return (File.Exists("/etc/os-release") &&
                    File.ReadAllText("/etc/os-release")
                        .Contains("ID=\"alpine\"", StringComparison.Ordinal)) ||
                   File.ReadAllText("/etc/os-release")
                       .Contains("ID=alpine", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }
}