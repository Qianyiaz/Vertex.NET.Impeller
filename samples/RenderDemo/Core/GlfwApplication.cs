using System.Diagnostics.CodeAnalysis;
using Hexa.NET.GLFW;
using Vertex.NET.Impeller;

namespace RenderDemo.Core;

public sealed class GlfwApplication : IApplication
{
    private readonly bool _isVulkanSupported = GLFW.VulkanSupported() == 1;
    private ImpellerContext _context;
    private SceneParameters _parameters = null!;
    private IScene? _scene;
    private ImpellerSurface _surface;
    private ImpellerVulkanSwapchain _swapChain;
    private GLFWwindowPtr _window;

    /// <summary>
    ///     Initialize GLFW and create the window. Returns true on success.
    /// </summary>
    public unsafe bool Initialize(int width, int height, ReadOnlySpan<byte> title)
    {
        if (GLFW.Init() == 0)
            return false;

        if (_isVulkanSupported)
        {
            GLFW.WindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_NO_API);
        }
        else
        {
            GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
            GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 1);
            GLFW.WindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_OPENGL_ES_API);
        }

        _window = GLFW.CreateWindow(width, height, title, null, null);
        if (_window.IsNull)
        {
            GLFW.Terminate();
            return false;
        }

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
            WindowsThemeHelper.SystemThemeWatcher.Watch(GLFW.GetWin32Window(_window));

        if (!_isVulkanSupported)
        {
            GLFW.MakeContextCurrent(_window);
            GLFW.SwapInterval(1); // V-sync On
        }

        _context = _isVulkanSupported
            ? Impeller.ContextCreateVulkanNew(
                Impeller.GetVersion(),
                new ImpellerContextVulkanSettings(null, &VulkanProcCallback))
            : Impeller.ContextCreateOpenGLESNew(
                Impeller.GetVersion(),
                GLProcAddressCallback,
                null);
        return !_context.IsNull && (!_isVulkanSupported || InitializeVulkanBackend());
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public unsafe bool Run()
    {
        if (_scene == null)
            return false;

        int width = 0, height = 0;
        GLFW.GetFramebufferSize(_window, ref width, ref height);
        Resize(_window, width, height);

        GLFW.SetFramebufferSizeCallback(_window, Resize);
        while (GLFW.WindowShouldClose(_window) == 0)
        {
            RenderFrame();
            GLFW.PollEvents();
        }

        _surface.Dispose();
        _swapChain.Dispose();
        _context.Dispose();

        GLFW.SetFramebufferSizeCallback(_window, default(GLFWframebuffersizefun));
        GLFW.DestroyWindow(_window);
        GLFW.Terminate();
        return true;
    }

    public void SetScene(IScene scene) => _scene = scene;

    private unsafe void Resize(GLFWwindow* _, int width, int height)
    {
        _surface.Dispose();
        _surface = _isVulkanSupported
            ? _swapChain.AcquireNextSurfaceNew()
            : _context.SurfaceCreateWrappedFboNew(0u, ImpellerPixelFormat.PixelFormatRgba8888,
                new ImpellerISize { Width = width, Height = height });
        _parameters = new SceneParameters(width, height);
        RenderFrame();
    }

    private void RenderFrame()
    {
        using (var builder = Impeller.DisplayListBuilderNew(new ImpellerRect
                   { Width = _parameters.Width, Height = _parameters.Height }))
        {
            _scene!.Render(builder, _parameters);
            using (var displayList = builder.CreateDisplayListNew()) 
                _surface.DrawDisplayList(displayList);
        }

        if (_isVulkanSupported)
            _surface.Present();
        else
            GLFW.SwapBuffers(_window);
    }

    private unsafe bool InitializeVulkanBackend()
    {
        ImpellerContextVulkanInfo vkInfo = default;
        if (!_context.GetVulkanInfo(ref vkInfo))
            return false;

        VkSurfaceKHR surfaceKhr = default;
        var result = GLFW.CreateWindowSurface(new VkInstance((nint)vkInfo.VkInstance), _window, null, ref surfaceKhr);

        if (result == 0)
            return false;

        _swapChain = _context.VulkanSwapchainCreateNew(surfaceKhr.Handle);
        return true;
    }

    // ReSharper disable once InconsistentNaming
    private static unsafe void* GLProcAddressCallback(byte* procName, void* _) =>
        GLFW.GetProcAddress(procName);

    private static unsafe void* VulkanProcCallback(void* vkInstance, byte* procName, void* userData) =>
        GLFW.GetInstanceProcAddress(new VkInstance((nint)vkInstance), procName);
}