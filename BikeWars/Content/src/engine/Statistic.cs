using System;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.engine;
public class Statistic
{
    private int _kills { get; set; }
    public int Kills {
        get => _kills;
        set
        {
            if (value <= 0) {
                _kills = 0;
                return;
            }
            _kills = value;
        }
    }

    private int _tookDamage { get; set; }
    public int TookDamage {
        get => _tookDamage;
        set
        {
            if (value <= 0) {
                _tookDamage = 0;
                return;
            }
            _tookDamage = value;
        }
    }

    private int _dealtDamage { get; set; }
    public int DealtDamage {
        get => _dealtDamage;
        set
        {
            if (value <= 0) {
                _dealtDamage = 0;
                return;
            }
            _dealtDamage = value;
        }
    }

    private int _xp { get; set; }
    public int XP {
        get => _xp;
        set
        {
            if (value <= 0) {
                _xp = 0;
                return;
            }
            _xp = value;
        }
    }

    private int _level { get; set; }
    public int Level {
        get => _level;
        set
        {
            if (value <= 0) {
                _level = 0;
                return;
            }
            _level = value;
        }
    }

    private float _time { get; set; } // now in seconds
    public float Time {
        get => _time;
        set
        {
            if (value <= 0) {
                _time = 0;
                return;
            }
            _time = value;
        }
    }

    private int _deathCount { get; set; } // Can be useful especially in multiplayer, because we don't have for one player multiple lives or so
    public int DeathCount {
        get => _deathCount;
        set
        {
            if (value <= 0) {
                _deathCount = 0;
                return;
            }
            _deathCount = value;
        }
    }

    public string TimeToMinuteDisplay()
    {
        TimeSpan time = TimeSpan.FromSeconds(_time);
        return $"{time.Minutes:00}:{time.Seconds:00}";
    }

    public void AddKill()
    {
        Kills += 1;
    }

    public void AddDeathCount()
    {
        DeathCount += 1;
    }

    public void AddDamage(CharacterBase c, int amount)
    {
        if (c is Player)
        {
            TookDamage += amount;
            return;
        }
        DealtDamage += amount;
    }

    public void CurrentXP(int xp)
    {
        XP = xp;
    }

    public void CurrentLevel(int xp, int level)
    {
        XP = xp;
        Level = level;
    }

    public void CurrentTime(float time)
    {
        Time = time;
    }

    public Statistic()
    {
        Kills = 0;
        DealtDamage = 0;
        TookDamage = 0;
        XP = 0;
        Level = 0;
        Time = 0f;
        DeathCount = 0;
    }

    public Statistic(int kills, int dealtDamage, int tookDamage, int xp, int level, float time, int deathCount)
    {
        Kills = kills;
        DealtDamage = dealtDamage;
        TookDamage = tookDamage;
        XP = xp;
        Level = level;
        Time = time;
        DeathCount = deathCount;
    }
}