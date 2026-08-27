using RenderDemo.Core;
using RenderDemo.Scenes;

var app = new GlfwApplication(800, 450, "Vertex.NET.Impeller RenderDemo - Press the key ESC to escape"u8);
app.Run(new TriangleScene());
// app.Run(new ParagraphScene());
// app.Run(new StarScene(), false);
// app.Run(new CirclingSquaresScene(), false);