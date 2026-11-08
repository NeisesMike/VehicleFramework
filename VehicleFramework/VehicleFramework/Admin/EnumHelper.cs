using System;
using System.Security.AccessControl;
using static VehicleUpgradeConsoleInput;

namespace VehicleFramework.Admin
{
    public static class EnumHelper
    {
        public const string VFEnumPrefix = "VehicleFrameworkEnum_";
        public const string ControlScheme = $"{VFEnumPrefix}ControlScheme";
        public const string ModuleType = $"{VFEnumPrefix}ModuleType";
        public const string ArmType = $"{VFEnumPrefix}ArmType";
        public const string InnateStorage = $"{VFEnumPrefix}InnateStorage";

        public const string ModuleTypeTemplate = $"{VFEnumPrefix}Module_";
        public const string ModuleMV = $"{ModuleTypeTemplate}MV";
        public const string ModuleSM = $"{ModuleTypeTemplate}SM";
        public const string ModuleES = $"{ModuleTypeTemplate}ES";
        public const string ModuleCC = $"{ModuleTypeTemplate}CC";
        public const string ModuleMVSM = $"{ModuleTypeTemplate}MVSM";
        public const string ModuleMVES = $"{ModuleTypeTemplate}MVES";
        public const string ModuleMVCC = $"{ModuleTypeTemplate}MVCC";
        public const string ModuleSMES = $"{ModuleTypeTemplate}SMES";
        public const string ModuleSMCC = $"{ModuleTypeTemplate}SMCC";
        public const string ModuleESCC = $"{ModuleTypeTemplate}ESCC";
        public const string ModuleMVSMES = $"{ModuleTypeTemplate}MVSMES";
        public const string ModuleMVSMCC = $"{ModuleTypeTemplate}MVSMCC";
        public const string ModuleMVESCC = $"{ModuleTypeTemplate}MVESCC";
        public const string ModuleSMESCC = $"{ModuleTypeTemplate}SMESCC";
        public const string ModuleMVSMESCC = $"{ModuleTypeTemplate}MVSMESCC";

        public const string ArmTypeTemplate = $"{VFEnumPrefix}ArmType_";
        public const string ArmMV = $"{ArmTypeTemplate}MV";
        public const string ArmES = $"{ArmTypeTemplate}ES";
        public const string ArmMVES = $"{ArmTypeTemplate}MVES";


        internal static void Setup()
        {
            Nautilus.Handlers.EnumHandler.AddEntry<Vehicle.ControlSheme>(ControlScheme);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleType);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ArmType);
            Nautilus.Handlers.EnumHandler.AddEntry<TechType>(InnateStorage);

            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleMV);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleSM);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleES);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleCC);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleMVSM);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleMVES);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleMVCC);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleSMES);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleSMCC);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleESCC);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleMVSMES);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleMVSMCC);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleMVESCC);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleSMESCC);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ModuleMVSMESCC);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ArmMV);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ArmES);
            Nautilus.Handlers.EnumHandler.AddEntry<EquipmentType>(ArmMVES);

        }

        public static Vehicle.ControlSheme GetScheme()
        {
            if (Nautilus.Handlers.EnumHandler.TryGetValue(ControlScheme, out Vehicle.ControlSheme result))
            {
                return result;
            }
            throw SessionManager.Fatal($"Could not get the Vehicle.ControlSheme enum for string {ControlScheme}");
        }
        public static EquipmentType GetVFModuleType(string typeName)
        {
            if (Nautilus.Handlers.EnumHandler.TryGetValue(typeName, out EquipmentType result))
            {
                return result;
            }
            throw SessionManager.Fatal($"Could not get the EquipmentType enum for string {typeName}");
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
        public static bool IsModuleName(string name)
        {
            return name.StartsWith(VehicleBuilding.ModuleBuilder.ModVehicleModulePrefix) && int.TryParse(name[VehicleBuilding.ModuleBuilder.ModVehicleModulePrefix.Length..], out _);
        }
    }
}
