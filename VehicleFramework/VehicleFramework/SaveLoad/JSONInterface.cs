using System;
using System.IO;
using Newtonsoft.Json;

namespace VehicleFramework.SaveLoad
{
    public static class JsonInterface
    {
        public static void Write<T>(ModVehicle mv, string fileTitle, T data)
        {
            Write<T>($"{fileTitle}-{mv.GetComponent<PrefabIdentifier>().Id}", data);
        }
        public static T Read<T>(ModVehicle mv, string fileTitle)
        {
            return Read<T>($"{fileTitle}-{mv.GetComponent<PrefabIdentifier>().Id}");
        }
        public static void Write<T>(string uniqueFileName, T data)
        {
            string fileName = GetFilePath(uniqueFileName);
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(fileName, json);
            }
            catch(Exception e)
            {
                Logger.Error($"Failed to serialize into file {uniqueFileName}!");
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
            }
        }
        public static T Read<T>(string uniqueFileName)
        {
            string fileName = GetFilePath(uniqueFileName);
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                return JsonConvert.DeserializeObject<T>(json);
            }
            return default;
        }
        private static string GetFilePath(string innerName)
        {
            string directoryPath = SaveLoadManager.GetTemporarySavePath();
            string configFolderPath = Path.Combine(directoryPath, "VFSaveData");
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }
            return Path.Combine(configFolderPath, $"{innerName}.json");
        }
    }
}
