using System;
using System.Runtime.CompilerServices;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.engine;
public class Achievement
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

    private int _regularKills { get; set; }
    public int RegularKills {
        get => _regularKills;
        set
        {
            if (value <= 0) {
                _regularKills = 0;
                return;
            }
            _regularKills = value;
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

    private int _shotsFired { get; set; } // How often a shot was fired especially for projectiles like bullets
    public int ShotsFired {
        get => _shotsFired;
        set
        {
            if (value <= 0) {
                _shotsFired = 0;
                return;
            }
            _shotsFired = value;
        }
    }
    private int _opponentsHit { get; set; } // We can use this for the ratio on how often we actually hit the opponent
    public int OpponentsHit {
        get => _opponentsHit;
        set
        {
            if (value <= 0) {
                _opponentsHit = 0;
                return;
            }
            _opponentsHit = value;
        }
    }

    private int _repairs { get; set; } // The amount of repairs at the bikeshop
    public int Repairs {
        get => _repairs;
        set
        {
            if (value <= 0) {
                _repairs = 0;
                return;
            }
            _repairs = value;
        }
    }

    private float _phaseFindBike { get; set; } // ADD MORE PHASES. Like this now with the bike and other phases too
    public float PhaseFindBike {
        get => _phaseFindBike;
        set
        {
            if (value <= 0) {
                _phaseFindBike = 0;
                return;
            }
            _phaseFindBike = value;
        }
    }

    public string TimeToMinuteDisplay()
    {
        TimeSpan time = TimeSpan.FromSeconds(_time);
        return $"{time.Minutes:00}:{time.Seconds:00}";
    }

    public string TimeToFindBikeMinuteDisplay()
    {
        TimeSpan time = TimeSpan.FromSeconds(_phaseFindBike);
        return $"{time.Minutes:00}:{time.Seconds:00}";
    }

    public void AddKill(bool IsRegularKill)
    {
        Kills += 1;
        if (IsRegularKill)
        {
            RegularKills += 1;
        }
    }

    public void AddDeathCount()
    {
        DeathCount += 1;
    }

    public void AddOpponentHit()
    {
        OpponentsHit += 1;
    }

    public void AddRepair()
    {
        Repairs += 1;
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

    public void CurrentPhaseFindBike(float time)
    {
        PhaseFindBike = time;
    }

    public void AddShotFired()
    {
        ShotsFired += 1;
    }

    public float Accuracy()
    {
        if (ShotsFired <= 0)
            return 0f;

        float accuracy = (float)OpponentsHit / ShotsFired * 100f;
        return (float)Math.Round(accuracy, 2);
    }

    // Phase to find Bike
    public void TimeToFindBike(float time)
    {
        PhaseFindBike = time;
    }

    public Achievement()
    {
        Kills = 0;
        RegularKills = 0;
        DealtDamage = 0;
        TookDamage = 0;
        XP = 0;
        Level = 0;
        Time = 0f;
        DeathCount = 0;
        ShotsFired = 0;
        OpponentsHit = 0;
        Repairs = 0;
        PhaseFindBike = 0f;
    }

    public Achievement(int kills, int regularKills, int dealtDamage, int tookDamage, int xp, int level, float time, int deathCount, int shotsFired, int opponentsHit, int repairs, float phaseFindBike)
    {
        Kills = kills;
        RegularKills = regularKills;
        DealtDamage = dealtDamage;
        TookDamage = tookDamage;
        XP = xp;
        Level = level;
        Time = time;
        DeathCount = deathCount;
        ShotsFired = shotsFired;
        OpponentsHit = opponentsHit;
        Repairs = repairs;
        PhaseFindBike = phaseFindBike;
    }
}