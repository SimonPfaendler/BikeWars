#nullable enable

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.engine.interfaces;
// interface for all screens existing
public interface IScreen: IDisposable
{
    public void Update(GameTime gameTime);

    public void Draw(GameTime gameeTime, SpriteBatch sb);
    public void OnActivated();

    // Decide whether the Screen below (on the Stack) should be Drawn and/or updated

    public bool UpdateLower { get; }
    public Viewport ViewPort {get; set;}

    public event Action<int, IScreen> BtnClicked;

    public bool DrawLower { get; }
    string? DesiredMusic => null;
    float MusicVolume => 1f;
    void Unload();
}