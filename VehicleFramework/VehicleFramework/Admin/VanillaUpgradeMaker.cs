using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework.UpgradeTypes;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;

namespace VehicleFramework.Admin
{
    internal static class VanillaUpgradeMaker
    {
        internal static CustomPrefab CreateModuleVanilla(ModVehicleUpgrade upgrade, bool isPdaSetup, PrefabInfo info, Utils.UpgradePath path)
        {
            CustomPrefab prefab = new CustomPrefab(info);
            var clone = new CloneTemplate(info, TechType.SeamothElectricalDefense);
            prefab.SetGameObject(clone);
            Nautilus.Crafting.RecipeData moduleRecipe = new Nautilus.Crafting.RecipeData();
            moduleRecipe.Ingredients.AddRange(upgrade.GetRecipe());
            prefab
                .SetRecipe(moduleRecipe)
                .WithFabricatorType(CraftTree.Type.Workbench)
                .WithStepsToFabricatorTab(new string[] { "VVUM", Admin.Utils.UpgradePathToString(path) })
                .WithCraftingTime(upgrade.CraftingTime);
            if (!isPdaSetup)
            {
                prefab.SetPdaGroupCategory(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
            }
            if (upgrade.UnlockedSprite != null && !upgrade.UnlockAtStart)
            {
                var scanningGadget = prefab.SetUnlock(upgrade.UnlockWith);
                scanningGadget.WithAnalysisTech(upgrade.UnlockedSprite, unlockMessage: upgrade.UnlockedMessage);
            }
            return prefab;
        }

        #region CreationMethods
        internal static void CreatePassiveModule(ModVehicleUpgrade upgrade, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            bool isPdaRegistered = isPdaSetup;
            if (!compat.skipSeamoth)
            {
                CreatePassiveModuleSeamoth(upgrade, ref utt, isPdaRegistered);
                isPdaRegistered = true;
            }
            if (!compat.skipExosuit)
            {
                CreatePassiveModuleExosuit(upgrade, ref utt, isPdaRegistered);
                isPdaRegistered = true;
            }
            if (!compat.skipCyclops)
            {
                //CreatePassiveModuleCyclops(upgrade, ref utt);
            }
        }
        internal static void CreateSelectModule(SelectableUpgrade select, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            bool isPdaRegistered = isPdaSetup;
            if (!compat.skipSeamoth)
            {
                CreateSelectModuleSeamoth(select, ref utt, isPdaRegistered);
                isPdaRegistered = true;
            }
            if (!compat.skipExosuit)
            {
                CreateSelectModuleExosuit(select, ref utt, isPdaRegistered);
                isPdaRegistered = true;
            }
            if (!compat.skipCyclops)
            {
                //CreateSelectModuleCyclops(select, ref utt);
            }
        }
        internal static void CreateChargeModule(SelectableChargeableUpgrade selectcharge, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            bool isPdaRegistered = isPdaSetup;
            if (!compat.skipSeamoth)
            {
                CreateChargeModuleSeamoth(selectcharge, ref utt, isPdaRegistered);
                isPdaRegistered = true;
            }
            if (!compat.skipExosuit)
            {
                CreateChargeModuleExosuit(selectcharge, ref utt, isPdaRegistered);
                isPdaRegistered = true;
            }
            if (!compat.skipCyclops)
            {
                //CreateSelectModuleCyclops(select, ref utt);
            }
        }
        internal static void CreateToggleModule(ToggleableUpgrade toggle, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            bool isPdaRegistered = isPdaSetup;
            if (!compat.skipSeamoth)
            {
                CreateToggleModuleSeamoth(toggle, ref utt, isPdaRegistered);
                isPdaRegistered = true;
            }
            if (!compat.skipExosuit)
            {
                CreateToggleModuleExosuit(toggle, ref utt, isPdaRegistered);
                isPdaRegistered = true;
            }
            if (!compat.skipCyclops)
            {
                //CreateSelectModuleCyclops(select, ref utt);
            }
        }
        internal static void CreateExosuitArm(ModVehicleArm arm, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            //throw new NotImplementedException();
        }
        #endregion

        #region AddActions
        internal static void AddPassiveActions(UpgradeModuleGadget gadget, ModVehicleUpgrade upgrade, PrefabInfo info)
        {
            gadget
                 .WithOnModuleAdded((Vehicle vehicleInstance, int slotId) =>
                 {
                     UpgradeTypes.AddActionParams addedParams = new UpgradeTypes.AddActionParams
                     {
                         vehicle = vehicleInstance,
                         slotID = slotId,
                         techType = info.TechType,
                         isAdded = true
                     };
                     upgrade.OnAdded(addedParams);
                 })
                 .WithOnModuleRemoved((Vehicle vehicleInstance, int slotId) =>
                 {
                     UpgradeTypes.AddActionParams addedParams = new UpgradeTypes.AddActionParams
                     {
                         vehicle = vehicleInstance,
                         slotID = slotId,
                         techType = info.TechType,
                         isAdded = false
                     };
                     upgrade.OnAdded(addedParams);
                 });
        }
        internal static void AddSelectActions(UpgradeModuleGadget gadget, SelectableUpgrade upgrade, PrefabInfo info)
        {
            gadget
                .WithCooldown(upgrade.Cooldown)
                .WithEnergyCost(upgrade.EnergyCost)
                .WithOnModuleUsed((Vehicle vehicleInstance, int slotId, float charge, float chargeFraction) =>
                {
                    UpgradeTypes.SelectableActionParams selectParams = new UpgradeTypes.SelectableActionParams
                    {
                        vehicle = vehicleInstance,
                        slotID = slotId,
                        techType = info.TechType
                    };
                    upgrade.OnSelected(selectParams);
                });
        }
        internal static void AddToggleActions(UpgradeModuleGadget gadget, ToggleableUpgrade upgrade, PrefabInfo info)
        {
            gadget
                .WithEnergyCost(upgrade.EnergyCostPerActivation)
                .WithCooldown(upgrade.RepeatRate)
                .WithOnModuleToggled((Vehicle vehicleInstance, int slotId, float energyCost, bool isActive) =>
                {
                    UpgradeTypes.ToggleActionParams toggleParams = new UpgradeTypes.ToggleActionParams
                    {
                        vehicle = vehicleInstance,
                        slotID = slotId,
                        techType = info.TechType,
                        active = isActive
                    };
                    upgrade.OnRepeat(toggleParams);
                });
        }
        internal static void AddChargeActions(UpgradeModuleGadget gadget, SelectableChargeableUpgrade upgrade, PrefabInfo info)
        {
            gadget
                .WithMaxCharge(upgrade.MaxCharge) // this creates a harmless Nautilus warning
                .WithEnergyCost(upgrade.EnergyCost) // this creates a harmless Nautilus warning
                .WithOnModuleUsed((Vehicle vehicleInstance, int slotId, float charge, float chargeFraction) =>
                {
                    UpgradeTypes.SelectableChargeableActionParams chargeParams = new UpgradeTypes.SelectableChargeableActionParams
                    {
                        vehicle = vehicleInstance,
                        slotID = slotId,
                        techType = info.TechType,
                        charge = charge,
                        slotCharge = chargeFraction
                    };
                    upgrade.OnSelected(chargeParams);
                });
        }
        #endregion

        #region PassiveModules
        internal static TechType CreatePassiveModuleVanilla(ModVehicleUpgrade upgrade, bool isPdaSetup, PrefabInfo info, EquipmentType equipType, Utils.UpgradePath path)
        {
            CustomPrefab prefab = CreateModuleVanilla(upgrade, isPdaSetup, info, path);
            UpgradeModuleGadget gadget = prefab.SetVehicleUpgradeModule(equipType, QuickSlotType.Passive);
            AddPassiveActions(gadget, upgrade, info);
            prefab.Register();
            return info.TechType;
        }
        internal static void CreatePassiveModuleSeamoth(ModVehicleUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Seamoth", "Seamoth " + upgrade.DisplayName, "An upgrade for the Seamoth. " + upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            utt.forSeamoth = prefabInfo.TechType;
            CreatePassiveModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.SeamothModule, Utils.UpgradePath.Seamoth);
        }
        internal static void CreatePassiveModuleExosuit(ModVehicleUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Exosuit", "Exosuit " + upgrade.DisplayName, "An upgrade for the Exosuit. " + upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            utt.forExosuit = prefabInfo.TechType;
            CreatePassiveModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.ExosuitModule, Utils.UpgradePath.Exosuit);
        }
        internal static void CreatePassiveModuleCyclops(ModVehicleUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Cyclops", "Cyclops " + upgrade.DisplayName, "An upgrade for the Exosuit. " + upgrade.Description)
                .WithIcon(upgrade.Icon);
            utt.forCyclops = prefabInfo.TechType;
            CreatePassiveModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.CyclopsModule, Utils.UpgradePath.Cyclops);
        }
        #endregion

        #region SelectModules
        internal static TechType CreateSelectModuleVanilla(SelectableUpgrade upgrade, bool isPdaSetup, PrefabInfo info, EquipmentType equipType, Utils.UpgradePath path)
        {
            CustomPrefab prefab = CreateModuleVanilla(upgrade, isPdaSetup, info, path);
            UpgradeModuleGadget gadget = prefab.SetVehicleUpgradeModule(equipType, QuickSlotType.Selectable);
            AddPassiveActions(gadget, upgrade, info);
            AddSelectActions(gadget, upgrade, info);
            prefab.Register();
            return info.TechType;
        }
        internal static void CreateSelectModuleSeamoth(SelectableUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Seamoth", "Seamoth " + upgrade.DisplayName, "An upgrade for the Seamoth. " + upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            utt.forSeamoth = prefabInfo.TechType;
            CreateSelectModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.SeamothModule, Utils.UpgradePath.Seamoth);
        }
        internal static void CreateSelectModuleExosuit(SelectableUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Exosuit", "Exosuit " + upgrade.DisplayName, "An upgrade for the Exosuit. " + upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            utt.forExosuit = prefabInfo.TechType;
            CreateSelectModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.ExosuitModule, Utils.UpgradePath.Exosuit);
        }
        internal static void CreateSelectModuleCyclops(SelectableUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Cyclops", "Cyclops " + upgrade.DisplayName, "An upgrade for the Exosuit. " + upgrade.Description)
                .WithIcon(upgrade.Icon);
            utt.forCyclops = prefabInfo.TechType;
            CreateSelectModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.CyclopsModule, Utils.UpgradePath.Cyclops);
        }
        #endregion

        #region ChargeModules
        internal static TechType CreateChargeModuleVanilla(SelectableChargeableUpgrade upgrade, bool isPdaSetup, PrefabInfo info, EquipmentType equipType, Utils.UpgradePath path)
        {
            CustomPrefab prefab = CreateModuleVanilla(upgrade, isPdaSetup, info, path);
            UpgradeModuleGadget gadget = prefab.SetVehicleUpgradeModule(equipType, QuickSlotType.SelectableChargeable);
            AddPassiveActions(gadget, upgrade, info);
            AddChargeActions(gadget, upgrade, info);
            prefab.Register();
            return info.TechType;
        }
        internal static void CreateChargeModuleSeamoth(SelectableChargeableUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Seamoth", "Seamoth " + upgrade.DisplayName, "An upgrade for the Seamoth. " + upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            utt.forSeamoth = prefabInfo.TechType;
            CreateChargeModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.SeamothModule, Utils.UpgradePath.Seamoth);
        }
        internal static void CreateChargeModuleExosuit(SelectableChargeableUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Exosuit", "Exosuit " + upgrade.DisplayName, "An upgrade for the Exosuit. " + upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            utt.forExosuit = prefabInfo.TechType;
            CreateChargeModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.ExosuitModule, Utils.UpgradePath.Exosuit);
        }
        internal static void CreateChargeModuleCyclops(SelectableChargeableUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Cyclops", "Cyclops " + upgrade.DisplayName, "An upgrade for the Exosuit. " + upgrade.Description)
                .WithIcon(upgrade.Icon);
            utt.forCyclops = prefabInfo.TechType;
            CreateChargeModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.CyclopsModule, Utils.UpgradePath.Cyclops);
        }
        #endregion

        #region ToggleModules
        internal static TechType CreateToggleModuleVanilla(ToggleableUpgrade upgrade, bool isPdaSetup, PrefabInfo info, EquipmentType equipType, Utils.UpgradePath path)
        {
            CustomPrefab prefab = CreateModuleVanilla(upgrade, isPdaSetup, info, path);
            UpgradeModuleGadget gadget = prefab.SetVehicleUpgradeModule(equipType, QuickSlotType.Toggleable);
            AddPassiveActions(gadget, upgrade, info);
            AddToggleActions(gadget, upgrade, info);
            prefab.Register();
            return info.TechType;
        }
        internal static void CreateToggleModuleSeamoth(ToggleableUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Seamoth", "Seamoth " + upgrade.DisplayName, "An upgrade for the Seamoth. " + upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            utt.forSeamoth = prefabInfo.TechType;
            CreateToggleModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.SeamothModule, Utils.UpgradePath.Seamoth);
        }
        internal static void CreateToggleModuleExosuit(ToggleableUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Exosuit", "Exosuit " + upgrade.DisplayName, "An upgrade for the Exosuit. " + upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            utt.forExosuit = prefabInfo.TechType;
            CreateToggleModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.ExosuitModule, Utils.UpgradePath.Exosuit);
        }
        internal static void CreateToggleModuleCyclops(ToggleableUpgrade upgrade, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            var prefabInfo = PrefabInfo.WithTechType(upgrade.ClassId + "Cyclops", "Cyclops " + upgrade.DisplayName, "An upgrade for the Exosuit. " + upgrade.Description)
                .WithIcon(upgrade.Icon);
            utt.forCyclops = prefabInfo.TechType;
            CreateToggleModuleVanilla(upgrade, isPdaSetup, prefabInfo, EquipmentType.CyclopsModule, Utils.UpgradePath.Cyclops);
        }
        #endregion
    }
}
