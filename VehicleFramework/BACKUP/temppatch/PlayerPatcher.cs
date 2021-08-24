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

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(Player))]
    public class PlayerPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        public static bool StartPrefix()
        {
            AtramaManager.Init();
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix()
        {
            if (AtramaBuilder.atramaModel == null)
            {
                AtramaBuilder.Init();
                GameObject thisAtramaObject = GameObject.Instantiate(AtramaBuilder.atramaModel);
                thisAtramaObject.SetActive(false);
                Atrama thisAtrama = thisAtramaObject.EnsureComponent<Atrama>();
                AtramaBuilder.buildAtrama(thisAtrama);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePostfix(Player __instance)
        {
            Atrama thisAtrama = AtramaManager.getCurrentAtrama();
            if (thisAtrama == null)
            {
                return;
            }

            if (thisAtrama.isPlayerInside && !thisAtrama.isPlayerPiloting)
            {
                __instance.playerController.activeController.SetUnderWater(false);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("ExitLockedMode")]
        public static bool ExitLockedModePrefix(Player __instance, ref Player.Mode ___mode)
        {
            Atrama thisAtrama = AtramaManager.getCurrentAtrama();
            if (thisAtrama == null)
            {
                return true;
            }
            AtramaVehicle atramaVehicle = thisAtrama.vehicle;

            // check if we're level by comparing pitch and roll
            float roll = thisAtrama.transform.rotation.eulerAngles.z;
            float rollDelta = roll >= 180 ? 360 - roll : roll;
            float pitch = thisAtrama.transform.rotation.eulerAngles.x;
            float pitchDelta = pitch >= 180 ? 360 - pitch : pitch;

            if (rollDelta > 4f || pitchDelta > 4f)
            {
                BasicText message = new BasicText(500, 0);
                message.ShowMessage("Angle is too steep.", 5);
                return false;
            }

            // exit locked mode
            GameInput.ClearInput();

            // teleport the player to a walking position, just behind the chair
            Player.main.transform.position = atramaVehicle.transform.position - atramaVehicle.transform.forward * 1 + atramaVehicle.transform.up * 1f;
            //MainCameraControl.main.LookAt(__instance.currentMountedVehicle.transform.position + __instance.currentMountedVehicle.transform.forward * 10f);
            __instance.currentMountedVehicle = null;

            __instance.playerController.SetEnabled(true);
            ___mode = Player.Mode.Normal;
            __instance.playerModeChanged.Trigger(___mode);
            __instance.sitting = false;
            __instance.playerController.ForceControllerSize();

            __instance.transform.parent = null;

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("CanBreathe")]
        public static void CanBreathePostfix(Player __instance, ref bool __result)
        {
            if (AtramaManager.getCurrentAtrama() != null)
            {
                if (AtramaManager.getCurrentAtrama().vehicle.IsPowered())
                {
                    __result = true;
                }
                else
                {
                    __result = __instance.IsUnderwater();
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("UpdateIsUnderwater")]
        public static bool UpdateIsUnderwaterPrefix(Player __instance)
        {
            if (AtramaManager.getCurrentAtrama() != null)
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
            if (AtramaManager.getCurrentAtrama() != null && !AtramaManager.getCurrentAtrama().isPlayerPiloting)
            {
                __instance.playerController.SetMotorMode(Player.MotorMode.Walk);
                __instance.motorMode = Player.MotorMode.Walk;
                __instance.SetScubaMaskActive(false);
                __instance.playerMotorModeChanged.Trigger(Player.MotorMode.Walk);
                return false;
            }
            return true;
        }


    }
}
