using System;
using static VehicleUpgradeConsoleInput;

namespace VehicleFramework.Admin
{
    public static class EnumHelper
    {
        public const string ControlScheme = "VehicleFrameworkScheme";
        public const string ModuleType = "VehicleFrameworkModuleType";
        public const string ArmType = "VehicleFrameworkArmType";
        public const string InnateStorage = "VehicleFrameworkInnateStorage";

        internal static void Setup()
        {
            Nautilus.Handlers.EnumHandler.AddEntry<Vehicle.ControlSheme>(ControlScheme);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleType);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ArmType);
            Nautilus.Handlers.EnumHandler.AddEntry<TechType>(InnateStorage);
        }

        public static Vehicle.ControlSheme GetScheme()
        {
            if (Nautilus.Handlers.EnumHandler.TryGetValue(ControlScheme, out Vehicle.ControlSheme result))
            {
                return result;
            }
            throw SessionManager.Fatal($"Could not get the Vehicle.ControlSheme enum for string {ControlScheme}");
        }
        public static EquipmentType GetModuleType()
        {
            if (Nautilus.Handlers.EnumHandler.TryGetValue(ModuleType, out EquipmentType result))
            {
                return result;
            }
            throw SessionManager.Fatal($"Could not get the EquipmentType enum for string {ModuleType}");
        }
        public static EquipmentType GetArmType()
        {
            if (Nautilus.Handlers.EnumHandler.TryGetValue(ArmType, out EquipmentType result))
            {
                return result;
            }
            throw SessionManager.Fatal($"Could not get the EquipmentType enum for string {ArmType}");
        }
        public static TechType GetInnateStorageType()
        {
            if (Nautilus.Handlers.EnumHandler.TryGetValue(InnateStorage, out TechType result))
            {
                return result;
            }
            throw SessionManager.Fatal($"Could not get the TechType enum for string {InnateStorage}");
        }
    }
}
