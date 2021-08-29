﻿using System;
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
        public abstract List<VehicleParts.VehicleStorage> Storages { get; }
        public abstract List<VehicleParts.VehicleUpgrades> Upgrades { get; }
        public abstract List<VehicleParts.VehicleBattery> Batteries { get; }
        public abstract List<VehicleParts.VehicleLight> Lights { get; }
        public abstract List<GameObject> WalkableInteriors { get; }


        public FMOD_StudioEventEmitter lightsOnSound = null;
        public FMOD_StudioEventEmitter lightsOffSound = null;
        public List<GameObject> lights = new List<GameObject>();
        public List<GameObject> volumetricLights = new List<GameObject>();
        public PingInstance pingInstance = null;
        public FMOD_StudioEventEmitter ambienceSound;
        public List<Renderer> interiorRenderers = new List<Renderer>();

        public Equipment upgradesEquipment;

        private bool isPilotSeated = false;
        private bool isPlayerInside = false;

        // later
        public virtual List<GameObject> ControlPanels => null;
        public virtual List<GameObject> Arms => null;
        public virtual List<GameObject> Legs => null;

        // not sure what types these should be
        public virtual List<GameObject> SoundEffects => null;
        public virtual List<GameObject> TwoDeeAssets => null;

        public override void Awake()
        {
            base.Awake();

            var light = gameObject.EnsureComponent<VehicleLights>();
            light.mv = this;

            var gauge = gameObject.EnsureComponent<FuelGauge>();
            gauge.mv = this;

            var pilot = gameObject.EnsureComponent<AutoPilot>();
            pilot.mv = this;

            upgradesInput.equipment = new Equipment(gameObject, modulesRoot.transform);
            upgradesInput.equipment.SetLabel("VehicleUpgradesStorageLabel");

            VehicleManager.RegisterVehicle(this);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
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

        public override void EnterVehicle(Player player, bool teleport, bool playEnterAnimation = true)
        {
            base.EnterVehicle(player, teleport, playEnterAnimation);
        }
        public void BeginPiloting()
        {
            base.EnterVehicle(Player.main, true);
            isPilotSeated = true;
            BroadcastMessage("OnPilotBegin");
        }

        // called by Player.ExitLockedMode()
        // which is triggered on button press
        public void StopPiloting()
        {
            isPilotSeated = false;
            Player.main.transform.position = transform.Find("Hatch").position - transform.Find("Hatch").up;
            BroadcastMessage("OnPilotEnd");
        }

        public void PlayerEntry()
        {
            isPlayerInside = true;
            Player.main.currentMountedVehicle = this;
            Player.main.transform.parent = transform;


            Player.main.playerController.activeController.SetUnderWater(false);
            Player.main.isUnderwater.Update(false);
            Player.main.isUnderwaterForSwimming.Update(false);
            Player.main.playerController.SetMotorMode(Player.MotorMode.Walk);
            Player.main.motorMode = Player.MotorMode.Walk;
            Player.main.SetScubaMaskActive(false);
            Player.main.playerMotorModeChanged.Trigger(Player.MotorMode.Walk);


            BroadcastMessage("OnPlayerEntry");
        }
        public void PlayerExit()
        {
            isPlayerInside = false;
            Player.main.currentMountedVehicle = null;
            BroadcastMessage("OnPlayerExit");
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
        public override bool CanPilot()
        {
            return !FPSInputModule.current.lockMovement && base.IsPowered();
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
            "VehicleModule1",
            "VehicleModule2",
            "VehicleModule3",
            "VehicleModule4",
            "VehicleModule5",
            "VehicleModule6",
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
        public override InventoryItem GetSlotItem(int slotID)
        {
            if (slotID < 0 || slotID >= this.slotIDs.Length)
            {
                return null;
            }
            string slot = this.slotIDs[slotID];
            return upgradesEquipment.GetItemInSlot(slot);
        }

        */

    }
}
