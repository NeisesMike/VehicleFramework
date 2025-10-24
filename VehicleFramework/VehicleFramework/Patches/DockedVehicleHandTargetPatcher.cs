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
        [HarmonyPatch(nameof(DockedVehicleHandTarget.OnHandHover))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new(instructions);
            List<CodeInstruction> newCodes = new(codes.Count);
            CodeInstruction myNOP = new(OpCodes.Nop);
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
         */

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(DockedVehicleHandTarget.OnHandHover))]
        public static IEnumerable<CodeInstruction> DockedVehicleHandTargetOnHandHoverTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatch GetEnergyMixinMatch = new(i => i.opcode == OpCodes.Callvirt && i.operand.ToString().Contains("EnergyMixin"));

            CodeMatcher newInstructions = new CodeMatcher(instructions)
                .MatchForward(true, GetEnergyMixinMatch)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
                .Insert(Transpilers.EmitDelegate<Func<EnergyMixin, Vehicle, EnergyMixin?>>(ModVehicle.GetLeastChargedModVehicleEnergyMixinIfNull));

            return newInstructions.InstructionEnumeration();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(DockedVehicleHandTarget.OnHandHover))]
        public static void OnHandHoverPostfix(DockedVehicleHandTarget __instance)
        {
            ModVehicle? mv = __instance.dockingBay.GetDockedVehicle() as ModVehicle;
            if (mv != null)
            {
                string text = mv.HullName;
                if(mv is Submarine sub && sub.Hatches.Count > 0)
                {
                    text = sub.Hatches.First().Hatch.GetComponent<VehicleChildComponents.VehicleHatch>().EnterHint;
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
        public static bool OnHandClickPrefix(DockedVehicleHandTarget __instance)
        {
            Drone? thisDrone = __instance.dockingBay.GetDockedVehicle() as Drone;
            if (thisDrone != null)
            {
                return false;
            }

            ModVehicle? mv = __instance.dockingBay.GetDockedVehicle() as ModVehicle;
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
            Transform? moonpoolMaybe = __instance.dockingBay.transform.parent?.parent;
            if (subRootName.Contains("cyclops"))
            {
                Transform? cyclopsCollisionParent = (__instance.dockingBay.transform.parent?.parent?.parent) ?? throw Admin.SessionManager.Fatal("CyclopsCollisionParent == null in DockedVehicleHandTargetPatch.OnHandClickPrefix!");
                cyclopsCollisionParent.Find("CyclopsCollision").gameObject.SetActive(false);
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
                Admin.SessionManager.StartCoroutine(__instance.dockingBay.dockedVehicle.Undock(Player.main, __instance.dockingBay.transform.position.y));
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                SkyEnvironmentChanged.Broadcast(__instance.dockingBay.dockedVehicle.gameObject, (GameObject)null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            __instance.dockingBay.dockedVehicle = null;

            mv.IsUndockingAnimating = false;
            if (subRootName.Contains("cyclops"))
            {
                Transform? cyclopsCollisionParent = (__instance.dockingBay.transform.parent?.parent?.parent) ?? throw Admin.SessionManager.Fatal("CyclopsCollisionParent == null in DockedVehicleHandTargetPatch.OnHandClickPrefix!");
                IEnumerator ReEnableCollisionsInAMoment()
                {
                    yield return new WaitForSeconds(5);
                    cyclopsCollisionParent.Find("CyclopsCollision").gameObject.SetActive(true);
                    mv.useRigidbody.detectCollisions = true;
                }
                Admin.SessionManager.StartCoroutine(ReEnableCollisionsInAMoment());
            }
            else
            {
                float GetVehicleTop()
                {
                    if(mv.BoundingBoxCollider == null)
                    {
                        throw Admin.SessionManager.Fatal("mv.BoundingBoxCollider == null in DockedVehicleHandTargetPatch.OnHandClickPrefix!");
                    }
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
                    Admin.SessionManager.StartCoroutine(ReEnableCollisionsInAMoment());
                }
            }
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            SkyEnvironmentChanged.Broadcast(mv.gameObject, (GameObject)null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            mv.OnVehicleUndocked();


            return false;
        }
    }
}
