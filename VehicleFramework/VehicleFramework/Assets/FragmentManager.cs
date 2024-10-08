﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using BiomeData = LootDistributionData.BiomeData;


namespace VehicleFramework.Assets
{
    public class FragmentManager : MonoBehaviour
    {
        private static readonly List<PDAScanner.EntryData> PDAScannerData = new List<PDAScanner.EntryData>();
        public static PDAScanner.EntryData MakeGenericEntryData(TechType fragmentTT, TechType toUnlock, int numFragmentsToScan, string encyKey="IAmAnUnusedEncyclopediaKey")
        {
            PDAScanner.EntryData entryData = new PDAScanner.EntryData()
            {
                key = fragmentTT,
                locked = true,
                totalFragments = numFragmentsToScan,
                destroyAfterScan = true,
                encyclopedia = encyKey,
                blueprint = toUnlock,
                scanTime = 5f,
                isFragment = true
            };
            return entryData;
        }
        public static TechType RegisterFragment(GameObject fragment, ModVehicle vehicle, string classID, string displayName, string description, Sprite unlockSprite = null, List<Vector3> spawnLocations = null, string encyKey = "IAmAnUnusedEncyclopediaKey")
        {
            if (vehicle == null)
            {
                Logger.Error("RegisterFragment error: vehicle was null");
                return 0;
            }
            return RegisterFragment(fragment, vehicle.GetComponent<TechTag>().type, vehicle.FragmentsToScan, classID, displayName, description, unlockSprite, spawnLocations, encyKey);
        }
        public static TechType RegisterFragment(GameObject fragment, TechType toUnlock, int fragmentsToScan, string classID, string displayName, string description, Sprite sprite = null, List<Vector3> spawnLocations = null, string encyKey = "IAmAnUnusedEncyclopediaKey")
        {
            if (fragment == null)
            {
                Logger.Error("RegisterFragment error: fragment was null");
                return 0;
            }
            TechType fragmentTT = RegisterGenericFragment(fragment, classID, displayName, description, sprite, spawnLocations, "congration");
            PDAScannerData.Add(MakeGenericEntryData(fragmentTT, toUnlock, fragmentsToScan, encyKey));
            return fragmentTT;
        }
        public static TechType RegisterGenericFragment(GameObject fragment, string classID, string displayName, string description, Sprite unlockSprite = null, List<Vector3> spawnLocations = null, string unlockedMessage = "")
        {
            PrefabInfo fragmentInfo = PrefabInfo.WithTechType(classID, displayName, description);
            CustomPrefab armFragment = new CustomPrefab(fragmentInfo);
            fragment.AddComponent<BoxCollider>();
            fragment.AddComponent<PrefabIdentifier>().ClassId = classID;
            fragment.AddComponent<FragmentManager>();
            fragment.AddComponent<LargeWorldEntity>();
            fragment.AddComponent<SkyApplier>().enabled = true;
            SetupScannable(fragment, fragmentInfo.TechType);
            armFragment.SetGameObject(() => fragment);
            List<BiomeData> useBiomes = new List<BiomeData>();
            if (spawnLocations == null)
            {
                useBiomes = new List<BiomeData>
                {
                    new BiomeData { biome = BiomeType.SafeShallows_Grass, count = 4, probability = 0.01f },
                    new BiomeData { biome = BiomeType.SafeShallows_CaveFloor, count = 1, probability = 0.02f }
                };
                armFragment.SetSpawns(useBiomes.ToArray());
            }
            else
            {
                armFragment.SetSpawns(spawnLocations.Select(x => new SpawnLocation(x)).ToArray());
            }
            Logger.Log("Registering fragments. You may see errors below.");
            armFragment.Register();
            Logger.Log("Done registering fragments.");
            return fragmentInfo.TechType;
        }
        public static void AddScannerDataEntries()
        {
            void TryAddScannerData(PDAScanner.EntryData data)
            {
                if (PDAScanner.mapping.ContainsKey(data.key))
                {
                    return;
                }
                PDAScanner.mapping.Add(data.key, data);
            }
            PDAScannerData.ForEach(x => TryAddScannerData(x));
        }
        public void Start()
        {
            IEnumerator DestroyPickupable()
            {
                while (GetComponent<Pickupable>() != null)
                {
                    Component.Destroy(GetComponent<Pickupable>());
                    yield return null;
                }
                Component.Destroy(this);
            }
            UWE.CoroutineHost.StartCoroutine(DestroyPickupable());
        }

        public static void SetupScannable(GameObject obj, TechType tt)
        {
            var rt = obj.AddComponent<ResourceTracker>();
            rt.techType = tt;
            rt.prefabIdentifier = obj.GetComponent<PrefabIdentifier>();
            rt.prefabIdentifier.Id = "";
            rt.overrideTechType = TechType.None;
        }
    }
}
