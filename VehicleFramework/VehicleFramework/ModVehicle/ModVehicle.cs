using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using VehicleFramework.Engines;
using VehicleFramework.VehicleComponents;
using VehicleFramework.Assets;
using VehicleFramework.Admin;
using VehicleFramework.VehicleBuilding;
using VehicleFramework.LightControllers;
using VehicleFramework.StorageComponents;
using VehicleFramework.Interfaces;
using VehicleFramework.Extensions;
using VehicleFramework.VehicleTypes;
using VehicleFramework.AutoPilot;

namespace VehicleFramework
{
    /*
     * ModVehicle is the primary abstract class provided by Vehicle Framework.
     * All VF vehicles inherit from ModVehicle.
     */
    public abstract partial class ModVehicle : Vehicle, ICraftTarget, IProtoTreeEventListener
    {
        #region enumerations
        public enum PilotingStyleEnum
        {
            Cyclops,
            Seamoth,
            Prawn,
            Other
        }
        #endregion

        #region vehicle_overrides
        public override void Awake()
        {
            energyInterface = GetComponent<EnergyInterface>();
            base.Awake();
            VehicleManager.EnrollVehicle(this); // Register our new vehicle with Vehicle Framework
            upgradeOnAddedActions.Add(StorageModuleAction);
            upgradeOnAddedActions.Add(ArmorPlatingModuleAction);
            upgradeOnAddedActions.Add(PowerUpgradeModuleAction);

            VehicleBuilder.SetupVolumetricLights(this);
            gameObject.AddComponent<HeadLightsController>();
            gameObject.AddComponent<VolumetricLightController>();

            gameObject.EnsureComponent<AutoPilot.AutoPilot>();
            gameObject.EnsureComponent<AutoPilotVoice>();
            gameObject.EnsureComponent<AutoPilotOxygen>();
            gameObject.EnsureComponent<AutoPilotSignals>();
            gameObject.EnsureComponent<AutoPilotNavigator>();

            if (VFEngine == null)
            {
                VFEngine = GetComponent<VFEngine>();
            }
            VehicleBuilder.SetupCameraController(this);
            LazyInitialize();
            Upgrades?.ForEach(x => x.Interface.GetComponent<VehicleUpgradeConsoleInput>().equipment = modules);
            var warpChipThing = GetComponent("TelePingVehicleInstance");
            if(warpChipThing != null)
            {
                DestroyImmediate(warpChipThing);
            }
            vfxConstructing = GetComponent<VFXConstructing>();
        }
        public override void Start()
        {
            base.Start();

            upgradesInput.equipment = modules;
            modules.isAllowedToRemove = new(IsAllowedToRemove);

            // lost this in the update to Nautilus. We're no longer tracking our own tech type IDs or anything,
            // so I'm not able to provide the value easily here. Not even sure what a GameInfoIcon is :shrug:
            gameObject.EnsureComponent<GameInfoIcon>().techType = TechType;
            GameInfoIcon.Add(TechType);
            isInited = true;
        }
        public override void Update()
        {
            if (isScuttled)
            {
                if(IsVehicleDocked)
                {
                    this.Undock();
                }
                return;
            }
            base.Update();
            HandleExtraQuickSlotInputs();
        }
        public override void FixedUpdate()
        {
            ManagePhysics();
        }
        public new virtual void OnKill()
        {
            liveMixin.health = 0;
            if (IsUnderCommand && Drone.MountedDrone == null)
            {
                Player.main.playerController.SetEnabled(true);
                Player.main.mode = Player.Mode.Normal;
                Player.main.playerModeChanged.Trigger(Player.main.mode);
                Player.main.sitting = false;
                Player.main.playerController.ForceControllerSize();
                Player.main.transform.parent = null;
                StopPiloting();
                PlayerExit();
            }
            DestroyMV();
        }
        public override void OnUpgradeModuleToggle(int slotID, bool active)
        {
            TechType techType = modules.GetTechTypeInSlot(slotIDs[slotID]);
            UpgradeTypes.ToggleActionParams param = new()
            {
                active = active,
                vehicle = this,
                slotID = slotID,
                techType = techType
            };
            UpgradeRegistrar.OnToggleActions.ForEach(x => x(param));
            base.OnUpgradeModuleToggle(slotID, active);
        }
        public override void OnUpgradeModuleUse(TechType techType, int slotID)
        {
            UpgradeTypes.SelectableActionParams param = new()
            {
                vehicle = this,
                slotID = slotID,
                techType = techType
            };
            UpgradeRegistrar.OnSelectActions.ForEach(x => x(param));

            UpgradeTypes.SelectableChargeableActionParams param2 = new()
            {
                vehicle = this,
                slotID = slotID,
                techType = techType,
                charge = param.vehicle.quickSlotCharge[param.slotID],
                slotCharge = param.vehicle.GetSlotCharge(param.slotID)
            };
            UpgradeRegistrar.OnSelectChargeActions.ForEach(x => x(param2));

            Patches.CompatibilityPatches.BetterVehicleStoragePatcher.TryUseBetterVehicleStorage(this, slotID, techType);
            base.OnUpgradeModuleUse(techType, slotID);
        }
        public override void OnUpgradeModuleChange(int slotID, TechType techType, bool added)
        {
            upgradeOnAddedActions.ForEach(x => x(slotID, techType, added));
            UpgradeTypes.AddActionParams addedParams = new()
            {
                vehicle = this,
                slotID = slotID,
                techType = techType,
                isAdded = added
            };
            UpgradeRegistrar.OnAddActions.ForEach(x => x(addedParams));
        }
        public override InventoryItem? GetSlotItem(int slotID)
        {
            if (slotID < 0 || slotID >= slotIDs.Length)
            {
                return null;
            }
            string slot = slotIDs[slotID];
            if (upgradesInput.equipment.equipment.TryGetValue(slot, out InventoryItem result))
            {
                return result;
            }
            return null;
        }
        public override void GetDepth(out int depth, out int crushDepth)
        {
            depth = Mathf.FloorToInt(GetComponent<CrushDamage>().GetDepth());
            crushDepth = Mathf.FloorToInt(GetComponent<CrushDamage>().crushDepth);
        }
        public override void SubConstructionComplete()
        {
            Logger.DebugLog("ModVehicle SubConstructionComplete");
            if(pingInstance != null)
            {
                pingInstance.enabled = true;
            }
            worldForces.handleGravity = true;
            BuildBotManager.ResetGhostMaterial();
        }
        public override void DeselectSlots() // This happens when you press the Exit button while having a "currentMountedVehicle."
        {
            if (ignoreInput)
            {
                return;
            }
            int i = 0;
            int num = slotIDs.Length;
            while (i < num)
            {
                QuickSlotType quickSlotType = GetQuickSlotType(i, out _);
                if (quickSlotType == QuickSlotType.Toggleable || quickSlotType == QuickSlotType.Selectable || quickSlotType == QuickSlotType.SelectableChargeable)
                {
                    ToggleSlot(i, false);
                }
                quickSlotCharge[i] = 0f;
                i++;
            }
            activeSlot = -1;
            NotifySelectSlot(activeSlot);
            DoExitRoutines();
        }
        public override void SlotLeftDown()
        {
            base.SlotLeftDown();
            GetComponent<VFArmsManager>()?.DoArmDown(true);
        }
        public override void SlotLeftHeld()
        {
            base.SlotLeftHeld();
            GetComponent<VFArmsManager>()?.DoArmHeld(true);
        }
        public override void SlotLeftUp()
        {
            base.SlotLeftUp();
            GetComponent<VFArmsManager>()?.DoArmUp(true);
        }
        public override void SlotRightDown()
        {
            base.SlotRightDown();
            GetComponent<VFArmsManager>()?.DoArmDown(false);
        }
        public override void SlotRightHeld()
        {
            base.SlotRightHeld();
            GetComponent<VFArmsManager>()?.DoArmHeld(false);
        }
        public override void SlotRightUp()
        {
            base.SlotRightUp();
            GetComponent<VFArmsManager>()?.DoArmUp(false);
        }
        public override string[] slotIDs
        { // You probably do not want to override this
            get
            {
                _slotIDs ??= GenerateSlotIDs(VehicleConfig.GetConfig(this).NumUpgrades.Value, VehicleConfig.GetConfig(this).IsArms.Value);
                return _slotIDs;
            }
        }
        public override string vehicleDefaultName
        {
            get
            {
                Language main = Language.main;
                if (main == null)
                {
                    return Language.main.Get("VFVehicle");
                }
                return main.Get("ModVehicle");
            }
        }
        #endregion

