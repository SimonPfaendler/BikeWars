using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class CooldownWithDuration
{
    private float _duration;
    private float _cooldown;
    private float _durationTimer;
    private float _cooldownTimer;
    public float Duration {get; set;}
    public float Cooldown {get; set;}

    public bool IsActive => _durationTimer > 0;
    public bool IsOnCooldown => _cooldownTimer > 0;
    public bool Ready => _durationTimer <= 0 && _cooldownTimer <= 0;

    public CooldownWithDuration(float durationSeconds, float cooldownSeconds)
    {
        _duration = durationSeconds;
        _cooldown = cooldownSeconds;
        _durationTimer = 0;
        _cooldownTimer = 0;
    }

    public void Activate()
    {
        if (!Ready)
        {
            return;
        }
        _durationTimer = _duration;
    }

    public float RemainingDuration => _durationTimer;

    public float RemainingCooldown => _cooldownTimer;

    public void DecreaseCoolDownTimer(float amount)
    {
        if (_cooldownTimer - amount < 0f) {
            _cooldownTimer = 0f;
            return;
        }
        _cooldownTimer -= amount;
    }

    public void Update(GameTime gameTime)
    {
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // DURING USE
        if (_durationTimer > 0)
        {
            _durationTimer -= delta;

            if (_durationTimer <= 0)
            {
                _durationTimer = 0;
                _cooldownTimer = _cooldown;
            }
        }

        // COOLDOWN PHASE
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= delta;
            if (_cooldownTimer < 0)
                _cooldownTimer = 0;
        }
    }
}


