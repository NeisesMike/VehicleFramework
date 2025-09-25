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

namespace VehicleFramework
{
    /*
     * ModVehicle is the primary abstract class provided by Vehicle Framework.
     * All VF vehicles inherit from ModVehicle.
     * This file holds those virtual members of ModVehicles that require assets (models, sprites, etc)
     */
    public abstract partial class ModVehicle : Vehicle, ICraftTarget, IProtoTreeEventListener
    {
        #region abstract_members
        public abstract GameObject VehicleModel { get; } 
        public abstract GameObject[] CollisionModel { get; }
        #endregion

        public virtual GameObject StorageRootObject
        {
            get
            {
                GameObject? storageRO = transform.Find("StorageRootObject")?.gameObject;
                if (storageRO == null)
                {
                    storageRO = new("StorageRootObject");
                    storageRO.transform.SetParent(transform);
                }
                return storageRO;
            }
        }
        public virtual GameObject ModulesRootObject
        {
            get
            {
                GameObject? storageRO = transform.Find("ModulesRootObject")?.gameObject;
                if (storageRO == null)
                {
                    storageRO = new("ModulesRootObject");
                    storageRO.transform.SetParent(transform);
                }
                return storageRO;
            }
        }
        public virtual List<VehicleBattery>? Batteries => new();
        public virtual List<VehicleUpgrades>? Upgrades => new();
        public virtual VehicleArmsProxy Arms { get; set; }
        public virtual BoxCollider? BoundingBoxCollider { get; set; }
        public virtual List<GameObject>? WaterClipProxies => new();
        public virtual List<VehicleStorage>? InnateStorages => new();
        public virtual List<VehicleStorage>? ModularStorages => new();
        public virtual List<VehicleFloodLight>? HeadLights => new();
        public virtual List<GameObject>? CanopyWindows => new();
        public virtual GameObject? LeviathanGrabPoint => gameObject;
        public virtual List<Transform>? LavaLarvaAttachPoints => new();
        public virtual List<VehicleCamera>? Cameras => new();
        public virtual List<Collider>? DenyBuildingColliders => new();

        public virtual Sprite? PingSprite => StaticAssets.DefaultPingSprite;
        public virtual Sprite? SaveFileSprite => StaticAssets.DefaultSaveFileSprite;
        public virtual Sprite? UnlockedSprite => null;
        public virtual Sprite? CraftingSprite => StaticAssets.ModVehicleIcon;
        public virtual Sprite? EncyclopediaImage => null;
        public virtual Sprite? ModuleBackgroundImage => SpriteHelper.GetSprite("Sprites/VFModuleBackground.png");

        public virtual FMOD_CustomEmitter? LightsOnSound { get; set; } = null;
        public virtual FMOD_CustomEmitter? LightsOffSound { get; set; } = null;
    }
}
