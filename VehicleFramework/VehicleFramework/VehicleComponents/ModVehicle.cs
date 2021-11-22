using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Engines;

namespace VehicleFramework
{
    public abstract class ModVehicle : Vehicle, ICraftTarget
        {
            public override string vehicleDefaultName
            {
                get
                {
                    Language main = Language.main;
                    if (main == null)
                    {
                        return "VEHICLE";
                    }
                    return main.Get("VehicleDefaultName");
                }
            }

        // lists of game object references, used like a blueprint by VehicleBuilder
        public abstract GameObject VehicleModel { get; }
        public abstract GameObject CollisionModel { get; }
        public abstract GameObject StorageRootObject { get; }
        public abstract GameObject ModulesRootObject { get; }
        public abstract List<VehicleParts.VehiclePilotSeat> PilotSeats { get; }
        public abstract List<VehicleParts.VehicleHatchStruct> Hatches { get; }
        public abstract List<VehicleParts.VehicleStorage> ModularStorages { get; }
        public abstract List<VehicleParts.VehicleStorage> InnateStorages { get; }
        public abstract VehicleParts.VehicleUpgrades Upgrades { get; }
        public abstract List<VehicleParts.VehicleBattery> Batteries { get; }
        public abstract VehicleParts.VehicleBattery AutopilotBattery { get; }
        public abstract List<VehicleParts.VehicleFloodLight> HeadLights { get; }
        public abstract List<VehicleParts.VehicleFloodLight> FloodLights { get; }
        public abstract List<GameObject> NavigationPortLights { get; }
        public abstract List<GameObject> NavigationStarboardLights { get; }
        public abstract List<GameObject> NavigationPositionLights { get; }
        public abstract List<GameObject> NavigationWhiteStrobeLights { get; }
        public abstract List<GameObject> NavigationRedStrobeLights { get; }
        public abstract List<GameObject> WaterClipProxies { get; }
        public abstract List<GameObject> CanopyWindows { get; }
        public abstract GameObject BoundingBox { get; }
        public abstract List<GameObject> NameDecals { get; }
        public abstract List<GameObject> TetherSources { get; }
        public abstract GameObject ControlPanel { get; }
        public ControlPanel controlPanelLogic;
        public Transform thisStopPilotingLocation;
        public FloodLightsController floodlights;
        public InteriorLightsController interiorlights;
        public NavigationLightsController navlights;

        internal FMOD_CustomEmitter lightsOnSound = null;
        internal FMOD_CustomEmitter lightsOffSound = null;
        internal List<GameObject> lights = new List<GameObject>();
        internal List<GameObject> volumetricLights = new List<GameObject>();
        internal PingInstance pingInstance = null;
        internal FMOD_StudioEventEmitter ambienceSound;
        protected bool isPilotSeated = false;
        protected bool isPlayerInside = false;
        internal int numEfficiencyModules = 0;
        internal int numArmorModules = 0;
        internal ModVehicleEngine engine;
        internal HeadLightsController headlights;
        // if the player toggles the power off,
        // the vehicle is called "disgengaged,"
        // because it is unusable yet the batteries are not empty
        internal bool isPoweredOn = true;
        internal bool isRegistered = false;
        internal EnergyInterface AIEnergyInterface;
        internal int numVehicleModules;
        internal bool hasArms;
        internal AutoPilotVoice voice;
        internal PowerManager powerMan;
        internal GameObject fabricator = null;

        public override void Awake()
        {
            void MaybeSetupUniqueFabricator()
            {
                Transform fabLoc = transform.Find("Fabricator-Location");
                if (fabLoc == null)
                {
                    Logger.Log("Warning: " + name + " does not have a Fabricator-Location.");
                    return;
                }
                foreach (var thisOldFab in GetComponentsInChildren<Fabricator>())
                {
                    UnityEngine.GameObject.Destroy(thisOldFab.gameObject);
                }
                fabricator = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Fabricator, true));
                fabricator.transform.SetParent(transform);
                fabricator.transform.localPosition = fabLoc.localPosition;
                fabricator.transform.localRotation = fabLoc.localRotation;
                fabricator.transform.localScale = 0.85f * Vector3.one;
            }
            energyInterface = GetComponent<EnergyInterface>();
            base.Awake();
            gameObject.EnsureComponent<TetherSource>();
            floodlights = gameObject.EnsureComponent<FloodLightsController>();
            headlights = gameObject.EnsureComponent<HeadLightsController>();
            interiorlights = gameObject.EnsureComponent<InteriorLightsController>();
            navlights = gameObject.EnsureComponent<NavigationLightsController>();
            voice = gameObject.EnsureComponent<AutoPilotVoice>();
            gameObject.EnsureComponent<AutoPilot>();

