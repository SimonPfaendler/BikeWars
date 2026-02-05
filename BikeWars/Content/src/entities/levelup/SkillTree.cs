using System.Collections.Generic;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.entities.levelup;
// this class lists all Skills and their description
public class SkillTree
{
    public enum SkillId
    {
        MoreHp,
        MoreDamage,
        LongerSprintDuration,
        AutomaticFire,
        WeaponGun,
        WeaponBanana,
        WeaponBottle,
        CritChance,
    }
    
    public static string GetString(SkillId skill, Player player)
    {
        switch (skill)
        {
            case SkillId.MoreHp:
            {
                int hpGain;
                if (player.PlayerNumber == 1)
                {
                    hpGain = 20 * player.HpLevel;
                }
                else
                {
                    hpGain = 30 * (int)(1.5 * player.HpLevel);
                }

                return $"Mehr Leben: +{hpGain} HP";
            }

            case SkillId.MoreDamage:
            {
                int dmgGain = player.PlayerNumber == 1
                    ? 5 * (int)(1.5 * player.DamageLevel)
                    : 4 * player.DamageLevel;

                return $"Mehr Schaden: +{dmgGain} Schaden";
            }

            case SkillId.LongerSprintDuration:
                return player.PlayerNumber == 1
                    ? "Laengere Sprintdauer: +0,5s"
                    : "Laengere Sprintdauer: +0,7s";

            case SkillId.AutomaticFire:
                return "Dauerfeuer: Halte den Angriffsknopf gedrueckt!";

            case SkillId.CritChance:
                return "Kritischer Treffer: +5% Chance";

            case SkillId.WeaponGun:

                int level = player.GetWeaponLevel(Player.WeaponType.Gun) + 1;
                return $"Handfeuerwaffe: Bum Bum KAPOWWW Lvl: {level}";

            case SkillId.WeaponBanana:
                int level1 = player.GetWeaponLevel(Player.WeaponType.BananaThrow) + 1;
                return $"Bananenschale: Achtung rutschig! Lvl: {level1}";

            case SkillId.WeaponBottle:
                int level2 = player.GetWeaponLevel(Player.WeaponType.BottleThrow) + 1;
                return $"Pfandflasche: RECYCLINNGGG! Lvl: {level2}";
        }

        return skill.ToString();
    }
}