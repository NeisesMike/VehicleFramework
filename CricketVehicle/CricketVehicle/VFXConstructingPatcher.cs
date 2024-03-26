using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace CricketVehicle
{
    [HarmonyPatch(typeof(VFXConstructing))]
    public static class VFXConstructingPatch
    {
         // This patch sends a message to the CricketContainer upon construction-start.
        [HarmonyPostfix]
        [HarmonyPatch("StartConstruction")]
        public static void StartConstructionPostfix(VFXConstructing __instance)
        {
            if (__instance.GetComponent<CricketContainer>() != null)
            {
                __instance.BroadcastMessage("CricketContainerConstructionBeginning", null, (UnityEngine.SendMessageOptions)1);
                __instance.SendMessageUpwards("CricketContainerConstructionBeginning", null, (UnityEngine.SendMessageOptions)1);
            }
        }
    }
}
