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
        public const string SaveFileSpritesFileName = "SaveFileSprites";
        public static Dictionary<string, List<string>> hasTechTypeGameInfo = new Dictionary<string, List<string>>();

        // This patch collects hasTechTypeGameInfo, in order to have save file sprites displayed on the save cards
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SaveLoadManager.RegisterSaveGame))]
        public static void SaveLoadManagerRegisterSaveGamePostfix(string slotName)
        {
            string directoryPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string prefixToKeep = "Subnautica";
            int index = directoryPath.LastIndexOf(prefixToKeep);
            if(index == -1)
            {
                Logger.DebugLog("SaveLoadManager.RegisterSaveGamePostfix failed to find the save game json file path!");
                return;
            }
            string result = directoryPath.Substring(0, index);
            string savePath = Path.Combine(result, "SNAppData", "SavedGames", slotName, SaveLoad.JsonInterface.SaveFolderName, $"{SaveFileSpritesFileName}.json");
            if (!File.Exists(savePath))
            {
                Logger.DebugLog("SaveLoadManager.RegisterSaveGamePostfix failed to find the save game json file!");
                return;
            }
            try
            {
                string jsonContent = File.ReadAllText(savePath);
                List<string> hasTechTypes = JsonConvert.DeserializeObject<List<string>>(jsonContent);
                if (hasTechTypes != null)
                {
                    hasTechTypeGameInfo.Add(slotName, hasTechTypes);
                }
            }
            catch(System.Exception e)
            {
                Logger.Error("SaveLoadManager.RegisterSaveGamePostfix: Could not read json file!");
                Logger.Error(e.StackTrace);
            }
        }
    }
}
