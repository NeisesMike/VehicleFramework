using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(LightmappedPrefabs))]
    class LightmappedPrefabsPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void StartPostfix(Dictionary<string,GameObject> ___scene2prefab)
        {
            AtramaBuilder.scene2prefab = ___scene2prefab;
        }
    }
}
