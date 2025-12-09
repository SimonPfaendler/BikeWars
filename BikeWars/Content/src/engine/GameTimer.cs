using System;
using Microsoft.Xna.Framework;

// This class handles the timer seen on the gamescreen

namespace BikeWars.Content.engine
{
    public class GameTimer
    {
        private float _currentTime;
        private float _totalTime;
        private bool _isRunning;
        private bool _isPaused;

        public float CurrentTime => _currentTime;
        public float TotalTime => _totalTime;
        public bool IsRunning => _isRunning;
        public bool IsPaused => _isPaused;
        public bool IsFinished => _currentTime <= 0 && _isRunning;

        public event Action OnTimerFinished;

        public GameTimer(float totalTimeInSeconds = 300f) // 5 minutes
        {
            _totalTime = totalTimeInSeconds;
            Reset();
        }

        public void Start()
        {
            _currentTime = _totalTime;
            _isRunning = true;
            _isPaused = false;
        }

        public void Stop()
        {
            _isRunning = false;
            _isPaused = false;
        }

        public void Pause()
        {
            if (_isRunning && !_isPaused)
            {
                _isPaused = true;
            }
        }

        public void Resume()
        {
            if (_isRunning && _isPaused)
            {
                _isPaused = false;
            }
        }

        public void Reset()
        {
            _currentTime = _totalTime;
            _isRunning = false;
            _isPaused = false;
        }

        public void Restart()
        {
            Reset();
            Start();
        }

        public void Update(GameTime gameTime)
        {
            if (!_isRunning || _isPaused)
                return;

            _currentTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_currentTime <= 0)
            {
                _currentTime = 0;
                _isRunning = false;
                OnTimerFinished?.Invoke();
            }
        }

        public string GetFormattedTime()
        {
            int minutes = (int)(_currentTime / 60);
            int seconds = (int)(_currentTime % 60);
            return $"{minutes:00}:{seconds:00}";
        }
        
        public void SetFromSave(float currentTime, bool isRunning, bool isPaused)
        {
            _currentTime = currentTime;
            _isRunning = isRunning;
            _isPaused = isPaused;
        }
    }
}