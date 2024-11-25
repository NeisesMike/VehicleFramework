using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Admin;

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
        public virtual QuickSlotType QuickSlotType => QuickSlotType.Passive;
        public virtual bool UnlockAtStart => true;
        public virtual Color Color => Color.red;
        public virtual float CraftingTime => 3f;
        public virtual CraftTree.Type FabricatorType => CraftTree.Type.Workbench;
        public virtual Atlas.Sprite Icon => MainPatcher.UpgradeIcon;
        public virtual TechType UnlockWith => TechType.Workbench;
        public virtual string UnlockedMessage => "New vehicle upgrade acquired";
        public virtual Sprite UnlockedSprite => null;
        public virtual string TabName => "";
        public virtual string TabDisplayName => "";
        public virtual Atlas.Sprite TabIcon => MainPatcher.UpgradeIcon;
        public virtual List<Assets.Ingredient> Recipe => new List<Assets.Ingredient> { new Assets.Ingredient(TechType.Titanium, 1) };
        public virtual void OnAdded(AddActionParams param)
        {
            Logger.Log("Adding " + ClassId + " to ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
        }
        public virtual void OnRemoved(AddActionParams param)
        {
            Logger.Log("Removing " + ClassId + " to ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
        }
        private readonly List<UpgradeTechTypes> RecipeExtensions = new List<UpgradeTechTypes>();
        private readonly List<Assets.Ingredient> SimpleRecipeExtensions = new List<Assets.Ingredient>();
        public List<CraftData.Ingredient> GetRecipe(VehicleType type)
        {
            List<Assets.Ingredient> ret = new List<Assets.Ingredient>();
            ret.AddRange(Recipe);
            ret.AddRange(SimpleRecipeExtensions);
            switch (type)
            {
                case VehicleType.ModVehicle:
                    RecipeExtensions.ForEach(x => ret.Add(new Assets.Ingredient(x.forModVehicle, 1)));
                    break;
                case VehicleType.Seamoth:
                    RecipeExtensions.ForEach(x => ret.Add(new Assets.Ingredient(x.forSeamoth, 1)));
                    break;
                case VehicleType.Prawn:
                    RecipeExtensions.ForEach(x => ret.Add(new Assets.Ingredient(x.forExosuit, 1)));
                    break;
                case VehicleType.Cyclops:
                    RecipeExtensions.ForEach(x => ret.Add(new Assets.Ingredient(x.forCyclops, 1)));
                    break;
                default:
                    break;
            }
            return ret.Select(x => x.Get()).ToList();
        }
        public void ExtendRecipe(UpgradeTechTypes techTypes)
        {
            RecipeExtensions.Add(techTypes);
        }
        public void ExtendRecipeSimple(Assets.Ingredient ingredient)
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
    }
}
