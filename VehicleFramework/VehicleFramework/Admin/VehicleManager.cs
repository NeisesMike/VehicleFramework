using System.Collections.Generic;
using VehicleFramework.VehicleTypes;
using VehicleFramework.VehicleBuilding;
using System;
using System.Linq;

namespace VehicleFramework.Admin
{
    public static class VehicleManager
    {
        internal static readonly List<ModVehicle> VehiclesInPlay = new();
        internal static readonly List<VehicleEntry> vehicleTypes = new();
        public static void EnrollVehicle(ModVehicle mv)
        {
            if (mv.name.Contains("Clone") && !VehiclesInPlay.Contains(mv))
            {
                VehiclesInPlay.Add(mv);
                Logger.Log($"Enrolled the {mv.name}: {mv.GetName()}");
            }
        }
        public static void DeregisterVehicle(ModVehicle mv)
        {
            if (VehiclesInPlay.Contains(mv))
            {
                VehiclesInPlay.Remove(mv);
            }
        }
        public static List<ModVehicle> GetVehiclesWhere(Func<ModVehicle, bool> match) => VehiclesInPlay.Where(match).ToList();
        public static List<VehicleEntry> GetVehicleTypesWhere(Func<VehicleEntry, bool> match) => vehicleTypes.Where(match).ToList();
    }
}
