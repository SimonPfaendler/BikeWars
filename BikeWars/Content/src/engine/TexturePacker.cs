using System.Collections.Generic;

namespace BikeWars.Content.engine
{
    public class TexturePacker
    {
        public string Filename { get; set; }
        public TexturePackerRect Frame { get; set; }
        public bool Rotated { get; set; }
        public bool Trimmed { get; set; }
    }
    
    public class TexturePackerRect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }
    
    
    public class TexturePackerRoot
    {
        public List<TexturePacker> frames { get; set; }
        public object meta { get; set; }
    }
}
