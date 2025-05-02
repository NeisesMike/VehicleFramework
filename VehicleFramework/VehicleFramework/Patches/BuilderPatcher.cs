using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(Builder))]
    public class BuilderPatcher
    {
        // This patch allows Submarines to specify volumes in which things cannot be built or placed
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Builder.CheckAsSubModule))]
        public static void BuilderCheckAsSubModulePostfix(ref bool __result)
        {
            ModVehicle mv = Player.main.GetModVehicle();
            if (mv == null || !(mv is VehicleTypes.Submarine))
            {
                return;
            }
            for (int i = 0; i < Builder.bounds.Count; i++)
            {
                OrientedBounds orientedBounds = Builder.bounds[i];
                if (orientedBounds.rotation.IsDistinguishedIdentity())
                {
                    orientedBounds.rotation = Quaternion.identity;
                }
                orientedBounds.position = Builder.placePosition + Builder.placeRotation * orientedBounds.position;
                orientedBounds.rotation = Builder.placeRotation * orientedBounds.rotation;
                if (orientedBounds.extents.x > 0f && orientedBounds.extents.y > 0f && orientedBounds.extents.z > 0f)
                {
                    List<Collider> outputColliders = new List<Collider>();
                    Builder.GetOverlappedColliders(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, Builder.placeLayerMask.value, QueryTriggerInteraction.Collide, outputColliders);
                    if (outputColliders.Where(x => x.CompareTag(Builder.denyBuildingTag)).Any())
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}
