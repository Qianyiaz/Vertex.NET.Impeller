using Example.Core;
using Example.Scenes;

var app = new GlfwApplication(800, 450, "Vertex.NET.Impeller Example");
app.SetScene(new TriangleScene());
app.Run();