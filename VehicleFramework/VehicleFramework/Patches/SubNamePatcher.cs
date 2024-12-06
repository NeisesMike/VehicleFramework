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
            if (mv != null && mv.SubNameDecals != null)
            {
                SetSubNameDecals(mv);
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
    }
}
