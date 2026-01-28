using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;
using BikeWars.Utilities;

namespace BikeWars.Content.entities.items;
public class Chest: ObjectBase
{
    private bool _open;
    public bool Open => _open;
    public string? Item { get; private set; }
    // private BoxCollider _collisionCollider {get;set;}
    private Texture2D _texClosed;
    private Texture2D _texOpen;
    // public BoxCollider CollisionCollider {get => _collisionCollider; set => _collisionCollider = value; }
    private int PADDING_INTERACTION_AREA = 40;

    public Chest(Vector2 start, Point size, string? item, bool open = false)
    {
        Item = item;
        _open = open;
        Transform = new Transform(start, size);
        Collider = new BoxCollider(new Vector2(Transform.Position.X - PADDING_INTERACTION_AREA / 2, Transform.Position.Y - PADDING_INTERACTION_AREA / 2), Transform.Size.X + PADDING_INTERACTION_AREA, Transform.Size.Y + PADDING_INTERACTION_AREA, CollisionLayer.INTERACT, this);
        // CollisionCollider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.WALL, this);
        _texClosed = managers.SpriteManager.GetTexture("Chest");
        _texOpen = managers.SpriteManager.GetTexture("Chest_open");
        if (_open)
        {CurrentTex = _texOpen;}
        else
        {
            CurrentTex = _texClosed;
        }
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

    public ItemBase SpawnRandomItem(Vector2 dropPos)
    {
        double pGel = 0.25;
        double pDoping = 0.25;
        double pDog = 0.1;
        double pBeer = 0.1;
        double pFrelo = 0.1;
        double pRacing = 0.2;
        double pIce = 0.00;
            
            
            
            double val = RandomUtil.NextDouble();
            
            double spawnGel = pGel;
            double spawnDoping = spawnGel + pDoping;
            double spawnBeer = spawnDoping + pBeer;
            double spawnFrelo = spawnBeer + pFrelo;
            double spawnRacing = spawnFrelo + pRacing;
            


            if (val < spawnGel)
            {
                return new EnergyGel(dropPos, new Point(32, 32));
            }
            else if (val < spawnDoping)
            {
                return new DopingSpritze(dropPos, new Point(32, 32));
            }
            else if (val < spawnBeer)
            {
               return new Beer(dropPos, new Point(32, 32));
            }
            else if (val < spawnFrelo)
            {
                return new Frelo(dropPos, new Point(32, 32));

            }
            else if (val < spawnRacing)
            {
               return new RacingBike(dropPos, new Point(32, 32));
            }
            else
            {
               return new RacingBike(dropPos, new Point(32, 32));
            }
    }
    public ItemBase OpenChest()
    {
        if (_open) return null;
        _open = true;

        CurrentTex = managers.SpriteManager.GetTexture("Chest_open");
        // Item wird knapp ueber der Truhe gespawnt
        Vector2 dropPos = Transform.Position + new Vector2(0, - Transform.Size.Y);
        if (Item == null)
        {
            return SpawnRandomItem(dropPos);
        }
        else
        {
            return Item switch
            {
                "Energygel" => new EnergyGel(dropPos, new Point(32, 32)),
                "Frelo" => new Frelo(dropPos, new Point(32, 32)),
                "Racingbike" => new RacingBike(dropPos, new Point(32, 32)),
                "DogFood" => new DogFood(dropPos, new Point(32, 32)),
                "DopingSpritze" => new DopingSpritze(dropPos, new Point(32, 32)),
                "Beer" => new Beer(dropPos, new Point(32, 32)),
            };
        }
    }
}
