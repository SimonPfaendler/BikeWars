using System.Collections.Generic;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.managers;
// The screen manager handles all existing screens:
// -> decides which screen are drawn
// -> decide which screens are getting updated

public class ScreenManager
{
    // The Screen Stack contains all Screens that are in use at the moment
    // The manager handles Draw and Update for them using the stack
    private List<IScreen> _mScreenStack = new List<IScreen>();

    public void AddScreen(IScreen screen)
    {
        _mScreenStack.Add(screen); 
    }

    public void RemoveScreen(IScreen screen)
    {
        _mScreenStack.Remove(screen);
    }

    public void Draw(GameTime gameTime)
    {
        int lowestScreenToBeDrawn = _mScreenStack.Count - 1;
        for (int i = _mScreenStack.Count - 1; i >= 0; i--)
        {
            IScreen screen = _mScreenStack[i];
            if (screen.DrawLower)
            {
                lowestScreenToBeDrawn = i - 1;
            }
            else
            {
                break;
            }
        }
        // draw only the screens necessary
        for (int i = lowestScreenToBeDrawn; i < _mScreenStack.Count; i++)
        {
            IScreen screen = _mScreenStack[i];
            screen.Draw(gameTime);
        }
        
        
    }

    public void Update(GameTime gameTime)
    {
        // update all screens from the stack where DrawLower is true
        for (int i = _mScreenStack.Count - 1; i >= 0; i--)
        {
            IScreen screen = _mScreenStack[i];
            screen.Update(gameTime);
            if (!screen.UpdateLower)
            {
                break;
            }
        }
    }
}