using System.Diagnostics.CodeAnalysis;
using Hexa.NET.GLFW;
using Vertex.NET.Impeller;

namespace RenderDemo.Core;

public class GlfwApplication
{
    private readonly bool _isVulkanSupported = GLFW.VulkanSupported() == 1;
    private readonly GLFWwindowPtr _window;
    private IScene? _scene;

    public GlfwApplication(int width, int height, string title = "Window")
    {
        if (GLFW.Init() == 0)
            throw new Exception("Failed to create GLFW window");

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
            throw new Exception("Failed to create GLFW window");
        }

        if (_isVulkanSupported) return;

        GLFW.MakeContextCurrent(_window);
        GLFW.SwapInterval(1); // V-sync On
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public unsafe void Run()
    {
        using var context = _isVulkanSupported
            ? Impeller.ContextCreateVulkanNew(
                Impeller.GetVersion(),
                new ImpellerContextVulkanSettings(null, &VulkanProcCallback))
            : Impeller.ContextCreateOpenGLESNew(
                Impeller.GetVersion(),
                GLProcAddressCallback,
                null);

        ImpellerVulkanSwapchain swapChain = default;
        if (_isVulkanSupported)
        {
            ImpellerContextVulkanInfo vkInfo = default;
            if (!Impeller.ContextGetVulkanInfo(context, ref vkInfo))
                throw new Exception("Failed to get Vulkan info");

            VkSurfaceKHR surfaceKhr = default;
            GLFW.CreateWindowSurface(new VkInstance((nint)vkInfo.VkInstance), _window, 0, ref surfaceKhr);
            swapChain = Impeller.VulkanSwapchainCreateNew(context, surfaceKhr.Handle);
        }

        int width = 0, height = 0;
        GLFW.GetFramebufferSize(_window, ref width, ref height);
        var surface = _isVulkanSupported
            ? Impeller.VulkanSwapchainAcquireNextSurfaceNew(swapChain)
            : context.SurfaceCreateWrappedFboNew(
                0u, ImpellerPixelFormat.PixelFormatRgba8888, new ImpellerISize { Width = width, Height = height });

        var parameters = new SceneParameters(width, height);
        GLFW.SetFramebufferSizeCallback(_window, (_, w, h) =>
        {
            parameters = new SceneParameters(w, h);

            surface.Dispose();
            if (_isVulkanSupported)
            {
                surface = Impeller.VulkanSwapchainAcquireNextSurfaceNew(swapChain);
                RenderFrame(surface, parameters);
                surface.Present();
            }
            else
            {
                surface = context.SurfaceCreateWrappedFboNew(
                    0u, ImpellerPixelFormat.PixelFormatRgba8888, new ImpellerISize { Width = w, Height = h });
                RenderFrame(surface, parameters);
                GLFW.SwapBuffers(_window);
            }
        });

        while (GLFW.WindowShouldClose(_window) == 0)
        {
            GLFW.PollEvents();

            if (_isVulkanSupported)
            {
                RenderFrame(surface, parameters);
                surface.Present();
            }
            else
            {
                RenderFrame(surface, parameters);
                GLFW.SwapBuffers(_window);
            }
        }

        if (_isVulkanSupported)
            swapChain.Dispose();

        surface.Dispose();
        GLFW.DestroyWindow(_window);
        GLFW.Terminate();
    }

    private void RenderFrame(ImpellerSurface surface, SceneParameters parameters)
    {
        using var builder = Impeller.DisplayListBuilderNew(new ImpellerRect
            { Width = parameters.Width, Height = parameters.Height });

        _scene?.Render(builder, parameters);

        using var displayList = builder.CreateDisplayListNew();
        surface.DrawDisplayList(displayList);
    }

    public void SetScene(IScene scene) => _scene = scene;

    // ReSharper disable once InconsistentNaming
    private static unsafe void* GLProcAddressCallback(byte* procName, void* _) =>
        GLFW.GetProcAddress(procName);

    private static unsafe void* VulkanProcCallback(void* userData, byte* procName, void* reserved) =>
        GLFW.GetInstanceProcAddress(VkInstance.Null, procName);
}