using System.Linq;
using HarmonyLib;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(KnownTech))]
    public static class KnownTechPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(KnownTech.Add))]
        public static void UnlockPostfix(TechType techType)
        {
            // unlock the ency page when the vehicle is unlocked (if not already)
            VehicleManager.vehicleTypes.Where(x => x.techType == techType).Select(x => x.name).ForEach(x => PDAEncyclopedia.Add(x, false));
        }
    }
}
