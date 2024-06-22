using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(MapRoomCameraScreenFXController))]
    class MapRoomCameraScreenFXControllerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MapRoomCameraScreenFXController.OnPreRender))]
        public static void OnPreRenderPostfix(MapRoomCameraScreenFXController __instance)
        {
            Drone drone = Drone.mountedDrone;
            if(drone == null)
            {
                return;
            }
            float noise;
            float distance = Vector3.Distance(Player.main.transform.position, drone.transform.position);
            const float threshold = 350;
            if(distance < threshold)
            {
                noise = 0;
            }
            else
            {
                noise = 0.04f * ((distance - threshold) / 10f);
            }
            __instance.fx.noiseFactor = noise;
            if(drone.IsConnecting)
            {
                __instance.fx.noiseFactor = 10f;
            }
        }
    }
}
