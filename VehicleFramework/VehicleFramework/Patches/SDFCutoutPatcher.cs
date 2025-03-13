using HarmonyLib;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace VehicleFramework.Patches
{
    /*
    [HarmonyPatch(typeof(SDFCutout))]
    public class SDFCutoutPatcher
    {
        public static int myCount = 1;
        public static List<SDFCutout> cutouts = new List<SDFCutout>();
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SDFCutout.Start))]
        static IEnumerator SDFCutoutStartPostfix(IEnumerator result, SDFCutout __instance)
        {
            // Run original enumerator code
            while (result.MoveNext())
                yield return result.Current;

            // Run your postfix
            ModVehicle mv = __instance.GetComponent<ModVehicle>();
            if (mv != null && mv.BoundingBoxCollider != null)
            {
                cutouts.Add(__instance);
                yield return UWE.CoroutineHost.StartCoroutine(UpdateIt(__instance));
            }

        }
        public static Texture3D CreateSDFTexture3D(Texture3D sourceTexture, Vector3 size)
        {
            if (sourceTexture == null)
            {
                Debug.LogError("Source Texture3D is null.");
                return null;
            }
            int pixelsPerMeter = myCount++;
            // Create a new Texture3D with the same dimensions and format
            Texture3D newTexture = new Texture3D(
                Mathf.RoundToInt(size.x * pixelsPerMeter),
                Mathf.RoundToInt(size.y * pixelsPerMeter),
                Mathf.RoundToInt(size.z * pixelsPerMeter),
                sourceTexture.format,
                sourceTexture.mipmapCount > 1
            );
            int numPixels = Mathf.RoundToInt(size.x * size.y * size.z * pixelsPerMeter * pixelsPerMeter * pixelsPerMeter);
            Color[] pixels = new Color[numPixels];
            for (int i = 0; i < numPixels; i++)
            {
                pixels[i] = Color.white;
            }
            newTexture.SetPixels(pixels);
            newTexture.Apply();  // Apply changes to the GPU
            return newTexture;
        }

        public static IEnumerator UpdateIt(SDFCutout cutout)
        {
            ModVehicle mv = cutout.GetComponent<ModVehicle>();
            if (mv != null && mv.BoundingBoxCollider != null)
            {
                if (mv.BoundingBox != null)
                {
                    mv.BoundingBox.SetActive(true);
                }
                mv.BoundingBoxCollider.gameObject.SetActive(true);
                mv.BoundingBoxCollider.enabled = true;
                yield return null;

                cutout.distanceFieldBounds = mv.BoundingBoxCollider.bounds;
                cutout.distanceFieldBounds.center = mv.BoundingBoxCollider.bounds.center - mv.transform.position;
                cutout.distanceFieldTexture = CreateSDFTexture3D(cutout.distanceFieldTexture, mv.BoundingBoxCollider.bounds.extents);

                cutout.distanceFieldMin = cutout.distanceFieldBounds.center - mv.BoundingBoxCollider.bounds.extents;
                cutout.distanceFieldMax = cutout.distanceFieldBounds.center + mv.BoundingBoxCollider.bounds.extents;

                Vector3 vector = cutout.distanceFieldMax - cutout.distanceFieldMin;
                cutout.distanceFieldSizeRcp = new Vector3(
                    vector.x != 0 ? 1f / vector.x : 0f,
                    vector.y != 0 ? 1f / vector.y : 0f,
                    vector.z != 0 ? 1f / vector.z : 0f
                );

                mv.BoundingBoxCollider.gameObject.SetActive(false);
                mv.BoundingBoxCollider.enabled = false;
                if (mv.BoundingBox != null)
                {
                    mv.BoundingBox.SetActive(false);
                }

                //sdf.distanceFieldSizeRcp = prawnSDF.distanceFieldSizeRcp;
                //sdf.distanceFieldMin = prawnSDF.distanceFieldMin;
                //sdf.distanceFieldMax = prawnSDF.distanceFieldMax;
                //__instance.distanceFieldMin = -10 * Vector3.one;
                //__instance.distanceFieldMax = 10 * Vector3.one;
                //__instance.distanceFieldBounds = new Bounds(Vector3.zero, new Vector3(__instance.transform.localScale.x, __instance.transform.localScale.y, __instance.transform.localScale.z));

            }
        }
    }
    */
}
