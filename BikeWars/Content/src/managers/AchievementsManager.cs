using BikeWars.Content.engine;
using System;
using System.Collections.Generic;

namespace BikeWars.Content.managers;


// Use these IDS to check for which Achievement shuold be used
// Use these in the TILED map for the name
public enum AchievementIds
{
    NONE,
    UNIVERSITY_NEVER_FORGETS,
    BACHELOR_HERE_I_COME,
    OUCH,
    NERD,
    UZZ_UZZ,
    DIE_BY_TRAM,
    DIABETES,
}

// Use these triggers like the variables in the manager
public enum Triggers
{
    REACHED_AREA,
    WON_GAME,
    PLAYER_DIED,
    GOT_ALL_ACHIEVEMENTS,
    KILLED_RAVER,
    DIED_BY_TRAM,
    ATE_SNACKS
}
public static class AchievementIdConverter {

    // Use this function to get the tiledmap ID and convert it to the correct number
    // public static AchievementIds StringToIdConverter(string id)
    // That means we need OUCH for the same Id
    public static AchievementIds Convert(string id)
    {
        if (Enum.TryParse<AchievementIds>(id, out var achievementId))
            return achievementId;
        throw new ArgumentException($"Invalid AchievementId: {id}");
    }
}

public class AchievementsManager
{
    // For BACHELOR_HERE_I_COME
    private bool reached_area = false;

    // For BACHELOR and UNIVERSITY
    private bool won_game = false;

    // For OUCH
    private bool player_died = false;

    // For NERD
    private bool got_all_achievements = false;

    // For RAVER
    private bool killed_raver = false;

    // For DIED_BY_TRAM
    private bool died_by_tram = false;

    public int CrtSnackCount = 0;

    // For DIABETES
    private bool ate_snacks = false;
    private Dictionary<AchievementIds, Achievement> _achievements;
    public Dictionary<AchievementIds, Achievement> Achievements
    {
        get => _achievements;
        set => _achievements = value;
    }

    private Achievement _achievement {get; set;}
    public Achievement Achievement {get => _achievement; set => _achievement = value;} // Use for crtAchievement in Game

    public AchievementsManager()
    {
        Achievements = createAchievements();
        Achievement = createAchievement();
    }

    public AchievementsManager(Achievement a, Dictionary<AchievementIds, Achievement> at)
    {
        _achievements = at;
        Achievement = a;
    }

    private Dictionary<AchievementIds, Achievement> createAchievements()
    {
        Dictionary<AchievementIds, Achievement> achievements = new Dictionary<AchievementIds, Achievement>
        {
            { AchievementIds.UNIVERSITY_NEVER_FORGETS, UniversityNeverForgets() },
            { AchievementIds.BACHELOR_HERE_I_COME, BachelorHereICome() },
            { AchievementIds.OUCH, Ouch() },
            { AchievementIds.NERD, Nerd() },
            { AchievementIds.UZZ_UZZ, UzzUzz() },
            { AchievementIds.DIE_BY_TRAM, DieByTram() },
            { AchievementIds.DIABETES, Diabetes() },
        };
        return achievements;
    }
    private Achievement createAchievement()
    {
        return new Achievement();
    }

    private Achievement UniversityNeverForgets()
    {
        return new Achievement(AchievementIds.UNIVERSITY_NEVER_FORGETS, "Die Uni vergisst dich nie!", "Du hast das Spiel gewonnen, da du die maximale Zeit ueberlebt hast!", false);
    }

    private Achievement BachelorHereICome()
    {
        return new Achievement(AchievementIds.BACHELOR_HERE_I_COME, "Bachelor, ich komme!", "Gewinne das Spiel und habe dabei die Uni erreicht.", false);
    }

    private Achievement Ouch()
    {
        return new Achievement(AchievementIds.OUCH, "Autsch! Das tat weh!", "Stirb im Spiel.", false);
    }

    private Achievement Nerd()
    {
        return new Achievement(AchievementIds.NERD, "Streeeeeber!", "Erhalte alle Achievements die es gibt.", false);
    }
    private Achievement UzzUzz()
    {
        return new Achievement(AchievementIds.UZZ_UZZ, "Uzz! Uzz! Uzz!", "Besiege die Rave-Horde.", false);
    }
    private Achievement DieByTram()
    {
        return new Achievement(AchievementIds.DIE_BY_TRAM, "Die letzte verpasst? Die naechste kommt!", "Stirb wegen der Strassenbahn.", false);
    }
    private Achievement Diabetes()
    {
        return new Achievement(AchievementIds.DIABETES, "Diabetes", "Esse 20 Riegel in einem Spiel.", false);
    }
    public void SuccededAchievement(AchievementIds id)
    {
        _achievements[id].Succeeded = true;
    }

    // public void

    public void HandleAchievement(AchievementIds id, Triggers trigger)
    {
        switch(trigger)
        {
            case Triggers.REACHED_AREA:
            reached_area = true;
            break;
            case Triggers.WON_GAME:
            won_game = true;
            break;
            case Triggers.PLAYER_DIED:
            player_died = true;
            break;
            case Triggers.KILLED_RAVER:
            killed_raver = true;
            break;
            case Triggers.DIED_BY_TRAM:
            died_by_tram = true;
            break;
            case Triggers.ATE_SNACKS:
            ate_snacks = true;
            break;
        }

        if (reached_area && won_game && player_died && killed_raver && died_by_tram && ate_snacks)
        {
            got_all_achievements = true;
        }

        switch(id)
        {
            case AchievementIds.UNIVERSITY_NEVER_FORGETS:
            if (won_game)
            {
                SuccededAchievement(id);
            }
            break;
            case AchievementIds.BACHELOR_HERE_I_COME:
            if (reached_area && won_game)
            {
                SuccededAchievement(id);
            }
            break;
            case AchievementIds.OUCH:
            if (player_died)
            {
                SuccededAchievement(id);
            }
            break;
            case AchievementIds.NERD:
            if (got_all_achievements)
            {
                SuccededAchievement(id);
            }
            break;
            case AchievementIds.UZZ_UZZ:
            if (killed_raver)
            {
                SuccededAchievement(id);
            }
            break;
            case AchievementIds.DIE_BY_TRAM:
            if (died_by_tram)
            {
                SuccededAchievement(id);
            }
            break;
            case AchievementIds.DIABETES:
            if (ate_snacks)
            {
                SuccededAchievement(id);
            }
            break;
        }
    }
    public void SaveAchievement()
    {
        if (Achievements == null)
            Achievements = new Dictionary<AchievementIds, Achievement>();

        if (Achievement == null)
            Achievement = new Achievement();

        Achievements[Achievement.Id] = Achievement;
    }
}