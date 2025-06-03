using HarmonyLib;

// PURPOSE: Avoid an error at game-exit (LargeWorldStreamer.UnloadGlobalRoot) whereby this method is called with a null InventoryItem
// VALUE: Moderate. Important to make sure things continue to unload correctly (memory leak?)

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(EnergyMixin))]
    public class EnergyMixinPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnergyMixin.NotifyHasBattery))]
        public static bool EnergyMixinNotifyHasBatteryHarmonyPrefix(EnergyMixin __instance, InventoryItem item)
        {
            if (__instance.batteryModels == null) return false;
            if (__instance.batteryModels.Length > 0)
            {
                if (item == null || item.item == null)
                {
                    if(__instance.controlledObjects != null)
                    {
                        __instance.controlledObjects.ForEach(x => x.SetActive(false));
                    }
                    if (__instance.batteryModels != null)
                    {
                        __instance.batteryModels.ForEach(x => x.model.SetActive(false));
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
