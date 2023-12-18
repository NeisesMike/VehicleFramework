using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using VehicleFramework.Engines;
using UnityEngine.SceneManagement;

namespace VehicleFramework
{
    /*
     * ModVehicle is the class of self-leveling, walkable submarines
     */
    public abstract class ModVehicle : Vehicle, ICraftTarget
    {
        public override string vehicleDefaultName
            {
                get
                {
                    Language main = Language.main;
                    if (main == null)
                    {
                        return LocalizationManager.GetString(EnglishString.Vehicle);
                    }
                    return main.Get("VehicleDefaultName");
                }
        }
        public new SubName subName = new SubName();

        public abstract GameObject VehicleModel { get; }
        public abstract GameObject CollisionModel { get; }
        public abstract GameObject StorageRootObject { get; }
        public abstract GameObject ModulesRootObject { get; }

        // lists of game object references, used later like a blueprint
        public abstract List<VehicleParts.VehiclePilotSeat> PilotSeats { get; }
        public abstract List<VehicleParts.VehicleHatchStruct> Hatches { get; }
        public abstract List<VehicleParts.VehicleStorage> InnateStorages { get; }
        public abstract List<VehicleParts.VehicleStorage> ModularStorages { get; }
        public abstract List<VehicleParts.VehicleUpgrades> Upgrades { get; }
        public abstract List<VehicleParts.VehicleBattery> Batteries { get; }
        public abstract List<VehicleParts.VehicleBattery> BackupBatteries { get; }
        public abstract List<VehicleParts.VehicleFloodLight> HeadLights { get; }
        public abstract List<VehicleParts.VehicleFloodLight> FloodLights { get; }
        public abstract List<GameObject> NavigationPortLights { get; }
        public abstract List<GameObject> NavigationStarboardLights { get; }
        public abstract List<GameObject> NavigationPositionLights { get; }
        public abstract List<GameObject> NavigationWhiteStrobeLights { get; }
        public abstract List<GameObject> NavigationRedStrobeLights { get; }
        public abstract List<GameObject> WaterClipProxies { get; }
        public abstract List<GameObject> CanopyWindows { get; }
        public abstract List<GameObject> NameDecals { get; }
        public abstract List<GameObject> TetherSources { get; }
        public abstract GameObject BoundingBox { get; }
        public abstract GameObject ControlPanel { get; }
        public virtual GameObject Fabricator { get; }
        public virtual GameObject ColorPicker { get; }
        public virtual GameObject SteeringWheel { get; }
        public virtual GameObject SteeringWheelLeftHandTarget { get; }
        public virtual GameObject SteeringWheelRightHandTarget { get; }
        public ControlPanel controlPanelLogic;


        public FMOD_CustomEmitter lightsOnSound = null;
        public FMOD_CustomEmitter lightsOffSound = null;
        public List<GameObject> lights = new List<GameObject>();
        public List<GameObject> volumetricLights = new List<GameObject>();
        public PingInstance pingInstance = null;
        public FMOD_StudioEventEmitter ambienceSound;

        protected bool isPilotSeated = false;
        protected bool isPlayerInside = false;

        // TODO
        // These are tracked appropriately, but their values are never used for anything meaningful.
        public int numEfficiencyModules = 0;
        private int numArmorModules = 0;

        // if the player toggles the power off,
        // the vehicle is called "disgengaged,"
        // because it is unusable yet the batteries are not empty
        public bool isPoweredOn = true;

        public ModVehicleEngine engine;
        public Transform thisStopPilotingLocation;

        public FloodLightsController floodlights;
        public HeadLightsController headlights;
        public InteriorLightsController interiorlights;
        public NavigationLightsController navlights;

        public bool isRegistered = false;

        public EnergyInterface AIEnergyInterface;

        public int numVehicleModules;
        public bool hasArms;

        public AutoPilotVoice voice;

        public bool isInited = false;

        // later
        public virtual List<GameObject> Arms => null;
        public virtual List<GameObject> Legs => null;

        // not sure what types these should be
        public virtual List<GameObject> SoundEffects => null;
        public virtual List<GameObject> TwoDeeAssets => null;

