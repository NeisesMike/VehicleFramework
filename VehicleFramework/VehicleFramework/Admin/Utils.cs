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
        public static void ApplyMarmoset(GameObject go)
        {
            go.GetComponentsInChildren<MeshRenderer>(true).ForEach(x => x.materials.ForEach(y => y.shader = Shader.Find("MarmosetUBER")));
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
            depthmodule2.ExtendRecipe(depth1);
            UpgradeTechTypes depth2 = UpgradeRegistrar.RegisterUpgrade(depthmodule2, compat);

            var depthmodule3 = new DepthModules.DepthModule3();
            depthmodule3.ExtendRecipe(depth2);
            UpgradeTechTypes depth3 = UpgradeRegistrar.RegisterUpgrade(depthmodule3, compat);
        }
        public static void EvaluateDepthModules(AddActionParams param)
        {
            if(param.vehicle.GetComponent<ModVehicle>() == null)
            {
                Subtitles.Add("This upgrade is not compatible with this vehicle.");
                return;
            }
            ModVehicle mv = param.vehicle.GetComponent<ModVehicle>();
            // Iterate over all upgrade modules,
            // in order to determine our max depth module level
            int maxDepthModuleLevel = 0;
            List<string> upgrades = mv.GetCurrentUpgrades();
            foreach (String upgrade in upgrades)
            {
                if (string.Equals(upgrade, "ModVehicleDepthModule1(Clone)", StringComparison.OrdinalIgnoreCase))
                {
                    maxDepthModuleLevel = maxDepthModuleLevel < 1 ? 1 : maxDepthModuleLevel;
                }
                else if (string.Equals(upgrade, "ModVehicleDepthModule2(Clone)", StringComparison.OrdinalIgnoreCase))
                {
                    maxDepthModuleLevel = maxDepthModuleLevel < 2 ? 2 : maxDepthModuleLevel;
                }
                else if (string.Equals(upgrade, "ModVehicleDepthModule3(Clone)", StringComparison.OrdinalIgnoreCase))
                {
                    maxDepthModuleLevel = maxDepthModuleLevel < 3 ? 3 : maxDepthModuleLevel;
                }
            }
            int extraDepthToAdd = 0;
            extraDepthToAdd = maxDepthModuleLevel > 0 ? extraDepthToAdd += mv.CrushDepthUpgrade1 : extraDepthToAdd;
            extraDepthToAdd = maxDepthModuleLevel > 1 ? extraDepthToAdd += mv.CrushDepthUpgrade2 : extraDepthToAdd;
            extraDepthToAdd = maxDepthModuleLevel > 2 ? extraDepthToAdd += mv.CrushDepthUpgrade3 : extraDepthToAdd;
            mv.GetComponent<CrushDamage>().SetExtraCrushDepth(extraDepthToAdd);
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
        public static void EnableSimpleEmission(Material mat, float dayAmount = 1f, float nightAmount = 1f)
        {
            // This is the minumum requirement for emission under the marmosetuber shader.
            // No guarantees this will work well, but it's a good starting place.
            // For example, not all materials will want to use a specular map. In that case,
            // it can make a material look brighter, shinier, or more luminescent than it should be.
            mat.EnableKeyword("MARMO_EMISSION");
            mat.EnableKeyword("MARMO_SPECMAP");
            mat.SetFloat("_GlowStrength", 0);
            mat.SetFloat("_GlowStrengthNight", 0);
            mat.SetFloat("_EmissionLM", dayAmount);
            mat.SetFloat("_EmissionLMNight", nightAmount);
        }
    }
}
