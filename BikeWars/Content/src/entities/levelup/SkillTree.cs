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

        AutomaticFire
        // other option for example spacial skills can be added here
    }

    // Dictionary: Welcher Skill hat welche Beschreibung?
    public static readonly Dictionary<SkillId, string> All = new()
    {
        { SkillId.MoreHp, ("Mehr Leben: +30 HP") },

        { SkillId.MoreDamage, ("Mehr Schaden: +2 Schaden") },

        { SkillId.LongerSprintDuration, ("Laengere Sprintdauer: +0,5s Sprint dauer") },

        { SkillId.AutomaticFire, ("Dauerfeuer: Halte den Angriffsknopf gedrueckt!") },
    };
}