        internal GameObject fab = null; //fabricator
        internal PowerManager powerMan = null;

        private void TryRemoveDuplicateFabricator()
        {
            bool foundOne = false;
            foreach (Transform tran in transform)
            {
                if (tran.gameObject.name == "Fabricator(Clone)")
                {
                    if(foundOne)
                    {
                        UnityEngine.Object.Destroy(tran.gameObject);
                        continue;
                    }
                    foundOne = true;
                }
            }
        }
        public override void Awake()
        {
            IEnumerator TrySpawnFabricator()
            {
                Transform fabLoc = Fabricator.transform;
                if (fabLoc is null)
                {
                    fabLoc = transform.Find("Fabricator-Location");
                    if (fabLoc is null)
                    {
                        Logger.Warn("Warning: " + name + " does not have a Fabricator-Location.");
                        yield break;
                    }
                }

                TaskResult<GameObject> result = new TaskResult<GameObject>();
                yield return StartCoroutine(CraftData.InstantiateFromPrefabAsync(TechType.Fabricator, result, false));
                fab = result.Get();
                fab.GetComponent<SkyApplier>().enabled = true;
                fab.transform.SetParent(transform);
                fab.transform.localPosition = fabLoc.localPosition;
                fab.transform.localRotation = fabLoc.localRotation;
                if(fabLoc.transform.localScale.x == 0 || fabLoc.transform.localScale.y == 0 || fabLoc.transform.localScale.z == 0)
                {
                    fabLoc.transform.localScale = Vector3.one;
                }
                fab.transform.localScale = 0.85f * fabLoc.localScale;
                yield break;
            }

            energyInterface = GetComponent<EnergyInterface>();
            base.Awake();

            floodlights = gameObject.EnsureComponent<FloodLightsController>();
            headlights = gameObject.EnsureComponent<HeadLightsController>();
            interiorlights = gameObject.EnsureComponent<InteriorLightsController>();
            navlights = gameObject.EnsureComponent<NavigationLightsController>();

            //if(!(this is Submersible))
            //{
            gameObject.EnsureComponent<TetherSource>();
            voice = gameObject.EnsureComponent<AutoPilotVoice>();
            gameObject.EnsureComponent<AutoPilot>();
            controlPanelLogic.Init();
            StartCoroutine(TrySpawnFabricator());
            //}

            upgradeOnAddedActions.Add(storageModuleAction);
            upgradeOnAddedActions.Add(armorPlatingModuleAction);
            upgradeOnAddedActions.Add(powerUpgradeModuleAction);

            // perform normal vehicle lazyinitializing
            base.LazyInitialize();
        }
        public override void Start()
        {
            base.Start();


            upgradesInput.equipment = modules;
            modules.isAllowedToRemove = new IsAllowedToRemove(IsAllowedToRemove);

            // lost this in the update to Nautilus. We're no longer tracking our own tech type IDs or anything,
            // so I'm not able to provide the value easily here. Not even sure what a GameInfoIcon is :shrug:
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

            // now that we're in-game, load the color picker
            // we can't do this before we're in-game because not all assets are ready before the game is started
            if (!(ColorPicker is null))
            {
                if (ColorPicker.transform.Find("EditScreen") is null)
                {
                    StartCoroutine(SetupColorPicker());
                }
                else
                {
                    ActualEditScreen = ColorPicker.transform.Find("EditScreen").gameObject;
                }
            }

            // Ensure our name is still good
            vehicleName = OGVehicleName;
            NowVehicleName = OGVehicleName;

            // Register our new vehicle with Vehicle Framework
            VehicleManager.EnrollVehicle(this);
            isInited = true;
            voice.NotifyReadyToSpeak();
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
            HandleExtraQuickSlotInputs();
        }

