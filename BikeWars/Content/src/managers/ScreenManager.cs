using System;
using System.Collections.Generic;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
// The screen manager handles all existing screens:
// -> decides which screen are drawn
// -> decide which screens are getting updated
namespace BikeWars.Content.managers
{
    public class ScreenManager
    {
        // The Screen Stack contains all Screens that are in use at the moment
        // The manager handles Draw and Update for them using the stack
        private List<IScreen> _mScreenStack = new List<IScreen>();
        
        public event Action<IScreen> OnScreenAdded;
        public event Action<IScreen> OnScreenRemoved; 
        public event Action OnReturnToMainMenu;

        public void AddScreen(IScreen screen)
        {
            screen.ScreenManager = this;
            _mScreenStack.Add(screen);
            OnScreenAdded?.Invoke(screen);
        }

        public void RemoveScreen(IScreen screen)
        {
            _mScreenStack.Remove(screen);
            OnScreenRemoved?.Invoke(screen);
        }

        public void ReturnToMainMenu()
        {
            _mScreenStack.Clear();
            OnReturnToMainMenu?.Invoke();
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
            
            for (int i = lowestScreenToBeDrawn; i < _mScreenStack.Count; i++)
            {
                IScreen screen = _mScreenStack[i];
                screen.Draw(gameTime);
            }
        }

        public void Update(GameTime gameTime)
        {
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
}