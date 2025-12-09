using System;

namespace BikeWars.Content.events
{
    public static class GameEvents
    {
        public static event Action OnResumeTimer;
        
        public static void RaiseResumeTimer() => OnResumeTimer?.Invoke();
    }
}