        public new void OnKill()
        {
            IEnumerator SpillScrapMetal()
            {
                TaskResult<GameObject> result = new TaskResult<GameObject>();
                yield return CraftData.InstantiateFromPrefabAsync(TechType.ScrapMetal, result, false);
                GameObject go = result.Get();
                // spill out some scrap metal, lmao
                for (int i = 0; i < 4; i++)
                {
                    Vector3 loc = transform.position + 3 * UnityEngine.Random.onUnitSphere;
                    Vector3 rot = 360 * UnityEngine.Random.onUnitSphere;
                    go.transform.position = loc;
                    go.transform.eulerAngles = rot;
                    var rb = go.EnsureComponent<Rigidbody>();
                    rb.isKinematic = false;
                }
                yield break;
            }

            if (destructionEffect)
            {
                GameObject gameObject = Instantiate<GameObject>(destructionEffect);
                gameObject.transform.position = transform.position;
                gameObject.transform.rotation = transform.rotation;
            }

            StartCoroutine(SpillScrapMetal());
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
            //yield return new WaitForSeconds(1f);
            if (IsPlayerInside())
            {
                PlayerExit();
            }
            //yield return new WaitForSeconds(1f);
            Destroy(gameObject);
            yield return null;
        }
        public void storageModuleAction(int slotID, TechType techType, bool added)
        {
            if (techType == TechType.VehicleStorageModule)
            {
                SetStorageModule(slotID, added);
            }
        }
        public void armorPlatingModuleAction(int slotID, TechType techType, bool added)
        {
            if (techType == TechType.VehicleArmorPlating)
            {
                _ = added ? numArmorModules++ : numArmorModules--;
                GetComponent<DealDamageOnImpact>().mirroredSelfDamageFraction = 0.5f * Mathf.Pow(0.5f, (float)numArmorModules);
            }
        }
        public void powerUpgradeModuleAction(int slotID, TechType techType, bool added)
        {
            if (techType == TechType.VehiclePowerUpgradeModule)
            {
                _ = added ? numEfficiencyModules++ : numEfficiencyModules--;
            }
        }
        public List<Action<int, TechType, bool>> upgradeOnAddedActions = new List<Action<int, TechType, bool>>();
        List<string> GetCurrentUpgrades()
        {
            List<string> upgradeSlots = new List<string>();
            upgradesInput.equipment.GetSlots(VehicleBuilder.ModuleType, upgradeSlots);
            return upgradeSlots.GroupBy(x => x).Select(y => y.First()).Where(x => upgradesInput.equipment.GetItemInSlot(x) != null).Select(x => upgradesInput.equipment.GetItemInSlot(x).item.name).ToList();
        }
        public override void OnUpgradeModuleChange(int slotID, TechType techType, bool added)
        {
            upgradeOnAddedActions.ForEach(x => x(slotID, techType, added));
            UpgradeModules.ModulePrepper.upgradeOnAddedActions.ForEach(x => x(this, GetCurrentUpgrades(), slotID, techType, added));
            StartCoroutine(EvaluateDepthModuleLevel());
        }

