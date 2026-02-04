using System.Collections.Generic;

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
        // other option for example spacial skills can be added here
        WeaponGun,
        WeaponBanana,
        WeaponBottle,
        WeaponCircle,
        WeaponIce,
        WeaponFire
    }

    // Dictionary: Welcher Skill hat welche Beschreibung?
    public static IReadOnlyDictionary<SkillId, string> All { get; } = new Dictionary<SkillId, string>
    {
        { SkillId.MoreHp, ("Mehr Leben: +30 HP") },

        { SkillId.MoreDamage, ("Mehr Schaden: +2 Schaden") },

        { SkillId.LongerSprintDuration, ("Laengere Sprintdauer: +0,5s Sprint dauer") },

        { SkillId.AutomaticFire, ("Dauerfeuer: Halte den Angriffsknopf gedrueckt!") },

        { SkillId.WeaponGun, ("Handfeuerwaffe: Bum Bum KAPOWWW") },

        { SkillId.WeaponBanana, ("Bananenschale: Achtung rutschig!") },

        { SkillId.WeaponBottle, ("Pfandflasche: RECYCLINNGGG!") },
        { SkillId.WeaponCircle, ("Fahrradklingl: Flaechenschaden!! Ohne Fahrrad keine Fahrradklingel")},
        { SkillId.WeaponIce, ("Eisreifen: Hinterlaesst eisige Spur!v")}
    };
}