        #region virtual_methods
        public virtual void BeginPiloting()
        {
            // BeginPiloting is the VF trigger to start controlling a vehicle.
            EnterVehicle(Player.main, true);
            uGUI.main.quickSlots.SetTarget(this);
            NotifyStatus(PlayerStatus.OnPilotBegin);
            if (gameObject.GetComponentInChildren<MVCameraController>() != null)
            {
                Logger.PDANote($"{Language.main.Get("VFCameraHint")} {MainPatcher.NautilusConfig.NextCamera}, {MainPatcher.NautilusConfig.PreviousCamera}, and {MainPatcher.NautilusConfig.ExitCamera}");
            }
        }
        public virtual void StopPiloting()
        {
            // StopPiloting is the VF trigger to discontinue controlling a vehicle.
            uGUI.main.quickSlots.SetTarget(null);
            NotifyStatus(PlayerStatus.OnPilotEnd);
        }
        public virtual void PlayerEntry()
        {
            Logger.DebugLog("start modvehicle player entry");
            if (!isScuttled && !IsUnderCommand)
            {
                IsUnderCommand = true;
                Player.main.SetScubaMaskActive(false);
                if(CanopyWindows != null)
                {
                    foreach (GameObject window in CanopyWindows.Where(x => x != null))
                    {
                        window.SetActive(false);
                    }
                }
                if ((this as Drone) == null)
                {
                    Player.main.lastValidSub = GetComponent<SubRoot>();
                    Player.main.SetCurrentSub(GetComponent<SubRoot>(), true);
                    NotifyStatus(PlayerStatus.OnPlayerEntry);
                }
                if (pingInstance != null)
                {
                    pingInstance.enabled = false;
                }
            }
        }
        public virtual void PlayerExit()
        {
            Logger.DebugLog("start modvehicle player exit");
            if (IsUnderCommand)
            {
                if (CanopyWindows != null)
                {
                    foreach (GameObject window in CanopyWindows.Where(x => x != null))
                    {
                        window.SetActive(true);
                    }
                }
            }
            IsUnderCommand = false;
            if ((this as Drone) == null)
            {
                if (Player.main.GetCurrentSub() == GetComponent<SubRoot>())
                {
                    Player.main.SetCurrentSub(null);
                }
                if (Player.main.GetVehicle() == this)
                {
                    Player.main.currentMountedVehicle = null;
                }
                NotifyStatus(PlayerStatus.OnPlayerExit);
                Player.main.transform.SetParent(null);
                Player.main.TryEject(); // for DeathRun Remade Compat. See its patch in PlayerPatcher.cs
            }
            if (pingInstance != null)
            {
                pingInstance.enabled = true;
            }
        }
        public virtual void SubConstructionBeginning()
        {
            Logger.DebugLog("ModVehicle SubConstructionBeginning");
            if (pingInstance != null)
            {
                pingInstance.enabled = false;
            }
            worldForces.handleGravity = false;
        }
        public virtual void OnAIBatteryReload()
        {
        }
        public virtual float OnStorageOpen(string name, bool open)
        {
            // this function returns the number of seconds to wait before opening the PDA,
            // to show off the cool animations~
            return 0;
        }
        public virtual bool GetIsUnderwater()
        {
            bool isBeneathSurface = !worldForces.IsAboveWater();
            return isBeneathSurface && !precursorOutOfWater;
        }
        public virtual void OnCraftEnd(TechType techType)
        {
            IEnumerator GiveUsABatteryOrGiveUsDeath()
            {
                yield return new WaitForSeconds(2.5f);

                // give us an AI battery please
                TaskResult<GameObject> result = new();
                yield return CraftData.InstantiateFromPrefabAsync(TechType.PowerCell, result, false);
                GameObject newAIBattery = result.Get();
                newAIBattery.GetComponent<Battery>().charge = 200;
                newAIBattery.transform.SetParent(StorageRootObject.transform);
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
            }
            if (Batteries != null && Batteries.Count > 0)
            {
                SessionManager.StartCoroutine(GiveUsABatteryOrGiveUsDeath());
            }
        }
        public virtual void OnVehicleDocked(Vector3 exitLocation)
        {
            // The Moonpool invokes this once upon vehicle entry into the dock
            IsVehicleDocked = true;
            if (IsUnderCommand)
            {
                OnPlayerDocked(exitLocation);
            }
            useRigidbody.detectCollisions = false;
            foreach (var component in GetComponentsInChildren<IDockListener>())
            {
                (component as IDockListener).OnDock();
            }
        }
        public virtual void OnPlayerDocked(Vector3 exitLocation)
        {
            PlayerExit();
            if (exitLocation != Vector3.zero)
            {
                Player.main.transform.position = exitLocation;
                Player.main.transform.LookAt(transform);
            }
        }
        public virtual void OnVehicleUndocked()
        {
            // The Moonpool invokes this once upon vehicle exit from the dock
            if (!isScuttled && !ConsoleCommands.isUndockConsoleCommand)
            {
                OnPlayerUndocked();
            }
            IsVehicleDocked = false;
            foreach (var component in GetComponentsInChildren<IDockListener>())
            {
                (component as IDockListener).OnUndock();
            }
            IEnumerator EnsureCollisionsEnabledEventually()
            {
                yield return new WaitForSeconds(5f);
                useRigidbody.detectCollisions = true;
            }
            SessionManager.StartCoroutine(EnsureCollisionsEnabledEventually());
        }
        public virtual void OnPlayerUndocked()
        {
            PlayerEntry();
        }
        public virtual Vector3 GetBoundingDimensions()
        {
            BoxCollider? box = BoundingBoxCollider;
            if (box == null)
            {
                return Vector3.zero;
            }
            Vector3 boxDimensions = box.size;
            Vector3 worldScale = box.transform.lossyScale;
            return Vector3.Scale(boxDimensions, worldScale);
        }
        public virtual Vector3 GetDifferenceFromCenter()
        {
            BoxCollider? box = BoundingBoxCollider;
            if (box != null)
            {
                Vector3 colliderCenterWorld = box.transform.TransformPoint(box.center);
                Vector3 difference = colliderCenterWorld - transform.position;
                return difference;
            }
            return Vector3.zero;
        }
        public virtual void AnimateMoonPoolArms(VehicleDockingBay moonpool)
        {
            // AnimateMoonPoolArms is called in VehicleDockingBay.LateUpdate when a ModVehicle is docked in a moonpool.
            // This line sets the arms of the moonpool to do exactly as they do for the seamoth
            // There is also "exosuit_docked"
            SafeAnimator.SetBool(moonpool.animator, "seamoth_docked", moonpool.vehicle_docked_param && moonpool.dockedVehicle != null);
        }
        public virtual void DestroyMV()
        {
            DeathAction();
            ScuttleVehicle();
        }
        public virtual void DeathAction()
        {
            worldForces.enabled = true;
            worldForces.handleGravity = true;
            worldForces.underwaterGravity = 1.5f;
        }
        public virtual void ScuttleVehicle()
        {
            if(isScuttled)
            {
                return;
            }
            if (pingInstance != null)
            {
                pingInstance.enabled = false;
            }
            void OnCutOpen(Sealed sealedComp)
            {
                OnSalvage();
            }
            isScuttled = true;
            foreach (var component in GetComponentsInChildren<IScuttleListener>())
            {
                (component as IScuttleListener).OnScuttle();
            }
            WaterClipProxies?.ForEach(x => x.SetActive(false));
            isPoweredOn = false;
            gameObject.EnsureComponent<Scuttler>().Scuttle();
            var sealedThing = gameObject.EnsureComponent<Sealed>();
            sealedThing.openedAmount = 0;
            sealedThing.maxOpenedAmount = liveMixin.maxHealth / 5f;
            sealedThing.openedEvent.AddHandler(gameObject, new UWE.Event<Sealed>.HandleFunction(OnCutOpen));
        }
        public virtual void UnscuttleVehicle()
        {
            isScuttled = false;
            foreach (var component in GetComponentsInChildren<IScuttleListener>())
            {
                (component as IScuttleListener).OnUnscuttle();
            }
            WaterClipProxies?.ForEach(x => x.SetActive(true));
            isPoweredOn = true;
            gameObject.EnsureComponent<Scuttler>().Unscuttle();
        }
        public virtual void OnSalvage()
        {
            IEnumerator DropLoot(Vector3 place, GameObject root)
            {
                TaskResult<GameObject> result = new();
                if (Recipe != null)
                {
                    foreach (KeyValuePair<TechType, int> item in Recipe)
                    {
                        for (int i = 0; i < item.Value; i++)
                        {
                            yield return null;
                            if (UnityEngine.Random.value < 0.6f)
                            {
                                continue;
                            }
                            yield return CraftData.InstantiateFromPrefabAsync(item.Key, result, false);
                            GameObject go = result.Get();
                            Vector3 loc = place + 1.2f * UnityEngine.Random.onUnitSphere;
                            Vector3 rot = 360 * UnityEngine.Random.onUnitSphere;
                            go.transform.position = loc;
                            go.transform.eulerAngles = rot;
                            var rb = go.EnsureComponent<Rigidbody>();
                            rb.isKinematic = false;
                        }
                    }
                }
                while (root != null)
                {
                    Destroy(root);
                    yield return null;
                }
            }
            SessionManager.StartCoroutine(DropLoot(transform.position, gameObject));
        }
        public virtual void HandleOtherPilotingAnimations(bool isPiloting){}
        public virtual bool IsPlayerControlling()
        {
            return this switch
            {
                Submarine sub => sub.IsPlayerPiloting(),
                Submersible subbie => subbie.IsUnderCommand,
                Drone drone => drone.IsUnderCommand,
                _ => false
            };
        }
        public virtual void OnFinishedLoading()
        {

        }
        public virtual void PaintBaseColor(Vector3 hsb, Color color)
        {
            baseColor = color;
        }
        public virtual void PaintInteriorColor(Vector3 hsb, Color color)
        {
            interiorColor = color;
        }
        public virtual void PaintStripeColor(Vector3 hsb, Color color)
        {
            stripeColor = color;
        }
        #endregion

