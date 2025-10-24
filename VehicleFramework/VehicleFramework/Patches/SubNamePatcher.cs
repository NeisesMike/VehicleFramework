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
        }
        /*
        private static void SetSubNameDecals(ModVehicle mv)
        {
            if (mv.SubNameDecals == null)
            {
                return;
            }
            foreach (var tmprougui in mv.SubNameDecals)
            {
                tmprougui.font = Nautilus.Utility.FontUtils.Aller_Rg;
                tmprougui.text = mv.subName.GetName();
                tmprougui.color = color;
            }
        }
        */
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
            }
            else if (index == 1)
            {
                mv.PaintNameColor(mv.HullName, hsb, color);
            }
            else if (index == 2)
            {
                mv.PaintInteriorColor(hsb, color);
            }
            else if (index == 3)
            {
                mv.PaintStripeColor(hsb, color);
            }
            else
            {
                Logger.Warn("SubName.SetColor Error: Tried to set the color of an index that was not known!");
            }

        }
    }
}
