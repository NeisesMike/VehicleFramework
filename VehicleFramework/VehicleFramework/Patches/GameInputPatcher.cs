using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.IO;
using UnityEngine;
using System.Reflection;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(GameInput))]
    [HarmonyPatch("Awake")]
    public class GameInputPatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
        }
    }
}
