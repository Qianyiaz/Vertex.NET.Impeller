using RenderDemo.Core;
using RenderDemo.Scenes;

var app = new GlfwApplication(800, 450, "Vertex.NET.Impeller RenderDemo");
app.SetScene(new TriangleScene()); // Can be another one like: ParagraphScene,etc...
// app.SetScene(new ParagraphScene());
app.Run();