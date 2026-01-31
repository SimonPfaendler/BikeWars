using BikeWars.Content.components;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.src.utils.SaveLoadExample;
using BikeWars.Entities.Characters;
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

    // For RAVER
    private bool killed_raver = false;

    // For DIED_BY_TRAM
    private bool died_by_tram = false;

    public int CrtSnackCount = 0;

    private const int SNACK_COUNT = 20;

    public event Action SaveFile;

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
        Achievement = new Achievement();
    }

    public AchievementsManager(Achievement a, Dictionary<AchievementIds, Achievement> at)
    {
        _achievements = at;
        Achievement = a;
    }

    public Dictionary<AchievementIds, Achievement> createAchievements()
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
        var state = SaveLoad.LoadGame();
        foreach (Achievement a in state.Achievements)
        {
            if (a.Id == AchievementIds.NONE) continue;
            a.Picture = achievements[a.Id].Picture;
            achievements[a.Id] = a; // just overwrite the current values with what is saved in the file
        }
        return achievements;
    }
    private Achievement UniversityNeverForgets()
    {
        return new Achievement(AchievementIds.UNIVERSITY_NEVER_FORGETS, "Die Uni vergisst dich nie!", "Du hast das Spiel gewonnen, da du die maximale Zeit ueberlebt hast!", false, SpriteManager.GetTexture("UNIVERSITY_NEVER_FORGETS"));
    }

    private Achievement BachelorHereICome()
    {
        return new Achievement(AchievementIds.BACHELOR_HERE_I_COME, "Bachelor, ich komme!", "Gewinne das Spiel und habe dabei die Uni erreicht.", false, SpriteManager.GetTexture("BACHELOR_HERE_I_COME"));
    }

    private Achievement Ouch()
    {
        return new Achievement(AchievementIds.OUCH, "Autsch! Das tat weh!", "Stirb im Spiel.", false, SpriteManager.GetTexture("OUCH"));
    }

    private Achievement Nerd()
    {
        return new Achievement(AchievementIds.NERD, "Streeeeeber!", "Erhalte alle Achievements die es gibt.", false, SpriteManager.GetTexture("NERD"));
    }
    private Achievement UzzUzz()
    {
        return new Achievement(AchievementIds.UZZ_UZZ, "Uzz! Uzz! Uzz!", "Besiege die Rave-Horde.", false, SpriteManager.GetTexture("UZZ_UZZ"));
    }
    private Achievement DieByTram()
    {
        return new Achievement(AchievementIds.DIE_BY_TRAM, "Die letzte verpasst? Die naechste kommt!", "Stirb wegen der Strassenbahn.", false, SpriteManager.GetTexture("DIE_BY_TRAM"));
    }
    private Achievement Diabetes()
    {
        return new Achievement(AchievementIds.DIABETES, "Diabetes", "Esse 20 Riegel in einem Spiel.", false,SpriteManager.GetTexture("EnergyBar"));
    }
    public void SuccededAchievement(AchievementIds id)
    {
        _achievements[id].Succeeded = true;
    }

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
            CrtSnackCount += 1;
            if (CrtSnackCount >= SNACK_COUNT)
            {
                ate_snacks = true;
            }
            break;
        }

        switch(id)
        {
            case AchievementIds.UNIVERSITY_NEVER_FORGETS:
            if (_achievements[AchievementIds.UNIVERSITY_NEVER_FORGETS].Succeeded) return;
            if (won_game)
            {
                SuccededAchievement(id);
                SaveAchievement(false);
            }
            break;
            case AchievementIds.BACHELOR_HERE_I_COME:
            if (_achievements[AchievementIds.BACHELOR_HERE_I_COME].Succeeded) return;
            if (reached_area && won_game)
            {
                SuccededAchievement(id);
                SaveAchievement(false);
            }
            break;
            case AchievementIds.OUCH:
            if (_achievements[AchievementIds.OUCH].Succeeded) return;
            if (player_died)
            {
                SuccededAchievement(id);
                SaveAchievement(true);
            }
            break;
            case AchievementIds.UZZ_UZZ:
            if (_achievements[AchievementIds.UZZ_UZZ].Succeeded) return;
            if (killed_raver)
            {
                SuccededAchievement(id);
                SaveAchievement(true);
            }
            break;
            case AchievementIds.DIE_BY_TRAM:
            if (_achievements[AchievementIds.DIE_BY_TRAM].Succeeded) return;
            if (died_by_tram)
            {
                SuccededAchievement(id);
                SaveAchievement(true);
            }
            break;
            case AchievementIds.DIABETES:
            if (_achievements[AchievementIds.DIABETES].Succeeded) return;
            if (ate_snacks && CrtSnackCount >= SNACK_COUNT)
            {
                SuccededAchievement(id);
                SaveAchievement(true);
            }
            break;
        }

        if (!gotAllAchievements()) return;
        if (_achievements[AchievementIds.NERD].Succeeded) return;
        SuccededAchievement(AchievementIds.NERD);
        SaveAchievement(true);
    }

    private bool gotAllAchievements()
    {
        foreach (Achievement a in Achievements.Values)
        {
            if (!a.Succeeded && a.Id != 0 && a.Id != AchievementIds.NERD)
            {
                return false;
            }
        }
        return true;
    }
    public void OnEnergyBarPickedUp()
    {
        HandleAchievement(AchievementIds.DIABETES, Triggers.ATE_SNACKS);
    }
    public void OnDiedByTram(CharacterBase c, int amount, object hitBy)
    {
        if (c is not Player p) return;
        if (hitBy is not Tram) return;
        if (p.IsDead)
        {
            HandleAchievement(AchievementIds.DIE_BY_TRAM, Triggers.DIED_BY_TRAM);
        }
    }
    public void OnRaverGroupDied()
    {
        HandleAchievement(AchievementIds.UZZ_UZZ, Triggers.KILLED_RAVER);
    }
    public void OnPlayerDied(CharacterBase c)
    {
        if (c is not Player) return;
        HandleAchievement(AchievementIds.OUCH, Triggers.PLAYER_DIED);
    }

    // needsSavingInFile is just necessary if we don't want to save double the entries
    public void SaveAchievement(bool needsSavingInFile)
    {
        if (Achievements == null)
            Achievements = new Dictionary<AchievementIds, Achievement>();

        if (Achievement == null)
            Achievement = new Achievement();

        Achievements[Achievement.Id] = Achievement;
        if (!needsSavingInFile) return;
        SaveFile?.Invoke();
    }
}