namespace RenderDemo.Core;

public interface IApplication
{
    public bool Initialize(int width, int height, ReadOnlySpan<byte> title);
    public bool Run();
    public IApplication SetScene(IScene scene);
}