        #region public_fields
        public bool IsUnderCommand
        {// true when inside a vehicle (or piloting a drone)
            get
            {
                return _IsUnderCommand;
            }
            private set
            {
                _IsUnderCommand = value;
                IsPlayerDry = value;
            }
        }
        public FMOD_CustomEmitter? lightsOnSound = null;
        public FMOD_CustomEmitter? lightsOffSound = null;
        internal List<GameObject> lights = new();
        internal List<GameObject> volumetricLights = new();
        public PingInstance? pingInstance = null;
        public bool isInited = false;
        // if the player toggles the power off,
        // the vehicle is called "powered off,"
        // because it is unusable yet the batteries are not empty
        public bool isPoweredOn = true;
        public FMOD_StudioEventEmitter? ambienceSound;
        public int numEfficiencyModules = 0;
        public bool IsPlayerDry = false;
        public bool isScuttled = false;
        public bool IsUndockingAnimating = false;
        private readonly List<Action<int, TechType, bool>> upgradeOnAddedActions = new();
        public TechType TechType => GetComponent<TechTag>().type;
        public bool IsConstructed => vfxConstructing == null || vfxConstructing.IsConstructed();
        #endregion

        #region internal_fields
        private bool _IsUnderCommand = false;
        private int numArmorModules = 0;
        protected bool IsVehicleDocked = false;
        private string[]? _slotIDs = null;
        protected internal Color baseColor = Color.white;
        protected internal Color interiorColor = Color.white;
        protected internal Color stripeColor = Color.white;
        protected internal Color nameColor = Color.black;
        #endregion

