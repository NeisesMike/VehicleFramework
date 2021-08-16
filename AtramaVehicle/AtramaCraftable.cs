﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using System.IO;

namespace AtramaVehicle
{
    public class AtramaCraftable : Craftable
    {

        //===============================
        // Craftable overrides
        //===============================
        public override CraftTree.Type FabricatorType => CraftTree.Type.Constructor;
        public override string[] StepsToFabricatorTab => new[] { "Vehicles" };
        public override float CraftingTime => 10f;


        //===============================
        // PDAItem overrides
        //===============================
        //public override TechType RequiredForUnlock => TechType.Constructor;
        public override bool UnlockedAtStart => true;
        public override TechGroup GroupForPDA => TechGroup.Constructor;
        public override TechCategory CategoryForPDA => TechCategory.Constructor;
        public override PDAEncyclopedia.EntryData EncyclopediaEntryData
        {
            get
            {
                PDAEncyclopedia.EntryData entry = new PDAEncyclopedia.EntryData();
                entry.key = "Atrama";
                entry.path = "Tech/Vehicles";
                entry.nodes = new[] { "Tech", "Vehicles" };
                entry.unlocked = false;
                return entry;
            }
        }

        public AtramaCraftable(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.TitaniumIngot, 1),
                    new Ingredient(TechType.PlasteelIngot, 1),
                    new Ingredient(TechType.Lubricant, 1),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.Lead, 2),
                    new Ingredient(TechType.EnameledGlass, 2)
                },
                craftAmount = 1
            };
        }


        //===============================
        // Spawnable overrides
        //===============================
        protected override Atlas.Sprite GetItemSprite()
        {
            return null;
        }

        //===============================
        // ModPrefab overrides
        //===============================


        public override GameObject GetGameObject()
        {
            //Logger.Log("GetGameObject begin");
            if(AtramaPreparer.atramaPrefab == null)
            {
                AtramaPreparer.buildAtramaPrefab();
            }
            GameObject thisAtrama = AtramaPreparer.atramaPrefab;

            // "Add essential components"
            // What is this? Is it really necessary?
            thisAtrama.EnsureComponent<TechTag>().type = TechType;
            thisAtrama.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;

            /*
            Logger.Log("Enabling Atrama...");
            thisAtrama.SetActive(true);
            */

            /*
            for (int i = 0; i < thisAtrama.transform.childCount; i++)
            {
                thisAtrama.transform.GetChild(i).gameObject.SetActive(true);
            }
            */

            return thisAtrama;
        }



        /*

        BuildBotPath CreateBuildBotPath(GameObject gameobjectWithComponent, Transform parent)
        {
            var comp = gameobjectWithComponent.AddComponent<BuildBotPath>();
            comp.points = new Transform[parent.childCount];
            for (int i = 0; i < parent.childCount; i++)
            {
                comp.points[i] = parent.GetChild(i);
            }
            return comp;
        }

        Material GetGlassMaterial()
        {
            var reference = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Aquarium));

            Renderer[] renderers = reference.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.ToLower().Contains("glass"))
                    {
                        return material;
                    }
                }
            }
            Resources.UnloadAsset(reference);
            return null;
        }

        //I know this is horribly messy, I don't know what half the properties here do, but it works.
        void ApplyMaterials()
        {
            var renderers = prefab.GetComponentsInChildren<Renderer>();
            var shader = Shader.Find("MarmosetUBER");

            foreach (var renderer in renderers)
            {
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    Material mat = renderer.materials[i];
                    mat.shader = shader;
                    mat.SetFloat("_Glossiness", 0.6f);
                    Texture specularTexture = mat.GetTexture("_SpecGlossMap");
                    if (specularTexture != null)
                    {
                        mat.SetTexture("_SpecTex", specularTexture);
                        mat.SetFloat("_SpecInt", 1f);
                        mat.SetFloat("_Shininess", 3f);
                        mat.EnableKeyword("MARMO_SPECMAP");
                        mat.SetColor("_SpecColor", new Color(0.796875f, 0.796875f, 0.796875f, 0.796875f));
                        mat.SetFloat("_Fresnel", 0f);
                        mat.SetVector("_SpecTex_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    }

                    if (mat.GetTexture("_BumpMap"))
                    {
                        mat.EnableKeyword("_NORMALMAP");
                    }
                    if (mat.name.StartsWith("Decal4"))
                    {
                        mat.SetTexture("_SpecTex", QPatch.bundle.LoadAsset<Texture>("Console_spec.png"));
                    }
                    if (mat.name.StartsWith("Decal"))
                    {
                        mat.EnableKeyword("MARMO_ALPHA_CLIP");
                    }
                    Texture emissionTexture = mat.GetTexture("_EmissionMap");
                    if (emissionTexture || mat.name.Contains("illum"))
                    {
                        mat.EnableKeyword("MARMO_EMISSION");
                        mat.SetFloat("_EnableGlow", 1f);
                        mat.SetTexture("_Illum", emissionTexture);
                    }
                }
            }
            prefab.SearchChild("Window").GetComponent<MeshRenderer>().material = GetGlassMaterial();
        }

        */

    }
}
