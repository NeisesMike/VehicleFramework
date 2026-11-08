using System;
using System.Linq;
using VehicleFramework.UpgradeTypes;
using VehicleFramework.Assets;
using Nautilus.Assets.Gadgets;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using VehicleFramework.MiscComponents;

namespace VehicleFramework.Admin
{
    /*
     * For details about how upgrade actions are implemented (how a vehicle benefits from an upgrade), see
     * - ModVehicles - ModVehicle.cs
     * - SeaMoth - SeamothUpgradesPatcher.cs
     * - Prawn - ExosuitPatcher.cs
     * - Cyclops - UpgradeConsolePatcher.cs
     */
    public struct UpgradeCompat
    {
        public bool skipModVehicle;
        public bool skipSeamoth;
        public bool skipExosuit;
        public bool skipCyclops;
    }
    public struct UpgradeTechTypes
    {
        // This struct is no longer useful except for backward compatibility.
        public TechType forModVehicle;
        public TechType forSeamoth;
        public TechType forExosuit;
        public TechType forCyclops;
    }
    public enum VehicleType
    {
        ModVehicle,
        Seamoth,
        Prawn,
        Cyclops,
        Custom
    }
    public static class UpgradeRegistrar
    {
        internal static List<ModVehicleUpgrade> RegisteredUpgrades = new();
        public static Dictionary<string, Sprite> UpgradeIcons { get; private set; } = new(); // indexed by upgrade.ClassId
        internal static List<Action<AddActionParams>> OnAddActions = new();
        internal static List<Action<ToggleActionParams>> OnToggleActions = new();
        internal static List<Action<SelectableChargeableActionParams>> OnSelectChargeActions = new();
        internal static List<Action<SelectableActionParams>> OnSelectActions = new();
        internal static List<Action<ArmActionParams>> OnArmDownActions = new();
        internal static List<Action<ArmActionParams>> OnArmHeldActions = new();
        internal static List<Action<ArmActionParams>> OnArmUpActions = new();
        internal static List<Action<ArmActionParams>> OnArmAltActions = new();
        internal static List<Tuple<Vehicle, int, Coroutine>> toggledActions = new();

        [Obsolete("UpgradeRegistrar.RegisterUpgrade method was removed. Use UpgradeRegistrar.RegisterVFUpgrade as a drop in replacement.", false)]
        public static UpgradeTechTypes RegisterUpgrade(ModVehicleUpgrade upgrade, UpgradeCompat compat = default)
        {
            TechType soleTechType = InternalRegisterUpgrade(upgrade, compat);
            return new UpgradeTechTypes
            {
                forSeamoth = soleTechType,
                forCyclops = soleTechType,
                forExosuit = soleTechType,
                forModVehicle = soleTechType
            };
        }
        public static TechType RegisterVFUpgrade(ModVehicleUpgrade upgrade, UpgradeCompat compat = default)
        {
            return InternalRegisterUpgrade(upgrade, compat);
        }

