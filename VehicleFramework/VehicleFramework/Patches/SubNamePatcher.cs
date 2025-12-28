using HarmonyLib;
using UnityEngine;

// PURPOSE: Create ModVehicle API for changing colors via normal routines (eg MoonPool terminal)
// VALUE: High.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(SubName))]
    public class SubNamePatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SubName.SetName))]
        public static void SubNameSetNamePostfix(SubName __instance)
        {
            ModVehicle mv = __instance.GetComponent<ModVehicle>();
            mv?.PaintName(mv.HullName);
            mv?.LogisticalSetName(mv.HullName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SubName.SetColor))]
        public static void SubNameSetColorPostfix(SubName __instance, int index, Vector3 hsb, Color color)
        {
            ModVehicle mv = __instance.GetComponent<ModVehicle>();
            if (mv == null)
            {
                return;
            }
            if (index == 0)
            {
                mv.PaintBaseColor(hsb, color);
                mv.LogisticalSetBaseColor(color);
            }
            else if (index == 1)
            {
                mv.PaintNameColor(mv.HullName, hsb, color);
                mv.LogisticalSetName(mv.HullName, color);
            }
            else if (index == 2)
            {
                mv.PaintInteriorColor(hsb, color);
                mv.LogisticalSetInteriorColor(color);
            }
            else if (index == 3)
            {
                mv.PaintStripeColor(hsb, color);
                mv.LogisticalSetStripeColor(color);
            }
            else
            {
                Logger.Warn("SubName.SetColor Error: Tried to set the color of an index that was not known!");
            }
        }
    }
}
