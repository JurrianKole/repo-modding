using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using JurrianMod.Events;

namespace JurrianMod
{
    public class ChaosController
    {
        private readonly ManualLogSource _logger;
        private readonly Random _random;

        private readonly List<IChaosEvent> _events = new List<IChaosEvent>();

        public ChaosController(
            ManualLogSource logger,
            Random random)
        {
            _logger = logger;
            _random = random;
        }

        public void RegisterChaosEvents()
        {
            _events.Add(new HeavyFeetEvent(_logger));
        }

        public void RegisterAudioEvents(
            string filePath,
            float duration,
            float gain,
            bool loop,
            bool alsoLocalPlayback)
        {
            _events.Add(new KaraokeEvent(_logger, filePath, duration, gain, loop, alsoLocalPlayback));
        }
    
        public IEnumerator RunRandomEvent()
        {
            if (_events.Count == 0)
            {
                yield break;
            }
        
            var randomEvent = _events[_random.Next(_events.Count)];
            _logger.LogInfo($"[Shitshow] Starting event '{randomEvent.Name}' for {randomEvent.DurationInSeconds} seconds.");
        
            yield return randomEvent.Run();
        
            _logger.LogInfo($"[Shitshow] Ended event '{randomEvent.Name}'");
        }
    }
}