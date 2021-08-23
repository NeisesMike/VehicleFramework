using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.IO;
using UnityEngine;
using System.Reflection;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(GameInput))]
    [HarmonyPatch("Awake")]
    public class GameInputPatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            /*
            // load the asset bundle
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "assets/atrama"));
            if (myLoadedAssetBundle == null)
            {
                Logger.Log("Failed to load AssetBundle!");
                return;
            }
            Logger.Log("Found the asset bundle!");
            var prefab = myLoadedAssetBundle.LoadAsset<GameObject>("Atrama.prefab");
            Logger.Log("Found the Atrama!");
            GameObject __instanceVehicle = GameObject.Instantiate(prefab);
            Logger.Log("Instantiated the Atrama!");
            __instanceVehicle.transform.position = Player.main.transform.position + 10 * Player.main.transform.forward;
            */
        }
    }
}
