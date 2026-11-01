using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace RenderDemo.Core;

/// <summary>
///     Helper for enabling dark-mode title bars on Windows.
///     Uses the DWM ImmersiveDarkMode attribute plus undocumented UxTheme exports,
///     and subclasses the window to auto-resync when the system theme changes.
/// </summary>
/// <remarks>
///     Requires Windows 10 1809 (10.0.17763) or later. On earlier builds the
///     undocumented UxTheme ordinals do not exist and invoking them crashes the process.
/// </remarks>
[SupportedOSPlatform("windows10.0.17763")]
public static unsafe partial class WindowsThemeHelper
{
    private const uint MsgActivate = 0x0086;
    private const uint MsgSettingChange = 0x001A;
    private const uint ImmersiveDarkSubclassId = 0x1001;

    private const string DwmApi = "dwmapi.dll";
    private const string ComCtl32 = "comctl32.dll";
    private const string User32 = "user32.dll";
    private const string UxTheme = "uxtheme.dll";

    private static readonly int DwmUseImmersiveDarkMode =
        OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041) ? 20 : 19;

    static WindowsThemeHelper() => SetPreferredAppMode(1); // Allow Dark

    /// <summary>Gets whether the system is currently in dark mode (via undocumented UxTheme state).</summary>
    public static bool IsSystemUseDarkMode => GetSystemIsUseDarkMode();

    /// <summary>
    ///     Synchronizes the window title bar to the given dark/light mode.
    ///     If the current mode already matches, no write is performed.
    /// </summary>
    /// <param name="hwnd">Target window handle.</param>
    /// <param name="isDarkMode"><see langword="true" /> for a dark title bar, <see langword="false" /> for light.</param>
    /// <exception cref="ArgumentNullException"><paramref name="hwnd" /> is zero.</exception>
    public static void SyncWindowThemeMode(nint hwnd, bool isDarkMode)
    {
        if (hwnd == IntPtr.Zero)
            throw new ArgumentNullException(nameof(hwnd));

        var hr = DwmGetWindowAttribute(hwnd, DwmUseImmersiveDarkMode, out var current, sizeof(int));
        if (hr == 0 && current == isDarkMode)
            return;

        ApplyWindowThemeMode(hwnd, isDarkMode);
    }

    /// <summary>
    ///     Unconditionally applies the dark/light mode: writes the DWM attribute,
    ///     flushes menu themes, and dispatches two WM_ACTIVATE messages to force a
    ///     title-bar repaint.
    /// </summary>
    /// <param name="hwnd">Target window handle.</param>
    /// <param name="isDarkMode"><see langword="true" /> for dark, <see langword="false" /> for light.</param>
    /// <exception cref="ArgumentNullException"><paramref name="hwnd" /> is zero.</exception>
    /// <remarks>
    ///     Dispatching WM_ACTIVATE twice (wParam = 0, then 1) is a community-known
    ///     trick: on some DWM versions <c>DwmSetWindowAttribute</c> alone does not
    ///     immediately repaint the non-client area, and manually dispatching an
    ///     activation message forces the refresh.
    /// </remarks>
    public static void ApplyWindowThemeMode(nint hwnd, bool isDarkMode)
    {
        if (hwnd == IntPtr.Zero)
            throw new ArgumentNullException(nameof(hwnd));

        AllowDarkModeForWindow(hwnd, isDarkMode);
        DwmSetWindowAttribute(hwnd, DwmUseImmersiveDarkMode, ref isDarkMode, sizeof(int));

        FlushMenuThemes();

        DefWindowProcW(hwnd, MsgActivate, IntPtr.Zero, IntPtr.Zero);
        DefWindowProcW(hwnd, MsgActivate, new IntPtr(1), IntPtr.Zero);
    }

    /// <summary>
    ///     Subclasses a window to listen for <c>WM_SETTINGCHANGE("ImmersiveColorSet")</c>
    ///     and automatically re-synchronize the title bar when the system light/dark
    ///     mode changes.
    /// </summary>
    /// <remarks>
    ///     This is a static implementation with a fixed <c>dwRefData = 0</c>:
    ///     <list type="bullet">
    ///         <item>
    ///             No <c>WM_NCDESTROY</c> self-cleanup — <see cref="UnWatch" /> must be
    ///             called explicitly before the window is destroyed.
    ///         </item>
    ///         <item>
    ///             Only one subscription per window is supported; calling
    ///             <see cref="Watch" /> twice overrides the previous subclass (same id).
    ///         </item>
    ///     </list>
    /// </remarks>
    public static class SystemThemeWatcher
    {
        private static readonly delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nuint, nint, nint>
            SubclassProcPtr = &OnSubclass;

        /// <summary>
        ///     Installs the theme-change watcher on the specified window and
        ///     immediately synchronizes the current system theme once.
        /// </summary>
        /// <param name="hwnd">Target window handle; must be valid.</param>
        /// <exception cref="ArgumentException"><paramref name="hwnd" /> is zero.</exception>
        public static void Watch(nint hwnd)
        {
            if (hwnd == IntPtr.Zero)
                throw new ArgumentException("Invalid window handle", nameof(hwnd));

            SyncWindowThemeMode(hwnd, IsSystemUseDarkMode);
            SetWindowSubclass(hwnd, SubclassProcPtr, ImmersiveDarkSubclassId, IntPtr.Zero);
        }

        /// <summary>
        ///     Removes the theme-change watcher from the specified window.
        ///     Must be called before the window is destroyed.
        /// </summary>
        /// <param name="hwnd">Window handle previously passed to <see cref="Watch" />.</param>
        public static void UnWatch(nint hwnd) => RemoveWindowSubclass(hwnd, SubclassProcPtr, ImmersiveDarkSubclassId);

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static nint OnSubclass(nint hwnd, uint msg, nint wParam, nint lParam, nuint uIdSubclass, nint dwRefData)
        {
            switch (msg)
            {
                case MsgSettingChange when Marshal.PtrToStringUni(lParam) is "ImmersiveColorSet":
                    SyncWindowThemeMode(hwnd, IsSystemUseDarkMode);
                    break;
            }

            return DefSubclassProc(hwnd, msg, wParam, lParam);
        }
    }

    #region Imports

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(DwmApi)]
    private static partial void DwmSetWindowAttribute(nint hwnd, int attr,
        [MarshalAs(UnmanagedType.Bool)] ref bool attrValue, int attrSize);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(DwmApi)]
    private static partial int DwmGetWindowAttribute(nint hwnd, int dwAttribute,
        [MarshalAs(UnmanagedType.Bool)] out bool pvAttribute, int cbAttribute);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(User32)]
    private static partial void DefWindowProcW(nint hwnd, uint msg, nint wParam, nint lParam);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(UxTheme, EntryPoint = "#133")]
    private static partial void AllowDarkModeForWindow(nint hwnd, [MarshalAs(UnmanagedType.Bool)] bool allow);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(UxTheme, EntryPoint = "#135")]
    private static partial void SetPreferredAppMode(int appMode);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(UxTheme, EntryPoint = "#136")]
    private static partial void FlushMenuThemes();

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    [LibraryImport(UxTheme, EntryPoint = "#138")]
    private static partial bool GetSystemIsUseDarkMode();

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(ComCtl32)]
    private static partial void SetWindowSubclass(nint hwnd,
        delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nuint, nint, nint> pfnSubclass,
        nuint uIdSubclass, nint dwRefData);

    [SuppressGCTransition]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(ComCtl32)]
    private static partial void RemoveWindowSubclass(nint hwnd,
        delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nuint, nint, nint> pfnSubclass,
        nuint uIdSubclass);

    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [LibraryImport(ComCtl32)]
    private static partial nint DefSubclassProc(nint hwnd, uint uMsg, nint wParam, nint lParam);

    #endregion
}