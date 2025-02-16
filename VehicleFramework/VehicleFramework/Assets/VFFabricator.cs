using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using Nautilus.Utility;
using static CraftData;
using UnityEngine;

namespace VehicleFramework.Assets
{
    internal static class VFFabricator
    {
        private const string ClassID = "VFFabricatorClassID";
        private const string DisplayName = "VF Fabricator";
        private const string Description = "A fabricator for building upgrades for the Seamoth, Prawn, and Cyclops, as well as all Vehicle Framework vehicles.";
        internal static CraftTree.Type TreeType = default;
        internal static void CreateAndRegister()
        {
            var Info = PrefabInfo.WithTechType(ClassID, DisplayName, Description, "English", false)
                .WithIcon(SpriteManager.Get(TechType.Fabricator));

            var prefab = new CustomPrefab(Info);

            if (GetBuilderIndex(TechType.Fabricator, out var group, out var category, out _))
            {
                var scanGadget = prefab.SetPdaGroupCategoryAfter(group, category, TechType.Fabricator);
                scanGadget.RequiredForUnlock = TechType.Constructor;
            }

            var fabGadget = prefab.CreateFabricator(out var treeType);
            TreeType = treeType;

            var vfFabTemplate = new FabricatorTemplate(Info, TreeType)
            {
                ModifyPrefab = ModifyFabricatorPrefab,
                FabricatorModel = FabricatorTemplate.Model.MoonPool,
                ConstructableFlags = ConstructableFlags.Wall | ConstructableFlags.Base | ConstructableFlags.Submarine
                | ConstructableFlags.Inside | ConstructableFlags.AllowedOnConstructable,
            };

            prefab.SetGameObject(vfFabTemplate);

            CraftDataHandler.SetRecipeData(Info.TechType, GetBlueprintRecipe());
            prefab.Register();
        }
        private static Nautilus.Crafting.RecipeData GetBlueprintRecipe()
        {
            return new Nautilus.Crafting.RecipeData
            {
                craftAmount = 1,
                Ingredients =
                {
                    new CraftData.Ingredient(TechType.Titanium, 1),
                    new CraftData.Ingredient(TechType.ComputerChip, 1),
                    new CraftData.Ingredient(TechType.Diamond, 1),
                }
            };
        }
        private static void ModifyFabricatorPrefab(GameObject obj)
        {
            obj.transform.localScale *= 0.67f;
            obj.transform.Find("submarine_fabricator_03").localPosition += new Vector3(0, 0, 0.1f);
            Transform geo = obj.transform.Find("submarine_fabricator_03/submarine_fabricator_03_geo");
            var but = geo.GetComponent<Renderer>();
            Color fabColor = new Color32(0x80, 0x59, 0xA0, 0xFF);
            but.materials[0].color = fabColor;
            but.materials[3].color = fabColor;
            //8059A0FF
        }
    }
}
