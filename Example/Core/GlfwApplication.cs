using Hexa.NET.GLFW;
using Vertex.NET.Impeller;

namespace Example.Core;

public class GlfwApplication
{
    private readonly ImpellerContext _context;
    private readonly GLFWwindowPtr _window;
    private IScene? _scene;

    public unsafe GlfwApplication(int width, int height, string title = "Window")
    {
        if (GLFW.Init() == 0)
            throw new Exception("Failed to create GLFW window");

        var isSupportVulkan = GLFW.VulkanSupported() == 1;
        if (!isSupportVulkan)
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

        _context = isSupportVulkan
            ? Impeller.ContextCreateVulkanNew(Impeller.GetVersion(),
                new ImpellerContextVulkanSettings(procAddressCallback: &VulkanProcCallback))
            : Impeller.ContextCreateOpenGLESNew(Impeller.GetVersion(),
                GlProcAddressCallback,
                null);
    }

    public unsafe void Run()
    {
        int width = 0, height = 0;
        GLFW.GetWindowSize(_window, ref width, ref height);

        var surface = _context.SurfaceCreateWrappedFboNew(0u, ImpellerPixelFormat.PixelFormatRgba8888,
            new ImpellerISize { Width = width, Height = height });
        var builder = Impeller.DisplayListBuilderNew(new ImpellerRect { Width = width, Height = height });

        _scene?.Render(builder, new SceneParameters(width, height));

        var displayList = builder.CreateDisplayListNew();

        GLFW.SetFramebufferSizeCallback(_window, (_, w, h) =>
        {
            displayList.Release();
            surface.Release();

            surface = _context.SurfaceCreateWrappedFboNew(0u, ImpellerPixelFormat.PixelFormatRgba8888,
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

        displayList.Release();
        builder.Release();
        surface.Release();
        _context.Release();

        GLFW.DestroyWindow(_window);
        GLFW.Terminate();
    }

    public void SetScene(IScene scene) => _scene = scene;

    private static unsafe void* GlProcAddressCallback(byte* procName, void* _) =>
        GLFW.GetProcAddress(procName);

    private static unsafe void* VulkanProcCallback(void* userData, byte* procName, void* reserved) =>
        GLFW.GetProcAddress(procName);
}