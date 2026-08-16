using Example.Core;
using Example.Scenes;

var app = new GlfwApplication(800, 450, "Vertex.NET.Impeller Example");
app.SetScene(new TriangleScene()); // Can be another one like: ParagraphScene,etc...
// app.SetScene(new ParagraphScene());
app.Run();