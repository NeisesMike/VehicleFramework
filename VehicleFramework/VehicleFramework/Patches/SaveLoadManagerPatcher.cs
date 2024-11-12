using HarmonyLib;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace VehicleFramework.Patches
{
    // See also: MainMenuLoadPanelPatcher
    [HarmonyPatch(typeof(SaveLoadManager))]
    public class SaveLoadManagerPatcher
    {
        public static Dictionary<string, List<string>> hasTechTypeGameInfo = new Dictionary<string, List<string>>();
        public static List<string> ReadJsonFile(string filePath)
        {
            // Read the entire JSON file
            string jsonContent = File.ReadAllText(filePath);

            // Parse the JSON
            JObject jsonObject = JObject.Parse(jsonContent);

            // Retrieve the "HasVehicleTechTypes" array as a list of strings
            JToken hasVehicleTechTypesToken = jsonObject["HasVehicleTechTypes"];

            if (hasVehicleTechTypesToken != null && hasVehicleTechTypesToken.Type == JTokenType.Array)
            {
                List<string> vehicleTechTypes = hasVehicleTechTypesToken.ToObject<List<string>>();
                return vehicleTechTypes;
            }
            else
            {
                Logger.DebugLog("Property 'HasVehicleTechTypes' not found or is not an array.");
            }
            return null;
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SaveLoadManager.RegisterSaveGame))]
        public static void SaveLoadManagerRegisterSaveGamePostfix(string slotName)
        {
            string savePath = "";
            string directoryPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string prefixToKeep = "Subnautica";
            int index = directoryPath.LastIndexOf(prefixToKeep);
            if (index != -1)
            {
                string result = directoryPath.Substring(0, index);
                savePath = Path.Combine(result, "SNAppData", "SavedGames", slotName, "VehicleFramework", "vehicle_storage.json");
            }
            else
            {
                Logger.DebugLog("SaveLoadManager.RegisterSaveGamePostfix failed to find the save game json file!");
            }

            try
            {
                List<string> hasTechTypes = ReadJsonFile(savePath);
                if (hasTechTypes != null)
                {
                    hasTechTypeGameInfo.Add(slotName, hasTechTypes);
                }
            }
            catch
            {
                Logger.Warn("Could not read json file!");
            }
        }
    }
}
