using HarmonyLib;

// PURPOSE: Allow custom handling of the player's body during ModVehicle piloting
// VALUE: Moderate-low. Convenient for developers. Maybe better to do it in a per-vehicle way

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(ArmsController))]
    public class ArmsControllerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ArmsController.Update))]
        public static void ArmsControllerUpdatePostfix()
        {
            ModVehicle mv = Player.main.GetModVehicle();
            if (mv != null)
            {
                mv.HandlePilotingAnimations();
            }
        }
    }
}
