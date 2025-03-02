﻿using System;
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
            string json;
            try
            {
                json = JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            catch(Exception e)
            {
                Logger.Error($"Failed to serialize json data for file {uniqueFileName}!");
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
                return;
            }
            try
            {
                File.WriteAllText(fileName, json);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to write file {uniqueFileName}!");
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
            }
        }
        public static T Read<T>(string uniqueFileName)
        {
            string fileName = GetFilePath(uniqueFileName);
            if (!File.Exists(fileName))
            {
                return default;
            }
            string json;
            try
            {
                json = File.ReadAllText(fileName);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to read file {uniqueFileName}!");
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
                return default;
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to deserialize json from file {uniqueFileName}!");
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
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
