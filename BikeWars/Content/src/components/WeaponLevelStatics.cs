using BikeWars.Entities;

namespace BikeWars.Content.components;
// Use these classes to set the weaponAttributes and even the special abilites based on the level
public class GunStatics: WeaponAttributes
{
    public const int MAX_LEVEL = 5;
    public GunStatics(int level, object owner): base()
    {
        Level = level;
        _max_level = MAX_LEVEL;
        Owner = owner;
        Upgrade();
    }

    public void Upgrade()
    {
        switch(Level)
        {
            case 1:
                Damage = 5;
                Speed = 200f;
            break;
            case 2:
                Damage = 10;
                Speed = 270f;
            break;
            case 3:
                Damage = 20;
                Speed = 350f;
            break;
            case 4:
                Damage = 30;
                Speed = 450f;
            break;
            case 5:
                Damage = 45;
                Speed = 600f;
            break;
        }
    }
}

public class BananaStatics : WeaponAttributes
{
    public const int MAX_LEVEL = 5;
    public BananaStatics(int level, object owner) : base()
    {
        Level = level;
        _max_level = MAX_LEVEL;
        Owner = owner;
        Upgrade();
    }

    public void Upgrade()
    {
        switch (Level)
        {
            case 1:
                Damage = 40;
                Speed = 150f;
                break;
            case 2:
                Damage = 55;
                Speed = 160f;
                break;
            case 3:
                Damage = 70;
                Speed = 170f;
                break;
            case 4:
                Damage = 90;
                Speed = 180f;
                break;
            case 5:
                Damage = 120;
                Speed = 200f;
                break;
        }
    }
}

public class BottleStatics : WeaponAttributes
{
    public const int MAX_LEVEL = 5;
    public BottleStatics(int level, object owner) : base()
    {
        Level = level;
        _max_level = MAX_LEVEL;
        Owner = owner;
        Upgrade();
    }

    public void Upgrade()
    {
        switch (Level)
        {
            case 1:
                Damage = 40;
                Speed = 150f;
                break;
            case 2:
                Damage = 55;
                Speed = 160f;
                break;
            case 3:
                Damage = 70;
                Speed = 170f;
                break;
            case 4:
                Damage = 90;
                Speed = 180f;
                break;
            case 5:
                Damage = 120;
                Speed = 200f;
                break;
        }
    }
}