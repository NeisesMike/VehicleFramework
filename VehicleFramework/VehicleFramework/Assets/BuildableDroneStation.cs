using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Nautilus.Assets.Gadgets;
using UnityEngine;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Assets;

namespace VehicleFramework
{
    public static class BuildableDroneStation
    {
        public const string classID = "DroneStation";
        public static string displayName = Language.main.Get("VFDroneStationDisplayName");
        public static string description = Language.main.Get("VFDroneStationDesc");
        public static string encyclopediaDesc = Language.main.Get("VFDroneStationEncy");

        public static TechType RegisterConsole(GameObject droneStation, Atlas.Sprite crafter, Sprite unlock)
        {
            Nautilus.Assets.PrefabInfo Info = Nautilus.Assets.PrefabInfo.WithTechType(classID, displayName, description)
                .WithIcon(crafter);
            Nautilus.Assets.CustomPrefab prefab = new Nautilus.Assets.CustomPrefab(Info);
            Nautilus.Utility.ConstructableFlags constructableFlags = Nautilus.Utility.ConstructableFlags.Inside | Nautilus.Utility.ConstructableFlags.Wall | Nautilus.Utility.ConstructableFlags.Submarine;
            droneStation.AddComponent<DroneStation>();
            Admin.Utils.ApplyMarmoset(droneStation);
            Nautilus.Utility.PrefabUtils.AddBasicComponents(droneStation, classID, Info.TechType, LargeWorldEntity.CellLevel.Medium);
            Nautilus.Utility.PrefabUtils.AddConstructable(droneStation, Info.TechType, constructableFlags, droneStation.transform.Find("model").gameObject);
            prefab.SetGameObject(droneStation);
            prefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);
            prefab.SetRecipe(new Nautilus.Crafting.RecipeData(new CraftData.Ingredient(TechType.ComputerChip, 1), new CraftData.Ingredient(TechType.Glass, 1), new CraftData.Ingredient(TechType.Titanium, 1), new CraftData.Ingredient(TechType.Silver, 1)));
            prefab.SetUnlock(TechType.Fragment)
                .WithAnalysisTech(unlock, unlockMessage: Language.main.Get("VFDroneStationUnlockText"));
            prefab.Register();
            return Info.TechType;
        }

        public static void Register()
        {
            VehicleAssets DSAssets = AssetBundleInterface.GetVehicleAssetsFromBundle("dronestation", "DroneStation", "DSSpriteAtlas", "", "DSCrafterSprite", "Fragment", "DSUnlockSprite");
            TechType consoleTT = RegisterConsole(DSAssets.model, DSAssets.crafter, DSAssets.unlock);
            List<Vector3> spawnLocations = new List<Vector3>
            {
                new Vector3 (375.1f, -69.4f, -22.4f),
                new Vector3 (122.4f, -38.9f, -131.4f),
                new Vector3 (89.9f, -30.5f, -162.6f),
                new Vector3 (30.2f, -42.4f, -217.5f),
                new Vector3 (46.9f, -20.1f, -86.7f),
                new Vector3 (-148.1f, -31.7f, 252.8f),
                new Vector3 (-150.2f, -47.7f, 234.4f),
                new Vector3 (-228.7f, -66.2f, 159.8f),
                new Vector3 (172.2f, -73.6f, -7.1f),
                new Vector3 (394.4f, -98.7f, 83.3f),
                new Vector3 (379.4f, -117.9f, 122.3f),
                new Vector3 (424.8f, -112.5f, 104.3f),
                new Vector3 (375.1f, -69.4f, -22.4f),
                new Vector3 (-148.1f, -31.7f, 252.8f)
            };
            PDAEncyclopedia.EntryData entry = new PDAEncyclopedia.EntryData
            {
                key = classID,
                path = "Tech/Habitats",
                nodes = new[] { "Tech", "Habitats" },
                unlocked = true,
                popup = DSAssets.unlock,
                image = DSAssets.unlock.texture,
            };
            Admin.Utils.AddEncyclopediaEntry(entry);
            Patches.LanguagePatcher.SetLanguageLine($"Ency_{classID}", displayName);
            Patches.LanguagePatcher.SetLanguageLine($"EncyDesc_{classID}", encyclopediaDesc);
            FragmentData fragmentData = new FragmentData
            {
                fragment = DSAssets.fragment,
                toUnlock = consoleTT,
                fragmentsToScan = 3,
                scanTime = 5f,
                classID = classID + "Fragment",
                displayName = displayName + " Fragment",
                description = description + " ...fragment",
                spawnLocations = spawnLocations,
                spawnRotations = null,
                encyKey = classID
            };
            FragmentManager.RegisterFragment(fragmentData);
        }

