using RenderDemo.Core;
using RenderDemo.Scenes;

var app = new GlfwApplication(800, 450, "Vertex.NET.Impeller RenderDemo");
app.Run(new TriangleScene());
// app.Run(new ParagraphScene());
// app.Run(new CirclingSquaresScene(), false);