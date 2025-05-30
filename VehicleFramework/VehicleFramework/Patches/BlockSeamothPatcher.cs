using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

// PURPOSE: Prevent ModVehicles from entering "moon gates"
// VALUE: high, for the sake of world consistency

namespace VehicleFramework.Patches
{
    internal class BlockModVehicle : MonoBehaviour
    {
        private readonly Dictionary<ModVehicle, int> MVs = new Dictionary<ModVehicle, int>();
        internal void FixedUpdate()
        {
            MVs.ForEach(x => x.Key.useRigidbody.AddForce(transform.forward * 3f, ForceMode.VelocityChange));
        }
        internal void OnTriggerEnter(Collider other)
        {
            ModVehicle mv = other.GetComponentInParent<ModVehicle>();
            if (mv == null)
            {
                return;
            }
            if (MVs.ContainsKey(mv))
            {
                MVs[mv]++;
            }
            else
            {
                MVs.Add(mv, 1);
            }
        }
        internal void OnTriggerExit(Collider other)
        {
            ModVehicle mv = other.GetComponentInParent<ModVehicle>();
            if (mv == null)
            {
                return;
            }
            MVs[mv]--;
            if (MVs[mv] <= 0)
            {
                MVs.Remove(mv);
            }
        }
    }


    /* 
     * Prevent ModVehicles from entering "moon gates"
     * which are vertical force fields that prevent seamoth entry.
     * There's one at "prison" and one at "lavacastlebase"
     */
    [HarmonyPatch(typeof(BlockSeamoth))]
    public class BlockSeamothPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockSeamoth.FixedUpdate))]
        public static void BlockSeamothFixedUpdatePostfix(BlockSeamoth __instance)
        {
            if(__instance.GetComponent<BlockModVehicle>() == null)
            {
                __instance.gameObject.AddComponent<BlockModVehicle>();
            }
        }
    }
}
