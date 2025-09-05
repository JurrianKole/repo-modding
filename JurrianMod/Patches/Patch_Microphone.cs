using HarmonyLib;
using JurrianMod.Audio;
using UnityEngine;

namespace JurrianMod.Patches
{
    [HarmonyPatch(typeof(Microphone))]
    public static class Patch_Microphone
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Microphone.Start), typeof(string), typeof(bool), typeof(int), typeof(int))]
        public static bool Start_Prefix(
            ref AudioClip __result,
            string deviceName,
            bool loop,
            int lengthSec,
            int frequency)
        {
            if (!VirtualMic.Active || !VirtualMic.IsReady)
            {
                return true;
            }

            __result = VirtualMic.AudioClip;
            return false;
        }
    }
}