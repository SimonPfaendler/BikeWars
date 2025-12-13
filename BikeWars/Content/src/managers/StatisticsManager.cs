using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using System.Collections.Generic;

namespace BikeWars.Content.managers;
public class StatisticsManager
{
    private List<Statistic> _statistics {get; set;}
    public List<Statistic> Statistics {get => _statistics; set => _statistics = value;}

    private Statistic _statistic {get; set;}
    public Statistic Statistic {get => _statistic; set => _statistic = value;} // Use for crtStatistic in Game

    public StatisticsManager()
    {
        _statistics = new List<Statistic>();
        Statistic = new Statistic();
    }

    public StatisticsManager(Statistic s, List<Statistic> st)
    {
        _statistics = st;
        Statistic = s;
    }

    public void HandleCharacterDied(CharacterBase c)
    {
        if (c is not Player)
        {
            Statistic.AddKill();
        }
    }
    public void SaveStatistic()
    {
        Statistics.Add(Statistic);
    }
}