using BikeWars.Content.screens;

namespace BikeWars.Entities;
public class WeaponAttributes
{
    public object Owner { get; set; }
    private int _level { get; set; }
    private int _max_level { get; set;}

    public int Level
    {
        get => _level;
        set
        {
            if (value <= 0)
            {
                _level = 0;
                return;
            }
            _level = value;
        }
    }

    private int _damage {get; set;}
    public int Damage
    {
        get => _damage;
        set
        {
            if (value <= 0)
            {
                _damage = 0;
                return;
            }
            _damage = value;
        }
    }

    private float _speed {get; set;}
    public float Speed
    {
        get => _speed;
        set
        {
            if (value <= 0)
            {
                _speed = 0;
                return;
            }
            _speed = value;
        }
    }

    private float _arcScale {get; set;}
    public float ArcScale
    {
        get => _arcScale;
        set
        {
            if (value <= 0)
            {
                _arcScale = 0;
                return;
            }
            _arcScale = value;
        }
    }

    private float _lingerDuration {get; set;}
    public float LingerDuration
    {
        get => _lingerDuration;
        set
        {
            if (value <= 0)
            {
                _lingerDuration = 0;
                return;
            }
            _lingerDuration = value;
        }
    }

    public WeaponAttributes()
    {
        Owner = null;
        Level = 0;
        Damage = 0;
        Speed = 0f;
        ArcScale = 0f;
        LingerDuration = 0f;
        _max_level = 0;
    }
    public WeaponAttributes(object owner, int level, int max_level, int damage, float speed, float arcScale, float lingerDuration)
    {
        Owner = owner;
        Level = level;
        Damage = damage;
        Speed = speed;
        ArcScale = arcScale;
        LingerDuration = lingerDuration;
        _max_level = max_level;
    }

    public void LevelUp()
    {
        _level += 1;
        if (_level > _max_level)
        {
            _level = _max_level;
        }
    }
}