        private List<Tuple<int, Coroutine>> toggledActions = new List<Tuple<int, Coroutine>>();
        public override void OnUpgradeModuleToggle(int slotID, bool active)
        {
            if (active)
            {
                TechType techType = modules.GetTechTypeInSlot(slotIDs[slotID]);
                var moduleToggleAction = UpgradeModules.ModulePrepper.upgradeToggleActions.Where(x => x.Item2 == techType).FirstOrDefault();
                if (moduleToggleAction is null)
                {
                    return;
                }
                IEnumerator DoToggleAction(ModVehicle thisMV, int thisSlotID, TechType tt, float timeToFirstActivation, float repeatRate, float energyCostPerActivation)
                {
                    yield return new WaitForSeconds(timeToFirstActivation);
                    while (true)
                    {
                        if (!thisMV.IsPlayerInside())
                        {
                            ToggleSlot(thisSlotID, false);
                            yield break;
                        }
                        moduleToggleAction.Item1(thisMV, thisSlotID);
                        int whatWeGot = 0;
                        energyInterface.TotalCanProvide(out whatWeGot);
                        if (whatWeGot < energyCostPerActivation)
                        {
                            ToggleSlot(thisSlotID, false);
                            yield break;
                        }
                        energyInterface.ConsumeEnergy(energyCostPerActivation);
                        yield return new WaitForSeconds(repeatRate);
                    }
                }
                toggledActions.Add(new Tuple<int, Coroutine>(slotID, StartCoroutine(DoToggleAction(this, slotID, techType, moduleToggleAction.Item3, moduleToggleAction.Item4, moduleToggleAction.Item5))));
            }
            else
            {
                toggledActions.Where(x => x.Item1 == slotID).Where(x => x.Item2 != null).ToList().ForEach(x => StopCoroutine(x.Item2));
            }
        }
        public override void OnUpgradeModuleUse(TechType techType, int slotID)
        {
            foreach (var moduleUseAction in UpgradeModules.ModulePrepper.upgradeOnUseActions)
            {
                bool result = moduleUseAction.Item1(this, slotID, techType);
                if (result)
                {
                    this.quickSlotTimeUsed[slotID] = Time.time;
                    this.quickSlotCooldown[slotID] = moduleUseAction.Item2;
                    energyInterface.ConsumeEnergy(moduleUseAction.Item3);
                }
            }
            foreach (var moduleUseAction in UpgradeModules.ModulePrepper.upgradeOnUseChargeableActions)
            {
                float charge = quickSlotCharge[slotID];
                float slotCharge = GetSlotCharge(slotID);
                moduleUseAction.Item1(this, slotID, techType, charge, slotCharge);
                energyInterface.ConsumeEnergy(moduleUseAction.Item3);
            }
        }
        public IEnumerator EvaluateDepthModuleLevel()
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
            // this one is correct ?
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
        protected IEnumerator SitDownInChair()
        {
            Player.main.playerAnimator.SetBool("chair_sit", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_sit", false);
        }
        protected IEnumerator StandUpFromChair()
        {
            Player.main.playerAnimator.SetBool("chair_stand_up", true);
            yield return null;
            Player.main.playerAnimator.SetBool("chair_stand_up", false);
        }
        protected IEnumerator TryStandUpFromChair()
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
            base.EnterVehicle(Player.main, true);
            Player.main.EnterSittingMode();
            StartCoroutine(SitDownInChair());
            StartCoroutine(TryStandUpFromChair());
            isPilotSeated = true;
            uGUI.main.quickSlots.SetTarget(this);
            Player.main.armsController.ikToggleTime = 0;
            Player.main.armsController.SetWorldIKTarget(SteeringWheelLeftHandTarget?.transform, SteeringWheelRightHandTarget?.transform);
            NotifyStatus(PlayerStatus.OnPilotBegin);
        }
        public void StopPiloting()
        {
            // this function
            // called by Player.ExitLockedMode()
            // which is triggered on button press
            StartCoroutine(StandUpFromChair());
            isPilotSeated = false;
            Player.main.transform.SetParent(transform);
            if (thisStopPilotingLocation == null)
            {
                Logger.Warn("Warning: pilot exit location was null. Defaulting to first tether.");
                Player.main.transform.position = TetherSources[0].transform.position;
            }
            else
            {
                Player.main.transform.position = thisStopPilotingLocation.position;
            }
            Player.main.SetScubaMaskActive(false);
            uGUI.main.quickSlots.SetTarget(null);
            Player.main.armsController.ikToggleTime = 0.5f;
            Player.main.armsController.SetWorldIKTarget(null, null);
            NotifyStatus(PlayerStatus.OnPilotEnd);
        }
        public void PlayerEntry()
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

