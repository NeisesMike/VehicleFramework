using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.SaveLoad
{
    public static class ModVehicleSaveLoad
    {
        const string upgradesFileName = "";
        const string innateStorageFileName = "";
        const string modularStorageFileName = "";
        const string batteriesFileName = "";
        const string backupBatteriesFileName = "";
        const string playerInsideFileName = "";
        const string aestheticsFileName = "";
        const string playerControllingFileName = "";
        const string subNameFileName = "";
        #region saving
        public static void Save(ModVehicle mv)
        {
            SerializeUpgrades(mv);
            SerializeInnateStorage(mv);
            SerializeModularStorage(mv);
            SerializeBatteries(mv);
            SerializeBackupBatteries(mv);
            SerializePlayerInside(mv);
            SerializeAesthetics(mv);
            SerializePlayerControlling(mv);
            DeserializeSubName(mv);
        }

        private static void SerializeUpgrades(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void SerializeInnateStorage(ModVehicle mv)
        {
            Dictionary<string, Tuple<TechType, float>> result = new Dictionary<string, Tuple<TechType, float>>();
            foreach (InnateStorageContainer vsc in mv.GetComponentsInChildren<InnateStorageContainer>())
            {
            }
            JsonInterface.Write<Dictionary<string, Tuple<TechType, float>>>(mv, innateStorageFileName, null);
        }

        private static void SerializeModularStorage(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void SerializeBatteries(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void SerializeBackupBatteries(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void SerializePlayerInside(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void SerializeAesthetics(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void SerializePlayerControlling(ModVehicle mv)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region loading
        public static void Load(ModVehicle mv)
        {
            DeserializeUpgrades(mv);
            DeserializeInnateStorage(mv);
            DeserializeModularStorage(mv);
            DeserializeBatteries(mv);
            DeserializeBackupBatteries(mv);
            DeserializePlayerInside(mv);
            DeserializeAesthetics(mv);
            DeserializePlayerControlling(mv);
            DeserializeSubName(mv);
        }

        private static void DeserializeUpgrades(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void DeserializeInnateStorage(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void DeserializeModularStorage(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void DeserializeBatteries(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void DeserializeBackupBatteries(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void DeserializePlayerInside(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void DeserializeAesthetics(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void DeserializePlayerControlling(ModVehicle mv)
        {
            throw new NotImplementedException();
        }

        private static void DeserializeSubName(ModVehicle mv)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
