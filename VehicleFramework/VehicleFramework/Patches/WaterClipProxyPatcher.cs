using HarmonyLib;
using UnityEngine;

// PURPOSE: permit waterclipproxies with negative scaling to act in an intuitive way
// VALUE: Moderate. Not really sure this is my problem to fix.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(WaterClipProxy))]
    class WaterClipProxyPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(WaterClipProxy.UpdateMaterial))]
        static void VehicleDockedPostfix(WaterClipProxy __instance)
        {
            ModVehicle vehicle = __instance.GetComponentInParent<ModVehicle>();
            if (vehicle != null)
            {
                Vector3 oldScale = __instance.transform.lossyScale;
                Vector3 newScale = new Vector3(
                    Mathf.Abs(oldScale.x),
                    Mathf.Abs(oldScale.y),
                    Mathf.Abs(oldScale.z)
                    );
                __instance.clipMaterial.SetVector(ShaderPropertyID._ObjectScale, newScale);
            }
        }
    }
}
