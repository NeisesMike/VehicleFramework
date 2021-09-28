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
    public class PlayerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(Player __instance)
        {
            // load any vehicles from save now.

            // prepare any HUDs
            HUDBuilder.BuildNormalHUD();

            __instance.gameObject.EnsureComponent<ModVehicleTether>();
            return;
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
                __instance.playerController.activeController.SetUnderWater(false);
                __instance.isUnderwater.Update(false);
                __instance.isUnderwaterForSwimming.Update(false);
                __instance.playerController.SetMotorMode(Player.MotorMode.Walk);
                __instance.motorMode = Player.MotorMode.Walk;
                __instance.SetScubaMaskActive(false);
                __instance.playerMotorModeChanged.Trigger(Player.MotorMode.Walk);
                __instance.depthLevel = -10f;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("ExitLockedMode")]
        public static bool ExitLockedModePrefix(Player __instance, ref Player.Mode ___mode)
        {
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
                BasicText message = new BasicText(500, 0);
                message.ShowMessage("Angle is too steep.\nDouble tap " + GameInput.Button.Exit.ToString() + "\nButton to auto-level.", 5);
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
        [HarmonyPatch("UpdateIsUnderwater")]
        public static bool UpdateIsUnderwaterPrefix(Player __instance)
        {
            ModVehicle mv = __instance.GetVehicle() as ModVehicle;
            if (mv != null)
            {
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
                __instance.SetMotorMode(Player.MotorMode.Walk);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetDepthClass")]
        public static bool GetDepthClass(Player __instance, ref Ocean.DepthClass __result)
        {
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
    }
}
