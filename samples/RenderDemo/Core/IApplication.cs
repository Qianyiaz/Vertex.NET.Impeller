namespace RenderDemo.Core;

/// <summary>
/// Represents a renderable application backed by a native window and a graphics backend.
/// </summary>
/// <remarks>
/// Lifecycle contract:
/// <list type="number">
/// <item><see cref="Initialize"/> must be called exactly once before <see cref="Run"/>.</item>
/// <item><see cref="SetScene"/> must be called at least once before <see cref="Run"/>.</item>
/// <item><see cref="Run"/> blocks until the window is closed; it is not re-entrant.</item>
/// </list>
/// </remarks>
public interface IApplication
{
    /// <summary>
    /// Initializes the windowing system and creates the native window.
    /// </summary>
    /// <param name="width">Initial client-area width in pixels.</param>
    /// <param name="height">Initial client-area height in pixels.</param>
    /// <param name="title">Window title, encoded as UTF-8 (no null terminator required).</param>
    /// <returns>
    /// <see langword="true"/> on success; <see langword="false"/> if the windowing system,
    /// window, or graphics backend could not be initialized.
    /// On failure the application is left in an undefined state and must not be used further.
    /// </returns>
    bool Initialize(int width, int height, ReadOnlySpan<byte> title);

    /// <summary>
    /// Enters the main render loop and blocks until the window is closed.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the loop exited normally;
    /// <see langword="false"/> if a required dependency (e.g. the scene) was not set.
    /// </returns>
    /// <remarks>
    /// <see cref="Initialize"/> and <see cref="SetScene"/> must be called before this method.
    /// </remarks>
    bool Run();

    /// <summary>
    /// Sets the scene to render. Must be called before <see cref="Run"/>.
    /// </summary>
    /// <param name="scene">The scene to render; must not be <see langword="null"/>.</param>
    void SetScene(IScene scene);
}