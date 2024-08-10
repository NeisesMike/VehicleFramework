using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

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
                    oldScale.x < 0 ? -oldScale.x : oldScale.x,
                    oldScale.y < 0 ? -oldScale.y : oldScale.y,
                    oldScale.z < 0 ? -oldScale.z : oldScale.z
                    );
                __instance.clipMaterial.SetVector(ShaderPropertyID._ObjectScale, newScale);
            }
        }
    }
}
