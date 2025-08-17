using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using Nautilus.Assets.Gadgets;

namespace VehicleFramework.Assets
{
    public struct FragmentData
    {
        // you should set either fragment or fragments, but not both.
        public GameObject fragment; // if you just have one fragment object
        public List<GameObject> fragments; // if you want to supply multiple fragment objects
        public TechType toUnlock;
        public int fragmentsToScan;
        public float scanTime;
        public string classID;
        public string displayName;
        public string description;
        public List<Vector3> spawnLocations;
        public List<Vector3>? spawnRotations;
        public string encyKey;
    }
    public class FragmentManager : MonoBehaviour
    {
        private static readonly List<PDAScanner.EntryData> PDAScannerData = new();
        internal static PDAScanner.EntryData MakeGenericEntryData(TechType fragmentTT, FragmentData frag)
        {
            PDAScanner.EntryData entryData = new()
            {
                key = fragmentTT,
                locked = true,
                totalFragments = frag.fragmentsToScan,
                destroyAfterScan = true,
                encyclopedia = frag.encyKey,
                blueprint = frag.toUnlock,
                scanTime = frag.scanTime,
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
            if(frag.fragment == null && (frag.fragments == null || frag.fragments.Count() < 1))
            {
                Logger.Error("RegisterFragment error: no fragment objects were supplied");
                return 0;
            }
            if(frag.fragment != null && frag.fragments != null && frag.fragments.Count() > 0)
            {
                Logger.Warn("RegisterFragment warning: fragment and fragments were both supplied. Fragment will be ignored.");
            }
            if(frag.spawnLocations == null || frag.spawnLocations.Count < 1)
            {
                Logger.Error("For classID: " + frag.classID + ": Tried to register fragment without any spawn locations!");
            }
            if (frag.spawnLocations != null && frag.spawnRotations != null && frag.spawnLocations.Count() != frag.spawnRotations.Count())
            {
                Logger.Error("For classID: " + frag.classID + ": Tried to register fragment with unequal number of spawn locations and rotations. Ensure there is one rotation for every location, or else don't specify any rotations.");
                return TechType.None;
            }
            TechType fragmentTT;
            if(frag.fragment != null && (frag.fragments == null || frag.fragments.Count() < 1))
            {
                Nautilus.Assets.CustomPrefab customPrefab = RegisterFragmentGenericSingle(frag, frag.fragment, true, out fragmentTT);
                customPrefab.Register();
                Logger.Log("Registered fragment: " + frag.classID);
            }
            else
            {
                fragmentTT = RegisterFragmentGeneric(frag);
                Logger.Log("Registered fragment: " + frag.classID + " with " + (frag.fragments.Count() + 1).ToString() + " variations.");
            }
            PDAScannerData.Add(MakeGenericEntryData(fragmentTT, frag));
            return fragmentTT;
        }
        internal static TechType RegisterFragmentGeneric(FragmentData frag)
        {
            GameObject head = frag.fragments.First();
            List<GameObject> tail = frag.fragments;
            tail.Remove(head);
            List<Nautilus.Assets.CustomPrefab> customPrefabs = new()
            {
                RegisterFragmentGenericSingle(frag, head, false, out TechType fragmentType)
            };
            int numberFragments = 1;
            foreach (GameObject fragmentObject in tail)
            {
                Admin.Utils.ApplyMarmoset(fragmentObject);
                Nautilus.Assets.PrefabInfo fragmentInfo = Nautilus.Assets.PrefabInfo.WithTechType(frag.classID + numberFragments.ToString(), frag.displayName, frag.description);
                numberFragments++;
                fragmentInfo.TechType = fragmentType;
                Nautilus.Assets.CustomPrefab customFragmentPrefab = new(fragmentInfo);
                fragmentObject.EnsureComponent<BoxCollider>();
                fragmentObject.EnsureComponent<PrefabIdentifier>().ClassId = frag.classID;
                fragmentObject.EnsureComponent<FragmentManager>();
                fragmentObject.EnsureComponent<LargeWorldEntity>();
                fragmentObject.EnsureComponent<SkyApplier>().enabled = true;
                SetupScannable(fragmentObject, fragmentInfo.TechType);
                customFragmentPrefab.SetGameObject(() => fragmentObject);
                customPrefabs.Add(customFragmentPrefab);
            }
            int numberPrefabsRegistered = 0;
            foreach (Nautilus.Assets.CustomPrefab customPrefab in customPrefabs)
            {
                List<Vector3> spawnLocationsToUse = new();
                List<int> indexes = new();
                int iterator = numberPrefabsRegistered;
                while(iterator < frag.spawnLocations.Count())
                {
                    spawnLocationsToUse.Add(frag.spawnLocations[iterator]);
                    indexes.Add(iterator);
                    iterator += customPrefabs.Count();
                }
                if (frag.spawnRotations == null)
                {
                    customPrefab.SetSpawns(spawnLocationsToUse.Select(x => new Nautilus.Assets.SpawnLocation(x)).ToArray()); // this creates a harmless Nautilus error
                }
                else
                {
                    List<Nautilus.Assets.SpawnLocation> spawns = new();
                    for (int i = 0; i < spawnLocationsToUse.Count(); i++)
                    {
                        spawns.Add(new Nautilus.Assets.SpawnLocation(spawnLocationsToUse[i], frag.spawnRotations[indexes[i]]));
                    }
                    customPrefab.SetSpawns(spawns.ToArray()); // this creates a harmless Nautilus error
                }
                customPrefab.Register();
                numberPrefabsRegistered++;
            }
            return fragmentType;
        }
        internal static Nautilus.Assets.CustomPrefab RegisterFragmentGenericSingle(FragmentData frag, GameObject fragmentObject, bool doSpawnLocations, out TechType result)
        {
            Admin.Utils.ApplyMarmoset(fragmentObject);
            Nautilus.Assets.PrefabInfo fragmentInfo = Nautilus.Assets.PrefabInfo.WithTechType(frag.classID, frag.displayName, frag.description);
            Nautilus.Assets.CustomPrefab customFragmentPrefab = new(fragmentInfo);
            fragmentObject.EnsureComponent<BoxCollider>();
            Nautilus.Utility.PrefabUtils.AddBasicComponents(fragmentObject, frag.classID, fragmentInfo.TechType, LargeWorldEntity.CellLevel.Global);
            fragmentObject.EnsureComponent<FragmentManager>();
            SetupScannable(fragmentObject, fragmentInfo.TechType);
            customFragmentPrefab.SetGameObject(() => fragmentObject);
            if (doSpawnLocations)
            {
                if (frag.spawnRotations == null)
                {
                    customFragmentPrefab.SetSpawns(frag.spawnLocations.Select(x => new Nautilus.Assets.SpawnLocation(x)).ToArray()); // this creates a harmless Nautilus error
                }
                else
                {
                    List<Nautilus.Assets.SpawnLocation> spawns = new();
                    for (int i = 0; i < frag.spawnLocations.Count(); i++)
                    {
                        spawns.Add(new Nautilus.Assets.SpawnLocation(frag.spawnLocations[i], frag.spawnRotations[i]));
                    }
                    customFragmentPrefab.SetSpawns(spawns.ToArray()); // this creates a harmless Nautilus error
                }
            }
            result = fragmentInfo.TechType;
            return customFragmentPrefab;
        }
        internal static void AddScannerDataEntries()
        {
            static void TryAddScannerData(PDAScanner.EntryData data)
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
            Admin.Utils.StartCoroutine(DestroyPickupable());
        }

        internal static void SetupScannable(GameObject obj, TechType tt)
        {
            var rt = obj.EnsureComponent<ResourceTracker>();
            rt.techType = tt;
            rt.prefabIdentifier = obj.GetComponent<PrefabIdentifier>();
            rt.prefabIdentifier.Id = "";
            rt.overrideTechType = TechType.None;
        }
    }
}
