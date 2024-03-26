using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(VFXConstructing))]
    public static class VFXConstructingPatch
    {
        public static IEnumerator ManageColor(VFXConstructing vfx, ModVehicle mv)
        {
            for (int i = 0; i < 20; i++)
            {
                if (vfx.ghostMaterial != null && mv.ConstructionGhostColor != Color.black)
                {
                    vfx.ghostMaterial.color = mv.ConstructionGhostColor;
                }
                if (mv.ConstructionWireframeColor != Color.black)
                {
                    vfx.wireColor = mv.ConstructionWireframeColor;
                }
                yield return null;
            }
        }
        /*
         * This patches ensures it takes several seconds for the build-bots to build our vehicle.
         */
        [HarmonyPostfix]
        [HarmonyPatch("StartConstruction")]
        public static void StartConstructionPostfix(VFXConstructing __instance)
        {
            ModVehicle mv = __instance.GetComponent<ModVehicle>();
            if (mv != null)
            {
                __instance.timeToConstruct = mv.TimeToConstruct;
                __instance.BroadcastMessage("SubConstructionBeginning", null, (UnityEngine.SendMessageOptions)1);
                __instance.SendMessageUpwards("SubConstructionBeginning", null, (UnityEngine.SendMessageOptions)1);
                UWE.CoroutineHost.StartCoroutine(ManageColor(__instance, mv));
            }
        }
    }
}
