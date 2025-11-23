using BikeWars.Content.managers;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
// interface for all screens existing
public interface IScreen
{
    public void Update(GameTime gameTime);
    
    public void Draw(GameTime gameTime);

    // Decide whether the Screen below (on the Stack) should be Drawn and/or updated

    public bool UpdateLower { get; }

    public bool DrawLower { get; }
    
    ScreenManager ScreenManager { set; }
}