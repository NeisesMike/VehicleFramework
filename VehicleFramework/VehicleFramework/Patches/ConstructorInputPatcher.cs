using HarmonyLib;
using System.Linq;
using UnityEngine;

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
                // Only do something for ModVehicles
                return true;
            }
            Vector3 zero = Vector3.zero;
            Quaternion identity = Quaternion.identity;
            __instance.GetCraftTransform(techType, ref zero, ref identity);
            if (!CrafterLogic.ConsumeResources(techType))
            {
                return false;
            }
            duration = 10f;

            if ((__instance as Crafter)._logic != null && (__instance as Crafter)._logic.Craft(techType, duration))
            {
                (__instance as Crafter).state = true;
                (__instance as Crafter).OnCraftingBegin(techType, duration);
            }

            return false;
        }
    }
}
