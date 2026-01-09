using System.Collections.Generic;

namespace BikeWars.Content.engine
{
    /// <summary>
    /// Root container for TexturePacker JSON output.
    /// </summary>
    public class TexturePackerRoot
    {
        public List<TexturePackerFrame> Frames { get; set; }
        public TexturePackerMeta Meta { get; set; }
    }

    /// <summary>
    /// Represents a single frame entry from TexturePacker JSON.
    /// </summary>
    public class TexturePackerFrame
    {
        public string Filename { get; set; }
        public TexturePackerRect Frame { get; set; }
        public bool Rotated { get; set; }
        public bool Trimmed { get; set; }
        public TexturePackerRect SpriteSourceSize { get; set; }
        public TexturePackerSize SourceSize { get; set; }
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
        public int W { get; set; }
        public int H { get; set; }
    }

    /// <summary>
    /// Metadata from TexturePacker JSON.
    /// </summary>
    public class TexturePackerMeta
    {
        public string App { get; set; }
        public string Version { get; set; }
        public string Image { get; set; }
        public string Format { get; set; }
        public TexturePackerSize Size { get; set; }
        public string Scale { get; set; }
    }
}
