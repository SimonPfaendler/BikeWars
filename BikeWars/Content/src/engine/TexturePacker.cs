using System.Collections.Generic;

namespace BikeWars.Content.engine
{
    /// <summary>
    /// Root container for TexturePacker JSON output.
    /// </summary>
    public class TexturePackerRoot
    {
        public List<TexturePackerFrame> Frames { get; set; }
    }

    /// <summary>
    /// Represents a single frame entry from TexturePacker JSON.
    /// </summary>
    public class TexturePackerFrame
    {
        public string Filename { get; set; }
        public TexturePackerRect Frame { get; set; }
    }

    /// <summary>
    /// Rectangle coordinates from TexturePacker (x, y, w, h).
    /// </summary>
    public class TexturePackerRect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }

    /// <summary>
    /// Size dimensions from TexturePacker (w, h).
    /// </summary>
    public class TexturePackerSize
    {
    }
}
