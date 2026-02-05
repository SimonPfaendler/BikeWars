using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;
using BikeWars.Utilities;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.entities.items;
public class Chest: ObjectBase
{
    private bool _open;
    public bool Open => _open;
    public ChestItemType? Item { get; private set; }
    private Texture2D _texClosed;
    private Texture2D _texOpen;
    private int PADDING_INTERACTION_AREA = 40;

    public enum ChestItemType
    {
        Energygel,
        Frelo,
        Racingbike,
        DogFood,
        DopingSpritze,
        Beer,
        Flame,
        Ice
    }

    public Chest(Vector2 start, Point size, ChestItemType? item, bool open = false)
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
        double roll = RandomUtil.NextDouble();

        if (roll < 0.25) // 25% Weapons
        {
            double weaponRoll = Utilities.RandomUtil.NextDouble();
            Player.WeaponType weaponType;
            if (weaponRoll < 0.25) weaponType = Player.WeaponType.Flamethrower;
            else if (weaponRoll < 0.50) weaponType = Player.WeaponType.IceTrail;
            else if (weaponRoll < 0.75) weaponType = Player.WeaponType.FireTrail;
            else weaponType = Player.WeaponType.DamageCircle;

            return new WeaponItem(dropPos, new Point(32, 32), weaponType);
        }
        else if (roll < 0.35) // 10% EnergyGel
        {
            return new EnergyGel(dropPos, new Point(32, 32));
        }
        else if (roll < 0.45) // 10% Beer
        {
            return new Beer(dropPos, new Point(32, 32));
        }
        else if (roll < 0.55) // 10% DopingSpritze
        {
            return new DopingSpritze(dropPos, new Point(32, 32));
        }
        else if (roll < 0.65) // 10% DogFood
        {
            return new DogFood(dropPos, new Point(32, 32));
        }
        else if (roll < 0.95) // 30% Bikes
        {
            return new Frelo(dropPos, new Point(32, 32));
        }
        else if (roll < 0.97)
        {return new RacingBike(dropPos, new Point(32, 32));}
        else
        {
            return new Xp_Money(dropPos, new Point(32, 32), 7);
        }
    }
    public ItemBase OpenChest()
    {
        if (_open) return null;
        _open = true;

        CurrentTex = managers.SpriteManager.GetTexture("Chest_open");
        // Item wird knapp ueber der Truhe gespawnt
        Vector2 dropPos = Transform.Position + new Vector2(0, - Transform.Size.Y);

        // random loot logic
        if (Item == null)
        {
            return SpawnRandomItem(dropPos);
        }
        return Item switch
        {
            ChestItemType.Energygel => new EnergyGel(dropPos, new Point(32, 32)),
            ChestItemType.Frelo => new Frelo(dropPos, new Point(32, 32)),
            ChestItemType.Racingbike => new RacingBike(dropPos, new Point(32, 32)),
            ChestItemType.DogFood => new DogFood(dropPos, new Point(32, 32)),
            ChestItemType.DopingSpritze => new DopingSpritze(dropPos, new Point(32, 32)),
            ChestItemType.Beer => new Beer(dropPos, new Point(32, 32)),
            ChestItemType.Flame => new WeaponItem(
                dropPos,
                new Point(32, 32),
                Player.WeaponType.Flamethrower
            ),
            ChestItemType.Ice => new WeaponItem(
                dropPos,
                new Point(32, 32),
                Player.WeaponType.IceTrail
            ),

            _ => SpawnRandomItem(dropPos)
        };
    }
}
