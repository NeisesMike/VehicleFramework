using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AtramaVehicle.Builder
{
    public static partial class AtramaBuilder
    {
        public static void applyMarmosetShader(Atrama atrama)
        {
            Logger.Log("Apply Marmoset Shader");
            // Add the marmoset shader to all renderers
            Shader marmosetShader = Shader.Find("MarmosetUBER");
            foreach (var renderer in atrama.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material mat in renderer.materials)
                {
                    // skip some materials
                    if (renderer.gameObject.name.Contains("Light"))
                    {
                        continue;
                    }

                    mat.shader = marmosetShader;

                    // add emission to certain materials
                    // in order to light the interior
                    if (
                        (renderer.gameObject.name == "Main-Body" && mat.name.Contains("Material"))
                        || renderer.gameObject.name == "Mechanical-Panel"
                        || renderer.gameObject.name == "AtramaPilotChair"
                        || renderer.gameObject.name == "Hatch"
                        )
                    {
                        atrama.interiorRenderers.Add(renderer);

                        // TODO move this to OnPowered and OnUnpowered
                        mat.EnableKeyword("MARMO_EMISSION");
                        mat.SetFloat("_EmissionLM", 0.25f);
                        mat.SetFloat("_EmissionLMNight", 0.25f);
                        mat.SetFloat("_GlowStrength", 0f);
                        mat.SetFloat("_GlowStrengthNight", 0f);
                    }

                }
            }
        }
    }
}
