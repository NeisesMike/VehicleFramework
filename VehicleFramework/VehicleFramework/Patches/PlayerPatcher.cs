using System.Linq;
using HarmonyLib;
using UnityEngine;
using VehicleFramework.VehicleTypes;

// PURPOSE: ensure the Player behaves as expected when ModVehicles are involved
// VALUE: Very high.

namespace VehicleFramework
{ 
    [HarmonyPatch(typeof(Player))]
    public static class PlayerPatcher
    {
        /*
         * This collection of patches covers many topics.
         * Generally, it regards behavior in a ModVehicle while underwater and exiting the pilot seat.
         * TODO: there is likely some redundancy here with PlayerControllerPatcher
         */
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.Awake))]
        public static void AwakePostfix(Player __instance)
        {
            new GameObject().AddComponent<Admin.ConsoleCommands>();
            VehicleFramework.Admin.GameStateWatcher.IsPlayerAwaked = true;
            VehicleFramework.Assets.FragmentManager.AddScannerDataEntries();
            HUDBuilder.DecideBuildHUD();

            // Setup build bot paths.
            // We have to do this at game-start time,
            // because the new objects we create are wiped on scene-change.
            MainPatcher.Instance.StartCoroutine(BuildBotManager.SetupBuildBotPathsForAllMVs());
            return;
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.Start))]
        public static void StartPostfix(Player __instance)
        {
            VehicleFramework.Admin.GameStateWatcher.IsPlayerStarted = true;
            return;
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.TryEject))]
        public static bool PlayerTryEjectPrefix(Player __instance)
        {
            // Player.TryEject does not serve ModVehicles.
            // The only reason it gets called at all, for a ModVehicle,
            // is for compatibility with DeathRun remade,
            // which spends energy on Player.TryEject.
            // So we'll gut it and call it at the appropriate time,
            // so that the DeathRun functionality can exist.
            return __instance.GetModVehicle() == null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.GetDepthClass))]
        public static bool GetDepthClass(Player __instance, ref Ocean.DepthClass __result)
        {
            /*
             * TODO
             * I believe this function relates to the PDA voice communicating depth information to you.
             * "Passing 400 meters," that sort of thing.
             * I'm not sure this patch is strictly necessary.
             */
            VehicleTypes.Submarine mv = __instance.GetVehicle() as VehicleTypes.Submarine;
            if (mv != null && !mv.IsPlayerControlling())
            {
                //var crushDamage = __instance.gameObject.GetComponentInParent<CrushDamage>();
                //__result = crushDamage.GetDepthClass();
                //__instance.crushDepth = crushDamage.crushDepth;
                __result = Ocean.DepthClass.Safe;
                __instance.crushDepth = 200f;
                return false;
            }
            return true;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.Update))]
        public static void UpdatePostfix(Player __instance)
        {
            VehicleTypes.Submarine mv = __instance.GetVehicle() as VehicleTypes.Submarine;
            if (mv == null)
            {
                return;
            }
            if (mv.IsPlayerInside() && !mv.IsPlayerControlling())
            {
                // animator stuff to ensure we don't act like we're swimming at any point
                __instance.playerAnimator.SetBool("is_underwater", false);
                __instance.playerAnimator.SetBool("on_surface", false);
                __instance.playerAnimator.SetBool("diving", false);
                __instance.playerAnimator.SetBool("diving_land", false);
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.UpdateIsUnderwater))]
        public static bool UpdateIsUnderwaterPrefix(Player __instance)
        {
            VehicleTypes.Submarine mv = __instance.GetVehicle() as VehicleTypes.Submarine;
            if (mv != null)
            {
                // declare we aren't underwater,
                // since we're wholly within an air bubble
                __instance.isUnderwater.Update(false);
                __instance.isUnderwaterForSwimming.Update(false);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.UpdateMotorMode))]
        public static bool UpdateMotorModePrefix(Player __instance)
        {
            VehicleTypes.Submarine mv = __instance.GetVehicle() as VehicleTypes.Submarine;
            if (mv != null && !mv.IsPlayerControlling())
            {
                // ensure: if we're in a modvehicle and we're not piloting, then we're walking.
                __instance.SetMotorMode(Player.MotorMode.Walk);
                return false;
            }
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.CanBreathe))]
        public static void CanBreathePostfix(Player __instance, ref bool __result)
        {
            if(__instance.currentMountedVehicle != null)
            {
                ModVehicle mv = __instance.currentMountedVehicle as ModVehicle;
                switch(mv)
                {
                    case Submarine _:
                        __result = mv.IsPowered() && mv.IsUnderCommand;
                        return;
                    default:
                        return;
                }
            }
            if (Drone.mountedDrone?.lastSubRoot?.powerRelay != null)
            {
                __result = Drone.mountedDrone.lastSubRoot.powerRelay.IsPowered();
                return;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.IsFreeToInteract))]
        public static void IsFreeToInteractPostfix(Player __instance, ref bool __result)
        {
            var list = Admin.GameObjectManager<Drone>.Where(x => x.IsPlayerControlling());
            if (list.Count() == 0)
            {
                return;
            }
            Drone drone = list.First();
            __result = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.UpdatePosition))]
        public static bool UpdatePositionPrefix(Player __instance)
        {
            // don't do this if a drone is being piloted
            if (Drone.mountedDrone != null)
            {
                return false;
            }

            // don't do this if there is a parent and mounted vehicle mismatch
            // This is a weird thing. How do we handle it?

            var fcc = MainCameraControl.main.GetComponent<FreecamController>();
            if(fcc.mode || fcc.ghostMode)
            {
                return true;
            }

            if (__instance.currentMountedVehicle is ModVehicle && __instance.mode == Player.Mode.LockedPiloting && !Admin.Utils.IsAnAncestorTheCurrentMountedVehicle(Player.main.transform))
            {
                Logger.Error("Mismatch between the Player's mounted vehicle and the Player's parent!");
                // Don't skip. This is a weird problem and it needs resolved, so let it die strangely.
                //return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.ExitLockedMode))]
        public static bool PlayerExitLockedModePrefix(Player __instance)
        {
            // if we're in an MV, do our special way of exiting a vehicle instead
            ModVehicle mv = __instance.GetModVehicle();
            if (mv == null)
            {
                return true;
            }
            mv.DeselectSlots();
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.OnKill))]
        public static void PlayerOnKillPostfix(Player __instance)
        {
            // if we're in an MV, do our special way of exiting a vehicle instead
            ModVehicle mv = __instance.GetModVehicle();
            if (mv == null)
            {
                return;
            }
            mv.StopPiloting();
            mv.PlayerExit();
        }
    }
}
