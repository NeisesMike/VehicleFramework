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
                ?? Player.main.GetVehicle() as ModVehicle
                ?? Player.main.currentSub?.GetComponent<ModVehicle>();
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
            UWE.CoroutineHost.StartCoroutine(vehicle.Undock(Player.main, thisBay.transform.position.y));
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
    }
}
