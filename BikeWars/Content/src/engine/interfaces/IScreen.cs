using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
// interface for all screens existing
public interface IScreen
{
    public void Update(GameTime gameTime);
    
    public void Draw(GameTime gameTime);

    public bool UpdateLower { get; }

    public bool DrawLower { get; }
}