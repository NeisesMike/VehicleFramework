using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VehicleFramework.Engines;
using VehicleFramework;
using UnityEngine;
using VehicleFramework.VehicleBuilding;

namespace VehicleFramework.Composition
{
    /// <summary>
    /// Objects, transforms, and components as identified by the derived mod vehicle.
    /// GameObjects, Transforms, and Components are expected to be contained by the vehicle
    /// or its children.
    /// </summary>
    internal class VehicleComposition
    {
        /// <summary>
        /// The parent object for all storage objects.
        /// Must not be null. Must be different from the vehicle game object.
        /// </summary>
        public GameObject StorageRootObject { get; }

        /// <summary>
        /// The parent object for all modules.
        /// Must not be null. Must be different from the vehicle game object.
        /// </summary>
        public GameObject ModulesRootObject { get; }

        /// <summary>
        /// Base objects containing colliders (and nothing else).
        /// AVS can do without but the Subnautica system uses these object to switch off colliders while docked.
        /// Therefore, this value must be set, even if the referenced game objects contain nothing.
        /// Should not contain the vehicle root as this would disable everything.
        /// </summary>
        public GameObject[] CollisionModel { get; }

        /// <summary>
        /// Entry/exit hatches for the submarine.
        /// Required not empty
        /// </summary>
        public IReadOnlyList<VehicleHatch> Hatches { get; } = Array.Empty<VehicleHatch>();

        /// <summary>
        /// Power cell definitions. Can be empty which disallows any batteries.
        /// </summary>
        public IReadOnlyList<VehicleBattery> Batteries { get; } = Array.Empty<VehicleBattery>();

        /// <summary>
        /// Upgrade module definitions. Can be empty which disallows any upgrades.
        /// </summary>
        public IReadOnlyList<VehicleUpgrades> Upgrades { get; } = Array.Empty<VehicleUpgrades>();

        /// <summary>
        /// Single box collider that contains the entire vehicle.
        /// While the code can handle this not being set, it really should be set.
        /// </summary>
        public BoxCollider? BoundingBoxCollider { get; }

        /// <summary>
        /// Empty game objects that each define a box that clips the water surface.
        /// These objects must not contain any components (renderers or otherwise).
        /// The position identifies their center, the size their extents.
        /// </summary>
        public IReadOnlyList<GameObject> WaterClipProxies { get; } = Array.Empty<GameObject>();


        /// <summary>
        /// Mobile water parks in the vehicle.
        /// </summary>
        //public IReadOnlyList<MobileWaterPark> WaterParks { get; }

        /// <summary>
        /// Storages that the vehicle always has.
        /// </summary>
        public IReadOnlyList<VehicleStorage> InnateStorages { get; }
        /// <summary>
        /// Storages that can be added to the vehicle by the player.
        /// </summary>
        public IReadOnlyList<VehicleStorage> ModularStorages { get; }

        /// <summary>
        /// Collection and configuration of headlights, to be rendered volumetrically while the player is outside the vehicle. Controllable while piloting.
        /// </summary>
        public IReadOnlyList<VehicleFloodLight> Headlights { get; } = Array.Empty<VehicleFloodLight>();

        /// <summary>
        /// Collection and configuration of headlights, to be rendered volumetrically while the player is outside the vehicle.
        /// </summary>
        public IReadOnlyList<VehicleFloodLight> FloodLights { get; } = Array.Empty<VehicleFloodLight>();

        /// <summary>
        /// Window objects automatically hidden when the vehicle is being piloted as to avoid reflections.
        /// </summary>
        public IReadOnlyList<GameObject> CanopyWindows { get; } = Array.Empty<GameObject>();

        /// <summary>
        /// Batteries exclusively used for the AI. Not sure anyone uses these.
        /// </summary>
        public IReadOnlyList<VehicleBattery> BackupBatteries { get; } = Array.Empty<VehicleBattery>();


        /// <summary>
        /// Base object building is not permitted within these colliders.
        /// </summary>
        public IReadOnlyList<Collider> DenyBuildingColliders { get; } = Array.Empty<Collider>();

        /// <summary>
        /// Text output objects containing the vehicle name decals.
        /// </summary>
        public IReadOnlyList<TMPro.TextMeshProUGUI> SubNameDecals { get; } = Array.Empty<TMPro.TextMeshProUGUI>();

        /// <summary>
        /// Contains the attach points for the Lava Larvae (to suck away energy).
        /// If empty, the vehicle will not be attacked by Lava Larvae.
        /// </summary>
        public IReadOnlyList<Transform> LavaLarvaAttachPoints { get; } = Array.Empty<Transform>();
        /// <summary>
        /// Leviathan grab point, used by the Leviathan to grab the vehicle.
        /// If empty, the vehicle's own object is used as the grab point.
        /// </summary>
        public GameObject? LeviathanGrabPoint { get; }

        /// <summary>
        /// The engine that powers the vehicle. Must not be null.
        /// </summary>
        /// <remarks>
        /// The engine must be instantiated and attached during or before querying the vehicle's
        /// composition.
        /// As such, it is the only part that is not just derived from the model but rather newly
        /// created on demand.
        /// It is contained here because the mod vehicle requires it and it must be custom defined
        /// by the client vehicle.
        /// </remarks>
        public VFEngine Engine { get; }

