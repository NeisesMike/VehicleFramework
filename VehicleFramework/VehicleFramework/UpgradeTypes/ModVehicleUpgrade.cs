using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Admin;
using VehicleFramework.Assets;
using VehicleFramework.Extensions;

namespace VehicleFramework.UpgradeTypes
{
    public abstract class ModVehicleUpgrade
    {
        public UpgradeTechTypes TechTypes { get; internal set; }
        private TechType _unlockTechType = TechType.Fragment;
        internal TechType UnlockTechType
        {
            get
            {
                return _unlockTechType;
            }
            set
            {
                if(_unlockTechType == TechType.Fragment)
                {
                    _unlockTechType = value;
                }
            }
        }
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
        private readonly List<UpgradeTechTypes> RecipeExtensions = new();
        private readonly List<Ingredient> SimpleRecipeExtensions = new();
        public List<Ingredient> GetRecipe(VehicleType type)
        {
            List<Ingredient> ret = new();
            ret.AddRange(Recipe);
            ret.AddRange(SimpleRecipeExtensions);
            switch (type)
            {
                case VehicleType.ModVehicle:
                    RecipeExtensions.ForEach(x => ret.Add(new Ingredient(x.forModVehicle, 1)));
                    break;
                case VehicleType.Seamoth:
                    RecipeExtensions.ForEach(x => ret.Add(new Ingredient(x.forSeamoth, 1)));
                    break;
                case VehicleType.Prawn:
                    RecipeExtensions.ForEach(x => ret.Add(new Ingredient(x.forExosuit, 1)));
                    break;
                case VehicleType.Cyclops:
                    RecipeExtensions.ForEach(x => ret.Add(new Ingredient(x.forCyclops, 1)));
                    break;
                default:
                    break;
            }
            return ret;
        }
        public void ExtendRecipe(UpgradeTechTypes techTypes)
        {
            RecipeExtensions.Add(techTypes);
        }
        public void ExtendRecipeSimple(Ingredient ingredient)
        {
            SimpleRecipeExtensions.Add(ingredient);
        }
        public bool HasTechType(TechType tt)
        {
            if(tt == TechType.None)
            {
                return false;
            }
            return TechTypes.forModVehicle == tt
                || TechTypes.forSeamoth == tt
                || TechTypes.forExosuit == tt
                || TechTypes.forCyclops == tt;
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
