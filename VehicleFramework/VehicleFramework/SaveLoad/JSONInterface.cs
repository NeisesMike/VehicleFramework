using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
using VehicleFramework.Admin;

namespace VehicleFramework.SaveLoad
{
    public static class JsonInterface
    {
        public const string SaveFolderName = "VFSaveData";
        public static void Write<T>(Component comp, string fileTitle, T data)
        {
            if (comp == null)
            {
                Logger.Error($"Could not perform JsonInterface.Write because comp was null: {fileTitle}");
                return;
            }
            PrefabIdentifier prefabID = comp.GetComponent<PrefabIdentifier>();
            if (prefabID == null)
            {
                Logger.Error($"Could not perform JsonInterface.Write because comp had no PrefabIdentifier: {fileTitle}");
                return;
            }
            Write<T>($"{fileTitle}-{prefabID.Id}", data);
        }
        public static T? Read<T>(Component comp, string fileTitle)
        {
            if (comp == null)
            {
                Logger.Error($"Could not perform JsonInterface.Read because comp was null: {fileTitle}");
                return default;
            }
            PrefabIdentifier prefabID = comp.GetComponent<PrefabIdentifier>();
            if (prefabID == null)
            {
                Logger.Error($"Could not perform JsonInterface.Read because comp had no PrefabIdentifier: {fileTitle}");
                return default;
            }
            return Read<T>($"{fileTitle}-{prefabID.Id}");
        }
        public static void Write<T>(string uniqueFileName, T data)
        {
            string fileName;
            try
            {
                fileName = GetFilePath(uniqueFileName);
            }
            catch(Exception e)
            {
                Logger.LogException("Failed to GetFilePath!", e);
                return;
            }
            string json;
            try
            {
                json = JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            catch(Exception e)
            {
                Logger.LogException($"Failed to serialize json data for file {uniqueFileName}!", e);
                return;
            }
            try
            {
                if(fileName.Length > 260)
                {
                    throw SessionManager.Fatal("That file path was too long!");
                }
                File.WriteAllText(fileName, json);
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to write file {uniqueFileName}!", e);
            }
        }
        public static T? Read<T>(string uniqueFileName)
        {
            string fileName;
            try
            {
                fileName = GetFilePath(uniqueFileName);
            }
            catch (Exception e)
            {
                Logger.LogException("Failed to GetFilePath!", e);
                return default;
            }
            if (!File.Exists(fileName))
            {
                Logger.DebugLog($"File did not exist! {fileName}!");
                return default;
            }
            string json;
            try
            {
                json = File.ReadAllText(fileName);
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to read file {uniqueFileName}!", e);
                return default;
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to deserialize json from file {uniqueFileName}!", e);
            }
            return default;
        }
        private static string GetFilePath(string innerName)
        {
            string directoryPath = Path.Combine(PlatformServicesNull.DefaultSavePath, SaveLoadManager.main.GetCurrentSlot());
            string configFolderPath = Path.Combine(directoryPath, SaveFolderName);
            if (!Directory.Exists(configFolderPath))
            {
                Directory.CreateDirectory(configFolderPath);
            }
            return Path.Combine(configFolderPath, $"{innerName}.json");
        }
        internal static bool IsJsonSerializable(object obj)
        {
            try
            {
                JsonConvert.SerializeObject(obj);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
