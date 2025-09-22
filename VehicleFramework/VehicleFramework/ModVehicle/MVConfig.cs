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
     * This file holds those configuration values that can be set by the modder.
     */
    public abstract partial class ModVehicle : Vehicle, ICraftTarget, IProtoTreeEventListener
    {
        public virtual int BaseCrushDepth => 250;
        public virtual int MaxHealth => 100;
        public virtual int Mass => 1000;
        public virtual int NumModules => 4;
        public virtual bool HasArms => false;
        public virtual TechType UnlockedWith => TechType.Constructor;
        public virtual string UnlockedMessage => "New vehicle blueprint acquired";
        public virtual int CrushDepthUpgrade1 => 300;
        public virtual int CrushDepthUpgrade2 => 300;
        public virtual int CrushDepthUpgrade3 => 300;
        public virtual int CrushDamage => MaxHealth / 15;
        public virtual int CrushPeriod => 1;
        public virtual PilotingStyleEnum PilotingStyle => PilotingStyleEnum.Other; // used by Echelon, Odyssey, Blossom
        public virtual float GhostAdultBiteDamage => 150f;
        public virtual float GhostJuvenileBiteDamage => 100f;
        public virtual float ReaperBiteDamage => 120f;
        
        
        public virtual bool CanLeviathanGrab { get; set; } = true;
        public virtual bool CanMoonpoolDock { get; set; } = true;
        public virtual float TimeToConstruct { get; set; } = 15f; // Seamoth : 10 seconds, Cyclops : 20, Rocket Base : 25
        public virtual Color ConstructionGhostColor { get; set; } = Color.black;
        public virtual Color ConstructionWireframeColor { get; set; } = Color.black;
        public virtual bool AutoApplyShaders { get; set; } = true;
        public virtual List<TMPro.TextMeshProUGUI>? SubNameDecals => null;
        public virtual Quaternion CyclopsDockRotation => Quaternion.identity;

        public virtual Sprite? PingSprite => StaticAssets.DefaultPingSprite;
        public virtual Sprite? SaveFileSprite => StaticAssets.DefaultSaveFileSprite;
        public virtual Dictionary<TechType, int>? Recipe => new() { { TechType.Titanium, 1 } };
        public virtual Sprite? UnlockedSprite => null;
        public virtual Sprite? CraftingSprite => StaticAssets.ModVehicleIcon;
        public virtual string Description => "A vehicle";
        public virtual string EncyclopediaEntry => string.Empty;
        public virtual Sprite? EncyclopediaImage => null;
        public virtual Sprite? ModuleBackgroundImage => SpriteHelper.GetSprite("Sprites/VFModuleBackground.png");
    }
}
