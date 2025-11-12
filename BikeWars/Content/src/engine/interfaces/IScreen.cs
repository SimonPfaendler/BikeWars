namespace BikeWars.Content.engine.interfaces;
// interface for all screens existing
public interface IScreen
{
    void Update();
    
    void Draw();

    bool UpdateLower { get; }

    bool DrawLower { get; }
}