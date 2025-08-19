using HarmonyLib;
using UnityEngine;
using VehicleFramework.VehicleTypes;

// PURPOSE: Create visual noise when Drones are far away from their station
// VALUE: High.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(MapRoomCameraScreenFXController))]
    class MapRoomCameraScreenFXControllerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MapRoomCameraScreenFXController.OnPreRender))]
        public static void OnPreRenderPostfix(MapRoomCameraScreenFXController __instance)
        {
            Drone? drone = Drone.MountedDrone;
            if(drone == null)
            {
                return;
            }
            float noise;
            float distance = Vector3.Distance(Player.main.transform.position, drone.transform.position);
            float threshold = Drone.baseConnectionDistance + drone.addedConnectionDistance;
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
                __instance.fx.color = Color.white * 0.2f;
            }
            else
            {
                __instance.fx.color = Color.white;
            }
        }
    }
}
