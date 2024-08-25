using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VehicleFramework.UpgradeModules
{
    public static partial class ModuleManager
    {
        public static TechType AddArmModule(ModVehicleArm arm)
        {
            List<CraftData.Ingredient> recipe = arm.GetRecipe().Select(x => new CraftData.Ingredient(x.Item1, x.Item2)).ToList();
            arm.techType = ModulePrepper.RegisterArmGeneric(recipe, arm.GetClassId(), arm.GetDisplayName(), arm.GetDescription(), QuickSlotType.Selectable, arm.GetIcon(), arm.GetTabName());
            void WrappedOnAdded(ModVehicle mv, List<string> currentUpgrades, int slotID, TechType moduleTechType, bool added)
            {
                if (moduleTechType == arm.techType)
                {
                    if(added)
                    {
                        arm.OnAdded(mv, currentUpgrades, slotID);
                    }
                    else
                    {
                        arm.OnRemoved(mv, currentUpgrades, slotID);
                    }
                    mv.gameObject.EnsureComponent<VehicleComponents.VFArmsManager>().UpdateArms(arm, slotID);
                }
            }
            bool WrappedOnSelected(ModVehicle mv, int slotID, TechType moduleTechType)
            {
                if (moduleTechType == arm.techType)
                {
                    VehicleComponents.VFArmsManager vfam = mv.GetComponent<VehicleComponents.VFArmsManager>();
                    GameObject leftArm = vfam.leftArm;
                    GameObject rightArm = vfam.rightArm;
                    if (slotID == vfam.leftArmSlotID && leftArm != null)
                    {
                        arm.OnSelected(mv, leftArm, slotID);
                    }
                    else if (slotID == vfam.rightArmSlotID && rightArm != null)
                    {
                        arm.OnSelected(mv, rightArm, slotID);
                    }
                    return true;
                }
                return false;
            }
            ModulePrepper.upgradeOnUseActions.Add(new Tuple<Func<ModVehicle, int, TechType, bool>, float, float>(WrappedOnSelected, arm.GetCooldown(), arm.GetEnergyCost()));
            ModulePrepper.upgradeOnAddedActions.Add(WrappedOnAdded);
            return arm.techType;
        }
    }
}
