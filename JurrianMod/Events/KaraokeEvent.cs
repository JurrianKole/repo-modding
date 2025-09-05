using System;
using System.Collections;
using BepInEx.Logging;
using JurrianMod.Audio;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace JurrianMod.Events
{
    public class KaraokeEvent : IChaosEvent
    {
        private readonly string _filePath;
        private readonly ManualLogSource _logger;
        private readonly bool _loop;
        private readonly float _gain;
        private readonly bool _alsoLocalPlayback;

        private AudioSource _localSource;

        public KaraokeEvent(
            ManualLogSource logger,
            string filePath,
            float duration,
            float gain,
            bool loop,
            bool alsoLocalPlayback)
        {
            _logger = logger;
            _filePath = filePath;
            DurationInSeconds = duration;
            _gain = gain;
            _loop = loop;
            _alsoLocalPlayback = alsoLocalPlayback;
        }
        
        public string Name => "Did someone say karaoke time?";

        public float DurationInSeconds { get; private set; }
        
        public IEnumerator Run()
        {
            if (!VirtualMic.IsReady)
            {
                yield return LoadMp3(
                    _filePath,
                    VirtualMic.LoadFromClip);
            }

            if (!VirtualMic.IsReady)
            {
                _logger.LogWarning("MP3 could not be loaded for some reason unknown to mankind");
                yield break;
            }
            
            VirtualMic.Gain = _gain;
            VirtualMic.Loop = _loop;
            
            VirtualMic.SetActive(true);

            Microphone.End(null);
            
            if (_alsoLocalPlayback)
            {
                var go = new GameObject("JurrianMod_LocalPlayback");
                Object.DontDestroyOnLoad(go);
                _localSource = go.AddComponent<AudioSource>();
                _localSource.spatialBlend = 0f;
                _localSource.loop = _loop;
                _localSource.playOnAwake = false;
                _localSource.clip = VirtualMic.AudioClip;
                _localSource.volume = Mathf.Clamp01(_gain);
                _localSource.Play();
            }

            var time = 0f;

            while (time < DurationInSeconds)
            {
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            if (_alsoLocalPlayback && _localSource != null)
            {
                _localSource.Stop();
                Object.Destroy(_localSource.gameObject);
            }

            VirtualMic.SetActive(false);
        }

        private static IEnumerator LoadMp3(
            string absolutePath,
            Action<AudioClip> onLoaded)
        {
            var url = "file:///" + absolutePath.Replace('\\', '/');
            using var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                UnityEngine.Debug.LogError("MP3 load b0rked: " + request.error);
                yield break;
            }

            var clip = DownloadHandlerAudioClip.GetContent(request);
            onLoaded.Invoke(clip);
        }
    }
}