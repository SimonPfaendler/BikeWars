// ============================================================
// The BikeAttributes should be used for the basic Stats to play with. Like Health or Damage Similiar like CharacterAttributes
//
//
// ============================================================
using System;
using BikeWars.Content.entities.interfaces;

namespace BikeWars.Entities.Characters;
public class BikeAttributes
{
    public event Action<Bike> OnDestroyed;
    private int _health { get; set; }
    public int Health {
        get => _health;
        set
        {
            if (value <= 0) {
                _health = 0;
                OnDestroyed?.Invoke((Bike)owner);
                return;
            }
            if (value > MaxHealth)
            {
                _health = MaxHealth;
            }
            _health = value;
        }
    }

    private int _armor { get; set; }
    public int Armor {
        get => _armor;
        set
        {
            if (value <= 0) {
                _armor = 0;
                return;
            }
            _armor = value;
        }
    }

    private int _maxHealth { get; set; }
    public int MaxHealth {
        get => _maxHealth;
        set
        {
            if (value < 0)
            {
                _maxHealth = 0;
            }
            _maxHealth = value;
        }
    }
    public object owner {get; set;}

    private int _priority {get; set;}
    public int Priority { // this should be used on how valueable it is for a bike thief for example
        get => _priority;
        set {
            if (value < 0)
            {
                _priority = 0;
            }
            _priority = value;
        }
    }

    private float _speed {get; set;}
    public float Speed {
        get => _speed;
        set {
            if (value < 0)
            {
                _speed = 0;
            }
            _speed = value;
        }
    }

    private float _rotationAcceleration {get; set;}
    public float RotationAcceleration {
        get => _rotationAcceleration;
        set {
            if (value < 0)
            {
                _rotationAcceleration = 0;
            }
            _rotationAcceleration = value;
        }
    }
    private float _speedAcceleration {get; set;}
    public float SpeedAcceleration {
        get => _speedAcceleration;
        set {
            if (value < 0)
            {
                _speedAcceleration = 0;
            }
            _speedAcceleration = value;
        }
    }

    private float _sprintModificator {get; set;}
    public float SprintModificator {
        get => _sprintModificator;
        set {
            if (value < 0)
            {
                _sprintModificator = 0;
            }
            _sprintModificator = value;
        }
    }

    public BikeAttributes(object o, int maxHealth, int health, int armor, int speed, float sprintModificator, int priority, float speedAcceleration, float rotationAcceleration)
    {
        owner = o;
        MaxHealth = maxHealth;
        if (health <= 0)
        {
            Health = MaxHealth;
        } else
        {
            Health = health;
        }
        Armor = armor;
        Speed = speed;
        SprintModificator = sprintModificator;
        SpeedAcceleration = speedAcceleration;
        RotationAcceleration = rotationAcceleration;
        Priority = priority;
    }
}