using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    public static class TeleportUtils
    {
        public static bool GetDroneInUse()
        {
            if (VehicleTypes.Drone.mountedDrone != null)
            {
                Logger.Output("Can't teleport with a Drone");
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(GotoConsoleCommand))]
    public class GotoConsoleCommandPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GotoConsoleCommand.OnConsoleCommand_goto))]
        public static bool GotoConsoleCommandOnConsoleCommand_gotoPrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GotoConsoleCommand.OnConsoleCommand_gotofast))]
        public static bool GotoConsoleCommandOnConsoleCommand_gotofastPrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GotoConsoleCommand.OnConsoleCommand_gotospam))]
        public static bool GotoConsoleCommandOnConsoleCommand_gotospamPrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GotoConsoleCommand.OnConsoleCommand_gotostop))]
        public static bool GotoConsoleCommandOnConsoleCommand_gotostopPrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
    }
    [HarmonyPatch(typeof(BiomeConsoleCommand))]
    public class BiomeConsoleCommandPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BiomeConsoleCommand.OnConsoleCommand_biome))]
        public static bool BiomeConsoleCommandOnConsoleCommand_biomePrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
    }
    [HarmonyPatch(typeof(PAXTerrainController))]
    public class PAXTerrainControllerPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PAXTerrainController.OnConsoleCommand_batch))]
        public static bool PAXTerrainControllerOnConsoleCommand_batchPrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
    }
    [HarmonyPatch(typeof(Player))]
    public class PlayerConsoleCommandPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.OnConsoleCommand_warp))]
        public static bool PlayerOnConsoleCommand_warpPrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.OnConsoleCommand_warpme))]
        public static bool PlayerOnConsoleCommand_warpmePrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.OnConsoleCommand_warpforward))]
        public static bool PlayerOnConsoleCommand_warpforwardPrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.OnConsoleCommand_spawnnearby))]
        public static bool PlayerOnConsoleCommand_spawnnearbyPrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.OnConsoleCommand_kill))]
        public static bool PlayerOnConsoleCommand_killPrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
    }
    [HarmonyPatch(typeof(EscapePod))]
    public class EscapePodConsoleCommandPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(EscapePod.OnConsoleCommand_randomstart))]
        public static bool EscapePodOnConsoleCommand_randomstartPrefix()
        {
            return TeleportUtils.GetDroneInUse();
        }
    }
}
