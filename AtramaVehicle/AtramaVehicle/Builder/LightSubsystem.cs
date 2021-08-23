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
        public static void addLightsSubsystem(Atrama atrama)
        {
            Logger.Log("Add Lights Subsystem");
            // get seamoth flood lamp
            GameObject seamothHeadLight = seamoth.transform.Find("lights_parent/light_left").gameObject;
            Transform seamothVL = seamoth.transform.Find("lights_parent/light_left/x_FakeVolumletricLight"); // sic
            MeshFilter seamothVLMF = seamothVL.GetComponent<MeshFilter>();
            MeshRenderer seamothVLMR = seamothVL.GetComponent<MeshRenderer>();


            FMOD_StudioEventEmitter[] fmods = seamoth.GetComponents<FMOD_StudioEventEmitter>();
            foreach (FMOD_StudioEventEmitter fmod in fmods)
            {
                if (fmod.asset.name == "seamoth_light_on")
                {
                    atrama.lightsOnSound = CopyComponent(fmod, atrama.vehicle.gameObject);
                }
                else if (fmod.asset.name == "seamoth_light_off")
                {
                    atrama.lightsOffSound = CopyComponent(fmod, atrama.vehicle.gameObject);
                }
            }

            // check to see whether we've already got lamps
            if (atrama.transform.Find("LightsParent/LeftLight").gameObject.GetComponent<Light>() == null)
            {
                // create left and right atrama flood lamps
                GameObject atramaLeftHeadLight = atrama.transform.Find("LightsParent/LeftLight").gameObject;
                CopyComponent(seamothHeadLight.GetComponent<LightShadowQuality>(), atramaLeftHeadLight);
                var leftLight = atramaLeftHeadLight.EnsureComponent<Light>();
                leftLight.type = LightType.Spot;
                leftLight.spotAngle = 60;
                leftLight.innerSpotAngle = 45;
                leftLight.color = Color.white;
                leftLight.intensity = 2;
                leftLight.range = 120;
                leftLight.shadows = LightShadows.Hard;

                GameObject leftVolumetricLight = new GameObject("LeftVolumetricLight");
                leftVolumetricLight.transform.localEulerAngles = Vector3.zero;
                leftVolumetricLight.transform.parent = atramaLeftHeadLight.transform;
                leftVolumetricLight.transform.localScale = seamothVL.localScale;
                leftVolumetricLight.transform.localPosition = Vector3.zero;

                var lvlMeshFilter = leftVolumetricLight.AddComponent<MeshFilter>();
                lvlMeshFilter.mesh = seamothVLMF.mesh;
                lvlMeshFilter.sharedMesh = seamothVLMF.sharedMesh;

                var lvlMeshRenderer = leftVolumetricLight.AddComponent<MeshRenderer>();
                lvlMeshRenderer.material = seamothVLMR.material;
                lvlMeshRenderer.sharedMaterial = seamothVLMR.sharedMaterial;
                lvlMeshRenderer.shadowCastingMode = seamothVLMR.shadowCastingMode;
                lvlMeshRenderer.renderingLayerMask = seamothVLMR.renderingLayerMask;

                var leftVFX = CopyComponent(seamothHeadLight.GetComponent<VFXVolumetricLight>(), atramaLeftHeadLight);
                leftVFX.lightSource = leftLight;
                leftVFX.color = Color.white;
                leftVFX.volumGO = leftVolumetricLight;
                leftVFX.volumRenderer = lvlMeshRenderer;
                leftVFX.volumMeshFilter = lvlMeshFilter;

                GameObject atramaRightHeadLight = atrama.transform.Find("LightsParent/RightLight").gameObject;
                CopyComponent(seamothHeadLight.GetComponent<LightShadowQuality>(), atramaRightHeadLight);
                var rightLight = atramaRightHeadLight.EnsureComponent<Light>();
                rightLight.type = LightType.Spot;
                rightLight.spotAngle = 60;
                rightLight.innerSpotAngle = 45;
                rightLight.color = Color.white;
                rightLight.intensity = 2;
                rightLight.range = 120;
                rightLight.shadows = LightShadows.Hard;

                GameObject rightVolumetricLight = new GameObject("RightVolumetricLight");
                rightVolumetricLight.transform.localEulerAngles = Vector3.zero;
                rightVolumetricLight.transform.parent = atramaRightHeadLight.transform;
                rightVolumetricLight.transform.localScale = seamothVL.localScale;
                rightVolumetricLight.transform.localPosition = Vector3.zero;

                var rvlMeshFilter = rightVolumetricLight.AddComponent<MeshFilter>();
                rvlMeshFilter.mesh = seamothVLMF.mesh;
                rvlMeshFilter.sharedMesh = seamothVLMF.sharedMesh;

                var rvlMeshRenderer = rightVolumetricLight.AddComponent<MeshRenderer>();
                rvlMeshRenderer.material = seamothVLMR.material;
                rvlMeshRenderer.sharedMaterial = seamothVLMR.sharedMaterial;
                rvlMeshRenderer.shadowCastingMode = seamothVLMR.shadowCastingMode;
                rvlMeshRenderer.renderingLayerMask = seamothVLMR.renderingLayerMask;

                var rightVFX = CopyComponent(seamothHeadLight.GetComponent<VFXVolumetricLight>(), atramaRightHeadLight);
                rightVFX.lightSource = rightLight;
                rightVFX.color = Color.white;
                rightVFX.volumGO = rightVolumetricLight;
                rightVFX.volumRenderer = rvlMeshRenderer;
                rightVFX.volumMeshFilter = rvlMeshFilter;

                atrama.lights.Add(atramaLeftHeadLight);
                atrama.lights.Add(atramaRightHeadLight);
                atrama.volumetricLights.Add(leftVolumetricLight);
                atrama.volumetricLights.Add(rightVolumetricLight);

                atramaLeftHeadLight.transform.localEulerAngles = new Vector3(0, 350, 0);
                atramaRightHeadLight.transform.localEulerAngles = new Vector3(0, 10, 0);
            }
            else
            {
                atrama.lights.Add(atrama.transform.Find("LightsParent/LeftLight").gameObject);
                atrama.lights.Add(atrama.transform.Find("LightsParent/RightLight").gameObject);

                GameObject leftVolumetricLight = new GameObject("LeftVolumetricLight");
                leftVolumetricLight.transform.localEulerAngles = Vector3.zero;
                leftVolumetricLight.transform.parent = atrama.transform.Find("LightsParent/LeftLight").transform;
                leftVolumetricLight.transform.localScale = seamothVL.localScale;
                leftVolumetricLight.transform.localPosition = Vector3.zero;

                GameObject rightVolumetricLight = new GameObject("RightVolumetricLight");
                rightVolumetricLight.transform.localEulerAngles = Vector3.zero;
                rightVolumetricLight.transform.parent = atrama.transform.Find("LightsParent/RightLight").transform;
                rightVolumetricLight.transform.localScale = seamothVL.localScale;
                rightVolumetricLight.transform.localPosition = Vector3.zero;

                atrama.volumetricLights.Add(leftVolumetricLight);
                atrama.volumetricLights.Add(rightVolumetricLight);
            }
        }
    }
}
