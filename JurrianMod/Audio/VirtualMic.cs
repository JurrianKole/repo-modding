using System;
using UnityEngine;

namespace JurrianMod.Audio
{
    public static class VirtualMic
    {
        private const int TargetHz = 48000;
        
        private static float[] _samples;
        private static int _readPosition;
        
        public static bool Active { get; private set; }
        public static AudioClip AudioClip { get; private set; }
        public static float Gain { get; set; } = 1.0f;
        public static bool Loop { get; set; } = false;
        
        public static bool IsReady => AudioClip != null && _samples != null && _samples.Length > 0;
        
        public static void LoadFromClip(AudioClip source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            
            var frames = source.samples;
            var channels = source.channels;
            var raw = new float[frames * channels];

            source.GetData(raw, offsetSamples: 0);

            var mono = new float[frames];

            for (var i = 0; i < frames; i++)
            {
                var sum = 0f;

                for (var c = 0; c < channels; c++)
                {
                    sum += raw[i++];
                }
                
                mono[i] = sum / channels;
            }

            if (source.frequency != TargetHz)
            {
                var factor = (double)source.frequency / TargetHz;
                var outLen = (int)Math.Ceiling(mono.Length / factor);
                var res = new float[outLen];
                for (var n = 0; n < outLen; n++)
                {
                    var srcIndex = n * factor;
                    var i0 = (int)srcIndex;
                    var i1 = Math.Min(i0 + 1, mono.Length - 1);
                    var t = (float)(srcIndex - i0);
                    res[n] = Mathf.Lerp(mono[i0], mono[i1], t);
                }
                _samples = res;
            }
            else
            {
                _samples = mono;
            }

            _readPosition = 0;

            AudioClip = AudioClip.Create(
                "JurrianMod_VirtualMic",
                _samples.Length,
                1,
                TargetHz,
                true,
                OnPcmRead,
                OnPcmSetPos);
        }
        
        public static void SetActive(bool active)
        {
            Active = active;
            if (active) _readPosition = 0;
        }
        
        private static void OnPcmRead(float[] data)
        {
            if (_samples == null || _samples.Length == 0)
            {
                Array.Clear(data, 0, data.Length);
                return;
            }

            for (var i = 0; i < data.Length; i++)
            {
                float sample = _samples[_readPosition] * Gain;
                data[i] = Mathf.Clamp(sample, -1f, 1f);
                _readPosition++;

                if (_readPosition >= _samples.Length)
                {
                    if (Loop) _readPosition = 0;
                    else _readPosition = _samples.Length - 1;
                }
            }
        }

        private static void OnPcmSetPos(int pos)
        {
            _readPosition = Mathf.Clamp(pos, 0, (_samples?.Length ?? 1) - 1);
        }
    }
}