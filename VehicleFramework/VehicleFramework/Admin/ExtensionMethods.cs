using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Patches;

namespace VehicleFramework
{
    public static class ExtensionMethods
    {
        public static ModVehicle GetModVehicle(this Player player)
        {
            return 
                VehicleTypes.Drone.mountedDrone
                ?? player.GetVehicle() as ModVehicle
                ?? player.currentSub?.GetComponent<ModVehicle>();
        }
        public static List<string> GetCurrentUpgrades(this Vehicle vehicle)
        {
            return vehicle.modules.equipment.Select(x => x.Value).Where(x => x != null && x.item != null).Select(x=>x.item.name).ToList();
        }
        public static AudioSource Register(this AudioSource source)
        {
            return FreezeTimePatcher.Register(source);
        }
        public static void Undock(this Vehicle vehicle)
        {
            VehicleDockingBay thisBay = vehicle.transform.parent?.gameObject?.GetComponentsInChildren<VehicleDockingBay>()?.Where(x => x.dockedVehicle == vehicle)?.First();
            if(thisBay == null)
            {
                return;
            }
            UWE.CoroutineHost.StartCoroutine(thisBay.MaybeToggleCyclopsCollision());
            thisBay.vehicle_docked_param = false;
            UWE.CoroutineHost.StartCoroutine(vehicle.Undock(vehicle.liveMixin.IsAlive() ? Player.main : null, thisBay.transform.position.y));
            SkyEnvironmentChanged.Broadcast(vehicle.gameObject, (GameObject)null);
            thisBay.dockedVehicle = null;
            if(vehicle is ModVehicle)
            {
                (vehicle as ModVehicle).OnVehicleUndocked();
            }
        }
        public static IEnumerator MaybeToggleCyclopsCollision(this VehicleDockingBay bay)
        {
            if (bay.subRoot.name.ToLower().Contains("cyclops"))
            {
                bay.transform.parent.parent.parent.Find("CyclopsCollision").gameObject.SetActive(false);
                yield return new WaitForSeconds(2f);
                bay.transform.parent.parent.parent.Find("CyclopsCollision").gameObject.SetActive(true);
            }
            yield break;
        }
        public static bool IsPilotingCyclops(this Player player)
        {
            return player.IsInCyclops() && player.mode == Player.Mode.Piloting;
        }
        public static bool IsInCyclops(this Player player)
        {
            return player.currentSub != null && player.currentSub.name.ToLower().Contains("cyclops");
        }
        public static bool IsGameObjectAncestor(this Transform current, GameObject ancestor)
        {
            if (current == null || ancestor == null)
            {
                return false;
            }
            if (current.gameObject == ancestor)
            {
                return true;
            }
            return current.parent.IsGameObjectAncestor(ancestor);
        }
    }
}
