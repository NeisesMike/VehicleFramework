using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleParts;

using VehicleFramework.Engines;
using VehicleFramework;

namespace VehicleFramework.Configuration
{
    /// <summary>
    /// Read-only vehicle configuration
    /// </summary>
    /*
    internal class VehicleConfiguration
    {
        /// <summary>
        /// Sprite to show when the camera is sufficiently far away while the vehicle is not boarded.
        /// Also used on the map, if used.
        /// </summary>
        public Sprite PingSprite { get; }
        /// <summary>
        /// Sprite to attach to the save file in the preview.
        /// Should be very abstract, ideally just an outline.
        /// </summary>
        public Sprite SaveFileSprite { get; }
        /// <summary>
        /// Construction recipe.
        /// </summary>
        public Recipe Recipe { get; } = Recipe.Example;
        /// <summary>
        /// If true, the recipe can be overridden by a JSON file created in the "recipes" folder.
        /// If so, the imported recipe is passed to <see cref="AvsVehicle.OnRecipeOverride"/> before being applied.
        /// </summary>
        public bool AllowRecipeOverride { get; } = true;
        /// <summary>
        /// Optional sprite that shows in the popup when the tech type of this vehicle is unlocked
        /// </summary>
        public Sprite? UnlockedSprite { get; } = null;
        /// <summary>
        /// Localized description of the vehicle.
        /// </summary>
        public string Description { get; } = "A vehicle";
        /// <summary>
        /// Localized encyclopedia entry for this vehicle.
        /// </summary>
        public string EncyclopediaEntry { get; } = "";
        /// <summary>
        /// Image to show in the encyclopedia entry, if any.
        /// </summary>
        public Sprite? EncyclopediaImage { get; } = null;
        /// <summary>
        /// The sprite to show in the crafting menu of the mobile vehicle bay.
        /// </summary>
        public Sprite CraftingSprite { get; }
        /// <summary>
        /// The image to show in the background of the vehicle's module menu.
        /// </summary>
        public Sprite? ModuleBackgroundImage { get; }
        /// <summary>
        /// Type that, if unlocked, also automatically unlocks this vehicle for crafting.
        /// </summary>
        public TechType UnlockedWith { get; } = TechType.Constructor;
        /// <summary>
        /// Maximum health of the vehicle.
        /// 100 is very low.
        /// </summary>
        public int MaxHealth { get; } = 100;    //required > 0
        /// <summary>
        /// Absolute damage dealt to the vehicle when it decended below its crush depth.
        /// </summary>
        public int CrushDamage { get; } = 7; //= MaxHealth / 15;
        /// <summary>
        /// Number of times per second the vehicle will take damage when below its crush depth.
        /// </summary>
        public float CrushDamageFrequency { get; } = 1;

        /// <summary>
        /// Absolute damage dealt to the vehicle when it is bit by a adult ghost leviathan.
        /// </summary>
        public float GhostAdultBiteDamage { get; } = 150f;
        /// <summary>
        /// Absolute damage dealt to the vehicle when it is bit by a juvenile ghost leviathan.
        /// </summary>
        public float GhostJuvenileBiteDamage { get; } = 100f;
        /// <summary>
        /// Absolute damage dealt to the vehicle when it is bit by a reaper leviathan.
        /// </summary>
        public float ReaperBiteDamage { get; } = 120f;
        /// <summary>
        /// Physical mass of the vehicle. Must be greater than 0.
        /// For reference,
        /// Cyclop: 12000
        /// Abyss: 5000
        /// Atrama: 4250
        /// Odyssey: 3500
        /// Prawn: 1250
        /// Seamoth: 800
        /// </summary>
        public int Mass { get; } = 1000;
        /// <summary>
        /// Maximum number of modules that can be installed on this vehicle.
        /// </summary>
        public int NumModules { get; } = 4;
        /// <summary>
        /// PDA message shown when the vehicle is unlocked.
        /// </summary>
        public string UnlockedMessage { get; } = "New vehicle blueprint acquired";
        /// <summary>
        /// Gets the base crush depth of the vehicle, measured in meters.
        /// If it decends below this depth and there are up upgrades installed, it will take damage.
        /// Must be greater than 0.
        /// </summary>
        public int BaseCrushDepth { get; } = 300;
        /// <summary>
        /// Crush depth increase if a level 1 depth upgrade is installed.
        /// </summary>
        public int CrushDepthUpgrade1 { get; } = 200;
        /// <summary>
        /// Crush depth increase if a level 2 depth upgrade is installed.
        /// </summary>
        public int CrushDepthUpgrade2 { get; } = 600;
        /// <summary>
        /// Crush depth increase if a level 3 depth upgrade is installed.
        /// </summary>
        public int CrushDepthUpgrade3 { get; } = 600;

        /// <summary>
        /// The piloting style of the vehicle. Affects player animations.
        /// </summary>
        public PilotingStyle PilotingStyle { get; } = PilotingStyle.Other;
        /// <summary>
        /// The number of seconds it takes to construct the vehicle in the mobile vehicle bay.
        /// Reference times: Seamoth : 10 seconds, Cyclops : 20, Rocket Base : 25
        /// </summary>
        public float TimeToConstruct { get; } = 15f;
        /// <summary>
        /// Gets the color used for rendering construction ghost objects.
        /// Applied only if not black.
        /// </summary>
        public Color ConstructionGhostColor { get; } = Color.black;
        /// <summary>
        /// Gets the color used for rendering construction wireframes.
        /// Applied only if not black.
        /// </summary>
        public Color ConstructionWireframeColor { get; } = Color.black;
        /// <summary>
        /// True if the vehicle can be grabbed by a leviathan.
        /// </summary>
        public bool CanLeviathanGrab { get; set; } = true;
        /// <summary>
        /// True if the vehicle can be docked in a moonpool.
        /// </summary>
        public bool CanMoonpoolDock { get; set; } = true;
        /// <summary>
        /// Rotation applied when docking the vehicle in a cyclops.
        /// </summary>
        public Quaternion CyclopsDockRotation { get; } = Quaternion.identity;
        /// <summary>
        /// True to automatically correct shaders of the vehicle's materials.
        /// </summary>
        public bool AutoFixMaterials { get; } = true;

        /// <summary>
        /// Material adaptation configuration. If not provided, initialized with a new instance of <see cref="DefaultMaterialAdaptConfig" />.
        /// Effective only if <see cref="AutoFixMaterials"/> is true.
        /// </summary>
        public IMaterialAdaptConfig MaterialAdaptConfig { get; }


        /// <summary>
        /// The initial base color of the vehicle.
        /// </summary>
        public VehicleColor InitialBaseColor { get; set; } = VehicleColor.Default;
        /// <summary>
        /// The initial stripe color of the vehicle.
        /// </summary>
        public VehicleColor InitialStripeColor { get; set; } = VehicleColor.Default;
        /// <summary>
        /// The initial interior color of the vehicle.
        /// </summary>
        public VehicleColor InitialInteriorColor { get; set; } = VehicleColor.Default;
        /// <summary>
        /// The initial name color of the vehicle.
        /// </summary>
        public VehicleColor InitialNameColor { get; set; } = VehicleColor.Default;

        /// <summary>
        /// Gets the current setting regarding sound volume for voice messages sent by the vehicle's <see cref="VoiceQueue"/> component,
        /// further modified by Subnautica's global voice and master sound volumes.
        /// </summary>
        public Func<float> GetVoiceSoundVolume { get; }
        /// <summary>
        /// Gets the current setting whether to show subtitles for voice messages sent by the vehicle's <see cref="VoiceQueue"/> component.
        /// </summary>
        public Func<bool> GetVoiceSubtitlesEnabled { get; }

        /// <summary>
        /// True if the HUD temperature display is in Fahrenheit.
        /// Defaults to false.
        /// </summary>
        public bool HudTemperatureIsFahrenheit { get; }

        /// <summary>
        /// True if the player can enter the helm of the vehicle even if the vehicle has no power.
        /// Defaults to false.
        /// </summary>
        public bool CanEnterHelmWithoutPower { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleConfiguration"/> class with the specified parameters.
        /// </summary>
        /// <param name="pingSprite">Sprite to show when the camera is sufficiently far away. Also used on the map, if used.</param>
        /// <param name="saveFileSprite">Sprite to attach to the save file in the preview. Should be very abstract, ideally just an outline.</param>
        /// <param name="recipe">Construction recipe. If null, uses <see cref="Recipe.Example"/>.</param>
        /// <param name="allowRecipeOverride">If true, the recipe can be overridden by a JSON file created in the "recipes" folder.</param>
        /// <param name="unlockedSprite">Sprite shown when the vehicle is unlocked.</param>
        /// <param name="description">Localized description of the vehicle.</param>
        /// <param name="encyclopediaEntry">Localized encyclopedia entry for this vehicle.</param>
        /// <param name="encyclopediaImage">Image to show in the encyclopedia entry, if any.</param>
        /// <param name="craftingSprite">The sprite to show in the crafting menu of the mobile vehicle bay.</param>
        /// <param name="moduleBackgroundImage">The image to show in the background of the vehicle's module menu.</param>
        /// <param name="unlockedWith">Type that, if unlocked, also automatically unlocks this vehicle for crafting.</param>
        /// <param name="maxHealth">Maximum health of the vehicle. Must be greater than 0.</param>
        /// <param name="crushDamage">Absolute damage dealt to the vehicle when it descends below its crush depth.</param>
        /// <param name="ghostAdultBiteDamage">Absolute damage dealt to the vehicle when it is bit by an adult ghost leviathan.</param>
        /// <param name="ghostJuvenileBiteDamage">Absolute damage dealt to the vehicle when it is bit by a juvenile ghost leviathan.</param>
        /// <param name="reaperBiteDamage">Absolute damage dealt to the vehicle when it is bit by a reaper leviathan.</param>
        /// <param name="mass">Physical mass of the vehicle. Must be greater than 0.
        /// For reference,
        /// Cyclop: 12000
        /// Abyss: 5000
        /// Atrama: 4250
        /// Odyssey: 3500
        /// Prawn: 1250
        /// Seamoth: 800
        /// </param>
        /// <param name="numModules">Maximum number of modules that can be installed on this vehicle.</param>
        /// <param name="unlockedMessage">PDA message shown when the vehicle is unlocked.</param>
        /// <param name="baseCrushDepth">Base crush depth of the vehicle, measured in meters. Must be greater than 0.</param>
        /// <param name="crushDepthUpgrade1">Crush depth increase if a level 1 depth upgrade is installed.</param>
        /// <param name="crushDepthUpgrade2">Crush depth increase if a level 2 depth upgrade is installed.</param>
        /// <param name="crushDepthUpgrade3">Crush depth increase if a level 3 depth upgrade is installed.</param>
        /// <param name="crushDamageFrequency">Number of times per second the vehicle will take damage when below its crush depth.</param>
        /// <param name="pilotingStyle">The piloting style of the vehicle. Affects player animations.</param>
        /// <param name="timeToConstruct">The number of seconds it takes to construct the vehicle in the mobile vehicle bay.</param>
        /// <param name="constructionGhostColor">Color used for rendering construction ghost objects. Applied only if not black.</param>
        /// <param name="constructionWireframeColor">Color used for rendering construction wireframes. Applied only if not black.</param>
        /// <param name="canLeviathanGrab">True if the vehicle can be grabbed by a leviathan.</param>
        /// <param name="canMoonpoolDock">True if the vehicle can be docked in a moonpool.</param>
        /// <param name="cyclopsDockRotation">Rotation applied when docking the vehicle in a cyclops.</param>
        /// <param name="autoFixMaterials">True to automatically correct shaders to the vehicle's materials.</param>
        /// <param name="initialBaseColor">Initial base color of the vehicle. If null, defaults to <see cref="VehicleColor.Default"/>.</param>
        /// <param name="initialStripeColor">Initial stripe color of the vehicle. If null, defaults to <see cref="VehicleColor.Default"/>.</param>
        /// <param name="initialInteriorColor">Initial interior color of the vehicle. If null, defaults to <see cref="VehicleColor.Default"/>.</param>
        /// <param name="initialNameColor">Initial name color of the vehicle. If null, defaults to <see cref="VehicleColor.Default"/>.</param>
        /// <param name="getVoiceSoundVolume">Query function to get the sound volume for voice messages sent by the vehicle's <see cref="VoiceQueue"/> component. If null, defaults to always 1</param>
        /// <param name="getVoiceSubtitlesEnabled">Query function to get whether to show subtitles for voice messages sent by the vehicle's <see cref="VoiceQueue"/> component. If null, defaults to always false</param>
        /// <param name="materialAdaptConfig">Optional configuration for material adaptation</param>
        /// <param name="hudTemperatureIsFahrenheit">True if the HUD temperature display is in Fahrenheit.</param>
        /// <param name="canEnterHelmWithoutPower">True if the player can enter the helm of the vehicle even if the vehicle has no power.</param>
        public VehicleConfiguration(

            Sprite pingSprite,
            Sprite saveFileSprite,
            Sprite unlockedSprite,
            Sprite moduleBackgroundImage,
            Sprite craftingSprite,
            VehicleColor? initialBaseColor = null,
            VehicleColor? initialStripeColor = null,
            VehicleColor? initialInteriorColor = null,
            VehicleColor? initialNameColor = null,
            IMaterialAdaptConfig? materialAdaptConfig = null,
            Recipe? recipe = null,
            bool allowRecipeOverride = true,
            string description = "A vehicle",
            string encyclopediaEntry = "",
            Sprite? encyclopediaImage = null,
            TechType unlockedWith = TechType.Constructor,
            int maxHealth = 100,
            int? crushDamage = null,
            float ghostAdultBiteDamage = 150f,
            float ghostJuvenileBiteDamage = 100f,
            float reaperBiteDamage = 120f,
            int mass = 1000,
            int numModules = 4,
            string unlockedMessage = "New vehicle blueprint acquired",
            int baseCrushDepth = 300,
            int crushDepthUpgrade1 = 200,
            int crushDepthUpgrade2 = 600,
            int crushDepthUpgrade3 = 600,
            float crushDamageFrequency = 1,
            PilotingStyle pilotingStyle = PilotingStyle.Other,
            float timeToConstruct = 15f,
            Color? constructionGhostColor = null,
            Color? constructionWireframeColor = null,
            bool canLeviathanGrab = true,
            bool canMoonpoolDock = true,
            Quaternion? cyclopsDockRotation = null,
            bool autoFixMaterials = true,
            Func<float>? getVoiceSoundVolume = null,
            Func<bool>? getVoiceSubtitlesEnabled = null,
            bool hudTemperatureIsFahrenheit = false,
            bool canEnterHelmWithoutPower = false
        )
        {
            if (maxHealth <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(maxHealth), "MaxHealth must be greater than 0.");
            if (mass <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(mass), "Mass must be greater than 0.");
            if (baseCrushDepth <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(baseCrushDepth), "BaseCrushDepth must be greater than 0.");

            HudTemperatureIsFahrenheit = hudTemperatureIsFahrenheit;
            GetVoiceSoundVolume = getVoiceSoundVolume ?? (() => 1);
            GetVoiceSubtitlesEnabled = getVoiceSubtitlesEnabled ?? (() => false);
            InitialBaseColor = initialBaseColor ?? VehicleColor.Default;
            InitialStripeColor = initialStripeColor ?? VehicleColor.Default;
            InitialInteriorColor = initialInteriorColor ?? VehicleColor.Default;
            InitialNameColor = initialNameColor ?? VehicleColor.Default;
            MaterialAdaptConfig = materialAdaptConfig ?? new DefaultMaterialAdaptConfig();
            PingSprite = pingSprite;
            SaveFileSprite = saveFileSprite;
            Recipe = recipe ?? Recipe.Example;
            AllowRecipeOverride = allowRecipeOverride;
            UnlockedSprite = unlockedSprite;
            Description = description;
            EncyclopediaEntry = encyclopediaEntry;
            EncyclopediaImage = encyclopediaImage;
            CraftingSprite = craftingSprite;
            ModuleBackgroundImage = moduleBackgroundImage;
            UnlockedWith = unlockedWith;
            MaxHealth = maxHealth;
            CrushDamage = crushDamage ?? (maxHealth / 15);
            GhostAdultBiteDamage = ghostAdultBiteDamage;
            GhostJuvenileBiteDamage = ghostJuvenileBiteDamage;
            ReaperBiteDamage = reaperBiteDamage;
            Mass = mass;
            NumModules = numModules;
            UnlockedMessage = unlockedMessage;
            BaseCrushDepth = baseCrushDepth;
            CrushDepthUpgrade1 = crushDepthUpgrade1;
            CrushDepthUpgrade2 = crushDepthUpgrade2;
            CrushDepthUpgrade3 = crushDepthUpgrade3;
            CrushDamageFrequency = crushDamageFrequency;
            PilotingStyle = pilotingStyle;
            TimeToConstruct = timeToConstruct;
            ConstructionGhostColor = constructionGhostColor ?? Color.black;
            ConstructionWireframeColor = constructionWireframeColor ?? Color.black;
            CanLeviathanGrab = canLeviathanGrab;
            CanMoonpoolDock = canMoonpoolDock;
            CyclopsDockRotation = cyclopsDockRotation ?? Quaternion.identity;
            AutoFixMaterials = autoFixMaterials;
            CanEnterHelmWithoutPower = canEnterHelmWithoutPower;
        }
    }
    */
}
