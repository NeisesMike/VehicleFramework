using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public abstract class ModVehicle : Vehicle
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

        public abstract GameObject VehicleModel { get; }
        public virtual GameObject StorageRootObject { get { return VehicleModel; } }
        public virtual GameObject ModulesRootObject { get { return StorageRootObject; } }

        // lists of game object references, used later like a blueprint
        public abstract List<VehicleParts.VehiclePilotSeat> PilotSeats { get; }
        public abstract List<VehicleParts.VehicleHatchStruct> Hatches { get; }
        public abstract List<VehicleParts.VehicleStorage> InnateStorages { get; }
        public abstract List<VehicleParts.VehicleStorage> ModularStorages { get; }
        public abstract List<VehicleParts.VehicleUpgrades> Upgrades { get; }
        public abstract List<VehicleParts.VehicleBattery> Batteries { get; }
        public abstract List<VehicleParts.VehicleLight> Lights { get; }
        public abstract List<GameObject> WalkableInteriors { get; }
        public abstract List<GameObject> WaterClipProxies { get; }
        public abstract List<GameObject> CanopyWindows { get; }
        public abstract GameObject ControlPanel { get; }
        public ControlPanel controlPanelLogic;


        public FMOD_StudioEventEmitter lightsOnSound = null;
        public FMOD_StudioEventEmitter lightsOffSound = null;
        public List<GameObject> lights = new List<GameObject>();
        public List<GameObject> volumetricLights = new List<GameObject>();
        public PingInstance pingInstance = null;
        public FMOD_StudioEventEmitter ambienceSound;

        private bool isPilotSeated = false;
        private bool isPlayerInside = false;

        // TODO
        // These are tracked appropriately, but their values are never used for anything meaningful.
        public int numEfficiencyModules = 0;
        private int numArmorModules = 0;

        // if the player toggles the power off,
        // the vehicle is called "disgengaged,"
        // because it is unusable yet the batteries are not empty
        public bool IsDisengaged = false;

        public VehicleLights vLights;
        public VehicleEngine engine;

        // later
        public virtual List<GameObject> Arms => null;
        public virtual List<GameObject> Legs => null;

        // not sure what types these should be
        public virtual List<GameObject> SoundEffects => null;
        public virtual List<GameObject> TwoDeeAssets => null;

        public override void Awake()
        {
            base.Awake();

            vLights = gameObject.EnsureComponent<VehicleLights>();
            vLights.mv = this;

            var gauge = gameObject.EnsureComponent<FuelGauge>();
            gauge.mv = this;

            var pilot = gameObject.EnsureComponent<AutoPilot>();
            pilot.mv = this;

            controlPanelLogic.Init();

            base.LazyInitialize();
        }
        public override void Start()
        {
            base.Start();

            upgradesInput.equipment = modules;
            modules.isAllowedToRemove = new IsAllowedToRemove(IsAllowedToRemove);
            gameObject.EnsureComponent<GameInfoIcon>().techType = GetComponent<TechTag>().type;

            // load upgrades from file

            // load storage from file

            // load modular storage from file
            VehicleManager.RegisterVehicle(this);
        }
        public override void FixedUpdate()
        {
            if (worldForces.IsAboveWater() != wasAboveWater)
            {
                //TODO
                //PlaySplashSound();
                Logger.Log("splash!");
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
            if (IsPlayerInside())
            {
                PlayerExit();
            }
            if (destructionEffect)
            {
                GameObject gameObject = Instantiate<GameObject>(destructionEffect);
                gameObject.transform.position = transform.position;
                gameObject.transform.rotation = transform.rotation;
            }
            Destroy(gameObject);
        }
        public override void OnUpgradeModuleChange(int slotID, TechType techType, bool added)
        {
            //Logger.Log(slotID.ToString() + " : " + techType.ToString() + " : " + added.ToString());
            switch (techType)
            {
                case TechType.VehicleStorageModule:
                    {
                        SetStorageModule(slotID, added);
                        break;
                    }
                case TechType.VehicleArmorPlating:
                    {
                        var temp = added ? numArmorModules++ : numArmorModules--;
                        GetComponent<DealDamageOnImpact>().mirroredSelfDamageFraction = 0.5f * Mathf.Pow(0.5f, (float)numArmorModules);
                        break;
                    }
                case TechType.VehiclePowerUpgradeModule:
                    {
                        var temp = added ? numEfficiencyModules++ : numEfficiencyModules--;
                        break;
                    }
                /*
                case TechType.VehicleDepthModule1:
                    break;
                case TechType.VehicleDepthModule2:
                    break;
                case TechType.VehicleDepthModule3:
                    break;
                case TechType.VehicleStealthModule:
                    break;
                */
                default:
                    break;
            }
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
        public void BeginPiloting()
        {
            base.EnterVehicle(Player.main, true);
            isPilotSeated = true;
            //uGUI.main.transform.Find("ScreenCanvas/HUD/Content/QuickSlots").gameObject.SetActive(true);
            uGUI.main.quickSlots.SetTarget(this);
            NotifyStatus(VehicleStatus.OnPilotBegin);
        }
        public void StopPiloting()
        {
            // this function
            // called by Player.ExitLockedMode()
            // which is triggered on button press
            isPilotSeated = false;
            Player.main.transform.position = transform.Find("Hatch").position - transform.Find("Hatch").up;
            //uGUI.main.transform.Find("ScreenCanvas/HUD/Content/QuickSlots").gameObject.SetActive(false);
            uGUI.main.quickSlots.SetTarget(null);
            NotifyStatus(VehicleStatus.OnPilotEnd);
        }
        public void PlayerEntry()
        {
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

            //uGUI.main.transform.Find("ScreenCanvas/HUD/Content/QuickSlots").gameObject.SetActive(false);

            foreach (GameObject window in CanopyWindows)
            {
                window.SetActive(false);
            }

            NotifyStatus(VehicleStatus.OnPlayerEntry);
        }
        public void PlayerExit()
        {
            isPlayerInside = false;
            Player.main.currentMountedVehicle = null;
            Player.main.transform.SetParent(null);
            foreach (GameObject window in CanopyWindows)
            {
                window.SetActive(true);
            }
            NotifyStatus(VehicleStatus.OnPlayerExit);
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
		public override string[] slotIDs
		{
			get
			{
				return _slotIDs;
			}
        }
        private static readonly string[] _slotIDs = new string[]
        {
            "VehicleModule0",
            "VehicleModule1",
            "VehicleModule2",
            "VehicleModule3",
            "VehicleModule4",
            "VehicleModule5",
            "VehicleArmLeft",
            "VehicleArmRight"
        };
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
                        if(0 <= slotID && slotID <= 1)
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
        public void TrySpendEnergy(float val)
        {
            float desired = val;
            float available = energyInterface.TotalCanProvide(out _);
            if (available < desired)
            {
                desired = available;
            }
            energyInterface.ConsumeEnergy(desired);
        }
        public void TogglePower()
        {
            IsDisengaged = !IsDisengaged;
            if(!IsDisengaged)
            {
                gameObject.GetComponent<VehicleLights>().EnableInteriorLighting();
                gameObject.GetComponent<VehicleLights>().EnableExteriorLighting();
            }
        }
        public void NotifyStatus(VehicleStatus vs)
        {
            foreach (var component in GetComponentsInChildren<IVehicleStatusListener>())
            {
                switch (vs)
                {
                    case VehicleStatus.OnPlayerEntry:
                        component.OnPlayerEntry();
                        break;
                    case VehicleStatus.OnPlayerExit:
                        component.OnPlayerExit();
                        break;
                    case VehicleStatus.OnPilotBegin:
                        component.OnPilotBegin();
                        break;
                    case VehicleStatus.OnPilotEnd:
                        component.OnPilotEnd();
                        break;
                    case VehicleStatus.OnPowerUp:
                        component.OnPowerUp();
                        break;
                    case VehicleStatus.OnPowerDown:
                        component.OnPowerDown();
                        break;
                    case VehicleStatus.OnExteriorLightsOn:
                        component.OnExteriorLightsOn();
                        break;
                    case VehicleStatus.OnExteriorLightsOff:
                        component.OnExteriorLightsOff();
                        break;
                    case VehicleStatus.OnInteriorLightsOn:
                        component.OnInteriorLightsOn();
                        break;
                    case VehicleStatus.OnInteriorLightsOff:
                        component.OnInteriorLightsOff();
                        break;
                    case VehicleStatus.OnTakeDamage:
                        component.OnTakeDamage();
                        break;
                    case VehicleStatus.OnAutoLevel:
                        component.OnAutoLevel();
                        break;
                    case VehicleStatus.OnAutoPilotBegin:
                        component.OnAutoPilotBegin();
                        break;
                    case VehicleStatus.OnAutoPilotEnd:
                        component.OnAutoPilotEnd();
                        break;
                    default:
                        Logger.Log("Error: tried to notify using an invalid status");
                        break;
                }
            }
        }


    }
}
