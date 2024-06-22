using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.IO;
using UnityEngine;
using System.Reflection;

using System.Reflection.Emit;

using Nautilus.Utility;
using UWE;
using VehicleFramework.VehicleTypes;

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
        [HarmonyPatch("Awake")]
        public static void AwakePostfix(Player __instance)
        {
            VehicleFramework.Admin.GameStateWatcher.IsPlayerAwaked = true;
            return;
        }
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(Player __instance)
        {
            /*
            IEnumerator raycastme()
            {
                while (true)
                {
                    yield return new WaitForSeconds(0.5f);
                    RaycastHit[] allHits;
                    allHits = Physics.RaycastAll(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, 100f);
                    allHits
                        .Where(hit=>hit.transform.GetComponent<TerrainChunkPieceCollider>() != null)
                        .ForEach(x => Logger.Error("Did Hit: " + x.ToString() + " : " + x.transform.name + " : " + x.collider.gameObject.name));
                }
            }
            UWE.CoroutineHost.StartCoroutine(raycastme());
            */


            HUDBuilder.DecideBuildHUD();

            // Setup build bot paths.
            // We have to do this at game-start time,
            // because the new objects we create are wiped on scene-change.
            UWE.CoroutineHost.StartCoroutine(BuildBotManager.SetupBuildBotPathsForAllMVs());
            MainPatcher.VFPlayerStartActions.ForEach(x => x(__instance));
            VehicleFramework.Admin.GameStateWatcher.IsPlayerStarted = true;
            return;
        }


        [HarmonyPrefix]
        [HarmonyPatch("ExitLockedMode")]
        public static bool ExitLockedModePrefix(Player __instance, ref Player.Mode ___mode)
        {
            /*
             * This patch ensures we exit out of the pilot seat correctly.
             * It also controls pilot-initiated auto-leveling.
             */
            void DoExitActions(ref Player.Mode mode)
            {
                GameInput.ClearInput();
                __instance.playerController.SetEnabled(true);
                mode = Player.Mode.Normal;
                __instance.playerModeChanged.Trigger(mode);
                __instance.sitting = false;
                __instance.playerController.ForceControllerSize();
                __instance.transform.parent = null;
            }
            VehicleTypes.Submersible mvSubmersible = __instance.GetVehicle() as VehicleTypes.Submersible;
            VehicleTypes.Walker mvWalker = __instance.GetVehicle() as VehicleTypes.Walker;
            VehicleTypes.Skimmer mvSkimmer = __instance.GetVehicle() as VehicleTypes.Skimmer;
            VehicleTypes.Drone mvDrone = VehicleTypes.Drone.mountedDrone;
            VehicleTypes.Submarine mvSubmarine = __instance.GetVehicle() as VehicleTypes.Submarine;
            if (mvSubmersible != null)
            {
                // exit locked mode
                DoExitActions(ref ___mode);
                mvSubmersible.StopPiloting();
                return false;
            }
            if (mvWalker != null)
            {
                DoExitActions(ref ___mode);
                mvWalker.StopPiloting();
                return false;
            }
            if (mvSkimmer != null)
            {
                DoExitActions(ref ___mode);
                mvSkimmer.StopPiloting();
                return false;
            }
            if (mvDrone != null)
            {
                mvDrone.StopControlling();
            }
            if (mvSubmarine != null)
            {
                // check if we're level by comparing pitch and roll
                float roll = mvSubmarine.transform.rotation.eulerAngles.z;
                float rollDelta = roll >= 180 ? 360 - roll : roll;
                float pitch = mvSubmarine.transform.rotation.eulerAngles.x;
                float pitchDelta = pitch >= 180 ? 360 - pitch : pitch;

                if (rollDelta > 4f || pitchDelta > 4f)
                {
                    BasicText message;
                    if (HUDBuilder.IsVR)
                    {
                        message = new BasicText(250, 250);
                    }
                    else
                    {
                        message = new BasicText(500, 0);
                    }
                    message.ShowMessage(LocalizationManager.GetString(EnglishString.TooSteep) + GameInput.Button.Exit.ToString(), 3);
                    return false;
                }
                else if (mvSubmarine.useRigidbody.velocity.magnitude > 2f)
                {
                    BasicText message;
                    if (HUDBuilder.IsVR)
                    {
                        message = new BasicText(250, 250);
                    }
                    else
                    {
                        message = new BasicText(500, 0);
                    }
                    message.ShowMessage(LocalizationManager.GetString(EnglishString.TooFast) + GameInput.Button.Exit.ToString(), 3);
                    return false;
                }

                // teleport the player to a walking position, just behind the chair
                Player.main.transform.position = mvSubmarine.PilotSeats[0].Seat.transform.position - mvSubmarine.PilotSeats[0].Seat.transform.forward * 1 + mvSubmarine.PilotSeats[0].Seat.transform.up * 1f;

                DoExitActions(ref ___mode);
                mvSubmarine.StopPiloting();
                return false;
            }

            return true;

        }
        [HarmonyPrefix]
        [HarmonyPatch("GetDepthClass")]
        public static bool GetDepthClass(Player __instance, ref Ocean.DepthClass __result)
        {
            /*
             * TODO
             * I believe this function relates to the PDA voice communicating depth information to you.
             * "Passing 400 meters," that sort of thing.
             * I'm not sure this patch is strictly necessary.
             */
            VehicleTypes.Submarine mv = __instance.GetVehicle() as VehicleTypes.Submarine;
            if (mv != null && !mv.IsPlayerPiloting())
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
        [HarmonyPatch("Update")]
        public static void UpdatePostfix(Player __instance)
        {
            VehicleTypes.Submarine mv = __instance.GetVehicle() as VehicleTypes.Submarine;
            if (mv == null)
            {
                return;
            }
            if (mv.IsPlayerInside() && !mv.IsPlayerPiloting())
            {
                // animator stuff to ensure we don't act like we're swimming at any point
                __instance.playerAnimator.SetBool("is_underwater", false);
                __instance.playerAnimator.SetBool("on_surface", false);
                __instance.playerAnimator.SetBool("diving", false);
                __instance.playerAnimator.SetBool("diving_land", false);
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch("UpdateIsUnderwater")]
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
        [HarmonyPatch("UpdateMotorMode")]
        public static bool UpdateMotorModePrefix(Player __instance)
        {
            VehicleTypes.Submarine mv = __instance.GetVehicle() as VehicleTypes.Submarine;
            if (mv != null && !mv.IsPlayerPiloting())
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
                    case Submarine sub:
                        __result = mv.IsPowered() && mv.IsPlayerDry;
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
            var list = Admin.GameObjectManager<Drone>.Where(x => x.IsPlayerDry);
            if (list.Count() == 0)
            {
                return;
            }
            Drone drone = list.First();
            __result = true;
        }

    }
}