        private static bool isRegistered = false;
        internal static void TryRegister(ModVehicle mv)
        {
            if(!isRegistered && mv is Drone)
            {
                Register();
                isRegistered = true;
            }
        }
    }

    public class DroneStation : HandTarget, IHandTarget
    {
        public static DroneStation BroadcastingStation = null;
        private Drone _pairedDrone = null;
        public Drone pairedDrone
        {
            get
            {
                return _pairedDrone;
            }
            private set
            {
                _pairedDrone = value;
            }
        }
        public override void Awake()
        {
            base.Awake();
            Admin.GameObjectManager<DroneStation>.Register(this);
            transform.SetParent(Player.main.transform.parent);
        }
        public void Start()
        {
            IEnumerator WaitThenAct()
            {
                yield return new WaitUntil(() => Admin.GameStateWatcher.IsPlayerStarted);
                Drone nearest = Admin.GameObjectManager<Drone>.FindNearestSuch(transform.position);
                DroneStation.FastenConnection(this, nearest);
                if (GetComponent<Rigidbody>())
                {
                    Component.Destroy(GetComponent<Rigidbody>());
                }
            }
            UWE.CoroutineHost.StartCoroutine(WaitThenAct());
        }
        public void Update()
        {
            GetComponentInChildren<Collider>().enabled = !Player.main.IsPilotingCyclops();
        }
        public static void FastenConnection(DroneStation station, Drone drone)
        {
            if(drone == null || station == null)
            {
                return;
            }
            station.pairedDrone = drone;
            drone.pairedStation = station;
        }
        void IHandTarget.OnHandClick(GUIHand hand)
        {
        }
        void IHandTarget.OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
            Targeting.GetTarget(Player.main.gameObject, 6f, out GameObject target, out float _);
            OnScreenHover();
        }
        public Drone SelectDrone(List<Drone> list, bool next)
        {
            int index = list.FindIndex(x => x == pairedDrone);
            if (list.Count() == 0)
            {
                return null;
            }
            else
            {
                if (index == -1)
                {
                    return list.First();
                }
                else
                {
                    if(next)
                    {
                        index++;
                    }
                    else
                    {
                        index--;
                    }
                    if(index < 0)
                    {
                        index = list.Count() - 1;
                    }
                    if((list.Count - 1) < index)
                    {
                        index = 0;
                    }
                    return list[index];
                }
            }
        }
        private bool IsConstructed()
        {
            return GetComponent<Constructable>().constructed;
        }
        private bool IsPowered()
        {
            if(GetComponentInParent<ModVehicle>() is ModVehicle mv)
            {
                return mv.energyInterface.hasCharge;
            }
            else if(GetComponentInParent<SubRoot>() is SubRoot sr)
            {
                return sr.powerRelay.IsPowered();
            }
            else
            {
                return false;
            }
        }
        public void OnScreenHover()
        {
            if(!IsConstructed() || !IsPowered())
            {
                return;
            }
            var list = Admin.GameObjectManager<Drone>.Where(x => x.gameObject.activeSelf && !x.isAsleep); // Pickupable Vehicles, for example, sets drones inactive.
            if (pairedDrone != null && !list.Contains(pairedDrone))
            {
                // if our paired drone recently became unavailable, unpair it from this DroneStation
                pairedDrone.pairedStation = null;
                pairedDrone = null;
            }
            if (pairedDrone == null && list.Count() > 0)
            {
                // if we ain't never peeped it before, grab one if possible
                FastenConnection(this, list.First());
            }
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, BuildScreenText());
            if (GameInput.GetButtonDown(GameInput.Button.LeftHand) && pairedDrone != null)
            {
                if (pairedDrone.isScuttled || !pairedDrone.energyInterface.hasCharge)
                {
                    ShowDetails(pairedDrone);
                }
                else
                {
                    FastenConnection(this, pairedDrone);
                    pairedDrone.BeginControlling();
                }
            }
            List<Drone> availableDrones = list.Where(x => GetComponentInParent<Player>() == null).ToList();
            if (availableDrones.Count() > 0)
            {
                Drone selected = null;
                if (GameInput.GetButtonDown(GameInput.Button.CycleNext))
                {
                    selected = SelectDrone(availableDrones, true);
                }
                else if (GameInput.GetButtonDown(GameInput.Button.CyclePrev))
                {
                    selected = SelectDrone(availableDrones, false);
                }
                if (selected != null)
                {
                    FastenConnection(this, selected);
                }
            }
        }
        public static string GetStatus(Drone drone)
        {
            if(drone.isScuttled)
            {
                return "Destroyed";
            }
            else if(!drone.energyInterface.hasCharge)
            {
                return "Low power";
            }
            else
            {
                drone.GetHUDValues(out float hp, out float energy);
                string health = Mathf.CeilToInt(100 * hp).ToString();
                string power = Mathf.CeilToInt(100 * energy).ToString();
                return "HP " + health + "%, Power " + power + "%";
            }
        }
        public string BuildScreenText()
        {
            string ret = $"{Language.main.Get("VFDroneHint1")}: {((pairedDrone != null) ? pairedDrone.GetName() : $"[{Language.main.Get("VFDroneStationHint1")}]")}\n";
            if (pairedDrone == null)
            {
                return ret;
            }
            ret += $"{Language.main.Get("VFDroneStationHint2")}: {GetStatus(pairedDrone)}\n";
            if(pairedDrone.isScuttled || !pairedDrone.energyInterface.hasCharge)
            {
                ret += HandReticle.main.GetText($"{Language.main.Get("VFDroneStationHint3")} ", false, GameInput.Button.LeftHand) + "\n";
            }
            else
            {
                ret += HandReticle.main.GetText($"{Language.main.Get("VFDroneStationHint4")} ", false, GameInput.Button.LeftHand) + "\n";
            }
            ret += HandReticle.main.GetText($"{Language.main.Get("VFDroneStationHint5")}: ", false, GameInput.Button.CycleNext) + "\n";
            ret += HandReticle.main.GetText($"{Language.main.Get("VFDroneStationHint6")}: ", false, GameInput.Button.CyclePrev) + "\n";
            return ret;
        }
        public void ShowDetails(Drone drone)
        {
            if(drone == null)
            {
                return;
            }
            IEnumerator PingPingForAWhile()
            {
                drone.pingInstance.enabled = true;
                yield return new WaitForSeconds(60);
                drone.pingInstance.enabled = false;
            }
            UWE.CoroutineHost.StartCoroutine(PingPingForAWhile());
            string ret = $"{Language.main.Get("VFDroneHint1")}:  + {drone.subName.hullName.text}\n";
            ret += $"{Language.main.Get("VFDroneHint2")}: {Mathf.CeilToInt(Vector3.Distance(drone.transform.position, transform.position))}\n";
            if(drone.isScuttled)
            {
                ret += $"{Language.main.Get("VFDroneHint3")}\n";
                ret += $"{Language.main.Get("VFDroneHint4")}\n";
            }
            else
            {
                ret += $"{Language.main.Get("VFDroneHint5")}\n";
                ret += $"{Language.main.Get("VFDroneHint6")}\n";
            }
            ret += $"{Language.main.Get("VFDroneHint7")}\n";

            Logger.PDANote(ret, 8);
        }
    }
}
