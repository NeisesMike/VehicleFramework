using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public static class VehicleManager
    {
        public static List<ModVehicle> vehicles = new List<ModVehicle>();
        public static List<VehicleHUD> huds = new List<VehicleHUD>();


        public static void RegisterVehicle(ModVehicle mv)
        {
            vehicles.Add(mv);
        }
        public static void DeregisterVehicle(ModVehicle mv)
        {
            vehicles.Remove(mv);
        }

        public static void SaveVehicles()
        {

        }
        public static void LoadVehicles()
        {

        }

        public static void UpdateVehicles()
        {
            foreach(ModVehicle mv in vehicles)
            {
                mv.ModUpdate();
            }
        }
    }
}
