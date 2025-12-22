using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;

namespace BikeWars.Content.entities.interfaces;
// infos in this record, for more flexibility and because it felt wrong to write it in Colison Manager directly
public record TiledObjectInfo (
    Rectangle Rect,
    TiledMapProperties Properties
);