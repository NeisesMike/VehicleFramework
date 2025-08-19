using System.Collections;
using HarmonyLib;
using UnityEngine;
using VehicleFramework.VehicleTypes;

// PURPOSE: configure timeToConstruct. Broadcasts the SubConstructionBeginning signal. Manages the building fx colors.
// VALUE: Very high. Important ModVehicle and developer utilities.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(VFXConstructing))]
    public static class VFXConstructingPatch
    {
        public static IEnumerator ManageColor(VFXConstructing vfx, ModVehicle mv)
        {
            if (vfx != null)
            {
                yield return new WaitUntil(() => vfx.ghostMaterial != null);
                if (mv.ConstructionGhostColor != Color.black)
                {
                    Material customGhostMat = new(Shader.Find(Admin.Utils.marmosetUberName));
                    customGhostMat.CopyPropertiesFromMaterial(vfx.ghostMaterial);
                    vfx.ghostMaterial = customGhostMat;
                    vfx.ghostMaterial.color = mv.ConstructionGhostColor;
                    vfx.ghostOverlay.material.color = mv.ConstructionGhostColor;
                }
                if (mv.ConstructionWireframeColor != Color.black)
                {
                    vfx.wireColor = mv.ConstructionWireframeColor;
                }
            }
        }
        /*
         * This patches ensures it takes several seconds for the build-bots to build our vehicle.
         */
        [HarmonyPostfix]
        [HarmonyPatch(nameof(VFXConstructing.StartConstruction))]
        public static void StartConstructionPostfix(VFXConstructing __instance)
        {
            ModVehicle mv = __instance.GetComponent<ModVehicle>();
            if (mv != null)
            {
                __instance.timeToConstruct = mv.TimeToConstruct;
                __instance.BroadcastMessage("SubConstructionBeginning", null, (UnityEngine.SendMessageOptions)1);
                __instance.SendMessageUpwards("SubConstructionBeginning", null, (UnityEngine.SendMessageOptions)1);
                Admin.SessionManager.StartCoroutine(ManageColor(__instance, mv));
            }
        }
    }
}
