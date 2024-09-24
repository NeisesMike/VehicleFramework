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
    public static class UpgradeRegistrar
    {
        internal static List<Action<AddActionParams>> OnAddActions = new List<Action<AddActionParams>>();
        internal static List<Action<ToggleActionParams>> OnToggleActions = new List<Action<ToggleActionParams>>();
        internal static List<Action<SelectableChargeableActionParams>> OnSelectChargeActions = new List<Action<SelectableChargeableActionParams>>();
        internal static List<Action<SelectableActionParams>> OnSelectActions = new List<Action<SelectableActionParams>>();
        internal static List<Action<ArmActionParams>> OnArmActions = new List<Action<ArmActionParams>>();
        public static TechType RegisterUpgrade(ModVehicleUpgrade upgrade, bool verbose = false)
        {
            Logger.Log("Registering ModVehicleUpgrade " + upgrade.ClassId + " : " + upgrade.DisplayName);
            bool result = true;
            result &= ValidateModVehicleUpgrade(upgrade);
            if(result)
            {
                upgrade.TechType = RegisterModVehicleUpgrade(upgrade);
                RegisterUpgradeMethods(upgrade);
                return upgrade.TechType;
            }
            else
            {
                Logger.Error("Failed to register upgrade: " + upgrade.ClassId);
                return 0;
            }
        }
        private static bool ValidateModVehicleUpgrade(ModVehicleUpgrade upgrade)
        {
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
            module_CustomPrefab.SetUnlock(upgrade.UnlockWith);
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
            module_CustomPrefab.Register();
            return module_info.TechType;
        }
        private static void RegisterUpgradeMethods(ModVehicleUpgrade upgrade)
        {
            RegisterPassiveUpgradeActions(upgrade);
            RegisterSelectableUpgradeActions(upgrade);
            RegisterSelectableChargeableUpgradeActions(upgrade);
            RegisterToggleableUpgradeActions(upgrade);
            RegisterArmUpgradeActions(upgrade);
        }
        private static void RegisterPassiveUpgradeActions(ModVehicleUpgrade upgrade)
        {
            void WrappedOnAdded(AddActionParams param)
            {
                if (param.techType == upgrade.TechType)
                {
                    if (param.isAdded)
                    {
                        upgrade.OnAdded(param);
                    }
                    else
                    {
                        upgrade.OnRemoved(param);
                    }
                    if(upgrade as ModVehicleArm != null)
                    {
                        var armsManager = param.mv.gameObject.EnsureComponent<VehicleComponents.VFArmsManager>();
                        UWE.CoroutineHost.StartCoroutine(armsManager.UpdateArms(upgrade as ModVehicleArm, param.slotID));
                    }
                }
            }
            OnAddActions.Add(WrappedOnAdded);
        }
        private static void RegisterSelectableUpgradeActions(ModVehicleUpgrade upgrade)
        {
            SelectableUpgrade select = upgrade as SelectableUpgrade;
            if (select != null)
            {
                void WrappedOnSelected(SelectableActionParams param)
                {
                    if (param.techType == upgrade.TechType)
                    {
                        select.OnSelected(param);
                        param.mv.quickSlotTimeUsed[param.slotID] = Time.time;
                        param.mv.quickSlotCooldown[param.slotID] = select.Cooldown;
                        param.mv.energyInterface.ConsumeEnergy(select.EnergyCost);
                    }
                }
                OnSelectActions.Add(WrappedOnSelected);
            }
        }
        private static void RegisterSelectableChargeableUpgradeActions(ModVehicleUpgrade upgrade)
        {
            SelectableChargeableUpgrade selectcharge = upgrade as SelectableChargeableUpgrade;
            if (selectcharge != null)
            {
                Nautilus.Handlers.CraftDataHandler.SetMaxCharge(selectcharge.TechType, selectcharge.MaxCharge);
                Nautilus.Handlers.CraftDataHandler.SetEnergyCost(selectcharge.TechType, selectcharge.EnergyCost);
                void WrappedOnSelectedCharged(SelectableChargeableActionParams param)
                {
                    if (param.techType == upgrade.TechType)
                    {
                        selectcharge.OnSelected(param);
                        param.mv.energyInterface.ConsumeEnergy(selectcharge.EnergyCost);
                    }
                }
                OnSelectChargeActions.Add(WrappedOnSelectedCharged);
            }
        }
        private static void RegisterToggleableUpgradeActions(ModVehicleUpgrade upgrade)
        {
            ToggleableUpgrade toggle = upgrade as ToggleableUpgrade;
            if (toggle != null)
            {
                IEnumerator DoToggleAction(ToggleActionParams param, float timeToFirstActivation, float repeatRate, float energyCostPerActivation)
                {
                    yield return new WaitForSeconds(timeToFirstActivation);
                    while (true)
                    {
                        if (!param.mv.IsUnderCommand)
                        {
                            param.mv.ToggleSlot(param.slotID, false);
                            yield break;
                        }
                        toggle.OnRepeat(param);
                        int whatWeGot = 0;
                        param.mv.energyInterface.TotalCanProvide(out whatWeGot);
                        if (whatWeGot < energyCostPerActivation)
                        {
                            param.mv.ToggleSlot(param.slotID, false);
                            yield break;
                        }
                        param.mv.energyInterface.ConsumeEnergy(energyCostPerActivation);
                        yield return new WaitForSeconds(repeatRate);
                    }
                }
                void WrappedOnToggle(ToggleActionParams param)
                {
                    if (param.techType == upgrade.TechType)
                    {
                        if (param.active)
                        {
                            param.mv.toggledActions.Add(new Tuple<int, Coroutine>(param.slotID, param.mv.StartCoroutine(DoToggleAction(param, toggle.TimeToFirstActivation, toggle.RepeatRate, toggle.EnergyCostPerActivation))));
                        }
                        else
                        {
                            param.mv.toggledActions.Where(x => x.Item1 == param.slotID).Where(x => x.Item2 != null).ToList().ForEach(x => param.mv.StopCoroutine(x.Item2));
                        }
                    }
                }
                OnToggleActions.Add(WrappedOnToggle);
            }
        }
        private static void RegisterArmUpgradeActions(ModVehicleUpgrade upgrade)
        {
            ModVehicleArm arm = upgrade as ModVehicleArm;
            if (arm != null)
            {
                void WrappedOnArm(ArmActionParams param)
                {
                    if (param.techType == upgrade.TechType)
                    {
                        param.mv.quickSlotTimeUsed[param.slotID] = Time.time;
                        param.mv.quickSlotCooldown[param.slotID] = arm.Cooldown;
                        param.mv.energyInterface.ConsumeEnergy(arm.EnergyCost);
                        arm.OnArmSelected(param);
                    }
                }
                OnArmActions.Add(WrappedOnArm);
            }
        }
    }
}
