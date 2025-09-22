using HarmonyLib;
using UnityEngine;
using VehicleFramework.Extensions;

// PURPOSE: ensure the Silence doesn't kill us in a way that softlocks the game
// VALUE: High, unfortunately

namespace VehicleFramework.Patches.CompatibilityPatches
{
    [HarmonyPatch(typeof(Player))]
    public class SilencePlayerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.CanBeAttacked))]
        public static void PlayerCanBeAttackedHarmonyPostfix(ref bool __result)
        {
            ModVehicle? mv = Player.main.GetModVehicle();
            if (mv != null)
            {
                if (GetComponentByName(mv.gameObject))
                {
                    __result = true;
                    mv.PlayerExit();
                }
            }
        }
        private static bool GetComponentByName(GameObject obj)
        {
            // Get all components on the GameObject
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp.GetType().Name == "VehicleLock")
                {
                    return true;
                }
            }
            return false; // Return false if no match is found
        }
    }
}
