using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Random = System.Random;

namespace JurrianMod
{
    [BepInPlugin("nl.jurrian.repo.chaos", "Jurrian's chaotic plugin", "1.3.3.7")]
    [BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID)]
    public class ChaosPlugin : BaseUnityPlugin
    {
        private Harmony _harmony = null!;
        private Coroutine? _loop;
        private Random _random = null!;
    
        // Config stuff
        private ConfigEntry<bool>? _isShitShowEnabled;
        private ConfigEntry<float>? _minimumWaitTimeForNextEventInSeconds;
        private ConfigEntry<float>? _maximumWaitTimeForNextEventInSeconds;
        private ConfigEntry<string>? _mp3Path;
        private ConfigEntry<float>? _micGain;
        private ConfigEntry<bool>? _micLoop;
        private ConfigEntry<bool>? _alsoLocal;
        private ConfigEntry<float>? _eventDuration;
    
        private void Awake()
        {
            Logger.LogInfo("Initializing Jurrian's mod");

            _isShitShowEnabled = Config.Bind("General", "EnableChaos", true, "Master switch for chaos :D");
            _minimumWaitTimeForNextEventInSeconds = Config.Bind("General", "MinimumWaitTimeForNextEventInSeconds", 20f, "Minimum wait time for next event in seconds");
            _maximumWaitTimeForNextEventInSeconds = Config.Bind("General", "MaximumWaitTimeForNextEventInSeconds", 60f, "Maximum wait time for next event in seconds");
            
            _mp3Path = Config.Bind("Audio", "Mp3Path", Paths.PluginPath + "\\JurrianMod\\Audio\\audio.mp3", "Absoluut pad naar MP3.");
            _micGain = Config.Bind("Audio", "MicGain", 1.0f, "Versterking voor virtuele mic (0.1..3.0).");
            _micLoop = Config.Bind("Audio", "Loop", false, "Herhaal MP3 tijdens event.");
            _alsoLocal = Config.Bind("Audio", "AlsoPlayLocally", true, "Speel ook lokaal via AudioSource.");
            _eventDuration = Config.Bind("Audio", "PlayAsMicDuration", 8f, "Duur van het MP3-als-mic event (s).");

            _random = new Random();
        
            _harmony = new Harmony("nl.jurrian.repo.chaos");
            _harmony.PatchAll();

            if (_isShitShowEnabled.Value)
            {
                _loop = StartCoroutine(JurrianModLoop());
            }

            Logger.LogInfo("Jurrian's mod initialized!");
        }

        private void OnDestroy()
        {
            if (_loop != null)
            {
                StopCoroutine(_loop);
            }
        
            _harmony?.UnpatchSelf();
        }

        private IEnumerator JurrianModLoop()
        {
            var controller = new ChaosController(Logger, _random);
            //controller.RegisterChaosEvents();
            controller.RegisterAudioEvents(
                _mp3Path.Value,
                duration: Mathf.Max(1f, _eventDuration.Value),
                gain: Mathf.Clamp(_micGain.Value, 0.1f, 3f),
                loop: _micLoop.Value,
                alsoLocalPlayback: _alsoLocal.Value);
            
            while (true)
            {
                var waitTime = Mathf.Lerp(
                    _minimumWaitTimeForNextEventInSeconds?.Value ?? 20f,
                    _maximumWaitTimeForNextEventInSeconds?.Value ?? 60f,
                    (float)_random.NextDouble());
                
                yield return new WaitForSeconds(waitTime);

                yield return controller.RunRandomEvent();
            }
        }
    }
}