        /// <summary>
        /// Constructs a VehicleComposition with required and optional parts.
        /// </summary>
        /// <param name="storageRootObject">The parent object for all storage objects. Must not be null and not the same as vehicle object.</param>
        /// <param name="modulesRootObject">The parent object for all modules. Must not be null and not the same as vehicle object.</param>
        /// <param name="collisionModel">Base objects containing all colliders. Must not be null. Should contain at least one object. Must not contain the submarine root. Expect these to be disabled</param>
        /// <param name="hatches">Entry/exit hatches. Must not be null or empty.</param>
        /// <param name="batteries">Power cell definitions. Optional.</param>
        /// <param name="upgrades">Upgrade module definitions. Optional.</param>
        /// <param name="boundingBoxCollider">Single box collider for the vehicle. Can be null.</param>
        /// <param name="waterClipProxies">Water clip proxies. Optional.</param>
        /// <param name="innateStorages">Innate storages. Optional.</param>
        /// <param name="modularStorages">Modular storages. Optional.</param>
        /// <param name="headlights">Headlights. Optional.</param>
        /// <param name="canopyWindows">Canopy windows. Optional.</param>
        /// <param name="backupBatteries">Backup batteries. Optional.</param>
        /// <param name="denyBuildingColliders">Deny building colliders. Optional.</param>
        /// <param name="subNameDecals">Sub name decals. Optional.</param>
        /// <param name="lavaLarvaAttachPoints">Lava larva attach points. Optional.</param>
        /// <param name="leviathanGrabPoint">Leviathan grab point. Optional.</param>
        /// <param name="waterParks">Mobile water parks. Optional.</param>
        /// <param name="engine">The engine that powers the vehicle. Must not be null.</param>
        public VehicleComposition(
            GameObject storageRootObject,
            GameObject modulesRootObject,
            IReadOnlyList<VehicleHatch> hatches,
            VFEngine engine,
            GameObject[] collisionModel,
            IReadOnlyList<VehicleBattery>? batteries = null,
            IReadOnlyList<VehicleUpgrades>? upgrades = null,
            BoxCollider? boundingBoxCollider = null,
            IReadOnlyList<GameObject>? waterClipProxies = null,
            IReadOnlyList<VehicleStorage>? innateStorages = null,
            IReadOnlyList<VehicleStorage>? modularStorages = null,
            IReadOnlyList<VehicleFloodLight>? headlights = null,
            //IReadOnlyList<VehicleFloodLight>? floodlights = null,
            IReadOnlyList<GameObject>? canopyWindows = null,
            IReadOnlyList<VehicleBattery>? backupBatteries = null,
            IReadOnlyList<Collider>? denyBuildingColliders = null,
            IReadOnlyList<TMPro.TextMeshProUGUI>? subNameDecals = null,
            IReadOnlyList<Transform>? lavaLarvaAttachPoints = null,
            //IReadOnlyList<MobileWaterPark>? waterParks = null,
            GameObject? leviathanGrabPoint = null
        )
        {
            if (!storageRootObject)
                throw new ArgumentNullException(nameof(storageRootObject));
            if (!modulesRootObject)
                throw new ArgumentNullException(nameof(modulesRootObject));
            if (hatches == null || hatches.Count == 0)
                throw new ArgumentException("Hatches must not be null or empty.", nameof(hatches));
            if (!engine)
                throw new ArgumentNullException(nameof(engine));
            Engine = engine;
            LeviathanGrabPoint = leviathanGrabPoint;
            StorageRootObject = storageRootObject;
            ModulesRootObject = modulesRootObject;
            CollisionModel = collisionModel;
            Hatches = hatches;
            Batteries = batteries ?? Array.Empty<VehicleBattery>();
            Upgrades = upgrades ?? Array.Empty<VehicleUpgrades>();
            BoundingBoxCollider = boundingBoxCollider;
            WaterClipProxies = waterClipProxies ?? Array.Empty<GameObject>();
            InnateStorages = innateStorages ?? Array.Empty<VehicleStorage>();
            ModularStorages = modularStorages ?? Array.Empty<VehicleStorage>();
            Headlights = headlights ?? Array.Empty<VehicleFloodLight>();
            CanopyWindows = canopyWindows ?? Array.Empty<GameObject>();
            BackupBatteries = backupBatteries ?? Array.Empty<VehicleBattery>();
            DenyBuildingColliders = denyBuildingColliders ?? Array.Empty<Collider>();
            SubNameDecals = subNameDecals ?? Array.Empty<TMPro.TextMeshProUGUI>();
            LavaLarvaAttachPoints = lavaLarvaAttachPoints ?? Array.Empty<Transform>();
            //WaterParks = waterParks ?? Array.Empty<MobileWaterPark>();

            if (Upgrades == null)
                throw new InvalidOperationException($"Something went wrong. Upgrades should not be null at this point. {upgrades}");
        }


    }
}
