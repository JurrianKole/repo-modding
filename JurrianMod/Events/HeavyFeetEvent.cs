using System.Collections;
using BepInEx.Logging;
using UnityEngine;

namespace JurrianMod.Events
{
    public class HeavyFeetEvent : IChaosEvent
    {
        private readonly ManualLogSource _logger;

        public HeavyFeetEvent(ManualLogSource logger)
        {
            _logger = logger;
        }

        public string Name => "Happy feet? No! Heavy feet! Try jumping now!";

        public float DurationInSeconds => 10f;
    
        public IEnumerator Run()
        {
            var originalGravity = Physics.gravity;

            try
            {
                Physics.gravity = originalGravity * 10;
            
                yield return new WaitForSeconds(DurationInSeconds);
            }
            finally
            {
                Physics.gravity = originalGravity;
            }
        }
    }
}