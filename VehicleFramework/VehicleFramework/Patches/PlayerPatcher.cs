using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.IO;
using UnityEngine;
using System.Reflection;

using System.Reflection.Emit;

using SMLHelper.V2.Utility;

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
        [HarmonyPatch("Start")]
        public static void StartPostfix(Player __instance)
        {
            __instance.gameObject.EnsureComponent<ModVehicleTether>();
            HUDBuilder.DecideBuildHUD();

            // Setup build bot paths.
            // We have to do this at game-start time,
            // because the new objects we create are wiped on scene-change.
            // TODO
            // Knowing this, we might be able to factor out some gameobjects,
            // that we'd been requiring in the assetbundle side of things.
            __instance.StartCoroutine(BuildBotManager.SetupBuildBotPaths());

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

            ModVehicle mv = __instance.GetVehicle() as ModVehicle;
            if (mv == null)
            {
                return true;
            }

            // check if we're level by comparing pitch and roll
            float roll = mv.transform.rotation.eulerAngles.z;
            float rollDelta = roll >= 180 ? 360 - roll : roll;
            float pitch = mv.transform.rotation.eulerAngles.x;
            float pitchDelta = pitch >= 180 ? 360 - pitch : pitch;

            if (rollDelta > 4f || pitchDelta > 4f)
            {
                BasicText message;
                if(HUDBuilder.IsVR)
                {
                    message = new BasicText(250, 250);
                }
                else
                {
                    message = new BasicText(500, 0);
                }
                message.ShowMessage("Angle is too steep.\nDouble tap " + GameInput.Button.Exit.ToString() + "\nButton to auto-level.", 5);
                return false;
            }
            else if (mv.useRigidbody.velocity.magnitude > 2f)
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
                message.ShowMessage("Velocity is too great.\nDouble tap " + GameInput.Button.Exit.ToString() + "\nButton to auto-brake.", 5);
                return false;
            }

            // exit locked mode
            GameInput.ClearInput();

            // teleport the player to a walking position, just behind the chair
            Player.main.transform.position = mv.PilotSeats[0].Seat.transform.position - mv.PilotSeats[0].Seat.transform.forward * 1 + mv.PilotSeats[0].Seat.transform.up * 1f;

            __instance.playerController.SetEnabled(true);
            ___mode = Player.Mode.Normal;
            __instance.playerModeChanged.Trigger(___mode);
            __instance.sitting = false;
            __instance.playerController.ForceControllerSize();

            __instance.transform.parent = null;

            mv.StopPiloting();

            return false;
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
            ModVehicle mv = __instance.GetVehicle() as ModVehicle;
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
            ModVehicle mv = __instance.GetVehicle() as ModVehicle;
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
            ModVehicle mv = __instance.GetVehicle() as ModVehicle;
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
            ModVehicle mv = __instance.GetVehicle() as ModVehicle;
            if (mv != null && !mv.IsPlayerPiloting())
            {
                // ensure: if we're in a modvehicle and we're not piloting, then we're walking.
                __instance.SetMotorMode(Player.MotorMode.Walk);
                return false;
            }
            return true;
        }

    }
}
