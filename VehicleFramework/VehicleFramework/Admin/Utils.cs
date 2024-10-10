using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.UpgradeTypes;

namespace VehicleFramework.Admin
{
    public static class Utils
    {
        internal enum UpgradePath
        {
            ModVehicle,
            Seamoth,
            Exosuit,
            Cyclops
        }
        internal static string UpgradePathToString(UpgradePath path)
        {
            switch (path)
            {
                case UpgradePath.ModVehicle:
                    return "MVCM";
                case UpgradePath.Seamoth:
                    return "VVSM";
                case UpgradePath.Exosuit:
                    return "VVEM";
                case UpgradePath.Cyclops:
                    return "VVCM";
                default:
                    return "ERROR";
            }
        }
        internal static void AddFabricatorMenus()
        {
            // patch in the crafting node for the vehicle upgrade menu
            string[] mvUpgradesPath = { "MVUM" };
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, mvUpgradesPath[0], LocalizationManager.GetString(EnglishString.MVModules), MainPatcher.ModVehicleIcon);
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, "MVDM", LocalizationManager.GetString(EnglishString.MVDepthModules), MainPatcher.ModVehicleIcon, mvUpgradesPath);
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradePathToString(UpgradePath.ModVehicle), "Mod Vehicle Common Modules", MainPatcher.ModVehicleIcon, mvUpgradesPath);
            string[] vanillaUpgradesPath = { "VVUM" };
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, vanillaUpgradesPath[0], "Vanilla Vehicle Modules", MainPatcher.ModVehicleIcon);
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradePathToString(UpgradePath.Seamoth), "VF Seamoth Modules", MainPatcher.ModVehicleIcon, vanillaUpgradesPath);
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradePathToString(UpgradePath.Exosuit), "VF Exosuit Modules", MainPatcher.ModVehicleIcon, vanillaUpgradesPath);
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradePathToString(UpgradePath.Cyclops), "VF Cyclops Modules", MainPatcher.ModVehicleIcon, vanillaUpgradesPath);
        }
        public static Shader StoreShader(List<MeshRenderer> rends)
        {
            Shader m_ShaderMemory = null;
            foreach (var rend in rends) //go.GetComponentsInChildren<MeshRenderer>(true)
            {
                // skip some materials
                foreach (Material mat in rend.materials)
                {
                    if (mat.shader != null)
                    {
                        m_ShaderMemory = mat.shader;
                        break;
                    }
                }
            }
            return m_ShaderMemory;
        }
        public static void ListShadersInUse()
        {
            HashSet<string> shaderNames = new HashSet<string>();

            // Find all materials currently loaded in the game.
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();

            foreach (var material in materials)
            {
                if (material.shader != null)
                {
                    // Add the shader name to the set to ensure uniqueness.
                    shaderNames.Add(material.shader.name);
                }
            }

            // Now you have a unique list of shader names in use.
            foreach (var shaderName in shaderNames)
            {
                Debug.Log("Shader in use: " + shaderName);
            }
        }
        public static void ListShaderProperties()
        {
            Shader shader = Shader.Find("MarmosetUBER");
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string propertyName = shader.GetPropertyName(i);
                Debug.Log($"Property {i}: {propertyName}, Type: {shader.GetPropertyType(i)}");
            }
        }
        public static void ApplyInteriorLighting()
        {
            //ListShadersInUse();
            //ListShaderProperties();
            //VehicleBuilder.ApplyShaders(this, shader4);
        }
        public static void LoadShader(ModVehicle mv, Shader shade)
        {
            VehicleBuilder.ApplyShaders(mv, shade);
        }
        public static bool IsAnAncestorTheCurrentMountedVehicle(Transform current)
        {
            if (current == null)
            {
                return false;
            }
            if (current.GetComponent<Vehicle>() != null)
            {
                return current.GetComponent<Vehicle>() == Player.main.GetVehicle();
            }
            return IsAnAncestorTheCurrentMountedVehicle(current.parent);
        }
        public static void RegisterDepthModules()
        {
            UpgradeCompat compat = new UpgradeCompat
            {
                skipModVehicle = false,
                skipCyclops = true,
                skipSeamoth = true,
                skipExosuit = true
            };
            UpgradeTechTypes depth1 = UpgradeRegistrar.RegisterUpgrade(new DepthModules.DepthModule1(), compat);

            var depthmodule2 = new DepthModules.DepthModule2();
            depthmodule2.ExtendRecipe(new Assets.Ingredient(depth1.forModVehicle, 1));
            UpgradeTechTypes depth2 = UpgradeRegistrar.RegisterUpgrade(depthmodule2, compat);

            var depthmodule3 = new DepthModules.DepthModule3();
            depthmodule3.ExtendRecipe(new Assets.Ingredient(depth2.forModVehicle, 1));
            UpgradeTechTypes depth3 = UpgradeRegistrar.RegisterUpgrade(depthmodule3, compat);
        }
        public static void EvaluateDepthModules(AddActionParams param)
        {
            if(param.vehicle.GetComponent<ModVehicle>() == null)
            {
                Subtitles.Add("This upgrade is not compatible with this vehicle.");
                return;
            }
            // Iterate over all upgrade modules,
            // in order to determine our max depth module level
            int maxDepthModuleLevel = 0;
            List<string> upgradeSlots = new List<string>();
            param.vehicle.upgradesInput.equipment.GetSlots(VehicleBuilder.ModuleType, upgradeSlots);
            foreach (String slot in upgradeSlots)
            {
                InventoryItem upgrade = param.vehicle.upgradesInput.equipment.GetItemInSlot(slot);
                if (upgrade != null)
                {
                    //Logger.Log(slot + " : " + upgrade.item.name);
                    if (upgrade.item.name == "ModVehicleDepthModule1(Clone)")
                    {
                        if (maxDepthModuleLevel < 1)
                        {
                            maxDepthModuleLevel = 1;
                        }
                    }
                    else if (upgrade.item.name == "ModVehicleDepthModule2(Clone)")
                    {
                        if (maxDepthModuleLevel < 2)
                        {
                            maxDepthModuleLevel = 2;
                        }
                    }
                    else if (upgrade.item.name == "ModVehicleDepthModule3(Clone)")
                    {
                        if (maxDepthModuleLevel < 3)
                        {
                            maxDepthModuleLevel = 3;
                        }
                    }
                }
            }
            int extraDepthToAdd = 0;
            extraDepthToAdd = maxDepthModuleLevel > 0 ? extraDepthToAdd += param.vehicle.GetComponent<ModVehicle>().CrushDepthUpgrade1 : extraDepthToAdd;
            extraDepthToAdd = maxDepthModuleLevel > 1 ? extraDepthToAdd += param.vehicle.GetComponent<ModVehicle>().CrushDepthUpgrade2 : extraDepthToAdd;
            extraDepthToAdd = maxDepthModuleLevel > 2 ? extraDepthToAdd += param.vehicle.GetComponent<ModVehicle>().CrushDepthUpgrade3 : extraDepthToAdd;
            param.vehicle.GetComponent<CrushDamage>().SetExtraCrushDepth(extraDepthToAdd);
        }
        public static TechType GetTechTypeFromVehicleName(string name)
        {
            try
            {
                VehicleEntry ve = VehicleManager.vehicleTypes.Where(x => x.name.Contains(name)).First();
                return ve.techType;
            }
            catch
            {
                Logger.Error("GetTechTypeFromVehicleName Error. Could not find a vehicle by the name: " + name + ". Here are all vehicle names:");
                VehicleManager.vehicleTypes.ForEach(x => Logger.Log(x.name));
                return 0;
            }
        }
        public static bool IsPilotingCyclops()
        {
            return IsInCyclops() && Player.main.mode == Player.Mode.Piloting;
        }
        public static bool IsInCyclops()
        {
            return Player.main.currentSub != null && Player.main.currentSub.name.ToLower().Contains("cyclops");
        }
        public static void EnableSimpleEmission(Material mat)
        {
            // This is the minumum requirement for emission under the marmosetuber shader.
            // No guarantees this will work well, but it's a good starting place.
            // For example, not all materials will want to use a specular map. In that case,
            // it can make a material look brighter, shinier, or more luminescent than it should be.
            mat.EnableKeyword("MARMO_EMISSION");
            mat.EnableKeyword("MARMO_SPECMAP");
            mat.SetFloat("_GlowStrength", 0);
            mat.SetFloat("_GlowStrengthNight", 0);
            mat.SetFloat("_EmissionLM", 1);
            mat.SetFloat("_EmissionLMNight", 1);
        }
    }
}
