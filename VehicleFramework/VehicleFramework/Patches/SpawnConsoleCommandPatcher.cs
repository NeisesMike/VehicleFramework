using System.Collections;
using System.Linq;
using HarmonyLib;
using UnityEngine;

// PURPOSE: allow the spawn console command to work for ModVehicles
// VALUE: Moderate. Could register a new console command instead.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(SpawnConsoleCommand))]
    public static class SpawnConsoleCommandPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpawnConsoleCommand.OnConsoleCommand_spawn))]
        public static void OnConsoleCommand_spawnPostfix(SpawnConsoleCommand __instance, NotificationCenter.Notification n)
        {
            if (n != null && n.data != null && n.data.Count > 0)
            {
                string text = (string)n.data[0];
                if (UWE.Utils.TryParseEnum<TechType>(text, out TechType techType))
                {
                    Admin.SessionManager.StartCoroutine(CheckSpawnForMVs(techType));
                }
            }
        }
        public static void FinishAnySpawningVehicles()
        {
            void FinishHim(ModVehicle mv)
            {
                mv.GetComponentInChildren<VFXConstructing>(true).constructed = 90f;
                mv.GetComponentInChildren<VFXConstructing>(true).delay = 0f;
            }
            VehicleManager.VehiclesInPlay
                .Where(x => x != null && x.GetComponentInChildren<VFXConstructing>(true) != null && x.GetComponentInChildren<VFXConstructing>(true).constructed < 100f)
                .ForEach(x => FinishHim(x));
        }
        public static IEnumerator CheckSpawnForMVs(TechType tt)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(tt, true);
            yield return request;
            GameObject result = request.GetResult();
            if (result != null && result.GetComponent<ModVehicle>() != null)
            {
                yield return new WaitForSeconds(0.5f);
                FinishAnySpawningVehicles();
            }
        }
    }
}