            TryRemoveDuplicateFabricator();
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
        public abstract string GetDescription();
        public abstract string GetEncyEntry();
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
        void HandleExtraQuickSlotInputs()
        {
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SlotKeyDown(5);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SlotKeyDown(6);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SlotKeyDown(7);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                SlotKeyDown(8);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                SlotKeyDown(9);
            }
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
            if(ModularStorages is null)
            {
                return;
            }
            if(ModularStorages.Count <= slotID)
            {
                Logger.Output("There is no storage expansion for slot ID: " + slotID.ToString());
                return;
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
                            Logger.Error("Error: ModGetStorageInSlot called on invalid innate storage slotID");
                            return null;
                        }
                        return vsc.container;
                    }
                case TechType.VehicleStorageModule:
                    {
                        InventoryItem slotItem = this.GetSlotItem(slotID);
                        if (slotItem == null)
                        {
                            Logger.Warn("Warning: failed to get item for that slotID: " + slotID.ToString());
                            return null;
                        }
                        Pickupable item = slotItem.item;
                        if (item.GetTechType() != techType)
                        {
                            Logger.Warn("Warning: failed to get pickupable for that slotID: " + slotID.ToString());
                            return null;
                        }
                        SeamothStorageContainer component = item.GetComponent<SeamothStorageContainer>();
                        if (component == null)
                        {
                            Logger.Warn("Warning: failed to get storage-container for that slotID: " + slotID.ToString());
                            return null;
                        }
                        return component.container;
                    }
                default:
                    {
                        Logger.Error("Error: tried to get storage for unsupported TechType");
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
                        Logger.Error("Error: tried to notify using an invalid status");
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
                        Logger.Error("Error: tried to notify using an invalid status");
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
                        Logger.Error("Error: tried to notify using an invalid status");
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
                        Logger.Error("Error: tried to notify using an invalid status");
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
                        Logger.Error("Error: tried to notify using an invalid status");
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
            if(Player.main.GetVehicle() != veh)
            {
                return;
            }
            ModVehicle mv = veh as ModVehicle;
            if (mv is null)
            {
                return;
            }
            if (Player.main.mode != Player.Mode.LockedPiloting)
            {
                return;
            }
            if (!mv.GetIsUnderwater())
            {
                return;
            }
            ModVehicleEngine mve = mv.GetComponent<ModVehicleEngine>();
            mve.ControlRotation();
        }
        public void OnCraftEnd(TechType techType)
        {
            Logger.DebugLog("ModVehicle OnCraftEnd");
            IEnumerator GiveUsABatteryOrGiveUsDeath()
            {
                yield return new WaitForSeconds(2.5f);

                // give us an AI battery please
                TaskResult<GameObject> result = new TaskResult<GameObject>();
                yield return CraftData.InstantiateFromPrefabAsync(TechType.PowerCell, result, false);
                GameObject newAIBattery = result.Get();
                newAIBattery.GetComponent<Battery>().charge = 200;
                newAIBattery.transform.SetParent(StorageRootObject.transform);
                if (AIEnergyInterface)
                {
                    AIEnergyInterface.sources.First().battery = newAIBattery.GetComponent<Battery>();
                    AIEnergyInterface.sources.First().batterySlot.AddItem(newAIBattery.GetComponent<Pickupable>());
                    newAIBattery.SetActive(false);
                }
                if (!energyInterface.hasCharge)
                {
                    yield return CraftData.InstantiateFromPrefabAsync(TechType.PowerCell, result, false);
                    GameObject newPowerCell = result.Get();
                    newPowerCell.GetComponent<Battery>().charge = 200;
                    newPowerCell.transform.SetParent(StorageRootObject.transform);
                    Batteries[0].BatterySlot.gameObject.GetComponent<EnergyMixin>().battery = newPowerCell.GetComponent<Battery>();
                    Batteries[0].BatterySlot.gameObject.GetComponent<EnergyMixin>().batterySlot.AddItem(newPowerCell.GetComponent<Pickupable>());
                    newPowerCell.SetActive(false);
                }
                //GetComponent<InteriorLightsController>().EnableInteriorLighting();
            }
            StartCoroutine(GiveUsABatteryOrGiveUsDeath());
        }
        public virtual void SubConstructionBeginning()
        {
            PaintVehicleDefaultStyle(OGVehicleName);
        }
        public override void SubConstructionComplete()
        {
            Logger.DebugLog("ModVehicle SubConstructionComplete");
            PaintNameDefaultStyle(OGVehicleName);
            // Setup the color picker with the odyssey's name
            var active = transform.Find("ColorPicker/EditScreen/Active");
            if (active)
            {
                active.transform.Find("InputField").GetComponent<uGUI_InputField>().text = NowVehicleName;
                active.transform.Find("InputField/Text").GetComponent<TMPro.TextMeshProUGUI>().text = NowVehicleName;
            }
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


        public virtual void PaintNameDefaultStyle(string name)
        {
            OnNameChangeMaybe(name);
        }
        public virtual void PaintVehicleDefaultStyle(string name)
        {
            ExteriorMainColor = Color.white;
            OldExteriorMainColor = Color.white;
            ExteriorPrimaryAccent = Color.blue;
            OldExteriorPrimaryAccent = Color.blue;
            ExteriorSecondaryAccent = Color.grey;
            OldExteriorSecondaryAccent = Color.grey;
            PaintNameDefaultStyle(name);
        }
        public enum TextureDefinition : int
        {
            twice = 4096,
            full = 2048,
            half = 1024
        }
        public virtual void PaintVehicleSection(string materialName, Color col)
        {
        }
        public virtual void PaintVehicleName(string name, Color nameColor, Color hullColor)
        {
            OnNameChangeMaybe(name);
        }





        public Color ExteriorMainColor;
        public Color ExteriorPrimaryAccent;
        public Color ExteriorSecondaryAccent;
        public Color ExteriorNameLabel;
        protected Color OldExteriorMainColor;
        protected Color OldExteriorPrimaryAccent;
        protected Color OldExteriorSecondaryAccent;
        protected Color OldExteriorNameLabel;
        protected string OGVehicleName;
        public string NowVehicleName;
        protected string OldVehicleName;
        public bool IsDefaultTexture = true;
        public virtual void SetColorPickerUIColor(string name, Color col)
        {
            ActualEditScreen.transform.Find("Active/"+ name +"/SelectedColor").GetComponent<Image>().color = col;
        }
        public virtual void OnColorChange(ColorChangeEventData eventData)
        {
            // determine which tab is selected
            // call the desired function
            List<string> tabnames = new List<string>() { "MainExterior", "PrimaryAccent", "SecondaryAccent", "NameLabel" };
            string selectedTab = "";
            foreach(string tab in tabnames)
            {
                if (ActualEditScreen.transform.Find("Active/" + tab + "/Background").gameObject.activeSelf)
                {
                    selectedTab = tab;
                    break;
                }
            }

            SetColorPickerUIColor(selectedTab, eventData.color);
            switch (selectedTab)
            {
                case "MainExterior":
                    IsDefaultTexture = false;
                    OldExteriorMainColor = ExteriorMainColor;
                    ExteriorMainColor = eventData.color;
                    break;
                case "PrimaryAccent":
                    IsDefaultTexture = false;
                    OldExteriorPrimaryAccent = ExteriorPrimaryAccent;
                    ExteriorPrimaryAccent = eventData.color;
                    break;
                case "SecondaryAccent":
                    IsDefaultTexture = false;
                    OldExteriorSecondaryAccent = ExteriorSecondaryAccent;
                    ExteriorSecondaryAccent = eventData.color;
                    break;
                case "NameLabel":
                    //IsDefaultTexture = false;
                    OldExteriorNameLabel = ExteriorNameLabel;
                    ExteriorNameLabel = eventData.color;
                    break;
                default:
                    break;
            }
            ActualEditScreen.transform.Find("Active/MainExterior/SelectedColor").GetComponent<Image>().color = ExteriorMainColor;
        }
        public virtual void OnNameChangeMaybe(string e)
        {
            if(NowVehicleName != e)
            {
                OldVehicleName = NowVehicleName;
                NowVehicleName = e;
                vehicleName = e;
            }
        }
        public virtual void OnNameChange(string e)
        {
            OldVehicleName = NowVehicleName;
            NowVehicleName = e;
            vehicleName = e;
        }
        public virtual void OnColorSubmit()
        {
            if (ExteriorMainColor != OldExteriorMainColor)
            {
                PaintVehicleSection("ExteriorMainColor", ExteriorMainColor);
            }
            if (ExteriorPrimaryAccent != OldExteriorPrimaryAccent)
            {
                PaintVehicleSection("ExteriorPrimaryAccent", ExteriorPrimaryAccent);
            }
            if (ExteriorSecondaryAccent != OldExteriorSecondaryAccent)
            {
                PaintVehicleSection("ExteriorSecondaryAccent", ExteriorSecondaryAccent);
            }
            if (IsDefaultTexture)
            {
                PaintVehicleDefaultStyle(NowVehicleName);
            }
            else
            {
                PaintVehicleName(NowVehicleName, ExteriorNameLabel, ExteriorMainColor);
            }
            return;
        }

        public GameObject ActualEditScreen = null;

        public IEnumerator SetupColorPicker()
        {
            UnityAction CreateAction(string name)
            {
                void Action()
                {
                    List<string> tabnames = new List<string>() { "MainExterior", "PrimaryAccent", "SecondaryAccent", "NameLabel" };
                    foreach (string tab in tabnames.FindAll(x => x != name))
                    {
                        ActualEditScreen.transform.Find("Active/" + tab + "/Background").gameObject.SetActive(false);
                    }
                    ActualEditScreen.transform.Find("Active/" + name + "/Background").gameObject.SetActive(true);
                }
                return Action;
            }

            GameObject console = Resources.FindObjectsOfTypeAll<BaseUpgradeConsoleGeometry>()?.ToList().Find(x => x.gameObject.name.Contains("Short")).gameObject;

            if (console is null)
            {
                yield return StartCoroutine(Builder.BeginAsync(TechType.BaseUpgradeConsole));
                Builder.ghostModel.GetComponentInChildren<BaseGhost>().OnPlace();
                console = Resources.FindObjectsOfTypeAll<BaseUpgradeConsoleGeometry>().ToList().Find(x => x.gameObject.name.Contains("Short")).gameObject;
                Builder.End();
            }
            ActualEditScreen = GameObject.Instantiate(console.transform.Find("EditScreen").gameObject);
            ActualEditScreen.GetComponentInChildren<SubNameInput>().enabled = false;
            ActualEditScreen.name = "EditScreen";
            ActualEditScreen.SetActive(true);
            ActualEditScreen.transform.Find("Inactive").gameObject.SetActive(false);


            GameObject frame = ColorPicker;
            ActualEditScreen.transform.SetParent(frame.transform);
            ActualEditScreen.transform.localPosition = new Vector3(.15f, .33f, 0f);
            ActualEditScreen.transform.localEulerAngles = new Vector3(0, 180, 0);

            var but = ActualEditScreen.transform.Find("Active/BaseTab");
            but.name = "MainExterior";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LocalizationManager.GetString(EnglishString.MainExterior);
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("MainExterior"));

            but = ActualEditScreen.transform.Find("Active/NameTab");
            but.name = "PrimaryAccent";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LocalizationManager.GetString(EnglishString.PrimaryAccent);
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("PrimaryAccent"));

