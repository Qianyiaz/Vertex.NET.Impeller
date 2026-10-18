using RenderDemo.Core;
using RenderDemo.Scenes;

IScene scene = args.FirstOrDefault() switch
{
    "paragraph" => new ParagraphScene(),
    "star" => new StarScene(),
    "squares" => new CirclingSquaresScene(),
    _ => new TriangleScene()
};

var app = new GlfwApplication();
if (app.Initialize(800, 450, "Vertex.NET.Impeller RenderDemo"u8))
{
    app.SetScene(scene);
    app.Run();
}