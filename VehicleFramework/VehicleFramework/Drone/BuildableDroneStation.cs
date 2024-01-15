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


            //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //PrefabTemplate cloneTemplate = new CloneTemplate(Info, TechType.Workbench);
            CloneTemplate cloneTemplate = new CloneTemplate(Info, TechType.PictureFrame);

            cloneTemplate.ModifyPrefab += obj =>
            {
                // find the object that holds the model:
                //GameObject model = obj.transform.Find("model/submarine_Workbench/workbench_geo").gameObject;
                GameObject model = obj.transform.Find("mesh/submarine_Picture_Frame").gameObject;
                ConstructableFlags constructableFlags = ConstructableFlags.Inside | ConstructableFlags.Wall | ConstructableFlags.Submarine;
                //obj.GetComponentsInChildren<Renderer>().ForEach(r => r.materials.ForEach(m => m.color = Color.red));
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
                while(!VehicleManager.isWorldLoaded)
                {
                    yield return null;
                }
                GetComponent<PictureFrame>().enabled = false;
                transform.Find("Trigger").gameObject.SetActive(false);
                transform.Find("mesh/submarine_Picture_Frame/submarine_Picture_Frame_button").gameObject.AddComponent<BoxCollider>();
                gameObject.AddComponent<BoxCollider>();
                PairWithDrone(FindNearestUnpairedDrone());
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
                pairedDrone = VehicleManager.VehiclesInPlay.Where(x => x as Drone != null).Where(x => (x as IDroneInterface).IsInPairingModeAsInitiator()).First() as Drone;
                pairedDrone.pairedStation = this;
                (this as IDroneInterface).FinalizePairingMode();
            }
            else
            {
                (this as IDroneInterface).InitiatePairingMode();
            }
        }
        public void HandleRemoteControlClick()
        {

        }


        void IHandTarget.OnHandClick(GUIHand hand)
        {
            Targeting.GetTarget(Player.main.gameObject, 6f, out GameObject target, out float num);
            Logger.Log(target.name);

            if (target.name.Contains("DroneStation"))
            {
                HandleRemoteControlClick();
            }
            else
            {
                HandlePairingClick();
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
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Confirm Pairing");
            }
            else
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Enter Pairing Mode");
            }
        }
        public void HandleRemoteControlHover()
        {
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "...");
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
        public void RemoteControl()
        {
            pairedDrone.BeginControlling();
        }
        public void PairWithDrone(Drone drone)
        {
            pairedDrone = drone;
        }
        public void Unpair()
        {
            pairedDrone = null;
        }
        void IDroneInterface.InitiatePairingMode()
        {
            isInitiator = true;
            VehicleManager.VehiclesInPlay.Where(x => x as Drone != null).ForEach(x => (x as IDroneInterface).RespondWithPairingMode());
            Admin.GameObjectManager<DroneStation>.AllSuchObjects.Where(x => x != this).ForEach(x => (x as IDroneInterface).ExitPairingMode());
        }
        void IDroneInterface.FinalizePairingMode()
        {
            Admin.GameObjectManager<DroneStation>.AllSuchObjects.ForEach(x => (x as IDroneInterface).ExitPairingMode());
            VehicleManager.VehiclesInPlay.Where(x => x as Drone != null).ForEach(x => (x as IDroneInterface).ExitPairingMode());
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