        #region internal_methods
        internal List<string> VehicleModuleSlots => GenerateModuleSlots(VehicleConfig.GetConfig(this).NumUpgrades.Value).ToList(); // use config value instead
        internal static List<string> VehicleArmSlots => new() { ModuleBuilder.LeftArmSlotName, ModuleBuilder.RightArmSlotName };
        internal Dictionary<EquipmentType, List<string>> VehicleTypeToSlots => new()
                {
                    { EnumHelper.GetModuleType(), VehicleModuleSlots },
                    { EnumHelper.GetArmType(), VehicleArmSlots }
                };
        private void StorageModuleAction(int slotID, TechType techType, bool added)
        {
            if (techType == TechType.VehicleStorageModule)
            {
                SetStorageModule(slotID, added);
            }
        }
        private void ArmorPlatingModuleAction(int slotID, TechType techType, bool added)
        {
            if (techType == TechType.VehicleArmorPlating)
            {
                _ = added ? numArmorModules++ : numArmorModules--;
                GetComponent<DealDamageOnImpact>().mirroredSelfDamageFraction = 0.5f * Mathf.Pow(0.5f, numArmorModules);
            }
        }
        private void PowerUpgradeModuleAction(int slotID, TechType techType, bool added)
        {
            if (techType == TechType.VehiclePowerUpgradeModule)
            {
                _ = added ? numEfficiencyModules++ : numEfficiencyModules--;
            }
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
        private void HandleExtraQuickSlotInputs()
        {
            if (IsPlayerControlling())
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
        }
        private void SetStorageModule(int slotID, bool activated)
        {
            if (InnateStorages != null)
            {
                foreach (var sto in InnateStorages)
                {
                    sto.Container.SetActive(true);
                }
            }
            if (ModularStorages == null)
            {
                return;
            }
            if (ModularStorages.Count <= slotID)
            {
                ErrorMessage.AddWarning("There is no storage expansion for slot ID: " + slotID.ToString());
                return;
            }
            var modSto = ModularStorages[slotID];
            modSto.Container.SetActive(activated);
            if (activated)
            {
                var modularContainer = GetSeamothStorageContainer(slotID) ?? throw SessionManager.Fatal("ModVehicle failed to get SeamothStorageContainer for slotID: " + slotID.ToString());
                modularContainer.height = modSto.Height;
                modularContainer.width = modSto.Width;
                var storageInSlot = ModGetStorageInSlot(slotID, TechType.VehicleStorageModule) ?? throw SessionManager.Fatal(mySubName + " failed to get storage in slot: " + slotID.ToString() + " for TechType: " + TechType.VehicleStorageModule.ToString());
                storageInSlot.Resize(modSto.Width, modSto.Height);
            }
        }
        internal SeamothStorageContainer? GetSeamothStorageContainer(int slotID)
        {
            InventoryItem? slotItem = GetSlotItem(slotID);
            if (slotItem == null)
            {
                Logger.Warn("Warning: failed to get item for that slotID: " + slotID.ToString());
                return null;
            }
            Pickupable item = slotItem.item;
            if (item.GetTechType() != TechType.VehicleStorageModule)
            {
                Logger.Warn("Warning: failed to get pickupable for that slotID: " + slotID.ToString());
                return null;
            }
            SeamothStorageContainer component = item.GetComponent<SeamothStorageContainer>();
            return component;
        }
        internal ItemsContainer? ModGetStorageInSlot(int slotID, TechType techType)
        {
            if (techType == EnumHelper.GetInnateStorageType())
            {
                if (InnateStorages == null)
                {
                    throw SessionManager.Fatal($"ModVehicle {name} failed to get InnateStorages for slotID: {slotID} for TechType: {techType.AsString()}");
                }
                InnateStorageContainer vsc;
                if (0 <= slotID && slotID < InnateStorages.Count)
                {
                    vsc = InnateStorages[slotID].Container.GetComponent<InnateStorageContainer>();
                }
                else
                {
                    Logger.Error("Error: ModGetStorageInSlot called on invalid innate storage slotID");
                    return null;
                }
                return vsc.Container;
            }
            else if (techType == TechType.VehicleStorageModule)
            {
                return GetSeamothStorageContainer(slotID)?.container ?? throw SessionManager.Fatal($"ModVehicle {name} failed to get SeamothStorageContainer for slotID: {slotID} for TechType: {techType.AsString()}");
            }
            else
            {
                Logger.Error("Error: tried to get storage for unsupported TechType");
                return null;
            }
        }
        internal void TogglePower()
        {
            isPoweredOn = !isPoweredOn;
        }
        private void ManagePhysics()
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
            bool shouldSetKinematic = teleporting || !constructionFallOverride && !GetPilotingMode() && (!GameStateWatcher.IsWorldSettled || docked || !vfxConstructing.IsConstructed());
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, shouldSetKinematic, true);
        }
        internal void HandlePilotingAnimations()
        {
            switch (PilotingStyle)
            {
                case PilotingStyleEnum.Cyclops:
                    SafeAnimator.SetBool(Player.main.armsController.animator, "cyclops_steering", IsPlayerControlling());
                    break;
                case PilotingStyleEnum.Seamoth:
                    SafeAnimator.SetBool(Player.main.armsController.animator, "in_seamoth", IsPlayerControlling());
                    break;
                case PilotingStyleEnum.Prawn:
                    SafeAnimator.SetBool(Player.main.armsController.animator, "in_exosuit", IsPlayerControlling());
                    break;
                default:
                    HandleOtherPilotingAnimations(IsPlayerControlling());
                    break;
            }
        }
        private static void MyExitLockedMode()
        {
            GameInput.ClearInput();
            Player.main.transform.parent = null;
            Player.main.transform.localScale = Vector3.one;
            Player.main.currentMountedVehicle = null;
            Player.main.playerController.SetEnabled(true);
            Player.main.mode = Player.Mode.Normal;
            Player.main.playerModeChanged.Trigger(Player.main.mode);
            Player.main.sitting = false;
            Player.main.playerController.ForceControllerSize();
        }
        private void DoExitRoutines()
        {
            Player myPlayer = Player.main;
            Player.Mode myMode = myPlayer.mode;
            GetComponent<MVCameraController>()?.ResetCamera();
            void DoExitActions(ref Player.Mode mode)
            {
                GameInput.ClearInput();
                myPlayer.playerController.SetEnabled(true);
                mode = Player.Mode.Normal;
                myPlayer.playerModeChanged.Trigger(mode);
                myPlayer.sitting = false;
                myPlayer.playerController.ForceControllerSize();
                myPlayer.transform.parent = null;
            }

            Submersible? mvSubmersible = this as Submersible;
            //Walker? mvWalker = this as Walker;
            //Skimmer mvSkimmer = this as Skimmer;
            Submarine? mvSubmarine = this as Submarine;
            if (Drone.MountedDrone != null)
            {
                Drone.MountedDrone.StopControlling();
                if (Player.main.GetVehicle() != null)
                {
                    myPlayer.playerController.SetEnabled(true);
                    return;
                }
                MyExitLockedMode();
                return;
            }
            else if (mvSubmersible != null)
            {
                // exit locked mode
                DoExitActions(ref myMode);
                myPlayer.mode = myMode;
                mvSubmersible.StopPiloting();
                return;
            }
            /*
            else if (mvWalker != null)
            {
                DoExitActions(ref myMode);
                myPlayer.mode = myMode;
                mvWalker.StopPiloting();
                return;
            }
            else if (mvSkimmer != null)
            {
                DoExitActions(ref myMode);
                myPlayer.mode = myMode;
                mvSkimmer.StopPiloting();
                return;
            }
            */
            else if (mvSubmarine != null)
            {
                // check if we're level by comparing pitch and roll
                float roll = mvSubmarine.transform.rotation.eulerAngles.z;
                float rollDelta = roll >= 180 ? 360 - roll : roll;
                float pitch = mvSubmarine.transform.rotation.eulerAngles.x;
                float pitchDelta = pitch >= 180 ? 360 - pitch : pitch;

                if (rollDelta > mvSubmarine.ExitRollLimit || pitchDelta > mvSubmarine.ExitPitchLimit)
                {
                    if (HUDBuilder.IsVR)
                    {
                        Logger.PDANote($"{Language.main.Get("VFTooSteep")} ({GameInput.Button.Exit})");
                    }
                    else
                    {
                        Logger.PDANote($"{Language.main.Get("VFTooSteep")} ({GameInput.Button.Exit})");
                    }
                    return;
                }
                else if (mvSubmarine.useRigidbody.velocity.magnitude > mvSubmarine.ExitVelocityLimit)
                {
                    if (HUDBuilder.IsVR)
                    {
                        Logger.PDANote($"{Language.main.Get("VFTooFast")} ({GameInput.Button.Exit})");
                    }
                    else
                    {
                        Logger.PDANote($"{Language.main.Get("VFTooFast")} ({GameInput.Button.Exit})");
                    }
                    return;
                }
                
                mvSubmarine.VFEngine?.KillMomentum();

                if (mvSubmarine.PilotSeat.ExitLocation == null)
                {
                    Player.main.transform.position = mvSubmarine.PilotSeat.Seat.transform.position - mvSubmarine.PilotSeat.Seat.transform.forward * 1 + mvSubmarine.PilotSeat.Seat.transform.up * 1f;
                }
                else
                {
                    Player.main.transform.position = mvSubmarine.PilotSeat.ExitLocation.position;
                }
                DoExitActions(ref myMode);
                myPlayer.mode = myMode;
                mvSubmarine.StopPiloting();
                return;
            }
            MyExitLockedMode();
            return;
        }
        #endregion

