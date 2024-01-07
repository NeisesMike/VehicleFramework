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
        /*
         * This patches ensures it takes several seconds for the build-bots to build our vehicle.
         */
        [HarmonyPostfix]
        [HarmonyPatch("StartConstruction")]
        public static void StartConstructionPostfix(VFXConstructing __instance)
        {
            if (__instance.GetComponent<CricketContainer>() != null)
            {
                __instance.timeToConstruct = 3f;
                __instance.BroadcastMessage("CricketContainerConstructionBeginning", null, (UnityEngine.SendMessageOptions)1);
                __instance.SendMessageUpwards("CricketContainerConstructionBeginning", null, (UnityEngine.SendMessageOptions)1);
            }
        }
    }
}
