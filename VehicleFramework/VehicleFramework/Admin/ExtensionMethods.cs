using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
