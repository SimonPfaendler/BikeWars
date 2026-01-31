using System;
using BikeWars.Entities;

// Use these classes to set the weaponAttributes and even the special abilites based on the level
public class GunStatics: WeaponAttributes
{
    // private WeaponAttributes _wp;
    public static int MAX_LEVEL = 5;
    public GunStatics(int level, object owner): base()
    {
        Level = level;
        Owner = owner;
        Upgrade();
    }

    private void Upgrade()
    {
        switch(Level)
        {
            case 1:
                Damage = 30;
                Speed = 200f;
            break;
            case 2:
                Damage = 40;
                Speed = 200f;
            break;
            case 3:
                Damage = 40;
                Speed = 250f;
            break;
            case 4:
                Damage = 50;
                Speed = 255f;
            break;
            case(5):
                Damage = 70;
                Speed = 255f;
            break;
        }
    }
}