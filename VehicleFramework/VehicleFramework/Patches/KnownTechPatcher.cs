using HarmonyLib;

// PURPOSE: Unlock all ModVehicle encyclopedia entries on Creative mode start or console command "unlock"
// VALUE: High. Can't really figure out a better way to do this in general.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(KnownTech))]
    public static class KnownTechPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(KnownTech.UnlockAll))]
        public static void KnownTechUnlockAllHarmonyPostfix()
        {
            VehicleManager.vehicleTypes.ForEach(x => PDAEncyclopedia.Add(x.name, false));
        }
    }
}