        internal static TechType InternalRegisterUpgrade(ModVehicleUpgrade upgrade, UpgradeCompat compat = default)
        {
            Logger.Log("Registering ModVehicleUpgrade " + upgrade.ClassId + " : " + upgrade.DisplayName);
            bool result = ValidateModVehicleUpgrade(upgrade, compat);
            if(result)
            {
                CraftTreeHandler.EnsureCraftingTabsAvailable(upgrade, compat);
                UpgradeIcons.Add(upgrade.ClassId, upgrade.Icon ?? StaticAssets.UpgradeIcon);
                TechType resultTechType = RegisterVehicleFrameworkUpgrade(upgrade, compat);
                upgrade.TechType = resultTechType;
                RegisteredUpgrades.Add(upgrade);
                RegisterUpgradeMethods(upgrade, compat);
                return resultTechType;
            }
            else
            {
                Logger.Error("Failed to register upgrade: " + upgrade.ClassId);
                return default;
            }
        }
        private static bool ValidateModVehicleUpgrade(ModVehicleUpgrade upgrade, UpgradeCompat compat)
        {
            if(compat.skipModVehicle && compat.skipSeamoth && compat.skipExosuit && compat.skipCyclops)
            {
                Logger.Error($"UpgradeRegistrar Error: ModVehicleUpgrade {upgrade.ClassId}: compat cannot skip all vehicle types!");
                return false;
            }
            if(upgrade.ClassId.Equals(string.Empty))
            {
                Logger.Error($"UpgradeRegistrar Error: ModVehicleUpgrade {upgrade.ClassId} cannot have empty class ID!");
                return false;
            }
            if(upgrade.GetRecipe(VehicleType.ModVehicle).Count == 0)
            {
                Logger.Error($"UpgradeRegistrar Error: ModVehicleUpgrade {upgrade.ClassId} cannot have empty recipe!");
                return false;
            }
            if (upgrade is ModVehicleArm arm)
            {
                if (arm.QuickSlotType != QuickSlotType.Selectable && arm.QuickSlotType != QuickSlotType.SelectableChargeable)
                {
                    Logger.Error($"UpgradeRegistrar Error: ModVehicleArm {upgrade.ClassId} must have QuickSlotType Selectable or SelectableChargeable!");
                    return false;
                }
            }
            if (!upgrade.UnlockAtStart)
            {
                if (!upgrade.UnlockedSprite && !upgrade.UnlockedMessage.Equals(ModVehicleUpgrade.DefaultUnlockMessage))
                {
                    Logger.Warn($"UpgradeRegistrar Warning: the upgrade {upgrade.ClassId} has UnlockAtStart false and UnlockedSprite null. When unlocked, its custom UnlockedMessage will not be displayed. Add an UnlockedSprite to resolve this.");
                }
            }
            return true;
        }
        private static TechType RegisterVehicleFrameworkUpgrade(ModVehicleUpgrade upgrade, UpgradeCompat compat)
        {
            Nautilus.Crafting.RecipeData moduleRecipe = new();
            moduleRecipe.Ingredients.AddRange(upgrade.GetRecipe(VehicleType.ModVehicle));
            Nautilus.Assets.PrefabInfo module_info = Nautilus.Assets.PrefabInfo
                .WithTechType(upgrade.ClassId, upgrade.DisplayName, upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            Nautilus.Assets.CustomPrefab module_CustomPrefab = new(module_info);
            Nautilus.Assets.PrefabTemplates.PrefabTemplate moduleTemplate = new Nautilus.Assets.PrefabTemplates.CloneTemplate(module_info, TechType.SeamothElectricalDefense)
            {
                ModifyPrefab = prefab => prefab.GetComponentsInChildren<Renderer>().ForEach(r => r.materials.ForEach(m => m.color = upgrade.Color))
            };
            module_CustomPrefab.SetGameObject(moduleTemplate);

            string[] steps;
            if (upgrade.IsVehicleSpecific)
            {
                steps = upgrade.ResolvePath(VehicleType.Custom);
            }
            else
            {
                steps = upgrade.ResolvePath(VehicleType.ModVehicle);
            }
            if (!CraftTreeHandler.IsValidCraftPath(steps))
            {
                throw Admin.SessionManager.Fatal($"UpgradeRegistrar: Invalid Crafting Path: there were tab nodes in that tab: {steps.Last()}. Cannot mix tab nodes and crafting nodes.");
            }
            CraftTreeHandler.CraftNodeTabNodes.Add(steps.Last());
            module_CustomPrefab
                .SetRecipe(moduleRecipe)
                .WithCraftingTime(upgrade.CraftingTime)
                .WithFabricatorType(Assets.VFFabricator.TreeType)
                .WithStepsToFabricatorTab(steps);
            module_CustomPrefab.SetPdaGroupCategory(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
            module_CustomPrefab
                .SetEquipment(EncodeCompatAsEquipment(compat, upgrade as ModVehicleArm != null))
                .WithQuickSlotType(upgrade.QuickSlotType);
            if (!upgrade.UnlockAtStart)
            {
                var scanningGadget = module_CustomPrefab.SetUnlock(upgrade.UnlockWith);
                if (upgrade.UnlockedSprite != null)
                {
                    scanningGadget.WithAnalysisTech(upgrade.UnlockedSprite, unlockMessage: upgrade.UnlockedMessage);
                }
            }
            module_CustomPrefab.Register(); // this line causes PDA voice lag by 1.5 seconds ???????
            return module_info.TechType;
        }
        private static void RegisterUpgradeMethods(ModVehicleUpgrade upgrade, UpgradeCompat compat)
        {
            RegisterPassiveUpgradeActions(upgrade, compat);
            RegisterSelectableUpgradeActions(upgrade, compat);
            RegisterSelectableChargeableUpgradeActions(upgrade, compat);
            RegisterToggleableUpgradeActions(upgrade, compat);
            RegisterArmUpgradeActions(upgrade, compat);
        }
        private static void RegisterPassiveUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat)
        {
            void WrappedOnAdded(AddActionParams param)
            {
                if (param.techType != TechType.None && (param.techType == upgrade.TechType))
                {
                    if (param.vehicle != null)
                    {
                        if (param.isAdded)
                        {
                            upgrade.OnAdded(param);
                        }
                        else
                        {
                            upgrade.OnRemoved(param);
                        }
                        if (upgrade is ModVehicleArm arm && param.vehicle is ModVehicle)
                        {
                            var armsManager = param.vehicle.gameObject.EnsureComponent<VehicleRootComponents.VFArmsManager>();
                            armsManager.UpdateArms(arm, param.slotID);
                        }
                    }
                    else if(param.cyclops != null)
                    {
                        if (param.isAdded)
                        {
                            if (upgrade is SelectableChargeableUpgrade || upgrade is SelectableUpgrade || upgrade is ToggleableUpgrade)
                            {
                                ErrorMessage.AddWarning($"The upgrade \"{upgrade.DisplayName}\" likely does not work in a Cyclops.");
                                Logger.Log($"The upgrade \"{upgrade.DisplayName}\" likely does not work in a Cyclops. This is because the Cyclops has no QuickSlots, so activating the upgrade depends on adding a HUD element.");
                            }
                        }
                        upgrade.OnCyclops(param);
                    }
                }
            }
            OnAddActions.Add(WrappedOnAdded);
        }
        private static void RegisterSelectableUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat)
        {
            if (upgrade is SelectableUpgrade select)
            {
                void WrappedOnSelected(SelectableActionParams param)
                {
                    if (param.techType != TechType.None && (param.techType == upgrade.TechType))
                    {
                        select.OnSelected(param);
                        param.vehicle.quickSlotTimeUsed[param.slotID] = Time.time;
                        param.vehicle.quickSlotCooldown[param.slotID] = select.Cooldown;
                        param.vehicle.energyInterface.ConsumeEnergy(select.EnergyCost);
                    }
                }
                OnSelectActions.Add(WrappedOnSelected);
            }
        }
        private static void RegisterSelectableChargeableUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat)
        {
            if (upgrade is SelectableChargeableUpgrade selectcharge)
            {
                Nautilus.Handlers.CraftDataHandler.SetMaxCharge(upgrade.TechType, selectcharge.MaxCharge);
                Nautilus.Handlers.CraftDataHandler.SetEnergyCost(upgrade.TechType, selectcharge.EnergyCost);
                void WrappedOnSelectedCharged(SelectableChargeableActionParams param)
                {
                    if (param.techType != TechType.None && (param.techType == upgrade.TechType))
                    {
                        selectcharge.OnSelected(param);
                        param.vehicle.energyInterface.ConsumeEnergy(selectcharge.EnergyCost);
                    }
                }
                OnSelectChargeActions.Add(WrappedOnSelectedCharged);
            }
        }
        private static void RegisterToggleableUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat)
        {
            if (upgrade is ToggleableUpgrade toggle)
            {
                IEnumerator DoToggleAction(ToggleActionParams param, float timeToFirstActivation, float repeatRate, float energyCostPerActivation)
                {
                    bool isModVehicle = param.vehicle.GetComponent<ModVehicle>() != null;
                    yield return new WaitForSeconds(timeToFirstActivation);
                    while (true)
                    {
                        bool shouldStopWorking = isModVehicle ? !param.vehicle.GetComponent<ModVehicle>().IsUnderCommand : !param.vehicle.GetPilotingMode();
                        if (shouldStopWorking)
                        {
                            param.vehicle.ToggleSlot(param.slotID, false);
                            yield break;
                        }
                        float availablePower = param.vehicle.energyInterface.TotalCanProvide(out _);
                        if (availablePower < energyCostPerActivation)
                        {
                            param.vehicle.ToggleSlot(param.slotID, false);
                            yield break;
                        }
                        toggle.OnRepeat(param);
                        param.vehicle.energyInterface.ConsumeEnergy(energyCostPerActivation);
                        yield return new WaitForSeconds(repeatRate);
                    }
                }
                void WrappedOnToggle(ToggleActionParams param)
                {
                    toggledActions.RemoveAll(x => x.Item3 == null);
                    if (param.techType != TechType.None && (param.techType == upgrade.TechType))
                    {
                        var relevantActions = toggledActions.Where(x => x.Item1 == param.vehicle).Where(x => x.Item2 == param.slotID).ToList();
                        if (param.active)
                        {
                            if (relevantActions.Any())
                            {
                                // Something triggers my Nautilus WithOnModuleToggled action doubly for the Seamoth.
                                // So if the toggle action already exists, don't add another one.
                                return;
                            }
                            var thisToggleCoroutine = param.vehicle.StartCoroutine(DoToggleAction(param, toggle.TimeToFirstActivation, toggle.RepeatRate, toggle.EnergyCostPerActivation));
                            toggledActions.Add(new(param.vehicle, param.slotID, thisToggleCoroutine));
                        }
                        else
                        {
                            foreach(var innerAction in relevantActions)
                            {
                                toggledActions.RemoveAll(x => x == innerAction);
                                param.vehicle.StopCoroutine(innerAction.Item3);
                            }
                        }
                    }
                }
                OnToggleActions.Add(WrappedOnToggle);
            }
        }
        private static void RegisterArmUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat)
        {
            if (upgrade is ModVehicleArm arm)
            {
                IEnumerator PrepareModVehicleArmPrefab()
                {
                    TaskResult<GameObject> armRequest = new();
                    yield return Admin.SessionManager.StartCoroutine(arm.GetArmPrefab(armRequest));
                    GameObject armPrefab = armRequest.Get();
                    if (armPrefab == null)
                    {
                        Logger.Error("ModVehicleArm Error: GetArmPrefab returned a null GameObject instead of a valid arm.");
                        yield break;
                    }
                    armPrefab.AddComponent<VFArm>();
                    VFArm.armLogics.Add(arm.TechType, arm);
                    VFArm.armPrefabs.Add(arm.TechType, armPrefab);
                    arm.armPrefab = armPrefab;
                }
                Admin.SessionManager.StartCoroutine(PrepareModVehicleArmPrefab());

                void WrappedOnArmDown(ArmActionParams param)
                {
                    if (param.techType != TechType.None && (param.techType == upgrade.TechType))
                    {
                        if (arm.ArmCooldowns.coolOnDown)
                        {
                            param.vehicle.quickSlotTimeUsed[param.slotID] = Time.time;
                            param.vehicle.quickSlotCooldown[param.slotID] = arm.ArmCooldowns.downCooldown;
                        }
                        if (arm.EnergyCosts.spendOnDown)
                        {
                            param.vehicle.energyInterface.ConsumeEnergy(arm.EnergyCosts.downEnergyCost);
                        }
                        arm.OnArmDown(param, out _);
                    }
                }
                OnArmDownActions.Add(WrappedOnArmDown);
                void WrappedOnArmHeld(ArmActionParams param)
                {
                    if (param.techType != TechType.None && (param.techType == upgrade.TechType))
                    {
                        if (arm.EnergyCosts.spendOnHeld)
                        {
                            param.vehicle.energyInterface.ConsumeEnergy(arm.EnergyCosts.heldEnergyCost);
                        }
                        arm.OnArmHeld(param, out _);
                    }
                }
                OnArmHeldActions.Add(WrappedOnArmHeld);
                void WrappedOnArmUp(ArmActionParams param)
                {
                    if (param.techType != TechType.None && (param.techType == upgrade.TechType))
                    {
                        if (arm.ArmCooldowns.coolOnUp)
                        {
                            param.vehicle.quickSlotTimeUsed[param.slotID] = Time.time;
                            param.vehicle.quickSlotCooldown[param.slotID] = arm.ArmCooldowns.upCooldown;
                        }
                        if (arm.EnergyCosts.spendOnUp)
                        {
                            param.vehicle.energyInterface.ConsumeEnergy(arm.EnergyCosts.upEnergyCost);
                        }
                        arm.OnArmUp(param, out _);
                    }
                }
                OnArmUpActions.Add(WrappedOnArmUp);
                void WrappedOnArmAlt(ArmActionParams param)
                {
                    if (param.techType != TechType.None && (param.techType == upgrade.TechType))
                    {
                        if (arm.ArmCooldowns.coolOnUp)
                        {
                            param.vehicle.quickSlotTimeUsed[param.slotID] = Time.time;
                            param.vehicle.quickSlotCooldown[param.slotID] = arm.ArmCooldowns.upCooldown;
                        }
                        if (arm.EnergyCosts.spendOnUp)
                        {
                            param.vehicle.energyInterface.ConsumeEnergy(arm.EnergyCosts.upEnergyCost);
                        }
                        arm.OnArmAltUse(param);
                    }
                }
                OnArmAltActions.Add(WrappedOnArmAlt);
            }
        }

        internal static EquipmentType EncodeCompatAsEquipment(UpgradeCompat compat, bool isArm)
        {
            bool MV = !compat.skipModVehicle;
            bool SM = !compat.skipSeamoth;
            bool ES = !compat.skipExosuit;
            bool CC = !compat.skipCyclops;

            int code =
                (isArm ? 1 : 0) |
                (MV ? 2 : 0)    |
                (SM ? 4 : 0)    |
                (ES ? 8 : 0)    |
                (CC ? 16 : 0);

            return code switch
            {
                0b00011 => EnumHelper.GetVFModuleType(EnumHelper.ArmMV),
                0b01011 => EnumHelper.GetVFModuleType(EnumHelper.ArmMVES),
                0b01001 => EnumHelper.GetVFModuleType(EnumHelper.ArmES),
                0b00010 => EnumHelper.GetVFModuleType(EnumHelper.ModuleMV),
                0b00100 => EnumHelper.GetVFModuleType(EnumHelper.ModuleSM),
                0b01000 => EnumHelper.GetVFModuleType(EnumHelper.ModuleES),
                0b10000 => EnumHelper.GetVFModuleType(EnumHelper.ModuleCC),
                0b00110 => EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSM),
                0b01010 => EnumHelper.GetVFModuleType(EnumHelper.ModuleMVES),
                0b10010 => EnumHelper.GetVFModuleType(EnumHelper.ModuleMVCC),
                0b01100 => EnumHelper.GetVFModuleType(EnumHelper.ModuleSMES),
                0b10100 => EnumHelper.GetVFModuleType(EnumHelper.ModuleSMCC),
                0b11000 => EnumHelper.GetVFModuleType(EnumHelper.ModuleESCC),
                0b01110 => EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMES),
                0b10110 => EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMCC),
                0b11010 => EnumHelper.GetVFModuleType(EnumHelper.ModuleMVESCC),
                0b11100 => EnumHelper.GetVFModuleType(EnumHelper.ModuleSMESCC),
                0b11110 => EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMESCC),
                0b11111 => EnumHelper.GetVFModuleType(EnumHelper.ArmMVES),
                _ => throw SessionManager.Fatal("Could not resolve that VF upgrade type!")
            };
        }
        internal static bool CheckEquipmentCompatibility(EquipmentType slot, EquipmentType upgrade)
        {
            if(slot == EnumHelper.GetModuleType())
            {
                return IsModuleCompatWithMV(upgrade);
            }
            else if(slot == EnumHelper.GetArmType())
            {
                return IsArmCompatWithMV(upgrade);
            }
            return slot switch
            {
                EquipmentType.VehicleModule => IsModuleCompatWithMV(upgrade) || IsModuleCompatWithSM(upgrade) || IsModuleCompatWithES(upgrade),
                EquipmentType.SeamothModule => IsModuleCompatWithSM(upgrade),
                EquipmentType.ExosuitModule => IsModuleCompatWithES(upgrade),
                EquipmentType.ExosuitArm => IsArmCompatWithES(upgrade),
                EquipmentType.CyclopsModule => IsModuleCompatWithCC(upgrade),
                _ => false
            };
        }
        private static bool IsArmCompatWithMV(EquipmentType upgrade)
        {
            return
                upgrade == EnumHelper.GetVFModuleType(EnumHelper.ArmMV)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ArmMVES);
        }
        private static bool IsArmCompatWithES(EquipmentType upgrade)
        {
            return
                upgrade == EnumHelper.GetVFModuleType(EnumHelper.ArmES)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ArmMVES);
        }
        private static bool IsModuleCompatWithMV(EquipmentType upgrade)
        {
            return
                upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMV)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSM)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVES)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMES)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVESCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMESCC);
        }
        private static bool IsModuleCompatWithSM(EquipmentType upgrade)
        {
            return
                upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleSM)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSM)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleSMES)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleSMCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMES)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleSMESCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMESCC);
        }
        private static bool IsModuleCompatWithES(EquipmentType upgrade)
        {
            return
                upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleES)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVES)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleSMES)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleESCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMES)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVESCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleSMESCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMESCC);
        }
        private static bool IsModuleCompatWithCC(EquipmentType upgrade)
        {
            return
                upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleSMCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleESCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVESCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleSMESCC)
             || upgrade == EnumHelper.GetVFModuleType(EnumHelper.ModuleMVSMESCC);
        }
    }
}
