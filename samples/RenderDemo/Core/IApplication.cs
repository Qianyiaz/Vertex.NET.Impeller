namespace RenderDemo.Core;

public interface IApplication
{
    public bool Initialize(int width, int height, ReadOnlySpan<byte> title);
    public bool Run();
    public void SetScene(IScene scene);
}