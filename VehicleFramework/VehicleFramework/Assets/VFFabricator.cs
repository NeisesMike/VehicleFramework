using Nautilus.Assets.Gadgets;
using static CraftData;
using UnityEngine;

namespace VehicleFramework.Assets
{
    internal static class VFFabricator
    {
        private const string ClassID = "VFFabricatorClassID";
        private static readonly string DisplayName = Language.main.Get("VFFabricatorDisplayName");
        private static readonly string Description = Language.main.Get("VFFabricatorDesc");
        internal static CraftTree.Type TreeType = default;
        internal static void CreateAndRegister()
        {
            var Info = Nautilus.Assets.PrefabInfo.WithTechType(ClassID, DisplayName, Description)
                .WithIcon(SpriteManager.Get(TechType.Fabricator));

            var prefab = new Nautilus.Assets.CustomPrefab(Info);

            if (GetBuilderIndex(TechType.Fabricator, out var group, out var category, out _))
            {
                var scanGadget = prefab.SetPdaGroupCategoryAfter(group, category, TechType.Fabricator);
                scanGadget.RequiredForUnlock = TechType.Constructor;
            }

            var fabGadget = prefab.CreateFabricator(out var treeType);
            TreeType = treeType;

            var vfFabTemplate = new Nautilus.Assets.PrefabTemplates.FabricatorTemplate(Info, TreeType)
            {
                ModifyPrefab = ModifyFabricatorPrefab,
                FabricatorModel = Nautilus.Assets.PrefabTemplates.FabricatorTemplate.Model.MoonPool,
                ConstructableFlags = Nautilus.Utility.ConstructableFlags.Wall | Nautilus.Utility.ConstructableFlags.Base | Nautilus.Utility.ConstructableFlags.Submarine
                | Nautilus.Utility.ConstructableFlags.Inside
            };

            prefab.SetGameObject(vfFabTemplate);

            Nautilus.Handlers.CraftDataHandler.SetRecipeData(Info.TechType, GetBlueprintRecipe());
            prefab.Register();
        }
        private static Nautilus.Crafting.RecipeData GetBlueprintRecipe()
        {
            return new Nautilus.Crafting.RecipeData
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.Titanium, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.Diamond, 1),
                }
            };
        }
        private static void ModifyFabricatorPrefab(GameObject obj)
        {
            obj.transform.localScale *= 0.67f;
            Component.DestroyImmediate(obj.GetComponent<Collider>());
            Transform fabRoot = obj.transform.Find("submarine_fabricator_03");
            Transform geo = fabRoot.Find("submarine_fabricator_03_geo");
            Color fabColor = new Color32(0x80, 0x59, 0xA0, 0xFF);
            fabRoot.localPosition += new Vector3(0, 0, 0.1f);
            fabRoot.GetComponent<BoxCollider>().center = new Vector3(-0.01f, 0.9f, 0.18f);
            var renderer = geo.GetComponent<Renderer>();
            renderer.materials[0].color = fabColor;
            renderer.materials[3].color = fabColor;
            obj.AddComponent<ConstructableBounds>().bounds = new OrientedBounds(
                fabRoot.GetComponent<BoxCollider>().center - new Vector3(0f, 0.30f, 0f),
                Quaternion.identity,
                fabRoot.GetComponent<BoxCollider>().size / 2 - new Vector3(0.15f, 0f, 0f));
            //8059A0FF
        }
    }
}
