using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.items;
public class Chest: ItemBase
{
    private bool _open;
    public bool Open => _open;
    public string Item { get; private set; }
    

    public Chest(Vector2 start, Point size, string item, bool open = false)
    {
        Item = item;
        _open = open;
        Transform = new Transform(start, size);
        Collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.ITEM, this);
        if (_open == false)
        {TexRight = managers.SpriteManager.GetTexture("Chest");}
        else
        {TexRight = managers.SpriteManager.GetTexture("Chest_open");}
        CurrentTex = TexRight;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public override void Update(GameTime gameTime)
    {
    }
    public override bool Intersects(ICollider collider)
    {
        return Collider.Intersects(collider);
    }
    public ItemBase OpenChest()
    {
        if (_open) return null;
        _open = true;

        CurrentTex = managers.SpriteManager.GetTexture("Chest_open");
        // Item wird knapp ueber der Truhe gespawnt
        Vector2 dropPos = Transform.Position + new Vector2(0, - Transform.Size.Y);

        return Item switch
        {
            "Energygel"  => new EnergyGel(dropPos, new Point(32, 32)),
            "Frelo"      => new Frelo(dropPos, new Point(32, 32)),
            "Racingbike" => new RacingBike(dropPos, new Point(32, 32)),
            "DogFood" => new DogFood(dropPos, new Point(32, 32)),
        };
    }
}
