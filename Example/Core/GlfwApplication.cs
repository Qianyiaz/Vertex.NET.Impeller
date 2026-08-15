using System.Diagnostics.CodeAnalysis;
using Hexa.NET.GLFW;
using Vertex.NET.Impeller;

namespace Example.Core;

public class GlfwApplication
{
    private readonly bool _isSupportVulkan = GLFW.VulkanSupported() == 1;
    private readonly GLFWwindowPtr _window;
    private IScene? _scene;

    public GlfwApplication(int width, int height, string title = "Window")
    {
        if (GLFW.Init() == 0)
            throw new Exception("Failed to create GLFW window");

        if (!_isSupportVulkan)
        {
            GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
            GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 1);
            GLFW.WindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_OPENGL_ES_API);
        }

        _window = GLFW.CreateWindow(width, height, title, null, null);
        if (_window.IsNull)
        {
            GLFW.Terminate();
            throw new Exception("Failed to create GLFW window");
        }

        GLFW.MakeContextCurrent(_window);
        GLFW.WindowHint(GLFW.GLFW_RESIZABLE, 1);
        GLFW.SwapInterval(1); // V-sync On
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public unsafe void Run()
    {
        using var context = _isSupportVulkan
            ? Impeller.ContextCreateVulkanNew(Impeller.GetVersion(),
                new ImpellerContextVulkanSettings(procAddressCallback: &VulkanProcCallback))
            : Impeller.ContextCreateOpenGLESNew(Impeller.GetVersion(),
                GlProcAddressCallback,
                null);

        int width = 0, height = 0;
        GLFW.GetWindowSize(_window, ref width, ref height);

        var surface = context.SurfaceCreateWrappedFboNew(0u, ImpellerPixelFormat.PixelFormatRgba8888,
            new ImpellerISize { Width = width, Height = height });

        using var builder = Impeller.DisplayListBuilderNew(new ImpellerRect { Width = width, Height = height });

        _scene?.Render(builder, new SceneParameters(width, height));
        var displayList = builder.CreateDisplayListNew();

        GLFW.SetFramebufferSizeCallback(_window, (_, w, h) =>
        {
            displayList.Dispose();
            surface.Dispose();

            surface = context.SurfaceCreateWrappedFboNew(0u, ImpellerPixelFormat.PixelFormatRgba8888,
                new ImpellerISize { Width = w, Height = h });

            _scene?.Render(builder, new SceneParameters(w, h));
            displayList = builder.CreateDisplayListNew();
        });

        GLFW.SetWindowRefreshCallback(_window, _ =>
        {
            surface.DrawDisplayList(displayList);
            GLFW.SwapBuffers(_window);
        });

        while (GLFW.WindowShouldClose(_window) == 0)
            GLFW.WaitEvents();

        displayList.Dispose();
        surface.Dispose();

        GLFW.DestroyWindow(_window);
        GLFW.Terminate();
    }

    public void SetScene(IScene scene) => _scene = scene;

    private static unsafe void* GlProcAddressCallback(byte* procName, void* _) =>
        GLFW.GetProcAddress(procName);

    private static unsafe void* VulkanProcCallback(void* userData, byte* procName, void* reserved) =>
        GLFW.GetProcAddress(procName);
}