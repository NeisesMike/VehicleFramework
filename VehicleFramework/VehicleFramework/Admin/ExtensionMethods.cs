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
            return player.GetVehicle() as ModVehicle
                ?? player.currentSub.GetComponent<ModVehicle>();
        }
    }
}
