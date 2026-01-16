using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using System;

// The Scrollbox should be used in the ui if we need more space. Or for space we don't know
// how much it will render.
// How to use:

// Example in StatisticsScreen:
// _statistics = new ScrollBox(
//     gd
//     bg,
//     _font,
//     new Rectangle(400, 100, 500, 300),
//     MakeAchievementList,
//     GetStatisticsHeight
// );

// MakeAchievementList and GetStatisticsHeight have to swtiched. because they are the main content
// That means we need a function to produce the content that should be rendered and a function on how much space in height will be used
// If necessary create new constructors!
namespace BikeWars.Content.components;
public class ScrollBox
{
    private float _scrollOffset = 0f;
    private RasterizerState _scissorRaster {get; set;}
    public RasterizerState ScissorRaster {get => _scissorRaster; set => _scissorRaster = value;}

    private const float SCROLL_SPEED_KEYBOARD = 5f;
    private const float SCROLL_SPEED_MOUSE = 12f;

    private Rectangle _scrollArea {get; set;}
    public Rectangle ScrollArea {get => _scrollArea; set => _scrollArea = value;}

    private readonly Action<SpriteBatch, Vector2> _drawContent; // This one is necessary so we can tell the parent what we want to draw
    private readonly Func<float> _getContentHeight; // Necessary to get the maximum of scrolling

    private readonly Texture2D _basicTexture;

    private Color _backgroundColor = new Color(255, 255, 255, 128);
    private Color _borderColor = Color.White;
    private int _borderThickness = 3;

    private int _padding = 5; // For the content. We can adapt it if we need like multiple directions.

    // protected Texture2D _backgroundTexture;
    protected SpriteFont _font;
    // private Texture2D _borderTexture;
    // private Texture2D _bgScroll;

    private int _fadeHeight = 20; // Fading effect
    private Color _fadeColor = Color.Black * 0.3f;

    public bool UpdateLower => throw new System.NotImplementedException();

    public bool DrawLower => throw new System.NotImplementedException();

    public ScrollBox(Texture2D basicTexture, SpriteFont font, Rectangle area, Action<SpriteBatch, Vector2> drawContent, Func<float> getContentHeight)
    {
        _basicTexture = basicTexture;
        // _backgroundTexture = basicTexture;
        _font = font;
        ScrollArea = area;

        ScissorRaster = new RasterizerState();
        ScissorRaster.MultiSampleAntiAlias = false;
        ScissorRaster.ScissorTestEnable = true;
        _drawContent = drawContent;
        _getContentHeight = getContentHeight;

        // _borderTexture = basicTexture; // I think we can still change that later if we need a different background.
        // _bgScroll = basicTexture; // Here the same
    }

    public void Update()
    {
        if (InputHandler.IsHeld(GameAction.UI_DOWN))
        {
            _scrollOffset += SCROLL_SPEED_KEYBOARD;
        }
        if (InputHandler.IsHeld(GameAction.UI_UP))
        {
            _scrollOffset -= SCROLL_SPEED_KEYBOARD;
        }
        _scrollOffset -= InputHandler.Mouse.ScrollDelta / SCROLL_SPEED_MOUSE;
        float maxScroll = Math.Max(0, _getContentHeight() - ScrollArea.Height);
        _scrollOffset = MathHelper.Clamp(_scrollOffset, 0, maxScroll);
    }

    private void DrawScrollFade(SpriteBatch sb)
    {
        float contentHeight = _getContentHeight();
        float maxScroll = Math.Max(0, contentHeight - ScrollArea.Height);

        if (_scrollOffset > 0)
        {
            // Fade upwards
            sb.Draw(_basicTexture,
                    new Rectangle(ScrollArea.X, ScrollArea.Y, ScrollArea.Width, _fadeHeight),
                    _fadeColor);
        }

        if (_scrollOffset < maxScroll)
        {
            // Fade downwards
            sb.Draw(_basicTexture,
                    new Rectangle(ScrollArea.X, ScrollArea.Bottom - _fadeHeight, ScrollArea.Width, _fadeHeight),
                    _fadeColor);
        }
    }
    public virtual void Draw(SpriteBatch sb)
    {
        var gd = sb.GraphicsDevice;
        var oldScissors = gd.ScissorRectangle;
        gd.ScissorRectangle = _scrollArea;
        sb.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            ScissorRaster
        );

        // Background
        sb.Draw(
            _basicTexture,
            ScrollArea,
            _backgroundColor
        );

        // content
        _drawContent?.Invoke(
            sb,
            new Vector2(_scrollArea.X + _padding, _scrollArea.Y - _scrollOffset + _padding)
        );
        sb.End();

        gd.ScissorRectangle = oldScissors;
        // Border outside the Scissors
        sb.Begin();
        DrawBorder(sb, ScrollArea, _borderColor, _borderThickness);
        DrawScrollFade(sb);
        sb.End();
    }

    private void DrawBorder(SpriteBatch sb, Rectangle rect, Color color, int thickness)
    {
        // Top
        sb.Draw(_basicTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        sb.Draw(_basicTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        // Left
        sb.Draw(_basicTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        sb.Draw(_basicTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }
}