            but = ActualEditScreen.transform.Find("Active/InteriorTab");
            but.name = "SecondaryAccent";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LocalizationManager.GetString(EnglishString.SecondaryAccent);
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("SecondaryAccent"));

            but = ActualEditScreen.transform.Find("Active/Stripe1Tab");
            but.name = "NameLabel";
            but.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = LocalizationManager.GetString(EnglishString.NameLabel);
            but.gameObject.EnsureComponent<Button>().onClick.AddListener(CreateAction("NameLabel"));

            GameObject colorPicker = ActualEditScreen.transform.Find("Active/ColorPicker").gameObject;
            colorPicker.GetComponentInChildren<uGUI_ColorPicker>().onColorChange.RemoveAllListeners();
            colorPicker.GetComponentInChildren<uGUI_ColorPicker>().onColorChange.AddListener(new UnityAction<ColorChangeEventData>(OnColorChange));
            ActualEditScreen.transform.Find("Active/Button").GetComponent<Button>().onClick.RemoveAllListeners();
            ActualEditScreen.transform.Find("Active/Button").GetComponent<Button>().onClick.AddListener(new UnityAction(OnColorSubmit));
            ActualEditScreen.transform.Find("Active/InputField").GetComponent<uGUI_InputField>().onEndEdit.RemoveAllListeners();
            ActualEditScreen.transform.Find("Active/InputField").GetComponent<uGUI_InputField>().onEndEdit.AddListener(new UnityAction<string>(OnNameChange));
            yield break;
        }

        public virtual void OnAIBatteryReload()
        {
        }

        // this function returns the number of seconds to wait before opening the PDF,
        // to show off the cool animations~
        public virtual float OnStorageOpen(string name, bool open)
        {
            return 0;
        }

        public virtual void ModVehicleReset()
        {
        }
    }
}