            controlPanelLogic.Init();
            base.LazyInitialize();
            MaybeSetupUniqueFabricator();
        }
        public override void Start()
        {
            base.Start();
            upgradesInput.equipment = modules;
            modules.isAllowedToRemove = new IsAllowedToRemove(IsAllowedToRemove);
            gameObject.EnsureComponent<GameInfoIcon>().techType = GetComponent<TechTag>().type;

            // todo fix pls
            // Not only is the syntax gross,
            // but the decals are inexplicably invisible in-game
            // I'm pretty sure this is the right camera...
            /*
            foreach (Canvas decalCanvas in NameDecals[0].transform.parent.gameObject.GetAllComponentsInChildren<Canvas>())
            {
                decalCanvas.worldCamera = MainCamera.camera;
            }
            */

            powerMan = gameObject.EnsureComponent<PowerManager>();
            if (!isRegistered)
            {
                VehicleManager.EnrollVehicle(this);
                isRegistered = true;
            }
        }
        public override void FixedUpdate()
        {
            if (worldForces.IsAboveWater() != wasAboveWater)
            {
                PlaySplashSound();
                wasAboveWater = worldForces.IsAboveWater();
            }
            if (stabilizeRoll)
            {
                StabilizeRoll();
            }
            prevVelocity = useRigidbody.velocity;
        }
        public override void Update()
        {
            base.Update();
        }
        public new void OnKill()
        {
            if (destructionEffect)
            {
                GameObject gameObject = Instantiate<GameObject>(destructionEffect);
                gameObject.transform.position = transform.position;
                gameObject.transform.rotation = transform.rotation;
            }
            // spill out some scrap metal, lmao
            for (int i = 0; i < 4; i++)
            {
                Vector3 loc = transform.position + 3 * UnityEngine.Random.onUnitSphere;
                Vector3 rot = 360 * UnityEngine.Random.onUnitSphere;
                GameObject go = UnityEngine.GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.ScrapMetal, true));
                go.transform.position = loc;
                go.transform.eulerAngles = rot;
                var rb = go.EnsureComponent<Rigidbody>();
                rb.isKinematic = false;
            }
            StartCoroutine(EnqueueDestroy());
        }
        public IEnumerator EnqueueDestroy()
        {
            if (IsPlayerPiloting())
            {
                Player.main.playerController.SetEnabled(true);
                Player.main.mode = Player.Mode.Normal;
                Player.main.playerModeChanged.Trigger(Player.main.mode);
                Player.main.sitting = false;
                Player.main.playerController.ForceControllerSize();
                Player.main.transform.parent = null;
                StopPiloting();
            }
            if (IsPlayerInside())
            {
                PlayerExit();
            }
            Destroy(gameObject);
            yield return null;
        }
        public override void OnUpgradeModuleChange(int slotID, TechType techType, bool added)
        {
            switch (techType)
            {
                case TechType.VehicleStorageModule:
                    {
                        SetStorageModule(slotID, added);
                        break;
                    }
                case TechType.VehicleArmorPlating:
                    {
                        _ = added ? numArmorModules++ : numArmorModules--;
                        GetComponent<DealDamageOnImpact>().mirroredSelfDamageFraction = 0.5f * Mathf.Pow(0.5f, (float)numArmorModules);
                        break;
                    }
                case TechType.VehiclePowerUpgradeModule:
                    {
                        _ = added ? numEfficiencyModules++ : numEfficiencyModules--;
                        break;
                    }
                default:
                    break;
            }
            StartCoroutine(EvaluateDepthModuleLevel());
        }
        public virtual IEnumerator EvaluateDepthModuleLevel()
        {
            // honestly I just do this to ensure the module is well-and-gone if we just removed one,
            // since this gets called on module-remove and on module-add
            yield return new WaitForSeconds(1);

            // Iterate over all upgrade modules,
            // in order to determine our max depth module level
            int maxDepthModuleLevel = 0;
            List<string> upgradeSlots = new List<string>();
            upgradesInput.equipment.GetSlots(VehicleBuilder.ModuleType, upgradeSlots);
            foreach (String slot in upgradeSlots)
            {
                InventoryItem upgrade = upgradesInput.equipment.GetItemInSlot(slot);
                if (upgrade != null)
                {
                    //Logger.Log(slot + " : " + upgrade.item.name);
                    if (upgrade.item.name == "ModVehicleDepthModule1(Clone)")
                    {
                        if (maxDepthModuleLevel < 1)
                        {
                            maxDepthModuleLevel = 1;
                        }
                    }
                    else if (upgrade.item.name == "ModVehicleDepthModule2(Clone)")
                    {
                        if (maxDepthModuleLevel < 2)
                        {
                            maxDepthModuleLevel = 2;
                        }
                    }
                    else if (upgrade.item.name == "ModVehicleDepthModule3(Clone)")
                    {
                        if (maxDepthModuleLevel < 3)
                        {
                            maxDepthModuleLevel = 3;
                        }
                    }
                }
            }
            GetComponent<CrushDamage>().SetExtraCrushDepth(maxDepthModuleLevel * 300);
        }
        public override void OnCollisionEnter(Collision col)
        {
            base.OnCollisionEnter(col);
        }
        public override void OnPilotModeBegin()
        {
            base.OnPilotModeBegin();
        }
        public bool IsPlayerInside()
        {
            return isPlayerInside;
        }
        public bool IsPlayerPiloting()
        {
            return isPilotSeated;
        }
        public override bool CanPilot()
        {
            return !FPSInputModule.current.lockMovement && IsPowered();
        }
        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            if (pickupable.GetTechType() == TechType.VehicleStorageModule)
            {
                // check the appropriate storage module for emptiness
                SeamothStorageContainer component = pickupable.GetComponent<SeamothStorageContainer>();
                if (component != null)
                {
                    bool flag = component.container.count == 0;
                    if (verbose && !flag)
                    {
                        ErrorMessage.AddDebug(Language.main.Get("SeamothStorageNotEmpty"));
                    }
                    return flag;
                }
                Debug.LogError("No VehicleStorageContainer found on VehicleStorageModule item");
            }
            return true;
        }

        public override void EnterVehicle(Player player, bool teleport, bool playEnterAnimation = true)
        {
            base.EnterVehicle(player, teleport, playEnterAnimation);
        }
        private IEnumerator SitDownInChair()
        {
            Player.main.playerAnimator.SetBool("chair_sit", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_sit", false);
            foreach(var seat in PilotSeats)
            {
                // disappear the chair
                seat.Seat.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        private IEnumerator StandUpFromChair()
        {
            Player.main.playerAnimator.SetBool("chair_stand_up", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_stand_up", false);
            foreach (var seat in PilotSeats)
            {
                // re-appear the chair
                seat.Seat.GetComponent<MeshRenderer>().enabled = true;
            }
        }
        private IEnumerator TryStandUpFromChair()
        {
            while (IsPlayerPiloting())
            {
                yield return new WaitForSeconds(1);
            }
            yield return new WaitForSeconds(2);
            Player.main.playerAnimator.SetBool("chair_stand_up", true);
        }
        public void BeginPiloting()
        {
            Player.main.EnterSittingMode();
            StartCoroutine(SitDownInChair());
            StartCoroutine(TryStandUpFromChair());
            base.EnterVehicle(Player.main, true);
            isPilotSeated = true;
            uGUI.main.quickSlots.SetTarget(this);
            NotifyStatus(PlayerStatus.OnPilotBegin);
        }
        public virtual void StopPiloting()
        {
            // this function
            // called by Player.ExitLockedMode()
            // which is triggered on button press
            StartCoroutine(StandUpFromChair());
            isPilotSeated = false;
            Player.main.transform.SetParent(transform);
            if (thisStopPilotingLocation == null)
            {
                Logger.Log("Warning: pilot exit location was null. Defaulting to first tether.");
                Player.main.transform.position = TetherSources[0].transform.position;
            }
            else
            {
                Player.main.transform.position = thisStopPilotingLocation.position;
            }
            Player.main.SetScubaMaskActive(false);
            uGUI.main.quickSlots.SetTarget(null);
            NotifyStatus(PlayerStatus.OnPilotEnd);
        }
        public virtual void PlayerEntry()
        {
            Player.main.currentSub = null;
            isPlayerInside = true;
            Player.main.currentMountedVehicle = this;
            Player.main.transform.SetParent(transform);
            Player.main.playerController.activeController.SetUnderWater(false);
            Player.main.isUnderwater.Update(false);
            Player.main.isUnderwaterForSwimming.Update(false);
            Player.main.playerController.SetMotorMode(Player.MotorMode.Walk);
            Player.main.motorMode = Player.MotorMode.Walk;
            Player.main.SetScubaMaskActive(false);
            Player.main.playerMotorModeChanged.Trigger(Player.MotorMode.Walk);
            foreach (GameObject window in CanopyWindows)
            {
                window.SetActive(false);
            }
            Player.main.lastValidSub = GetComponent<SubRoot>();
            NotifyStatus(PlayerStatus.OnPlayerEntry);
        }
        public void PlayerExit()
        {
            Player.main.currentSub = null;
            isPlayerInside = false;
            Player.main.currentMountedVehicle = null;
            Player.main.transform.SetParent(null);
            foreach (GameObject window in CanopyWindows)
            {
                window.SetActive(true);
            }
            NotifyStatus(PlayerStatus.OnPlayerExit);
        }
        public override void SetPlayerInside(bool inside)
        {
            base.SetPlayerInside(inside);
            Player.main.inSeamoth = inside;
        }
        public void GetHUDValues(out float health, out float power)
        {
            health = this.liveMixin.GetHealthFraction();
            float num;
            float num2;
            base.GetEnergyValues(out num, out num2);
            power = ((num > 0f && num2 > 0f) ? (num / num2) : 0f);
        }
        public override void GetDepth(out int depth, out int crushDepth)
        {
            depth = Mathf.FloorToInt(GetComponent<CrushDamage>().GetDepth());
            crushDepth = Mathf.FloorToInt(GetComponent<CrushDamage>().crushDepth);
        }
        public virtual string GetDescription()
        {
            return "";
        }
        private string[] _slotIDs = null;
		public override string[] slotIDs
		{
			get
			{
                if (_slotIDs == null)
                {
                    _slotIDs = GenerateSlotIDs(numVehicleModules, hasArms);
                }
				return _slotIDs;
			}
        }
        private static string[] GenerateSlotIDs(int modules, bool arms)
        {
            int numModules = arms ? modules + 2 : modules;
            string[] retIDs = new string[numModules];
            for(int i=0; i<modules; i++)
            {
                retIDs[i] = "VehicleModule" + i.ToString();
            }
            if(arms)
            {
                retIDs[modules] = "VehicleArmLeft";
                retIDs[modules + 1] = "VehicleArmRight";
            }
            return retIDs;
        }
        /*
        public override QuickSlotType GetQuickSlotType(int slotID, out TechType techType)
        {
            if (slotID >= 0 && slotID < this.slotIDs.Length)
            {
                techType = upgradesEquipment.GetTechTypeInSlot(this.slotIDs[slotID]);
                if (techType != TechType.None)
                {
                    return CraftData.GetQuickSlotType(techType);
                }
            }
            techType = TechType.None;
            return QuickSlotType.None;
        }
        public override TechType[] GetSlotBinding()
        {
            int num = this.slotIDs.Length;
            TechType[] array = new TechType[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = upgradesEquipment.GetTechTypeInSlot(this.slotIDs[i]);
            }
            return array;
        }
        public override TechType GetSlotBinding(int slotID)
        {
            if (slotID < 0 || slotID >= this.slotIDs.Length)
            {
                return TechType.None;
            }
            string slot = this.slotIDs[slotID];
            return upgradesEquipment.GetTechTypeInSlot(slot);
        }
        public override int GetSlotByItem(InventoryItem item)
        {
            if (item != null)
            {
                int i = 0;
                int num = this.slotIDs.Length;
                while (i < num)
                {
                    if (upgradesEquipment.GetItemInSlot(this.slotIDs[i]) == item)
                    {
                        return i;
                    }
                    i++;
                }
            }
            return -1;
        }


        */
        private void SetStorageModule(int slotID, bool activated)
        {
            foreach(var sto in InnateStorages)
            {
                sto.Container.SetActive(true);
            }
            ModularStorages[slotID].Container.SetActive(activated);
            //ModularStorages[slotID].Container.GetComponent<BoxCollider>().enabled = activated;
        }
        public override InventoryItem GetSlotItem(int slotID)
        {
            if (slotID < 0 || slotID >= this.slotIDs.Length)
            {
                return null;
            }
            string slot = this.slotIDs[slotID];

            InventoryItem result;
            if (upgradesInput.equipment.equipment.TryGetValue(slot, out result))
            {
                return result;
            }
            return null;
        }
        public ItemsContainer ModGetStorageInSlot(int slotID, TechType techType)
        {
            switch(techType)
            {
                case VehicleBuilder.InnateStorage:
                    {
                        InnateStorageContainer vsc;
                        if(0 <= slotID && slotID < InnateStorages.Count)
                        {
                            vsc = InnateStorages[slotID].Container.GetComponent<InnateStorageContainer>();
                        }
                        else
                        {
                            Logger.Log("Error: ModGetStorageInSlot called on invalid innate storage slotID");
                            return null;
                        }
                        return vsc.container;
                    }
                case TechType.VehicleStorageModule:
                    {
                        InventoryItem slotItem = this.GetSlotItem(slotID);
                        if (slotItem == null)
                        {
                            Logger.Log("Warning: failed to get item for that slotID: " + slotID.ToString());
                            return null;
                        }
                        Pickupable item = slotItem.item;
                        if (item.GetTechType() != techType)
                        {
                            Logger.Log("Warning: failed to get pickupable for that slotID: " + slotID.ToString());
                            return null;
                        }
                        SeamothStorageContainer component = item.GetComponent<SeamothStorageContainer>();
                        if (component == null)
                        {
                            Logger.Log("Warning: failed to get storage-container for that slotID: " + slotID.ToString());
                            return null;
                        }
                        return component.container;
                    }
                default:
                    {
                        Logger.Log("Error: tried to get storage for unsupported TechType");
                        return null;
                    }
            }
        }
        public void TogglePower()
        {
            isPoweredOn = !isPoweredOn;
        }
        public void NotifyStatus(LightsStatus vs)
        {
            foreach (var component in GetComponentsInChildren<ILightsStatusListener>())
            {
                switch (vs)
                {
                    case LightsStatus.OnHeadLightsOn:
                        component.OnHeadLightsOn();
                        break;
                    case LightsStatus.OnHeadLightsOff:
                        component.OnHeadLightsOff();
                        break;
                    case LightsStatus.OnInteriorLightsOn:
                        component.OnInteriorLightsOn();
                        break;
                    case LightsStatus.OnInteriorLightsOff:
                        component.OnInteriorLightsOff();
                        break;
                    case LightsStatus.OnFloodLightsOn:
                        component.OnFloodLightsOn();
                        break;
                    case LightsStatus.OnFloodLightsOff:
                        component.OnFloodLightsOff();
                        break;
                    case LightsStatus.OnNavLightsOn:
                        component.OnNavLightsOn();
                        break;
                    case LightsStatus.OnNavLightsOff:
                        component.OnNavLightsOff();
                        break;
                    default:
                        Logger.Log("Error: tried to notify using an invalid status");
                        break;
                }
            }
        }
        public void NotifyStatus(AutoPilotStatus vs)
        {
            foreach (var component in GetComponentsInChildren<IAutoPilotListener>())
            {
                switch (vs)
                {
                    case AutoPilotStatus.OnAutoLevelBegin:
                        component.OnAutoLevelBegin();
                        break;
                    case AutoPilotStatus.OnAutoLevelEnd:
                        component.OnAutoLevelEnd();
                        break;
                    case AutoPilotStatus.OnAutoPilotBegin:
                        component.OnAutoPilotBegin();
                        break;
                    case AutoPilotStatus.OnAutoPilotEnd:
                        component.OnAutoPilotEnd();
                        break;
                    default:
                        Logger.Log("Error: tried to notify using an invalid status");
                        break;
                }
            }
        }
        public void NotifyStatus(VehicleStatus vs)
        {
            foreach (var component in GetComponentsInChildren<IVehicleStatusListener>())
            {
                switch (vs)
                {
                    case VehicleStatus.OnTakeDamage:
                        component.OnTakeDamage();
                        break;
                    case VehicleStatus.OnNearbyLeviathan:
                        component.OnNearbyLeviathan();
                        break;
                    default:
                        Logger.Log("Error: tried to notify using an invalid status");
                        break;
                }
            }
        }
        public void NotifyStatus(PowerEvent vs)
        {
            foreach (var component in GetComponentsInChildren<IPowerListener>())
            {
                switch (vs)
                {
                    case PowerEvent.OnPowerUp:
                        component.OnPowerUp();
                        break;
                    case PowerEvent.OnPowerDown:
                        component.OnPowerDown();
                        break;
                    case PowerEvent.OnBatteryDead:
                        component.OnBatteryDead();
                        break;
                    case PowerEvent.OnBatteryRevive:
                        component.OnBatteryRevive();
                        break;
                    case PowerEvent.OnBatterySafe:
                        component.OnBatterySafe();
                        break;
                    case PowerEvent.OnBatteryLow:
                        component.OnBatteryLow();
                        break;
                    case PowerEvent.OnBatteryNearlyEmpty:
                        component.OnBatteryNearlyEmpty();
                        break;
                    case PowerEvent.OnBatteryDepleted:
                        component.OnBatteryDepleted();
                        break;
                    default:
                        Logger.Log("Error: tried to notify using an invalid status");
                        break;
                }
            }
        }
        public void NotifyStatus(PlayerStatus vs)
        {
            foreach (var component in GetComponentsInChildren<IPlayerListener>())
            {
                switch (vs)
                {
                    case PlayerStatus.OnPlayerEntry:
                        component.OnPlayerEntry();
                        break;
                    case PlayerStatus.OnPlayerExit:
                        component.OnPlayerExit();
                        break;
                    case PlayerStatus.OnPilotBegin:
                        component.OnPilotBegin();
                        break;
                    case PlayerStatus.OnPilotEnd:
                        component.OnPilotEnd();
                        break;
                    default:
                        Logger.Log("Error: tried to notify using an invalid status");
                        break;
                }
            }
        }
        public bool GetIsUnderwater()
        {
            // TODO: justify this constant
            return transform.position.y < 0.75f;
        }
        public static void MaybeControlRotation(Vehicle veh)
        {
            ModVehicle mv = veh as ModVehicle;
            if (mv != null && Player.main.GetVehicle() == veh && Player.main.mode == Player.Mode.LockedPiloting)
            {
                ModVehicleEngine mve = mv.GetComponent<ModVehicleEngine>();
                mve.ControlRotation();
            }
        }
        public void OnCraftEnd(TechType techType)
        {
            StartCoroutine(GiveUsABatteryOrGiveUsDeath());
        }
        private IEnumerator GiveUsABatteryOrGiveUsDeath()
        {
            yield return new WaitForSeconds(2.5f);

            // give us an AI battery please
            GameObject newBattery = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Battery, true));
            newBattery.GetComponent<Battery>().charge = 100;
            newBattery.transform.SetParent(StorageRootObject.transform);
            AIEnergyInterface.sources.First().battery = newBattery.GetComponent<Battery>();
            AIEnergyInterface.sources.First().batterySlot.AddItem(newBattery.GetComponent<Pickupable>());
            newBattery.SetActive(false);

            // ensure we've got at least one power cell
            if (!energyInterface.hasCharge)
            {
                GameObject thisItem = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.PowerCell, true));
                thisItem.GetComponent<Battery>().charge = 200;
                thisItem.transform.SetParent(StorageRootObject.transform);
                Batteries[0].BatterySlot.gameObject.GetComponent<EnergyMixin>().battery = thisItem.GetComponent<Battery>();
                Batteries[0].BatterySlot.gameObject.GetComponent<EnergyMixin>().batterySlot.AddItem(thisItem.GetComponent<Pickupable>());
                thisItem.SetActive(false);
            }
            //GetComponent<InteriorLightsController>().EnableInteriorLighting();
        }
        public void ForceExitLockedMode()
        {
            GameInput.ClearInput();
            Player.main.playerController.SetEnabled(true);
            Player.main.mode = Player.Mode.Normal;
            Player.main.playerModeChanged.Trigger(Player.main.mode);
            Player.main.sitting = false;
            Player.main.playerController.ForceControllerSize();
            Player.main.transform.parent = null;
            StopPiloting();
        }
    }
}
