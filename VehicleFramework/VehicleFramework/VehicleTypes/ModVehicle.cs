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
        #region enumerations
        public enum DeathStyle
        {
            Explode,
            Sink,
            Float
        }
        #endregion
        #region abstract_members
        /* These things require implementation
         * They are required to have a coherent concept of a vehicle
         */
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
        public abstract GameObject VehicleModel { get; }
        public abstract GameObject CollisionModel { get; }
        public abstract GameObject StorageRootObject { get; }
        public abstract GameObject ModulesRootObject { get; }
        public abstract List<VehicleParts.VehicleStorage> InnateStorages { get; }
        public abstract List<VehicleParts.VehicleStorage> ModularStorages { get; }
        public abstract List<VehicleParts.VehicleUpgrades> Upgrades { get; }
        public abstract List<VehicleParts.VehicleBattery> Batteries { get; }
        public abstract List<VehicleParts.VehicleFloodLight> HeadLights { get; }
        public abstract List<GameObject> WaterClipProxies { get; }
        public abstract List<GameObject> CanopyWindows { get; }
        public abstract GameObject BoundingBox { get; }
        public abstract Dictionary<TechType, int> Recipe { get; }
        public abstract Atlas.Sprite PingSprite { get; }
        public abstract string Description { get; }
        public abstract string EncyclopediaEntry { get; }
        public abstract int BaseCrushDepth { get; }
        public abstract int MaxHealth { get; }
        public abstract int Mass { get; }
        public abstract int NumModules { get; }
        public abstract bool HasArms { get; }
        #endregion

        #region virtual_properties
        public virtual List<VehicleParts.VehicleBattery> BackupBatteries { get; }
        public virtual TechType UnlockedWith
        {
            get
            {
                return TechType.Constructor;
            }
        }
        public virtual bool CanLeviathanGrab { get; set; } = true;
        public virtual GameObject LeviathanGrabPoint
        {
            get
            {
                {
                    return gameObject;
                }
            }
        }
        public virtual Atlas.Sprite CraftingSprite
        {
            get
            {
                return MainPatcher.ModVehicleIcon;
            }
        }
        public virtual int CrushDepthUpgrade1
        {
            get
            {
                return 300;
            }
        }
        public virtual int CrushDepthUpgrade2
        {
            get
            {
                return 300;
            }
        }
        public virtual int CrushDepthUpgrade3
        {
            get
            {
                return 300;
            }
        }
        public virtual bool CanMoonpoolDock { get; set; } = true;
        public virtual DeathStyle OnDeathBehavior { get; set; } = DeathStyle.Sink;
        public virtual float TimeToConstruct { get; set; } = 15f; // Seamoth : 10 seconds, Cyclops : 20, Rocket Base : 25
        public virtual Color ConstructionGhostColor { get; set; } = Color.black;
        public virtual Color ConstructionWireframeColor { get; set; } = Color.black;
        public virtual bool AutoApplyShaders { get; set; } = true;
        public override string[] slotIDs
        { // You probably do not want to override this
            get
            {
                if (_slotIDs == null)
                {
                    int numVehicleModules = HasArms ? NumModules + 2 : NumModules;
                    _slotIDs = GenerateSlotIDs(numVehicleModules, HasArms);
                }
                return _slotIDs;
            }
        }

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
            voice = gameObject.EnsureComponent<AutoPilotVoice>();
            gameObject.EnsureComponent<AutoPilot>();
            voice.voice = VoiceManager.GetDefaultVoice(this);

            if (BoundingBox != null && BoundingBox.GetComponent<BoxCollider>() != null)
            {
                BoundingBox.GetComponent<BoxCollider>().enabled = false;
            }

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

            powerMan = gameObject.EnsureComponent<PowerManager>();

            //TODO I don't think we want this
            //StartCoroutine(ManageMyWaterProxies());

            // Register our new vehicle with Vehicle Framework
            VehicleManager.EnrollVehicle(this);
            isInited = true;
            StartCoroutine(WaitUntilReadyToSpeak());
        }
        private IEnumerator WaitUntilReadyToSpeak()
        {
            while(!Admin.GameStateWatcher.IsWorldSettled)
            {
                yield return null;
            }
            voice.NotifyReadyToSpeak();
            yield break;
        }
        public IEnumerator ManageMyWaterProxies()
        {
            if(this as VehicleTypes.Drone != null)
            {
                yield break;
            }
            while (true)
            {
                yield return new WaitForSeconds(0.05f);
                if(IsPlayerDry)
                {
                    WaterClipProxies.ForEach(x => x.SetActive(true));
                }
                else
                {
                    WaterClipProxies.ForEach(x => x.SetActive(false));
                }

            }
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
            GetComponent<PingInstance>().enabled = false;
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
            if (IsPlayerDry)
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
                toggledActions.Add(new Tuple<int, Coroutine>(slotID, StartCoroutine(DoToggleAction(this, slotID, techType, moduleToggleAction.Item3, moduleToggleAction.Item4, moduleToggleAction.Item5))));
            }
            else
            {
                toggledActions.Where(x => x.Item1 == slotID).Where(x => x.Item2 != null).ToList().ForEach(x => StopCoroutine(x.Item2));
            }
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
            base.EnterVehicle(Player.main, true);
            uGUI.main.quickSlots.SetTarget(this);
            NotifyStatus(PlayerStatus.OnPilotBegin);
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
            StartCoroutine(EvaluateDepthModuleLevel());
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
            if (!isScuttled)
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
            GetComponent<PingInstance>().enabled = false;
        }
        public override void SubConstructionComplete()
        {
            Logger.DebugLog("ModVehicle SubConstructionComplete");
            GetComponent<PingInstance>().enabled = true;
            BuildBotManager.ResetGhostMaterial();
        }
        public virtual void ForceExitLockedMode()
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
        public virtual void OnAIBatteryReload()
        {
        }
        // this function returns the number of seconds to wait before opening the PDA,
        // to show off the cool animations~
        public virtual float OnStorageOpen(string name, bool open)
        {
            return 0;
        }
        public virtual void ModVehicleReset()
        {
            Logger.DebugLog("ModVehicle Reset");
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
            StartCoroutine(GiveUsABatteryOrGiveUsDeath());
        }
        List<ColliderState> storedStates;
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
        public virtual void StoreShader()
        {
            foreach (var renderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                // skip some materials
                if (renderer.gameObject.name.ToLower().Contains("light"))
                {
                    continue;
                }
                if (CanopyWindows != null && CanopyWindows.Contains(renderer.gameObject))
                {
                    continue;
                }
                foreach (Material mat in renderer.materials)
                {
                    if (mat.shader != null)
                    {
                        m_ShaderMemory = mat.shader;
                        break;
                    }
                }
            }
        }
        public void ListShadersInUse()
        {
            HashSet<string> shaderNames = new HashSet<string>();

            // Find all materials currently loaded in the game.
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();

            foreach (var material in materials)
            {
                if (material.shader != null)
                {
                    // Add the shader name to the set to ensure uniqueness.
                    shaderNames.Add(material.shader.name);
                }
            }

            // Now you have a unique list of shader names in use.
            foreach (var shaderName in shaderNames)
            {
                Debug.Log("Shader in use: " + shaderName);
            }
        }
        public void ListShaderProperties()
        {
            Shader shader = Shader.Find("MarmosetUBER");
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string propertyName = shader.GetPropertyName(i);
                Debug.Log($"Property {i}: {propertyName}, Type: {shader.GetPropertyType(i)}");
            }
        }
        public virtual void ApplyInteriorLighting()
        {
            //ListShadersInUse();
            //ListShaderProperties();
            //VehicleBuilder.ApplyShaders(this, shader4);
        }
        public virtual void LoadShader()
        {
            VehicleBuilder.ApplyShaders(this, m_ShaderMemory);
        }
        public virtual Vector3 GetBoundingDimensions()
        {
            if(BoundingBox == null || BoundingBox.GetComponentInChildren<BoxCollider>(true) == null)
            {
                return Vector3.zero;
            }
            var box = BoundingBox.GetComponentInChildren<BoxCollider>(true);
            Vector3 boxDimensions = box.size;
            Vector3 worldScale = box.transform.lossyScale;
            return Vector3.Scale(boxDimensions, worldScale);
        }
        public virtual Vector3 GetDifferenceFromCenter()
        {
            var box = BoundingBox.GetComponentInChildren<BoxCollider>(true);
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
            WaterClipProxies.ForEach(x => x.SetActive(false));
            voice.enabled = false;
            headlights.isLive = false;
            isPoweredOn = false;
            gameObject.EnsureComponent<Scuttler>().Scuttle();
        }
        public virtual void UnscuttleVehicle()
        {
            isScuttled = false;
            GetComponentsInChildren<ModVehicleEngine>().ForEach(x => x.enabled = true);
            GetComponentsInChildren<PilotingTrigger>().ForEach(x => x.isLive = true);
            GetComponentsInChildren<TetherSource>().ForEach(x => x.isLive = true);
            GetComponentsInChildren<AutoPilot>().ForEach(x => x.enabled = true);
            WaterClipProxies.ForEach(x => x.SetActive(true));
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
        public virtual void DeathExplodeAction()
        {
            IEnumerator SpillScrapMetal(Vector3 place)
            {
                TaskResult<GameObject> result = new TaskResult<GameObject>();
                // spill out some scrap metal, lmao
                for (int i = 0; i < 3; i++)
                {
                    yield return CraftData.InstantiateFromPrefabAsync(TechType.ScrapMetal, result, false);
                    GameObject go = result.Get();
                    Vector3 loc = place + 3 * UnityEngine.Random.onUnitSphere;
                    Vector3 rot = 360 * UnityEngine.Random.onUnitSphere;
                    go.transform.position = loc;
                    go.transform.eulerAngles = rot;
                    var rb = go.EnsureComponent<Rigidbody>();
                    rb.isKinematic = false;
                }
                yield break;
            }
            UWE.CoroutineHost.StartCoroutine(SpillScrapMetal(transform.position));
            // MAkeExplosionAndSmokeFX();
            Destroy(gameObject);
        }
        #endregion

        #region member_variables
        public new SubName subName = new SubName();
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
        public bool IsPlayerDry = false;
        public bool IsVehicleDocked = false;
        private string[] _slotIDs = null;
        private List<Tuple<int, Coroutine>> toggledActions = new List<Tuple<int, Coroutine>>();
        private Shader m_ShaderMemory;
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
            return upgradeSlots.GroupBy(x => x).Select(y => y.First()).Where(x => upgradesInput.equipment.GetItemInSlot(x) != null).Select(x => upgradesInput.equipment.GetItemInSlot(x).item.name).ToList();
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
            int numModules = arms ? modules + 2 : modules;
            string[] retIDs = new string[numModules];
            for (int i = 0; i < modules; i++)
            {
                retIDs[i] = "VehicleModule" + i.ToString();
            }
            if (arms)
            {
                retIDs[modules] = "VehicleArmLeft";
                retIDs[modules + 1] = "VehicleArmRight";
            }
            return retIDs;
        }
        public static void MaybeControlRotation(Vehicle veh)
        {
            ModVehicle mv = veh as ModVehicle;
            if (mv == null
                || Player.main.mode != Player.Mode.LockedPiloting
                || !mv.IsPlayerDry
                || mv.GetComponent<ModVehicleEngine>() == null
                || Player.main.GetPDA().isOpen)
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
        #endregion
    }
}
