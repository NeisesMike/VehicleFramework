using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.UpgradeTypes
{
    public abstract class ModVehicleUpgrade
    {
        public TechType TechType { get; internal set; }
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
        public virtual string TabName => Admin.Utils.UpgradePathToString(Admin.Utils.UpgradePath.ModVehicle);
        public virtual string[] StepsToFabricatorTab => new string[] { "MVUM", TabName };
        public virtual List<Assets.Ingredient> Recipe => new List<Assets.Ingredient> { new Assets.Ingredient(TechType.Titanium, 1) };
        public virtual void OnAdded(AddActionParams param)
        {
            Logger.Log("Adding " + ClassId + " to ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
        }
        public virtual void OnRemoved(AddActionParams param)
        {
            Logger.Log("Removing " + ClassId + " to ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
        }
        private List<Assets.Ingredient> RecipeExtensions = new List<Assets.Ingredient> { };
        public List<CraftData.Ingredient> GetRecipe()
        {
            List<Assets.Ingredient> ret = new List<Assets.Ingredient>();
            ret.AddRange(Recipe);
            ret.AddRange(RecipeExtensions);
            return ret.Select(x => x.Get()).ToList();
        }
        public void ExtendRecipe(Assets.Ingredient ingredient)
        {
            RecipeExtensions.Add(ingredient);
        }
    }
}
