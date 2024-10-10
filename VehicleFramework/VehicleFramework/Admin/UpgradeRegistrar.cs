using System;
using System.Linq;
using VehicleFramework.UpgradeTypes;
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
    public static class UpgradeRegistrar
    {
        internal static List<Action<AddActionParams>> OnAddActions = new List<Action<AddActionParams>>();
        internal static List<Action<ToggleActionParams>> OnToggleActions = new List<Action<ToggleActionParams>>();
        internal static List<Action<SelectableChargeableActionParams>> OnSelectChargeActions = new List<Action<SelectableChargeableActionParams>>();
        internal static List<Action<SelectableActionParams>> OnSelectActions = new List<Action<SelectableActionParams>>();
        internal static List<Action<ArmActionParams>> OnArmActions = new List<Action<ArmActionParams>>();
        internal static List<Tuple<Vehicle, int, Coroutine>> toggledActions = new List<Tuple<Vehicle, int, Coroutine>>();
        public static UpgradeTechTypes RegisterUpgrade(ModVehicleUpgrade upgrade, UpgradeCompat compat = default(UpgradeCompat), bool verbose = false)
        {
            Logger.Log("Registering ModVehicleUpgrade " + upgrade.ClassId + " : " + upgrade.DisplayName);
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
            if(upgrade.GetRecipe().Count == 0)
            {
                Logger.Error("ModVehicleUpgrade cannot have empty recipe!");
                return false;
            }
            return true;
        }
        private static TechType RegisterModVehicleUpgrade(ModVehicleUpgrade upgrade)
        {
            Nautilus.Crafting.RecipeData moduleRecipe = new Nautilus.Crafting.RecipeData();
            moduleRecipe.Ingredients.AddRange(upgrade.GetRecipe());
            PrefabInfo module_info = PrefabInfo
                .WithTechType(upgrade.ClassId, upgrade.DisplayName, upgrade.Description, unlockAtStart: upgrade.UnlockAtStart)
                .WithIcon(upgrade.Icon);
            CustomPrefab module_CustomPrefab = new CustomPrefab(module_info);
            PrefabTemplate moduleTemplate = new CloneTemplate(module_info, TechType.SeamothElectricalDefense)
            {
                ModifyPrefab = prefab => prefab.GetComponentsInChildren<Renderer>().ForEach(r => r.materials.ForEach(m => m.color = upgrade.Color))
            };
            module_CustomPrefab.SetGameObject(moduleTemplate);
            module_CustomPrefab
                .SetRecipe(moduleRecipe)
                .WithCraftingTime(upgrade.CraftingTime)
                .WithFabricatorType(upgrade.FabricatorType)
                .WithStepsToFabricatorTab(upgrade.StepsToFabricatorTab);
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
            if (upgrade.UnlockedSprite != null && !upgrade.UnlockAtStart)
            {
                var scanningGadget = module_CustomPrefab.SetUnlock(upgrade.UnlockWith);
                scanningGadget.WithAnalysisTech(upgrade.UnlockedSprite, unlockMessage: upgrade.UnlockedMessage);
            }
            module_CustomPrefab.Register();
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
            TechType mvTT = utt.forModVehicle;
            TechType sTT = utt.forSeamoth;
            TechType eTT = utt.forExosuit;
            TechType cTT = utt.forCyclops;
            void WrappedOnAdded(AddActionParams param)
            {
                if (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT)
                {
                    if (param.isAdded)
                    {
                        upgrade.OnAdded(param);
                    }
                    else
                    {
                        upgrade.OnRemoved(param);
                    }
                    if (upgrade as ModVehicleArm != null)
                    {
                        var armsManager = param.vehicle.gameObject.EnsureComponent<VehicleComponents.VFArmsManager>();
                        UWE.CoroutineHost.StartCoroutine(armsManager.UpdateArms(upgrade as ModVehicleArm, param.slotID));
                    }
                }
            }
            OnAddActions.Add(WrappedOnAdded);
        }
        private static void RegisterSelectableUpgradeActions(ModVehicleUpgrade upgrade, UpgradeCompat compat, ref UpgradeTechTypes utt, ref bool isPDASetup)
        {
            if (upgrade is SelectableUpgrade select)
            {
                TechType mvTT = utt.forModVehicle;
                TechType sTT = utt.forSeamoth;
                TechType eTT = utt.forExosuit;
                TechType cTT = utt.forCyclops;
                void WrappedOnSelected(SelectableActionParams param)
                {
                    if (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT)
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
                foreach (System.Reflection.FieldInfo field in typeof(UpgradeTechTypes).GetFields())
                {
                    // Set MaxCharge and EnergyCost for all possible TechTypes emerging from this upgrade.
                    TechType value = (TechType)field.GetValue(utt);
                    Logger.Log(value.AsString());
                    Nautilus.Handlers.CraftDataHandler.SetMaxCharge(value, selectcharge.MaxCharge);
                    Nautilus.Handlers.CraftDataHandler.SetEnergyCost(value, selectcharge.EnergyCost);
                }
                TechType mvTT = utt.forModVehicle;
                TechType sTT = utt.forSeamoth;
                TechType eTT = utt.forExosuit;
                TechType cTT = utt.forCyclops;
                void WrappedOnSelectedCharged(SelectableChargeableActionParams param)
                {
                    if (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT)
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
                TechType mvTT = utt.forModVehicle;
                TechType sTT = utt.forSeamoth;
                TechType eTT = utt.forExosuit;
                TechType cTT = utt.forCyclops;
                void WrappedOnToggle(ToggleActionParams param)
                {
                    if (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT)
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
                TechType mvTT = utt.forModVehicle;
                TechType sTT = utt.forSeamoth;
                TechType eTT = utt.forExosuit;
                TechType cTT = utt.forCyclops;
                void WrappedOnArm(ArmActionParams param)
                {
                    if (param.techType == mvTT || param.techType == sTT || param.techType == eTT || param.techType == cTT)
                    {
                        param.vehicle.quickSlotTimeUsed[param.slotID] = Time.time;
                        param.vehicle.quickSlotCooldown[param.slotID] = arm.Cooldown;
                        param.vehicle.energyInterface.ConsumeEnergy(arm.EnergyCost);
                        arm.OnArmSelected(param);
                    }
                }
                OnArmActions.Add(WrappedOnArm);
            }
        }
    }
}
