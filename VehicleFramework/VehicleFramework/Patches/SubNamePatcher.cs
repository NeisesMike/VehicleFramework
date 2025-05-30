using HarmonyLib;
using UnityEngine;

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
            if(mv == null)
            {
                return;
            }
            if (mv.SubNameDecals != null)
            {
                SetSubNameDecals(mv);
            }
            if (mv is VehicleTypes.Submarine sub)
            {
                sub.PaintVehicleName(mv.subName.GetName(), mv.nameColor, mv.baseColor);
            }
        }
        private static void SetSubNameDecals(ModVehicle mv)
        {
            foreach (var tmprougui in mv.SubNameDecals)
            {
                tmprougui.font = Nautilus.Utility.FontUtils.Aller_Rg;
                tmprougui.text = mv.subName.GetName();
            }
        }

        private static void SetSubNameDecalsWithColor(ModVehicle mv, Vector3 hsb, Color color)
        {
            if (mv == null)
            {
                return;
            }
            mv.nameColor = color;
            SetSubNameDecals(mv);
            foreach (var tmprougui in mv.SubNameDecals)
            {
                tmprougui.color = color;
            }
            if(mv is VehicleTypes.Submarine sub)
            {
                sub.PaintVehicleName(mv.subName.GetName(), mv.nameColor, mv.baseColor);
            }
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
                mv.SetBaseColor(hsb, color);
            }
            else if (index == 1)
            {
                if (mv.SubNameDecals != null)
                {
                    SetSubNameDecalsWithColor(mv, hsb, color);
                }
            }
            else if (index == 2)
            {
                mv.SetInteriorColor(hsb, color);
            }
            else if (index == 3)
            {
                mv.SetStripeColor(hsb, color);
            }
            else
            {
                Logger.Warn("SubName.SetColor Error: Tried to set the color of an index that was not known!");
            }

        }
    }
}
