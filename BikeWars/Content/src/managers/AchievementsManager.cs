using BikeWars.Content.engine;
using System.Collections.Generic;

namespace BikeWars.Content.managers;
public class AchievementsManager
{
    private List<Achievement> _achievements {get; set;}
    public List<Achievement> Achievements {get => _achievements; set => _achievements = value;}

    private Achievement _achievement {get; set;}
    public Achievement Achievement {get => _achievement; set => _achievement = value;} // Use for crtAchievement in Game

    public AchievementsManager()
    {
        Achievements = createAchievements();
        Achievement = createAchievement();
    }

    public AchievementsManager(Achievement a, List<Achievement> at)
    {
        _achievements = at;
        Achievement = a;
    }

    private List<Achievement> createAchievements()
    {
        List<Achievement> achievements = [
            UniversityNeverForgets(),
            BachelorHereICome(),
            Ouch(),
            Nerd(),
            UzzUzz(),
            DieByTram(),
            Diabetes()
        ];
        return achievements;
    }
    private Achievement createAchievement()
    {
        return new Achievement();
    }

    private Achievement UniversityNeverForgets()
    {
        return new Achievement("Die Uni vergisst dich nie!", "Du hast das Spiel gewonnen, da du die maximale Zeit ueberlebt hast!", false);
    }

    private Achievement BachelorHereICome()
    {
        return new Achievement("Bachelor, ich komme!", "Gewinne das Spiel und habe dabei die Uni erreicht.", false);
    }

    private Achievement Ouch()
    {
        return new Achievement("Autsch! Das tat weh!", "Stirb im Spiel.", false);
    }

    private Achievement Nerd()
    {
        return new Achievement("Streeeeeber!", "Erhalte alle Achievements die es gibt.", false);
    }
    private Achievement UzzUzz()
    {
        return new Achievement("Uzz! Uzz! Uzz!", "Besiege die Rave-Horde.", false);
    }
    private Achievement DieByTram()
    {
        return new Achievement("Die letzte verpasst? Die nächste kommt!", "Stirb wegen der Straßenbahn.", false);
    }
    private Achievement Diabetes()
    {
        return new Achievement("Diabetes", "Esse 20 Riegel in einem Spiel.", false);
    }
    public void SaveStatistic()
    {
        if (Achievements == null)
            Achievements = new List<Achievement>();

        if (Achievement == null)
            Achievement = new Achievement();

        Achievements.Add(Achievement);
    }
}