using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleFramework.Admin;
using VehicleFramework.Assets;
using VehicleFramework.Extensions;

namespace VehicleFramework.UpgradeTypes
{
    public abstract class ModVehicleUpgrade
    {
        public TechType TechType { get; internal set; }
        public abstract string ClassId { get; }
        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        public virtual bool IsVehicleSpecific => false;
        public virtual QuickSlotType QuickSlotType => QuickSlotType.Passive;
        public virtual bool UnlockAtStart => true;
        public virtual Color Color => Color.red;
        public virtual float CraftingTime => 3f;
        public virtual Sprite? Icon => StaticAssets.UpgradeIcon;
        public virtual TechType UnlockWith => TechType.Constructor;
        public const string DefaultUnlockMessage = "New vehicle upgrade acquired";
        public virtual string UnlockedMessage => DefaultUnlockMessage;
        public virtual Sprite? UnlockedSprite => null;
        public virtual string TabName { get; set; } = string.Empty;
        public virtual string TabDisplayName => string.Empty;
        public virtual List<CraftingNode>? CraftingPath { get; set; } = null;
        public virtual Sprite? TabIcon => StaticAssets.UpgradeIcon;
        public virtual List<Ingredient> Recipe => new() { new Ingredient(TechType.Titanium, 1) };
        public virtual void OnAdded(AddActionParams param)
        {
            Logger.DebugLog("Adding " + ClassId + " to ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
        }
        public virtual void OnRemoved(AddActionParams param)
        {
            Logger.DebugLog("Removing " + ClassId + " to ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
        }
        public virtual void OnCyclops(AddActionParams param)
        {
            Logger.DebugLog("Bumping " + ClassId + " In Cyclops: '" + param.cyclops.subName + "' in slotID: " + param.slotID.ToString());
        }
        private readonly List<TechType> RecipeExtensions = new();
        private readonly List<Ingredient> SimpleRecipeExtensions = new();
        public List<Ingredient> GetRecipe(VehicleType type)
        {
            List<Ingredient> resultRecipe = new();
            resultRecipe.AddRange(Recipe);
            resultRecipe.AddRange(SimpleRecipeExtensions);
            RecipeExtensions.ForEach(x => resultRecipe.Add(new Ingredient(x, 1)));
            return resultRecipe;
        }
        public void ExtendRecipe(TechType techType)
        {
            RecipeExtensions.Add(techType);
        }
        public void ExtendRecipe(UpgradeTechTypes techTypes)
        {
            // This method just here for backward compatibility.
            // The UpgradeTechTypes struct is populated with a single value (i.e., all values are the same)
            RecipeExtensions.Add(techTypes.forModVehicle);
        }
        public void ExtendRecipeSimple(Ingredient ingredient)
        {
            SimpleRecipeExtensions.Add(ingredient);
        }
        public int GetNumberInstalled(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return 0;
            }
            return vehicle.GetCurrentUpgrades().Where(x => x.Contains(ClassId)).Count();
        }
        public int GetNumberInstalled(SubRoot Cyclops)
        {
            if (Cyclops == null)
            {
                return 0;
            }
            return Cyclops.GetCurrentUpgrades().Where(x => x.Contains(ClassId)).Count();
        }
        internal string[] ResolvePath(VehicleType vType)
        {
            // If TabName is string.Empty, use $"{CraftTreeHandler.GeneralTabName}{vType}"
            if (CraftingPath == null)
            {
                if (TabName.Equals(string.Empty))
                {
                    return CraftTreeHandler.UpgradeTypeToPath(vType).Append($"{CraftTreeHandler.GeneralTabName}{vType}").ToArray();
                }
                else
                {
                    return CraftTreeHandler.UpgradeTypeToPath(vType).Append(TabName).ToArray();
                }
            }
            else
            {
                return CraftTreeHandler.TraceCraftingPath(vType, CraftingPath, null);
            }
        }
    }
}
