using System;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// PURPOSE: Allow ModVehicles to be built basically anywhere (don't "need deeper water" unnecessarily)
// VALUE: High.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(ConstructorInput))]
    public class ConstructorInputPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ConstructorInput.Craft))]
        public static bool ConstructorInputCraftHarmonyPrefix(ConstructorInput __instance, TechType techType, float duration)
        {
            if (!Admin.VehicleManager.vehicleTypes.Select(x => x.techType).Contains(techType))
            {
                return true;
            }
            Vector3 zero = Vector3.zero;
            Quaternion identity = Quaternion.identity;
            __instance.GetCraftTransform(techType, ref zero, ref identity);
            if (techType != TechType.Seamoth && techType != TechType.Exosuit && !ReturnValidCraftingPositionSpecial(__instance, zero))
            {
                __instance.invalidNotification.Play();
                return false;
            }
            if (!CrafterLogic.ConsumeResources(techType))
            {
                return false;
            }
            duration = 10f;
            __instance.Craft(techType, duration);
            return false;
        }

        public static bool ReturnValidCraftingPositionSpecial(ConstructorInput instance, Vector3 pollPosition)
        {
            float num = Mathf.Clamp01((pollPosition.x + 2048f) / 4096f);
            float num2 = Mathf.Clamp01((pollPosition.z + 2048f) / 4096f);
            int x = (int)(num * (float)instance.validCraftPositionMap.width);
            int y = (int)(num2 * (float)instance.validCraftPositionMap.height);
            return instance.validCraftPositionMap.GetPixel(x, y).g > 0.5f;
        }
    }
}
