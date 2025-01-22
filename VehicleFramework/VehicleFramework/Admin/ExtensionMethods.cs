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
        public static List<string> GetCurrentUpgrades(this SubRoot subroot)
        {
            return subroot.upgradeConsole.modules.equipment.Select(x => x.Value).Where(x => x != null && x.item != null).Select(x => x.item.name).ToList();
        }
        public static AudioSource Register(this AudioSource source)
        {
            return FreezeTimePatcher.Register(source);
        }
        public static void Undock(this Vehicle vehicle)
        {
            void UndockModVehicle(Vehicle thisVehicle)
            {
                if (vehicle is ModVehicle)
                {
                    (vehicle as ModVehicle).OnVehicleUndocked();
                }
            }
            var theseBays = vehicle.transform.parent?.gameObject?.GetComponentsInChildren<VehicleDockingBay>()?.Where(x => x.dockedVehicle == vehicle);
            if(theseBays == null || theseBays.Count() == 0)
            {
                UndockModVehicle(vehicle);
                return;
            }
            VehicleDockingBay thisBay = theseBays.First();
            UWE.CoroutineHost.StartCoroutine(thisBay.MaybeToggleCyclopsCollision());
            thisBay.vehicle_docked_param = false;
            Player toUndock = vehicle.liveMixin.IsAlive() && !Admin.ConsoleCommands.isUndockConsoleCommand ? Player.main : null;
            UWE.CoroutineHost.StartCoroutine(vehicle.Undock(toUndock, thisBay.transform.position.y));
            SkyEnvironmentChanged.Broadcast(vehicle.gameObject, (GameObject)null);
            thisBay.dockedVehicle = null;
            UndockModVehicle(vehicle);
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
