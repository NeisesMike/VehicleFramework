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
        public static PDAScanner.EntryData MakeGenericEntryData(TechType fragmentTT, ModVehicle mv)
        {
            PDAScanner.EntryData entryData = new PDAScanner.EntryData()
            {
                key = fragmentTT,
                locked = true,
                totalFragments = mv.FragmentsToScan,
                destroyAfterScan = true,
                encyclopedia = "Irrelevant Text",
                blueprint = mv.GetComponent<TechTag>().type,
                scanTime = 5f,
                isFragment = true
            };
            return entryData;
        }
        public static TechType RegisterFragment(GameObject fragment, ModVehicle vehicle, string classID, string displayName, string description, Atlas.Sprite atlasSprite=null, List<BiomeData> biomeData=null)
        {
            if(fragment == null)
            {
                Logger.Error("RegisterFragment error: fragment was null");
                return 0;
            }
            if (vehicle == null)
            {
                Logger.Error("RegisterFragment error: vehicle was null");
                return 0;
            }
            Atlas.Sprite useSprite = atlasSprite;
            if(useSprite == null)
            {
                useSprite = VehicleManager.defaultPingSprite;
            }
            PrefabInfo fragmentInfo = PrefabInfo.WithTechType(classID, displayName, description);
            fragmentInfo.WithIcon(useSprite);
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
            PDAScannerData.Add(MakeGenericEntryData(fragmentInfo.TechType, vehicle));
            return fragmentInfo.TechType;
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
        public static void AddScannerDataEntries()
        {
            void TryAddScannerData(PDAScanner.EntryData data)
            {
                if(PDAScanner.mapping.ContainsKey(data.key))
                {
                    return;
                }
                PDAScanner.mapping.Add(data.key, data);
            }
            PDAScannerData.ForEach(x => TryAddScannerData(x));
        }
    }
}
