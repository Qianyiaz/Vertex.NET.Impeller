using RenderDemo.Core;
using RenderDemo.Scenes;

namespace RenderDemo;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        var app = new GlfwApplication();
        if (app.Initialize(800, 450, "Vertex.NET.Impeller RenderDemo"u8))
            app.SetScene(new TriangleScene()).Run();
        // app.SetScene(new ParagraphScene());
        // app.SetScene(new StarScene());
        // app.SetScene(new CirclingSquaresScene());
    }
}