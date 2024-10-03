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
    public struct FragmentData
    {
        public GameObject fragment;
        public TechType toUnlock;
        public int fragmentsToScan;
        public float scanTime;
        public string classID;
        public string displayName;
        public string description;
        public List<Vector3> spawnLocations;
        public List<Vector3> spawnRotations;
        public string encyKey;
    }
    public class FragmentManager : MonoBehaviour
    {
        private static readonly List<PDAScanner.EntryData> PDAScannerData = new List<PDAScanner.EntryData>();
        internal static PDAScanner.EntryData MakeGenericEntryData(TechType fragmentTT, TechType toUnlock, int numFragmentsToScan, float scanTime, string encyKey="IAmAnUnusedEncyclopediaKey")
        {
            PDAScanner.EntryData entryData = new PDAScanner.EntryData()
            {
                key = fragmentTT,
                locked = true,
                totalFragments = numFragmentsToScan,
                destroyAfterScan = true,
                encyclopedia = encyKey,
                blueprint = toUnlock,
                scanTime = scanTime,
                isFragment = true
            };
            return entryData;
        }
        /// <summary>
        /// Registers a fragment using a FragmentData struct as input. For a ModVehicle, you can access its techtype AFTER registration like this:
        /// vehicle.GetComponent<TechTag>().type
        /// </summary>
        /// <returns>The TechType of the new fragment.</returns>
        public static TechType RegisterFragment(FragmentData frag)
        {
            if (frag.fragment == null)
            {
                Logger.Error("RegisterFragment error: fragment was null");
                return 0;
            }
            TechType fragmentTT = RegisterFragmentGeneric(frag.fragment, frag.classID, frag.displayName, frag.description, frag.spawnLocations, frag.spawnRotations);
            PDAScannerData.Add(MakeGenericEntryData(fragmentTT, frag.toUnlock, frag.fragmentsToScan, frag.scanTime, frag.encyKey));
            return fragmentTT;
        }
        internal static TechType RegisterFragmentGeneric(GameObject fragment, string classID, string displayName, string description, List<Vector3> spawnLocations, List<Vector3> spawnRotations = null)
        {
            if(spawnLocations != null && spawnRotations != null && spawnLocations.Count() != spawnRotations.Count())
            {
                Logger.Error("For classID: " + classID + ": Tried to register fragment with unequal number of spawn locations and rotations. Ensure there is one rotation for every location, or else don't specify any rotations.");
                return TechType.None;
            }
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
            else if(spawnRotations == null)
            {
                armFragment.SetSpawns(spawnLocations.Select(x => new SpawnLocation(x)).ToArray());
            }
            else
            {
                List<SpawnLocation> spawns = new List<SpawnLocation>();
                for(int i=0; i<spawnLocations.Count(); i++)
                {
                    spawns.Add(new SpawnLocation(spawnLocations[i], spawnRotations[i]));
                }
                armFragment.SetSpawns(spawns.ToArray());
            }
            armFragment.Register();
            Logger.Log("Registered fragment: " + classID);
            return fragmentInfo.TechType;
        }
        internal static void AddScannerDataEntries()
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

        internal static void SetupScannable(GameObject obj, TechType tt)
        {
            var rt = obj.AddComponent<ResourceTracker>();
            rt.techType = tt;
            rt.prefabIdentifier = obj.GetComponent<PrefabIdentifier>();
            rt.prefabIdentifier.Id = "";
            rt.overrideTechType = TechType.None;
        }
    }
}
