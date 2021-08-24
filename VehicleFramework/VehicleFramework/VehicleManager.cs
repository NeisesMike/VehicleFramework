using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public static class VehicleManager
    {
        public static List<VehicleHUD> HUDs = new List<VehicleHUD>();
        public static List<ModVehicle> VehiclesInPlay = new List<ModVehicle>();

        public static void RegisterVehicle(ModVehicle mv)
        {
            VehiclesInPlay.Add(mv);
            Logger.Log(mv.name + " : " + mv.GetName() + " : " + mv.subName + " was registered!");
        }
        public static void DeregisterVehicle(ModVehicle mv)
        {
            VehiclesInPlay.Remove(mv);
        }
        public static void SaveVehicles()
        {

        }
        public static void LoadVehicles()
        {

        }
        public static void UpdateVehicles()
        {
            foreach(ModVehicle mv in VehiclesInPlay)
            {
                //mv.Update();
            }
        }
    }
}
