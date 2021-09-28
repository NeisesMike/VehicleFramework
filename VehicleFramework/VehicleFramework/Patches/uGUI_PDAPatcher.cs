using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(uGUI_PDA))]
    public class uGUI_PDAPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix()
        {
            VehicleBuilder.moduleBuilder = new GameObject("ModuleBuilder");
            ModuleBuilder.main = VehicleBuilder.moduleBuilder.AddComponent<ModuleBuilder>();
            ModuleBuilder.main.grabComponents();

            // setup build bot paths
            // we have to do this at game-start time
            // because the new objects we create are wiped on scene-change
            // TODO
            // knowing this, we might be able to factor out some gameobjects,
            // that we'd been requiring in the assetbundle side of things
            BuildBotManager.SetupBuildBotPaths();

            // TODO maybe clean up?
        }
    }
}
