using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(LargeWorldEntity))]
    public class LargeWorldEntityPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch("Register")]
        public static void RegisterPostfix(GameObject go)
        {
            if (go.GetComponent<Atrama>() != null)
            {
                AtramaManager.addAtrama(go.GetComponent<Atrama>());
            }
        }
    }
}
