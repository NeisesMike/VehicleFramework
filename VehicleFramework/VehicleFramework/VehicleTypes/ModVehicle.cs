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
     * ModVehicle is the primary abstract class provided by Vehicle Framework.
     * All VF vehicles inherit from ModVehicle.
     */
    public abstract class ModVehicle : Vehicle, ICraftTarget
    {
        #region enumerations
        public enum DeathStyle
        {
            Explode = 0,
            Sink = 1,
            Float = 2
        }
        #endregion
        #region abstract_members
        /* The model, collision model, storage root object, and modules root object
         * must all be unique game objects.
         * And since VF cannot add gameobjects to the prefab,
         * they must be supplied for each vehicle.
         */
        public abstract GameObject VehicleModel { get; } 
        public abstract GameObject CollisionModel { get; }
        public abstract GameObject StorageRootObject { get; }
        public abstract GameObject ModulesRootObject { get; }
        #endregion

        #region virtual_properties_optional
        public virtual List<VehicleParts.VehicleBattery> Batteries => new List<VehicleParts.VehicleBattery>();
        public virtual List<VehicleParts.VehicleUpgrades> Upgrades => new List<VehicleParts.VehicleUpgrades>();
        public virtual ModVehicleEngine Engine { get; set; }
        public virtual VehicleParts.VehicleArmsProxy Arms { get; set; }
        public virtual GameObject BoundingBox => null; // Prefer to use BoundingBoxCollider directly (don't use this)
        public virtual BoxCollider BoundingBoxCollider { get; set; }
        public virtual Atlas.Sprite PingSprite => VehicleManager.defaultPingSprite;
        public virtual List<GameObject> WaterClipProxies => new List<GameObject>();
        public virtual List<VehicleParts.VehicleStorage> InnateStorages => new List<VehicleParts.VehicleStorage>();
        public virtual List<VehicleParts.VehicleStorage> ModularStorages => new List<VehicleParts.VehicleStorage>();
        public virtual List<VehicleParts.VehicleFloodLight> HeadLights => new List<VehicleParts.VehicleFloodLight>();
        public virtual List<GameObject> CanopyWindows => new List<GameObject>();
        public virtual Dictionary<TechType, int> Recipe => new Dictionary<TechType, int>() { { TechType.Titanium, 1 } };
        public virtual List<VehicleParts.VehicleBattery> BackupBatteries => new List<VehicleParts.VehicleBattery>();
        public virtual Sprite UnlockedSprite => null;
        public virtual GameObject LeviathanGrabPoint => gameObject;
        public virtual Atlas.Sprite CraftingSprite => MainPatcher.ModVehicleIcon;
        public override string[] slotIDs
        { // You probably do not want to override this
            get
            {
                if (_slotIDs == null)
                {
                    _slotIDs = GenerateSlotIDs(NumModules, HasArms);
                }
                return _slotIDs;
            }
        }
        #endregion

        #region virtual_properties_nonnullable_static
        public virtual string Description => "A vehicle";
        public virtual string EncyclopediaEntry => "This is a vehicle you can build at the Mobile Vehicle Bay. It can be controlled either directly or with a Drone Station.";
        public virtual int BaseCrushDepth => 250;
        public virtual int MaxHealth => 100;
        public virtual int Mass => 1000;
        public virtual int NumModules => 4;
        public virtual bool HasArms => false;
        public virtual TechType UnlockedWith => TechType.Constructor;
        public virtual string UnlockedMessage => "New vehicle blueprint acquired";
        public virtual int FragmentsToScan => 3;
        public virtual int CrushDepthUpgrade1 => 300;
        public virtual int CrushDepthUpgrade2 => 300;
        public virtual int CrushDepthUpgrade3 => 300;
        public virtual int CrushDamage => MaxHealth / 15;
        public virtual int CrushPeriod => 1;
        /// <summary>
        /// You can set this to true to use the default damage tracking mechanisms
        /// ONLY IF ALL OF THE FOLLOWING ARE TRUE:
        /// Five special gameobjects must appear somewhere in your vehicle.
        /// Each should have at least one collider on itself or any of its children.
        /// For each gameobject, all of its colliders will be used as repair targets.
        /// The names are MVDAMAGE_HULL, MVDAMAGE_UPGRADES, MVDAMAGE_ENGINE, MVDAMAGE_BATTERIES, MVDAMAGE_LIGHTS
        /// </summary>
        public virtual bool UseDefaultDamageTracker => false;
        #endregion

        #region virtual_properties_nonnullable_dynamic
        public override string vehicleDefaultName
        {
            get
            {
                Language main = Language.main;
                if (main == null)
                {
                    return LocalizationManager.GetString(EnglishString.Vehicle);
                }
                return main.Get("ModVehicle");
            }
        }
        public virtual bool CanLeviathanGrab { get; set; } = true;
        public virtual bool CanMoonpoolDock { get; set; } = true;
        public virtual DeathStyle OnDeathBehavior { get; set; } = DeathStyle.Sink;
        public virtual float TimeToConstruct { get; set; } = 15f; // Seamoth : 10 seconds, Cyclops : 20, Rocket Base : 25
        public virtual Color ConstructionGhostColor { get; set; } = Color.black;
        public virtual Color ConstructionWireframeColor { get; set; } = Color.black;
        public virtual bool AutoApplyShaders { get; set; } = true;

        #endregion

        #region virtual_methods
        public override void Awake()
        {
            energyInterface = GetComponent<EnergyInterface>();
            base.Awake();

            if (HeadLights != null)
            {
                headlights = gameObject.EnsureComponent<HeadLightsController>();
            }
            gameObject.EnsureComponent<AutoPilot>();

            if (UseDefaultDamageTracker)
            {
                gameObject.EnsureComponent<VehicleComponents.VehicleDamageTracker>();
            }

            upgradeOnAddedActions.Add(storageModuleAction);
            upgradeOnAddedActions.Add(armorPlatingModuleAction);
            upgradeOnAddedActions.Add(powerUpgradeModuleAction);

            if (BoundingBoxCollider == null && BoundingBox != null)
            {
                BoundingBoxCollider = BoundingBox.GetComponentInChildren<BoxCollider>(true);
            }
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

            powerMan = gameObject.EnsureComponent<PowerManager>();

            // Register our new vehicle with Vehicle Framework
            VehicleManager.EnrollVehicle(this);
            isInited = true;
            IEnumerator WaitUntilReadyToSpeak()
            {
                while (!Admin.GameStateWatcher.IsWorldSettled)
                {
                    yield return null;
                }
                voice.NotifyReadyToSpeak();
                yield break;
            }
            StartCoroutine(WaitUntilReadyToSpeak());
        }
        public override void Update()
        {
            if (isScuttled)
            {
                return;
            }
            base.Update();
            HandleExtraQuickSlotInputs();
        }
        public override void FixedUpdate()
        {
            ManagePhysics();
        }
        public virtual void DestroyMV()
        {
            pingInstance.enabled = false;
            switch (OnDeathBehavior)
            {
                case DeathStyle.Explode:
                    DeathExplodeAction();
                    return;
                case DeathStyle.Sink:
                    DeathSinkAction();
                    return;
                case DeathStyle.Float:
                    DeathFloatAction();
                    return;
                default:
                    return;
            }
        }
        public new virtual void OnKill()
        {
            if (IsPlayerDry && VehicleTypes.Drone.mountedDrone == null)
            {
                Player.main.playerController.SetEnabled(true);
                Player.main.mode = Player.Mode.Normal;
                Player.main.playerModeChanged.Trigger(Player.main.mode);
                Player.main.sitting = false;
                Player.main.playerController.ForceControllerSize();
                Player.main.transform.parent = null;
                StopPiloting();
            }
            if (destructionEffect)
            {
                GameObject gameObject = Instantiate<GameObject>(destructionEffect);
                gameObject.transform.position = transform.position;
                gameObject.transform.rotation = transform.rotation;
            }
            DestroyMV();
        }
        public override void OnUpgradeModuleToggle(int slotID, bool active)
        {
            TechType techType = modules.GetTechTypeInSlot(slotIDs[slotID]);
            if (active)
            {
                var moduleToggleAction = UpgradeModules.ModulePrepper.upgradeToggleActions.Where(x => x.Item2 == techType).FirstOrDefault();
                if (moduleToggleAction != null)
                {
                    toggledActions.Add(new Tuple<int, Coroutine>(slotID, StartCoroutine(DoToggleAction(this, slotID, techType, moduleToggleAction.Item3, moduleToggleAction.Item4, moduleToggleAction.Item5))));
                }
                IEnumerator DoToggleAction(ModVehicle thisMV, int thisSlotID, TechType tt, float timeToFirstActivation, float repeatRate, float energyCostPerActivation)
                {
                    yield return new WaitForSeconds(timeToFirstActivation);
                    while (true)
                    {
                        if (!thisMV.IsPlayerDry)
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
            }
            else
            {
                toggledActions.Where(x => x.Item1 == slotID).Where(x => x.Item2 != null).ToList().ForEach(x => StopCoroutine(x.Item2));
            }
            UpgradeTypes.ToggleActionParams param = new UpgradeTypes.ToggleActionParams
            {
                active = active,
                mv = this,
                slotID = slotID,
                techType = techType
            };
            Admin.UpgradeRegistrar.OnToggleActions.ForEach(x => x(param));
            base.OnUpgradeModuleToggle(slotID, active);
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
            UpgradeTypes.SelectableActionParams param = new UpgradeTypes.SelectableActionParams
            {
                mv = this,
                slotID = slotID,
                techType = techType
            };
            Admin.UpgradeRegistrar.OnSelectActions.ForEach(x => x(param));
            UpgradeTypes.SelectableChargeableActionParams param2 = new UpgradeTypes.SelectableChargeableActionParams
            {
                mv = this,
                slotID = slotID,
                techType = techType,
                charge = param.mv.quickSlotCharge[param.slotID],
                slotCharge = param.mv.GetSlotCharge(param.slotID)
            };
            Admin.UpgradeRegistrar.OnSelectChargeActions.ForEach(x => x(param2));
            base.OnUpgradeModuleUse(techType, slotID);
        }
        public override void OnPilotModeBegin()
        {
            base.OnPilotModeBegin();
        }
        public override void EnterVehicle(Player player, bool teleport, bool playEnterAnimation = true)
        {
            // This function locks the player in and configures several variables for that purpose
            base.EnterVehicle(player, teleport, playEnterAnimation);
        }
        public virtual void BeginPiloting()
        {
            // BeginPiloting is the VF trigger to start controlling a vehicle.
            EnterVehicle(Player.main, true);
            uGUI.main.quickSlots.SetTarget(this);
            NotifyStatus(PlayerStatus.OnPilotBegin);
            if (gameObject.GetComponentInChildren<VehicleComponents.MVCameraController>() != null)
            {
                Logger.Output("Press " +
                              MainPatcher.VFConfig.nextCamera +
                              " and " +
                              MainPatcher.VFConfig.previousCamera +
                              " to switch cameras.\n" +
                              "Press " +
                              MainPatcher.VFConfig.exitCamera +
                              " to exit cameras."
                              , time: 2f
                              , y: 300);
            }
        }
        public virtual void StopPiloting()
        {
            // StopPiloting is the VF trigger to discontinue controlling a vehicle.
            // this function
            // called by Player.ExitLockedMode()
            // which is triggered on button press
            uGUI.main.quickSlots.SetTarget(null);
            NotifyStatus(PlayerStatus.OnPilotEnd);
        }
        public override void OnUpgradeModuleChange(int slotID, TechType techType, bool added)
        {
            upgradeOnAddedActions.ForEach(x => x(slotID, techType, added));
            var upgradeList = GetCurrentUpgrades();
            UpgradeModules.ModulePrepper.upgradeOnAddedActions.ForEach(x => x(this, upgradeList, slotID, techType, added));

            UpgradeTypes.AddActionParams addedParams = new UpgradeTypes.AddActionParams
            {
                mv = this,
                slotID = slotID,
                techType = techType,
                isAdded = added
            };
            Admin.UpgradeRegistrar.OnAddActions.ForEach(x => x(addedParams));
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
        public override void GetDepth(out int depth, out int crushDepth)
        {
            depth = Mathf.FloorToInt(GetComponent<CrushDamage>().GetDepth());
            crushDepth = Mathf.FloorToInt(GetComponent<CrushDamage>().crushDepth);
        }
        public virtual void PlayerEntry()
        {
            Logger.DebugLog("start modvehicle player entry");
            if (!isScuttled && !IsPlayerDry)
            {
                IsPlayerDry = true;
                Player.main.SetScubaMaskActive(false);
                try
                {
                    foreach (GameObject window in CanopyWindows)
                    {
                        window?.SetActive(false);
                    }
                }
                catch (Exception)
                {
                    //It's okay if the vehicle doesn't have a canopy
                }
                Player.main.lastValidSub = GetComponent<SubRoot>();
                Player.main.SetCurrentSub(GetComponent<SubRoot>(), true);
                NotifyStatus(PlayerStatus.OnPlayerEntry);
            }
        }
        public virtual void PlayerExit()
        {
            Logger.DebugLog("start modvehicle player exit");
            if (IsPlayerDry)
            {
                try
                {
                    foreach (GameObject window in CanopyWindows)
                    {
                        window?.SetActive(true);
                    }
                }
                catch (Exception)
                {
                    //It's okay if the vehicle doesn't have a canopy
                }
            }
            IsPlayerDry = false;
            Player.main.SetCurrentSub(null);
            NotifyStatus(PlayerStatus.OnPlayerExit);
        }
        public virtual void SubConstructionBeginning()
        {
            Logger.DebugLog("ModVehicle SubConstructionBeginning");
            pingInstance.enabled = false;
        }
        public override void SubConstructionComplete()
        {
            Logger.DebugLog("ModVehicle SubConstructionComplete");
            pingInstance.enabled = true;
            BuildBotManager.ResetGhostMaterial();
        }
        public virtual void ForceExitLockedMode()
        {
            // handle warper specially
            GameInput.ClearInput();
            Player.main.playerController.SetEnabled(true);
            Player.main.mode = Player.Mode.Normal;
            Player.main.playerModeChanged.Trigger(Player.main.mode);
            Player.main.sitting = false;
            Player.main.playerController.ForceControllerSize();
            Player.main.transform.parent = null;
            StopPiloting();
        }
        public virtual void OnAIBatteryReload()
        {
        }
        // this function returns the number of seconds to wait before opening the PDA,
        // to show off the cool animations~
        public virtual float OnStorageOpen(string name, bool open)
        {
            return 0;
        }
        public virtual bool GetIsUnderwater()
        {
            // TODO: justify this constant
            return transform.position.y < 0.75f;
        }
        public virtual void OnCraftEnd(TechType techType)
        {
            //Logger.Log("ModVehicle OnCraftEnd");
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
            if (Batteries != null && Batteries.Count() > 0)
            {
                StartCoroutine(GiveUsABatteryOrGiveUsDeath());
            }
        }
        public void SetDockedLighting(bool docked)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in renderer.materials)
                {
                    if (renderer.gameObject.name.ToLower().Contains("light"))
                    {
                        continue;
                    }
                    if (CanopyWindows != null && CanopyWindows.Contains(renderer.gameObject))
                    {
                        continue;
                    }
                    mat.EnableKeyword("MARMO_EMISSION");
                    mat.SetFloat("_EmissionLMNight", docked ? 0.4f : 0f);
                    mat.SetFloat("_EmissionLM", 0);
                    mat.SetFloat("_GlowStrength", 0);
                    mat.SetFloat("_GlowStrengthNight", 0);
                    mat.SetFloat("_SpecInt", 0f);
                    if(docked)
                    {
                        mat.EnableKeyword("MARMO_SPECMAP");
                    }
                    else
                    {
                        mat.DisableKeyword("MARMO_SPECMAP");
                    }
                }
            }
        }
        public virtual void OnVehicleDocked()
        {
            // The Moonpool invokes this once upon vehicle entry into the dock
            IsVehicleDocked = true;
            headlights.DisableHeadlights();
            //StoreShader();
            //ApplyInteriorLighting();
            if (IsPlayerDry)
            {
                OnPlayerDocked();
            }
            useRigidbody.detectCollisions = false;
            SetDockedLighting(true);
        }
        public virtual void OnPlayerDocked()
        {
            PlayerExit();
        }
        public virtual void OnVehicleUndocked()
        {
            // The Moonpool invokes this once upon vehicle exit from the dock
            //LoadShader();
            OnPlayerUndocked();
            IsVehicleDocked = false;
            useRigidbody.detectCollisions = true;
            SetDockedLighting(false);
        }
        public virtual void OnPlayerUndocked()
        {
            PlayerEntry();
        }
        public bool IsUndockingAnimating = false;
        public void OnUndockingStart()
        {
            IsUndockingAnimating = true;
        }
        public void OnUndockingComplete()
        {
            IsUndockingAnimating = false;
        }
        public virtual Vector3 GetBoundingDimensions()
        {
            BoxCollider box = BoundingBoxCollider;
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
            BoxCollider box = BoundingBoxCollider;
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
        public virtual void ScuttleVehicle()
        {
            isScuttled = true;
            GetComponentsInChildren<ModVehicleEngine>().ForEach(x => x.enabled = false);
            GetComponentsInChildren<PilotingTrigger>().ForEach(x => x.isLive = false);
            GetComponentsInChildren<TetherSource>().ForEach(x => x.isLive = false);
            GetComponentsInChildren<AutoPilot>().ForEach(x => x.enabled = false);
            WaterClipProxies?.ForEach(x => x.SetActive(false));
            voice.enabled = false;
            headlights.isLive = false;
            isPoweredOn = false;
            gameObject.EnsureComponent<Scuttler>().Scuttle();
            var sealedThing = gameObject.AddComponent<Sealed>();
            sealedThing.openedAmount = 0;
            sealedThing.maxOpenedAmount = liveMixin.maxHealth;
            sealedThing.openedEvent.AddHandler(gameObject, new UWE.Event<Sealed>.HandleFunction(OnCutOpen));
        }
        private void OnCutOpen(Sealed sealedComp)
        {
            DeathExplodeAction();
        }
        public virtual void UnscuttleVehicle()
        {
            isScuttled = false;
            GetComponentsInChildren<ModVehicleEngine>().ForEach(x => x.enabled = true);
            GetComponentsInChildren<PilotingTrigger>().ForEach(x => x.isLive = true);
            GetComponentsInChildren<TetherSource>().ForEach(x => x.isLive = true);
            GetComponentsInChildren<AutoPilot>().ForEach(x => x.enabled = true);
            WaterClipProxies?.ForEach(x => x.SetActive(true));
            voice.enabled = true;
            headlights.isLive = true;
            isPoweredOn = true;
            gameObject.EnsureComponent<Scuttler>().Unscuttle();
        }
        public virtual void DeathSinkAction()
        {
            ScuttleVehicle();
            worldForces.enabled = true;
            worldForces.handleGravity = true;
            worldForces.underwaterGravity = 1.5f;
        }
        public virtual void DeathFloatAction()
        {
            ScuttleVehicle();
            // set to buoyant, recalling that the ocean surface is the plane y=0
            worldForces.enabled = true;
            worldForces.handleGravity = true;
            worldForces.underwaterGravity = -1f;
            worldForces.aboveWaterGravity = 9.8f;
        }
        IEnumerator DropLoot(Vector3 place)
        {
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            foreach(KeyValuePair<TechType, int> item in Recipe)
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
        public virtual void DeathExplodeAction()
        {
            UWE.CoroutineHost.StartCoroutine(DropLoot(transform.position));
            Destroy(gameObject);
        }
        #endregion

        #region member_variables
        public FMOD_CustomEmitter lightsOnSound = null;
        public FMOD_CustomEmitter lightsOffSound = null;
        public List<GameObject> lights = new List<GameObject>();
        public List<GameObject> volumetricLights = new List<GameObject>();
        public PingInstance pingInstance = null;
        public HeadLightsController headlights;
        public bool isRegistered = false;
        public EnergyInterface AIEnergyInterface;
        public AutoPilotVoice voice;
        public bool isInited = false;
        // if the player toggles the power off,
        // the vehicle is called "disgengaged,"
        // because it is unusable yet the batteries are not empty
        public bool isPoweredOn = true;
        public FMOD_StudioEventEmitter ambienceSound;
        public int numEfficiencyModules = 0;
        private int numArmorModules = 0;
        public PowerManager powerMan = null;
        public bool IsPlayerDry = false; // true when inside a vehicle (or piloting a drone)
        public bool IsVehicleDocked = false;
        private string[] _slotIDs = null;
        internal List<Tuple<int, Coroutine>> toggledActions = new List<Tuple<int, Coroutine>>();
        public bool isScuttled = false;
        #endregion

        #region methods
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
        public List<string> GetCurrentUpgrades()
        {
            List<string> upgradeSlots = new List<string>();
            upgradesInput.equipment.GetSlots(VehicleBuilder.ModuleType, upgradeSlots);
            List<string> upgradeStrings = upgradeSlots
                .GroupBy(x => x)
                .Select(y => y.First())
                .Where(x => upgradesInput.equipment.GetItemInSlot(x) != null)
                .Select(x => upgradesInput.equipment.GetItemInSlot(x).item.name)
                .ToList();
            List<string> armSlots = new List<string>();
            upgradesInput.equipment.GetSlots(VehicleBuilder.ArmType, armSlots);
            List<string> armStrings= armSlots
                .GroupBy(x => x)
                .Select(y => y.First())
                .Where(x => upgradesInput.equipment.GetItemInSlot(x) != null)
                .Select(x => upgradesInput.equipment.GetItemInSlot(x).item.name)
                .ToList();
            upgradeStrings.AddRange(armStrings);
            return upgradeStrings.ToList();
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
            int extraDepthToAdd = 0;
            extraDepthToAdd = maxDepthModuleLevel > 0 ? extraDepthToAdd += CrushDepthUpgrade1 : extraDepthToAdd;
            extraDepthToAdd = maxDepthModuleLevel > 1 ? extraDepthToAdd += CrushDepthUpgrade2 : extraDepthToAdd;
            extraDepthToAdd = maxDepthModuleLevel > 2 ? extraDepthToAdd += CrushDepthUpgrade3 : extraDepthToAdd;
            GetComponent<CrushDamage>().SetExtraCrushDepth(extraDepthToAdd);
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
        private void SetStorageModule(int slotID, bool activated)
        {
            foreach (var sto in InnateStorages)
            {
                sto.Container.SetActive(true);
            }
            if (ModularStorages is null)
            {
                return;
            }
            if (ModularStorages.Count <= slotID)
            {
                Logger.Output("There is no storage expansion for slot ID: " + slotID.ToString());
                return;
            }
            ModularStorages[slotID].Container.SetActive(activated);
            //ModularStorages[slotID].Container.GetComponent<BoxCollider>().enabled = activated;
        }
        public ItemsContainer ModGetStorageInSlot(int slotID, TechType techType)
        {
            switch (techType)
            {
                case VehicleBuilder.InnateStorage:
                    {
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
        public void GetHUDValues(out float health, out float power)
        {
            health = this.liveMixin.GetHealthFraction();
            float num;
            float num2;
            base.GetEnergyValues(out num, out num2);
            power = ((num > 0f && num2 > 0f) ? (num / num2) : 0f);
        }
        public void ManagePhysics()
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

        #endregion

        #region static_methods
        private static string[] GenerateSlotIDs(int modules, bool arms)
        {
            string[] retIDs;
            int numUpgradesTotal = (arms || MainPatcher.VFConfig.forceArmsCompat) ? (modules + 2) : modules;
            retIDs = new string[numUpgradesTotal];
            for (int i = 0; i < modules; i++)
            {
                retIDs[i] = "VehicleModule" + i.ToString();
            }
            if (arms || MainPatcher.VFConfig.forceArmsCompat)
            {
                retIDs[modules] = ModuleBuilder.LeftArmSlotName;
                retIDs[modules + 1] = ModuleBuilder.RightArmSlotName;
            }
            return retIDs;
        }
        public static void MaybeControlRotation(Vehicle veh)
        {
            ModVehicle mv = veh as ModVehicle;
            if (mv == null
                || !veh.GetPilotingMode()
                || !mv.IsPlayerDry
                || mv.GetComponent<ModVehicleEngine>() == null
                || !veh.GetComponent<ModVehicleEngine>().enabled
                || Player.main.GetPDA().isOpen
                || !mv.energyInterface.hasCharge)
            {
                return;
            }
            mv.GetComponent<ModVehicleEngine>().ControlRotation();
        }
        public static EnergyMixin GetEnergyMixinFromVehicle(Vehicle veh)
        {
            if ((veh as ModVehicle) == null)
            {
                return veh.GetComponent<EnergyMixin>();
            }
            else
            {
                return (veh as ModVehicle).energyInterface.sources.First();
            }
        }
        public static void TeleportPlayer(Vector3 destination)
        {
            ModVehicle mv = Player.main.GetVehicle() as ModVehicle;
            Player.main.SetCurrentSub(null);
            IEnumerator doThing()
            {
                yield return null;
                Player.main.transform.position = destination;
                Player.main.SetCurrentSub(mv?.GetComponent<SubRoot>(), true);
            }
            UWE.CoroutineHost.StartCoroutine(doThing());
        }
        #endregion
    }
}
