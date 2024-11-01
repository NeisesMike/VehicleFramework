using HarmonyLib;
using UnityEngine;
using UWE;
using System.Collections.Generic;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(FreezeTime))]
    public class FreezeTimePatcher
    {
        private static List<AudioSource> audioSources = new List<AudioSource>();
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
            if(FreezeTime.HasFreezers())
            {
                audioSources.ForEach(x => x?.Pause());
            }
            else
            {
                audioSources.ForEach(x => x?.UnPause());
            }
        }
    }
}
