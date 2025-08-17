using HarmonyLib;
using UnityEngine;
using UWE;
using System.Collections.Generic;

// PURPOSE: Allow easy registration of AudioSources and pause them during game pause.
// VALUE: High.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(FreezeTime))]
    public class FreezeTimePatcher
    {
        private readonly static List<AudioSource> audioSources = new();
        public static AudioSource Register(AudioSource source)
        {
            audioSources.RemoveAll(item => item == null);
            audioSources.Add(source);
            return source;
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(FreezeTime.Set))]
        public static void FreezeTimeSetPostfix()
        {
            audioSources?.RemoveAll(item => item == null);
            if (FreezeTime.HasFreezers())
            {
                audioSources?.ForEach(x => x?.Pause());
            }
            else
            {
                audioSources?.ForEach(x => x?.UnPause());
            }
        }
    }
}
