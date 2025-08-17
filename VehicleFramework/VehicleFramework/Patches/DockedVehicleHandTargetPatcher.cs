using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using VehicleFramework.VehicleTypes;

// PURPOSE: allow docked vehicles to be hovered and clicked in the expected ways
// VALUE: High.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(DockedVehicleHandTarget))]
    public static class DockedVehicleHandTargetPatch
    {
        /* This transpiler makes one part of OnHandHover more generic
         * Optionally change GetComponent to GetComponentInChildren
         * Simple as
         */
        [HarmonyPatch(nameof(DockedVehicleHandTarget.OnHandHover))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            List<CodeInstruction> newCodes = new List<CodeInstruction>(codes.Count);
            CodeInstruction myNOP = new CodeInstruction(OpCodes.Nop);
            for (int i = 0; i < codes.Count; i++)
            {
                newCodes.Add(myNOP);
            }
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString().ToLower().Contains("energymixin"))
                {
                    newCodes[i] = CodeInstruction.Call(typeof(ModVehicle), nameof(ModVehicle.GetEnergyMixinFromVehicle));
                }
                else
                {
                    newCodes[i] = codes[i];
                }
            }
            return newCodes.AsEnumerable();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(DockedVehicleHandTarget.OnHandHover))]
        public static void OnHandHoverPostfix(DockedVehicleHandTarget __instance)
        {
            ModVehicle mv = __instance.dockingBay.GetDockedVehicle() as ModVehicle;
            if (mv != null)
            {
                string text = mv.subName.hullName.text;
                if((mv as Drone) != null)
                {
                    text = mv.subName.hullName.text;
                }
                else if(mv is Submarine && (mv as Submarine).Hatches.Any())
                {
                    text = (mv as Submarine).Hatches.First().Hatch.GetComponent<VehicleHatch>().EnterHint;
                }
                float energyActual = 0;
                float energyMax = 0;
                foreach (var battery in mv.energyInterface.sources)
                {
                    energyActual += battery.charge;
                    energyMax += battery.capacity;
                }
                float energyFraction = energyActual / energyMax;
                if (energyFraction == 1)
                {
                    string format2 = Language.main.GetFormat<float>("VehicleStatusChargedFormat", mv.liveMixin.GetHealthFraction());
                    HandReticle.main.SetText(HandReticle.TextType.Hand, text, true, ((mv as Drone) == null) ? GameInput.Button.LeftHand : GameInput.Button.None);
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, format2, false, GameInput.Button.None);
                }
                else
                {
                    string format = Language.main.GetFormat<float, float>("VehicleStatusFormat", mv.liveMixin.GetHealthFraction(), energyFraction);
                    HandReticle.main.SetText(HandReticle.TextType.Hand, text, true, ((mv as Drone) == null) ? GameInput.Button.LeftHand : GameInput.Button.None);
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, format, false, GameInput.Button.None);
                }
                HandReticle.main.SetIcon(((mv as Drone) == null) ? HandReticle.IconType.Hand : HandReticle.IconType.Default, 1f);
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(DockedVehicleHandTarget.OnHandClick))]
        public static bool OnHandClickPrefix(DockedVehicleHandTarget __instance, GUIHand hand)
        {
            Drone thisDrone = __instance.dockingBay.GetDockedVehicle() as Drone;
            if (thisDrone != null)
            {
                return false;
            }

            ModVehicle mv = __instance.dockingBay.GetDockedVehicle() as ModVehicle;
            if(mv == null)
            {
                return true;
            }

            if (!__instance.dockingBay.HasUndockingClearance())
            {
                return false;
            }

            __instance.dockingBay.OnUndockingStart();
            __instance.dockingBay.subRoot.BroadcastMessage("OnLaunchBayOpening", SendMessageOptions.DontRequireReceiver);

            mv.IsUndockingAnimating = true;
            string subRootName = __instance.dockingBay.subRoot.name.ToLower();
            Transform moonpoolMaybe = __instance.dockingBay.transform.parent?.parent;
            if (subRootName.Contains("cyclops"))
            {
                __instance.dockingBay.transform.parent.parent.parent.Find("CyclopsCollision").gameObject.SetActive(false);
            }
            else
            {
                if(moonpoolMaybe != null && moonpoolMaybe.name.Equals("BaseMoonpool(Clone)", StringComparison.OrdinalIgnoreCase))
                {
                    moonpoolMaybe.Find("Collisions").gameObject.SetActive(false);
                }
            }
            Player.main.SetCurrentSub(null, false);
            if (__instance.dockingBay.dockedVehicle != null)
            {
                MainPatcher.Instance.StartCoroutine(__instance.dockingBay.dockedVehicle.Undock(Player.main, __instance.dockingBay.transform.position.y));
                SkyEnvironmentChanged.Broadcast(__instance.dockingBay.dockedVehicle.gameObject, (GameObject)null);
            }
            __instance.dockingBay.dockedVehicle = null;

            mv.IsUndockingAnimating = false;
            if (subRootName.Contains("cyclops"))
            {
                IEnumerator ReEnableCollisionsInAMoment()
                {
                    yield return new WaitForSeconds(5);
                    __instance.dockingBay.transform.parent.parent.parent.Find("CyclopsCollision").gameObject.SetActive(true);
                    mv.useRigidbody.detectCollisions = true;
                }
                MainPatcher.Instance.StartCoroutine(ReEnableCollisionsInAMoment());
            }
            else
            {
                float GetVehicleTop()
                {
                    Vector3 worldCenter = mv.BoundingBoxCollider.transform.TransformPoint(mv.BoundingBoxCollider.center);
                    return worldCenter.y + (mv.BoundingBoxCollider.size.y * 0.5f * mv.BoundingBoxCollider.transform.lossyScale.y);
                }
                float GetMoonPoolPlane()
                {
                    return moonpoolMaybe.Find("Flood_BaseMoonPool/x_BaseWaterPlane").transform.position.y;
                }
                IEnumerator ReEnableCollisionsInAMoment()
                {
                    while (GetMoonPoolPlane() < GetVehicleTop())
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    moonpoolMaybe.Find("Collisions").gameObject.SetActive(true);
                    mv.useRigidbody.detectCollisions = true;
                }
                if (moonpoolMaybe != null && moonpoolMaybe.name.Equals("BaseMoonpool(Clone)", StringComparison.OrdinalIgnoreCase))
                {
                    MainPatcher.Instance.StartCoroutine(ReEnableCollisionsInAMoment());
                }
            }
            SkyEnvironmentChanged.Broadcast(mv.gameObject, (GameObject)null);
            mv.OnVehicleUndocked();


            return false;
        }
    }
}
