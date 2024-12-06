using System;
using System.Linq;
using VehicleFramework.UpgradeTypes;
using VehicleFramework.VehicleComponents;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using BiomeData = LootDistributionData.BiomeData;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace VehicleFramework.Admin
{
    public struct UpgradeCompat
    {
        public bool skipModVehicle;
        public bool skipSeamoth;
        public bool skipExosuit;
        public bool skipCyclops;
    }
    public struct UpgradeTechTypes
    {
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
        Cyclops
    }
    public static class UpgradeRegistrar
    {
        internal static List<Action<AddActionParams>> OnAddActions = new List<Action<AddActionParams>>();
        internal static List<Action<ToggleActionParams>> OnToggleActions = new List<Action<ToggleActionParams>>();
        internal static List<Action<SelectableChargeableActionParams>> OnSelectChargeActions = new List<Action<SelectableChargeableActionParams>>();
        internal static List<Action<SelectableActionParams>> OnSelectActions = new List<Action<SelectableActionParams>>();
        internal static List<Action<ArmActionParams>> OnArmDownActions = new List<Action<ArmActionParams>>();
        internal static List<Action<ArmActionParams>> OnArmHeldActions = new List<Action<ArmActionParams>>();
        internal static List<Action<ArmActionParams>> OnArmUpActions = new List<Action<ArmActionParams>>();
        internal static List<Action<ArmActionParams>> OnArmAltActions = new List<Action<ArmActionParams>>();
        internal static List<Tuple<Vehicle, int, Coroutine>> toggledActions = new List<Tuple<Vehicle, int, Coroutine>>();
        public static UpgradeTechTypes RegisterUpgrade(ModVehicleUpgrade upgrade, UpgradeCompat compat = default(UpgradeCompat), bool verbose = false)
        {
            Logger.Log("Registering ModVehicleUpgrade " + upgrade.ClassId + " : " + upgrade.DisplayName);
            if (upgrade.TabName != "")
            {
                AddCraftingTabs(upgrade.TabName, upgrade.TabDisplayName, upgrade.TabIcon);
            }
            bool result = ValidateModVehicleUpgrade(upgrade, compat);
            if(result)
            {
                UpgradeTechTypes utt = new UpgradeTechTypes();
                bool isPdaRegistered = false;
                if (!compat.skipModVehicle)
                {
                    utt.forModVehicle = RegisterModVehicleUpgrade(upgrade);
                    isPdaRegistered = true;
                }
                RegisterUpgradeMethods(upgrade, compat, ref utt, isPdaRegistered);
                upgrade.TechTypes = utt;
                return utt;
            }
            else
            {
                Logger.Error("Failed to register upgrade: " + upgrade.ClassId);
                return default;
            }
        }
        private static void AddCraftingTabs(string tabName, string tabDisplayName, Atlas.Sprite displayIcon)
        {
            Atlas.Sprite usedIcon = displayIcon;
            if(usedIcon == null)
            {
                usedIcon = MainPatcher.ModVehicleIcon;
            }
            CraftTreeHandler.AddCraftTreeNodesVF(
                tabName,
                tabDisplayName,
                usedIcon
            );
        }
        private static bool ValidateModVehicleUpgrade(ModVehicleUpgrade upgrade, UpgradeCompat compat)
        {
            if(compat.skipModVehicle && compat.skipSeamoth && compat.skipExosuit && compat.skipCyclops)
            {
                Logger.Error("ModVehicleUpgrade compat cannot skip all vehicle types!");
                return false;
            }
            if(upgrade.ClassId == "")
            {
                Logger.Error("ModVehicleUpgrade cannot have empty class ID!");
                return false;
            }
            if(upgrade.GetRecipe(VehicleType.ModVehicle).Count == 0)
            {
                Logger.Error("ModVehicleUpgrade cannot have empty recipe!");
                return false;
            }
            if (upgrade as ModVehicleArm != null)
            {
                if ((upgrade as ModVehicleArm).QuickSlotType != QuickSlotType.Selectable && (upgrade as ModVehicleArm).QuickSlotType != QuickSlotType.SelectableChargeable)
                {
                    Logger.Error("ModVehicleArm must have QuickSlotType Selectable or SelectableChargeable!");
                    return false;
                }
            }
            return true;
        }
        private static TechType RegisterModVehicleUpgrade(ModVehicleUpgrade upgrade)
        {
            Nautilus.Crafting.RecipeData moduleRecipe = new Nautilus.Crafting.RecipeData();
            moduleRecipe.Ingredients.AddRange(upgrade.GetRecipe(VehicleType.ModVehicle));
            PrefabInfo module_info = PrefabInfo
                .WithTechType(upgrade.ClassId, upgrade.DisplayName, upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            CustomPrefab module_CustomPrefab = new CustomPrefab(module_info);
            PrefabTemplate moduleTemplate = new CloneTemplate(module_info, TechType.SeamothElectricalDefense)
            {
                ModifyPrefab = prefab => prefab.GetComponentsInChildren<Renderer>().ForEach(r => r.materials.ForEach(m => m.color = upgrade.Color))
            };
            module_CustomPrefab.SetGameObject(moduleTemplate);
            string[] steps = CraftTreeHandler.UpgradeTypeToPath(VehicleType.ModVehicle);
            if (upgrade.TabName.Length > 0)
            {
                steps = steps.Append(upgrade.TabName).ToArray();
            }
            module_CustomPrefab
                .SetRecipe(moduleRecipe)
                .WithCraftingTime(upgrade.CraftingTime)
                .WithFabricatorType(upgrade.FabricatorType)
                .WithStepsToFabricatorTab(steps);
            module_CustomPrefab.SetPdaGroupCategory(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
            if (upgrade as ModVehicleArm != null)
            {
                module_CustomPrefab
                    .SetEquipment(VehicleBuilder.ArmType)
                    .WithQuickSlotType(upgrade.QuickSlotType);
            }
            else
            {
                module_CustomPrefab
                    .SetEquipment(VehicleBuilder.ModuleType)
                    .WithQuickSlotType(upgrade.QuickSlotType);
            }
            if (!upgrade.UnlockAtStart)
            {
                var scanningGadget = module_CustomPrefab.SetUnlock(upgrade.UnlockTechType == TechType.Fragment ? upgrade.UnlockWith : upgrade.UnlockTechType);
                if (upgrade.UnlockedSprite != null)
                {
                    scanningGadget.WithAnalysisTech(upgrade.UnlockedSprite, unlockMessage: upgrade.UnlockedMessage);
                }
            }
            module_CustomPrefab.Register(); // this line causes PDA voice lag by 1.5 seconds ???????
            upgrade.UnlockTechType = module_info.TechType;
            return module_info.TechType;
        }
        private static void RegisterUpgradeMethods(ModVehicleUpgrade upgrade, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaRegistered)
        {
            bool isPDASetup = isPdaRegistered;
            RegisterPassiveUpgradeActions(upgrade, compat, ref utt, ref isPDASetup);
            RegisterSelectableUpgradeActions(upgrade, compat, ref utt, ref isPDASetup);
            RegisterSelectableChargeableUpgradeActions(upgrade, compat, ref utt, ref isPDASetup);
            RegisterToggleableUpgradeActions(upgrade, compat, ref utt, ref isPDASetup);
            RegisterArmUpgradeActions(upgrade, compat, ref utt, ref isPDASetup);
        }
        private static void RegisterPassiveUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat, ref UpgradeTechTypes utt, ref bool isPDASetup)
        {
            if(    upgrade is SelectableUpgrade
                || upgrade is ToggleableUpgrade
                || upgrade is SelectableChargeableUpgrade
                || upgrade is ModVehicleArm)
            {

            }
            else
            {
                VanillaUpgradeMaker.CreatePassiveModule(upgrade, compat, ref utt, isPDASetup);
                isPDASetup = true;
            }
            TechType mvTT = utt.forModVehicle;
            TechType sTT = utt.forSeamoth;
            TechType eTT = utt.forExosuit;
            TechType cTT = utt.forCyclops;
            void WrappedOnAdded(AddActionParams param)
            {
                if (param.techType != TechType.None && (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT))
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
                        if (upgrade as ModVehicleArm != null && param.vehicle as ModVehicle != null)
                        {
                            var armsManager = param.vehicle.gameObject.EnsureComponent<VehicleComponents.VFArmsManager>();
                            armsManager.UpdateArms(upgrade as ModVehicleArm, param.slotID);
                        }
                    }
                    else if(param.cyclops != null)
                    {
                        upgrade.OnCyclops(param);
                    }
                }
            }
            OnAddActions.Add(WrappedOnAdded);
        }
        private static void RegisterSelectableUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat, ref UpgradeTechTypes utt, ref bool isPDASetup)
        {
            if (upgrade is SelectableUpgrade select)
            {
                VanillaUpgradeMaker.CreateSelectModule(select, compat, ref utt, isPDASetup);
                isPDASetup = true;
                TechType mvTT = utt.forModVehicle;
                TechType sTT = utt.forSeamoth;
                TechType eTT = utt.forExosuit;
                TechType cTT = utt.forCyclops;
                void WrappedOnSelected(SelectableActionParams param)
                {
                    if (param.techType != TechType.None && (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT))
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
        private static void RegisterSelectableChargeableUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat, ref UpgradeTechTypes utt, ref bool isPDASetup)
        {
            if (upgrade is SelectableChargeableUpgrade selectcharge)
            {
                VanillaUpgradeMaker.CreateChargeModule(selectcharge, compat, ref utt, isPDASetup);
                foreach (System.Reflection.FieldInfo field in typeof(UpgradeTechTypes).GetFields())
                {
                    // Set MaxCharge and EnergyCost for all possible TechTypes emerging from this upgrade.
                    TechType value = (TechType)field.GetValue(utt);
                    Logger.Log(value.AsString());
                    Nautilus.Handlers.CraftDataHandler.SetMaxCharge(value, selectcharge.MaxCharge);
                    Nautilus.Handlers.CraftDataHandler.SetEnergyCost(value, selectcharge.EnergyCost);
                }
                isPDASetup = true;
                TechType mvTT = utt.forModVehicle;
                TechType sTT = utt.forSeamoth;
                TechType eTT = utt.forExosuit;
                TechType cTT = utt.forCyclops;
                void WrappedOnSelectedCharged(SelectableChargeableActionParams param)
                {
                    if (param.techType != TechType.None && (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT))
                    {
                        selectcharge.OnSelected(param);
                        param.vehicle.energyInterface.ConsumeEnergy(selectcharge.EnergyCost);
                    }
                }
                OnSelectChargeActions.Add(WrappedOnSelectedCharged);
            }
        }
        private static void RegisterToggleableUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat, ref UpgradeTechTypes utt, ref bool isPDASetup)
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
                        toggle.OnRepeat(param);
                        int whatWeGot = 0;
                        param.vehicle.energyInterface.TotalCanProvide(out whatWeGot);
                        if (whatWeGot < energyCostPerActivation)
                        {
                            param.vehicle.ToggleSlot(param.slotID, false);
                            yield break;
                        }
                        param.vehicle.energyInterface.ConsumeEnergy(energyCostPerActivation);
                        yield return new WaitForSeconds(repeatRate);
                    }
                }
                VanillaUpgradeMaker.CreateToggleModule(toggle, compat, ref utt, isPDASetup);
                isPDASetup = true;
                TechType mvTT = utt.forModVehicle;
                TechType sTT = utt.forSeamoth;
                TechType eTT = utt.forExosuit;
                TechType cTT = utt.forCyclops;
                void WrappedOnToggle(ToggleActionParams param)
                {
                    if (param.techType != TechType.None && (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT))
                    {
                        if (param.active)
                        {
                            toggledActions.Add(new Tuple<Vehicle, int, Coroutine>(param.vehicle, param.slotID, param.vehicle.StartCoroutine(DoToggleAction(param, toggle.TimeToFirstActivation, toggle.RepeatRate, toggle.EnergyCostPerActivation))));
                        }
                        else
                        {
                            toggledActions.Where(x => x.Item1 == param.vehicle).Where(x => x.Item2 == param.slotID).Where(x => x.Item3 != null).ToList().ForEach(x => param.vehicle.StopCoroutine(x.Item3));
                        }
                    }
                }
                OnToggleActions.Add(WrappedOnToggle);
            }
        }
        private static void RegisterArmUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat, ref UpgradeTechTypes utt, ref bool isPDASetup)
        {
            if (upgrade is ModVehicleArm arm)
            {
                VanillaUpgradeMaker.CreateExosuitArm(arm, compat, ref utt, isPDASetup);
                isPDASetup = true;
                UpgradeTechTypes staticUTT = utt;
                IEnumerator PrepareModVehicleArmPrefab()
                {
                    TaskResult<GameObject> armRequest = new TaskResult<GameObject>();
                    yield return UWE.CoroutineHost.StartCoroutine(arm.GetArmPrefab(armRequest));
                    GameObject armPrefab = armRequest.Get();
                    if (armPrefab == null)
                    {
                        Logger.Error("ModVehicleArm Error: GetArmPrefab returned a null GameObject instead of a valid arm.");
                        yield break;
                    }
                    armPrefab.AddComponent<VFArm>();
                    VFArm.armLogics.Add(staticUTT, arm);
                    VFArm.armPrefabs.Add(staticUTT, armPrefab);
                    arm.armPrefab = armPrefab;
                }
                UWE.CoroutineHost.StartCoroutine(PrepareModVehicleArmPrefab());

                TechType mvTT = utt.forModVehicle;
                TechType sTT = utt.forSeamoth;
                TechType eTT = utt.forExosuit;
                TechType cTT = utt.forCyclops;

                void WrappedOnArmDown(ArmActionParams param)
                {
                    if (param.techType != TechType.None && (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT))
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
                    if (param.techType != TechType.None && (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT))
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
                    if (param.techType != TechType.None && (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT))
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
                    if (param.techType != TechType.None && (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT))
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
    }
}
