using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
    }
}
