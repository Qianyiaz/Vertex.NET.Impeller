using Vertex.NET.Impeller;

namespace Example.Core;

public class SceneParameters(int width, int height)
{
    public int Width { get; } = width;
    public int Height { get; } = height;
}

public interface IScene
{
    void Render(ImpellerDisplayListBuilder scene, SceneParameters sceneParameters);
}