        #region public_methods
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
        public void GetHUDValues(out float health, out float power)
        {
            health = liveMixin.GetHealthFraction();
            GetEnergyValues(out float num, out float num2);
            power = num > 0f && num2 > 0f ? num / num2 : 0f;
        }
        public bool HasRoomFor(Pickupable pickup)
        {
            if (InnateStorages != null)
            {
                foreach (var container in InnateStorages.Select(x => x.Container.GetComponent<InnateStorageContainer>().Container))
                {
                    if (container.HasRoomFor(pickup))
                    {
                        return true;
                    }
                }
            }
            foreach(var container in ModularStorageInput.GetAllModularStorageContainers(this))
            {
                if (container.HasRoomFor(pickup))
                {
                    return true;
                }
            }
            return false;
        }
        public bool HasInStorage(TechType techType, int count=1)
        {
            if (InnateStorages != null)
            {
                foreach (var container in InnateStorages.Select(x => x.Container.GetComponent<InnateStorageContainer>().Container))
                {
                    if (container.Contains(techType))
                    {
                        if (container.GetCount(techType) >= count)
                        {
                            return true;
                        }
                    }
                }
            }
            foreach (var container in ModularStorageInput.GetAllModularStorageContainers(this))
            {
                if (container.Contains(techType))
                {
                    if (container.GetCount(techType) >= count)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool AddToStorage(Pickupable pickup)
        {
            if (!HasRoomFor(pickup))
            {
                if (Player.main.GetVehicle() == this)
                {
                    ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
                }
                return false;
            }
            if (InnateStorages != null)
            {
                foreach (var container in InnateStorages.Select(x => x.Container.GetComponent<InnateStorageContainer>().Container))
                {
                    if (container.HasRoomFor(pickup))
                    {
                        string arg = Language.main.Get(pickup.GetTechName());
                        ErrorMessage.AddMessage(Language.main.GetFormat("VehicleAddedToStorage", arg));
                        uGUI_IconNotifier.main.Play(pickup.GetTechType(), uGUI_IconNotifier.AnimationType.From, null);
                        pickup.Initialize();
                        InventoryItem item = new(pickup);
                        container.UnsafeAdd(item);
                        pickup.PlayPickupSound();
                        return true;
                    }
                }
            }
            foreach (var container in ModularStorageInput.GetAllModularStorageContainers(this))
            {
                if (container.HasRoomFor(pickup))
                {
                    string arg = Language.main.Get(pickup.GetTechName());
                    ErrorMessage.AddMessage(Language.main.GetFormat("VehicleAddedToStorage", arg));
                    uGUI_IconNotifier.main.Play(pickup.GetTechType(), uGUI_IconNotifier.AnimationType.From, null);
                    pickup.Initialize();
                    InventoryItem item = new(pickup);
                    container.UnsafeAdd(item);
                    pickup.PlayPickupSound();
                    return true;
                }
            }
            return false;
        }
        public void GetStorageValues(out int stored, out int capacity)
        {
            int retStored = 0;
            int retCapacity = 0;

            int GetModularCapacity()
            {
                int ret = 0;
                var marty = ModularStorageInput.GetAllModularStorageContainers(this);
                marty.ForEach(x => ret += x.sizeX * x.sizeY);
                return ret;
            }
            int GetModularStored()
            {
                int ret = 0;
                var marty = ModularStorageInput.GetAllModularStorageContainers(this);
                marty.ForEach(x => x.ForEach(y => ret += y.width * y.height));
                return ret;
            }
            int GetInnateCapacity(VehicleStorage sto)
            {
                var container = sto.Container.GetComponent<InnateStorageContainer>();
                return container.Container.sizeX * container.Container.sizeY;
            }
            int GetInnateStored(VehicleStorage sto)
            {
                int ret = 0;
                var marty = (IEnumerable<InventoryItem>)sto.Container.GetComponent<InnateStorageContainer>().Container;
                marty.ForEach(x => ret += x.width * x.height);
                return ret;
            }

            if (InnateStorages != null)
            {
                InnateStorages.ForEach(x => retCapacity += GetInnateCapacity(x));
                InnateStorages.ForEach(x => retStored += GetInnateStored(x));
            }
            if(ModularStorages != null)
            {
                retCapacity += GetModularCapacity();
                retStored += GetModularStored();
            }
            stored = retStored;
            capacity = retCapacity;
        }

        public void SetBaseColor(Color col)
        {
            subName.SetColor(0, Vector3.zero, col);
        }
        public void SetInteriorColor(Color col)
        {
            subName.SetColor(2, Vector3.zero, col);
        }
        public void SetStripeColor(Color col)
        {
            subName.SetColor(3, Vector3.zero, col);
        }
        public void SetName(string name)
        {
            vehicleName = name;
            subName.SetName(name);
        }
        public void SetName(string name, Color col)
        {
            SetName(name);
            subName.SetColor(1, Vector3.zero, col); // see SubNamePatcher.cs for more details
        }
        private bool IsDefaultStyle = false;
        internal void SetVehicleDefaultStyle()
        {
            IsDefaultStyle = true;
            PaintVehicleDefaultStyle();
        }
        internal void SetVehicleDefaultStyle(string name)
        {
            SetVehicleDefaultStyle();
            SetName(name);
        }
        public virtual void PaintVehicleDefaultStyle()
        {
        }
        #endregion

        #region static_methods
        private static string[] GenerateModuleSlots(int modules)
        {
            string[] retIDs;
            retIDs = new string[modules];
            for (int i = 0; i < modules; i++)
            {
                retIDs[i] = ModuleBuilder.ModVehicleModulePrefix + i.ToString();
            }
            return retIDs;
        }
        private static string[] GenerateSlotIDs(int modules, bool arms)
        {
            string[] retIDs;
            int numUpgradesTotal = arms ? modules + 2 : modules;
            retIDs = new string[numUpgradesTotal];
            for (int i = 0; i < modules; i++)
            {
                retIDs[i] = ModuleBuilder.ModVehicleModulePrefix + i.ToString();
            }
            if (arms)
            {
                retIDs[modules] = ModuleBuilder.LeftArmSlotName;
                retIDs[modules + 1] = ModuleBuilder.RightArmSlotName;
            }
            return retIDs;
        }
        internal static void MaybeControlRotation(Vehicle veh)
        {
            ModVehicle? mv = veh as ModVehicle;
            if (mv == null
                || !veh.GetPilotingMode()
                || !mv.IsUnderCommand
                || mv.GetComponent<VFEngine>() == null
                || !veh.GetComponent<VFEngine>().enabled
                || Player.main.GetPDA().isOpen
                || AvatarInputHandler.main && !AvatarInputHandler.main.IsEnabled()
                || !mv.energyInterface.hasCharge)
            {
                return;
            }
            mv.GetComponent<VFEngine>().ControlRotation();
        }
        public static EnergyMixin? GetLeastChargedModVehicleEnergyMixinIfNull(EnergyMixin em, Vehicle veh)
        {
            ModVehicle? mv = veh as ModVehicle;
            if (em != null || mv == null)
            {
                return em;
            }
            if (mv.energyInterface != null && mv.energyInterface.sources != null && mv.energyInterface.sources.Length != 0)
            {
                return mv.energyInterface.sources.OrderBy(x => x.charge).First();
            }
            throw SessionManager.Fatal($"GetLeastChargedModVehicleEnergyMixinIfNull failed to get EnergyMixin for ModVehicle: {mv.GetName()}");
        }
        public static void TeleportPlayer(Vector3 destination)
        {
            ModVehicle? mv = Player.main.GetModVehicle();
            UWE.Utils.EnterPhysicsSyncSection();
            Player.main.SetCurrentSub(null, true);
            Player.main.playerController.SetEnabled(false);
            IEnumerator waitForTeleport()
            {
                yield return null;
                Player.main.SetPosition(destination);
                Player.main.SetCurrentSub(mv?.GetComponent<SubRoot>(), true);
                Player.main.playerController.SetEnabled(true);
                yield return null;
                UWE.Utils.ExitPhysicsSyncSection();
            }
            SessionManager.StartCoroutine(waitForTeleport());
        }
        #endregion
    }
}
