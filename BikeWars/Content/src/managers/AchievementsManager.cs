using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.managers;
public class AchievementsManager
{
    private List<Achievement> _achievements {get; set;}
    public List<Achievement> Achievements {get => _achievements; set => _achievements = value;}

    private Achievement _achievement {get; set;}
    public Achievement Achievement {get => _achievement; set => _achievement = value;} // Use for crtAchievement in Game

    public AchievementsManager()
    {
        Achievements = new List<Achievement>();
        Achievement = new Achievement();
    }

    public AchievementsManager(Achievement a, List<Achievement> at)
    {
        _achievements = at;
        Achievement = a;
    }

    // public void HandleCharacterDied(CharacterBase c)
    // {
    //     if (c is not Player)
    //     {
    //         Statistic.AddKill(c is Hobo || c is BikeThief || c is Dog); // Regular Kills
    //         return;
    //     }
    //     Statistic.AddDeathCount();
    // }

    // public void HandleLevel(int xp, int level)
    // {
    //     Statistic.CurrentLevel(xp, level);
    // }

    // public void HandleExperience(int xp)
    // {
    //     Statistic.CurrentXP(xp);
    // }

    // public void HandleTookDamage(CharacterBase c, int amount)
    // {
    //     Statistic.AddDamage(c, amount);
    //     if (c is Player)
    //     {
    //         return;
    //     }
    //     Statistic.AddOpponentHit();
    // }

    // public void HandleTime(float time)
    // {
    //     Statistic.CurrentTime(time);
    // }

    // public void HandleShotFired()
    // {
    //     Statistic.AddShotFired();
    // }

    // public void HandleThrowing(Vector2 direction)
    // {
    //     Statistic.AddShotFired();
    // }
    // public void HandleRepair()
    // {
    //     Statistic.AddRepair();
    // }
    // public void HandleFoundBike(float time)
    // {
    //     Statistic.CurrentPhaseFindBike(time);
    // }
    public void SaveStatistic()
    {
        if (Achievements == null)
            Achievements = new List<Achievement>();

        if (Achievement == null)
            Achievement = new Achievement();

        Achievements.Add(Achievement);
    }
}