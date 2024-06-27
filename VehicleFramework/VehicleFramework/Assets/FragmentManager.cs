using System;
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
        public static PDAScanner.EntryData MakeGenericEntryData(TechType fragmentTT, TechType toUnlock, int numFragmentsToScan)
        {
            PDAScanner.EntryData entryData = new PDAScanner.EntryData()
            {
                key = fragmentTT,
                locked = true,
                totalFragments = numFragmentsToScan,
                destroyAfterScan = true,
                encyclopedia = "Irrelevant Text",
                blueprint = toUnlock,
                scanTime = 5f,
                isFragment = true
            };
            return entryData;
        }
        public static TechType RegisterFragment(GameObject fragment, ModVehicle vehicle, string classID, string displayName, string description, Sprite unlockSprite=null, List<BiomeData> biomeData=null)
        {
            if (vehicle == null)
            {
                Logger.Error("RegisterFragment error: vehicle was null");
                return 0;
            }
            return RegisterFragment(fragment, vehicle.GetComponent<TechTag>().type, vehicle.FragmentsToScan, classID, displayName, description, unlockSprite, biomeData);
        }
        public static TechType RegisterFragment(GameObject fragment, TechType toUnlock, int fragmentsToScan, string classID, string displayName, string description, Sprite sprite = null, List<BiomeData> biomeData = null)
        {
            if (fragment == null)
            {
                Logger.Error("RegisterFragment error: fragment was null");
                return 0;
            }
            TechType fragmentTT = RegisterGenericFragment(fragment, classID, displayName, description, sprite, biomeData, "congration");
            PDAScannerData.Add(MakeGenericEntryData(fragmentTT, toUnlock, fragmentsToScan));
            return fragmentTT;
        }
        public static TechType RegisterGenericFragment(GameObject fragment, string classID, string displayName, string description, Sprite unlockSprite = null, List<BiomeData> biomeData = null, string unlockedMessage = "")
        {
            PrefabInfo fragmentInfo = PrefabInfo.WithTechType(classID, displayName, description);
            CustomPrefab armFragment = new CustomPrefab(fragmentInfo);
            fragment.AddComponent<BoxCollider>();
            fragment.AddComponent<PrefabIdentifier>().ClassId = classID;
            fragment.AddComponent<FragmentManager>();
            armFragment.SetGameObject(() => fragment);
            List<BiomeData> useBiomes = biomeData;
            if (useBiomes == null)
            {
                useBiomes = new List<BiomeData>
                {
                    new BiomeData { biome = BiomeType.SafeShallows_Grass, count = 4, probability = 0.3f },
                    new BiomeData { biome = BiomeType.SafeShallows_CaveFloor, count = 1, probability = 0.4f }
                };
            }
            armFragment.SetSpawns(useBiomes.ToArray());
            armFragment.Register();
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
    }
}
