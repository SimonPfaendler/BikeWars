using BikeWars.Entities;

namespace BikeWars.Content.engine.interfaces;
public interface IWeapon
{
    void LevelUp();
    WeaponAttributes WeaponAttributes();
}
