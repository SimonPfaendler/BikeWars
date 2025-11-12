namespace BikeWars.Content.engine.interfaces;
// interface for all screens existing
public interface IScreen
{
    public void Update();
    
    public void Draw();

    public bool UpdateLower { get; }

    public bool DrawLower { get; }
}