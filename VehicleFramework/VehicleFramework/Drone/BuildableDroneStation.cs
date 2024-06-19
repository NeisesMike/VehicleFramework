using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using Nautilus.Crafting;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using UnityEngine;
using Nautilus.Assets.PrefabTemplates;
using Ingredient = CraftData.Ingredient;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    public static class BuildableDroneStation
    {
        public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("DroneStation", "Drone Station", "A terminal from which to control drones remotely.")
            // set the icon to that of the vanilla locker:
            .WithIcon(SpriteManager.Get(TechType.PictureFrame));
        public static void Register()
        {
            CustomPrefab prefab = new CustomPrefab(Info);
            CloneTemplate cloneTemplate = new CloneTemplate(Info, TechType.PictureFrame);
            cloneTemplate.ModifyPrefab += obj =>
            {
                GameObject model = obj.transform.Find("mesh/submarine_Picture_Frame").gameObject;
                ConstructableFlags constructableFlags = ConstructableFlags.Inside | ConstructableFlags.Wall | ConstructableFlags.Submarine;
                obj.AddComponent<DroneStation>();
                PrefabUtils.AddConstructable(obj, Info.TechType, constructableFlags, model);
            };
            prefab.SetGameObject(cloneTemplate);
            prefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);
            prefab.SetRecipe(new RecipeData(new Ingredient(TechType.ComputerChip, 1), new Ingredient(TechType.Glass, 1), new Ingredient(TechType.Titanium, 1), new Ingredient(TechType.Silver, 1)));
            prefab.Register();
        }
    }

    public class DroneStation : HandTarget, IHandTarget, IDroneInterface
    {
        public static DroneStation BroadcastingStation = null;
        public Drone pairedDrone;
        public override void Awake()
        {
            base.Awake();
            Admin.GameObjectManager<DroneStation>.Register(this);
        }
        public void Start()
        {
            IEnumerator WaitThenAct()
            {
                while(!Admin.GameStateWatcher.IsPlayerStarted)
                {
                    yield return null;
                }
                GetComponent<PictureFrame>().enabled = false;
                transform.Find("Trigger").gameObject.SetActive(false);
                transform.Find("mesh/submarine_Picture_Frame/submarine_Picture_Frame_button").gameObject.AddComponent<BoxCollider>();
                gameObject.AddComponent<BoxCollider>();
                DroneStation.FastenConnection(this, FindNearestUnpairedDrone());
                Component.Destroy(GetComponent<Rigidbody>());
            }
            StartCoroutine(WaitThenAct());
        }
        public void HandlePairingClick()
        {
            if ((this as IDroneInterface).IsInPairingModeAsInitiator())
            {
                (this as IDroneInterface).FinalizePairingMode();
            }
            else if ((this as IDroneInterface).IsInPairingModeAsResponder())
            {
                DroneStation.FastenConnection(this, Drone.BroadcastingDrone);
                (this as IDroneInterface).FinalizePairingMode();
            }
            else
            {
                (this as IDroneInterface).InitiatePairingMode();
            }
        }
        public void HandleRemoteControlClick()
        {
            if ((this as IDroneInterface).IsInPairingModeAsInitiator())
            {
                (this as IDroneInterface).FinalizePairingMode();
                if (pairedDrone != null)
                {
                    pairedDrone.BeginControlling();
                }
            }
            else if ((this as IDroneInterface).IsInPairingModeAsResponder())
            {
                (this as IDroneInterface).ExitPairingMode();
            }
            else
            {
                if (pairedDrone == null)
                {
                    (this as IDroneInterface).InitiatePairingMode();
                }
                else
                {
                    pairedDrone.BeginControlling();
                }
            }
        }
        void IHandTarget.OnHandClick(GUIHand hand)
        {
            Targeting.GetTarget(Player.main.gameObject, 6f, out GameObject target, out float num);
            if (target.name.Contains("DroneStation"))
            {
                HandleRemoteControlClick();
            }
            else
            {
                HandlePairingClick();
            }
        }
        public void HandleRemoteControlHover()
        {
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            if ((this as IDroneInterface).IsInPairingModeAsInitiator())
            {
                if (pairedDrone == null)
                {
                    HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Cancel Pairing");
                }
                else
                {
                    HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Cancel Pairing and Connect");
                }
            }
            else if ((this as IDroneInterface).IsInPairingModeAsResponder())
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Ignore Pairing");
            }
            else
            {
                if (pairedDrone == null)
                {
                    HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Begin Pairing");
                }
                else
                {
                    HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Connect");
                }
            }
        }
        public void HandlePairingHover()
        {
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            if ((this as IDroneInterface).IsInPairingModeAsInitiator())
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Cancel Pairing");
            }
            else if ((this as IDroneInterface).IsInPairingModeAsResponder())
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Accept Pairing");
            }
            else
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Begin Pairing");
            }
        }
        void IHandTarget.OnHandHover(GUIHand hand)
        {
            Targeting.GetTarget(Player.main.gameObject, 6f, out GameObject target, out float num);
            if (target.name.Contains("DroneStation"))
            {
                HandleRemoteControlHover();
            }
            else
            {
                HandlePairingHover();
            }
        }
        Drone FindNearestUnpairedDrone()
        {
            return Admin.GameObjectManager<Drone>.FindNearestSuch(transform.position, (x => x.pairedStation is null));
        }
        public bool IsInPairingMode
        {
            get
            {
                return (this as IDroneInterface).IsInPairingModeAsInitiator() || (this as IDroneInterface).IsInPairingModeAsInitiator();
            }
        }
        public static void FastenConnection(DroneStation station, Drone drone)
        {
            if(drone == null || station == null)
            {
                return;
            }
            if(station.pairedDrone != null)
            {
                // if we have a paired drone already, we need to tell it we're finished
                station.pairedDrone.pairedStation = null;
            }
            station.pairedDrone = drone;

            if(drone.pairedStation != null)
            {
                // if our newly paired drone already had a paired station, we need to tell that station its pairing is history
                drone.pairedStation.pairedDrone = null;
            }
            drone.pairedStation = station;
        }
        void IDroneInterface.InitiatePairingMode()
        {
            DroneStation.BroadcastingStation = this;
            isInitiator = true;
            Admin.GameObjectManager<Drone>.AllSuchObjects.ForEach(x => (x as IDroneInterface).RespondWithPairingMode());
            Admin.GameObjectManager<DroneStation>.AllSuchObjects.Where(x => x != this).ForEach(x => (x as IDroneInterface).ExitPairingMode());
        }
        void IDroneInterface.FinalizePairingMode()
        {
            DroneStation.BroadcastingStation = null;
            Drone.BroadcastingDrone = null;
            Admin.GameObjectManager<DroneStation>.AllSuchObjects.ForEach(x => (x as IDroneInterface).ExitPairingMode());
            Admin.GameObjectManager<Drone>.AllSuchObjects.ForEach(x => (x as IDroneInterface).ExitPairingMode());
        }
        void IDroneInterface.RespondWithPairingMode()
        {
            isInitiator = false;
            isResponder = true;
        }
        void IDroneInterface.ExitPairingMode()
        {
            isInitiator = false;
            isResponder = false;
        }
        bool isInitiator = false;
        bool isResponder = false;
        bool IDroneInterface.IsInPairingModeAsInitiator()
        {
            return isInitiator;
        }
        bool IDroneInterface.IsInPairingModeAsResponder()
        {
            return isResponder;
        }
    }
}
