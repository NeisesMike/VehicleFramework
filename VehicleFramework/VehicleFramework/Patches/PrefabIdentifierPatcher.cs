using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    /*
     * This patch synergizes with CustomDataboxes and the ItemGrounder,
     * allowing me to scatter databoxes somewhat randomly,
     * and allowing them to settle to the floor naturally.
     */
    [HarmonyPatch(typeof(UniqueIdentifier))]
    class UniqueIdentifierPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch("Awake")]
        public static void AwakePrefix(UniqueIdentifier __instance)
        {
            if(__instance.ClassId == "atrama_databox")
            {
                __instance.GetComponent<Rigidbody>().isKinematic = false;
                __instance.GetComponent<Rigidbody>().drag = 5;
                __instance.GetComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
                __instance.gameObject.EnsureComponent<GroundedItems.ItemGrounder>().OnEnable();
            }
        }
    }
}
