using System.Diagnostics.CodeAnalysis;
using Hexa.NET.GLFW;
using Vertex.NET.Impeller;

namespace RenderDemo.Core;

public class GlfwApplication
{
    private readonly bool _isSupportVulkan = GLFW.VulkanSupported() == 1;
    private readonly GLFWwindowPtr _window;
    private IScene? _scene;

    public GlfwApplication(int width, int height, string title = "Window")
    {
        if (GLFW.Init() == 0)
            throw new Exception("Failed to create GLFW window");

        if (_isSupportVulkan)
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
            throw new Exception("Failed to create GLFW window");
        }

        if (_isSupportVulkan) return;

        GLFW.MakeContextCurrent(_window);
        GLFW.SwapInterval(1); // V-sync On
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public unsafe void Run()
    {
        using var context = _isSupportVulkan
            ? Impeller.ContextCreateVulkanNew(
                Impeller.GetVersion(),
                new ImpellerContextVulkanSettings(null, &VulkanProcCallback))
            : Impeller.ContextCreateOpenGLESNew(
                Impeller.GetVersion(),
                GLProcAddressCallback,
                null);

        ImpellerVulkanSwapchain swapChain = default;
        if (_isSupportVulkan)
        {
            ImpellerContextVulkanInfo vkInfo = default;
            if (!Impeller.ContextGetVulkanInfo(context, ref vkInfo))
                throw new Exception("Failed to get Vulkan info");

            VkSurfaceKHR surface = default;
            GLFW.CreateWindowSurface(new VkInstance((nint)vkInfo.VkInstance), _window, 0, ref surface);
            swapChain = Impeller.VulkanSwapchainCreateNew(context, surface.Handle);
        }

        GLFW.SetWindowRefreshCallback(_window, _ =>
        {
            if (_isSupportVulkan)
                RenderFrameVulkan(swapChain);
            else
                RenderFrameOpenGL(context);
        });

        while (GLFW.WindowShouldClose(_window) == 0)
        {
            GLFW.WaitEvents();

            if (_isSupportVulkan)
                RenderFrameVulkan(swapChain);
            else
                RenderFrameOpenGL(context);
        }

        GLFW.DestroyWindow(_window);
        GLFW.Terminate();
    }

    // ReSharper disable once InconsistentNaming
    private void RenderFrameOpenGL(ImpellerContext context)
    {
        int width = 0, height = 0;
        GLFW.GetWindowSize(_window, ref width, ref height);

        using var builder = Impeller.DisplayListBuilderNew(new ImpellerRect { Width = width, Height = height });
        using var surface = context.SurfaceCreateWrappedFboNew(
            0u,
            ImpellerPixelFormat.PixelFormatRgba8888,
            new ImpellerISize { Width = width, Height = height });

        _scene?.Render(builder, new SceneParameters(width, height));

        using var displayList = builder.CreateDisplayListNew();
        surface.DrawDisplayList(displayList);
        GLFW.SwapBuffers(_window);
    }

    private void RenderFrameVulkan(ImpellerVulkanSwapchain swapChain)
    {
        int width = 0, height = 0;
        GLFW.GetWindowSize(_window, ref width, ref height);

        using var surface = Impeller.VulkanSwapchainAcquireNextSurfaceNew(swapChain);
        using var builder = Impeller.DisplayListBuilderNew(new ImpellerRect { Width = width, Height = height });

        _scene?.Render(builder, new SceneParameters(width, height));

        using var displayList = builder.CreateDisplayListNew();
        surface.DrawDisplayList(displayList);
        surface.Present();
    }

    public void SetScene(IScene scene) => _scene = scene;

    // ReSharper disable once InconsistentNaming
    private static unsafe void* GLProcAddressCallback(byte* procName, void* _) =>
        GLFW.GetProcAddress(procName);

    private static unsafe void* VulkanProcCallback(void* userData, byte* procName, void* reserved) =>
        GLFW.GetInstanceProcAddress(VkInstance.Null, procName);
}