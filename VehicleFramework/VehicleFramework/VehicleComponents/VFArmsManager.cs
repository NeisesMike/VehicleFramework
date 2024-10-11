using System.Collections.Generic;
using UnityEngine;
using VehicleFramework.UpgradeTypes;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Admin;

namespace VehicleFramework.VehicleComponents
{
    // This component is applied to ModVehicles.
    // It manages ModVehicleArms and their actions.
    public class VFArmsManager : MonoBehaviour
    {
        public GameObject leftArm;
        public GameObject rightArm;
        internal int leftArmSlotID = 0;
        internal int rightArmSlotID = 0;

        public void UpdateArms(ModVehicleArm arm, int slotId)
        {
            ModVehicle mv = GetComponent<ModVehicle>();
            string thisSlotName = mv.slotIDs[slotId];
            TechType techTypeInSlot = mv.modules.GetTechTypeInSlot(mv.slotIDs[slotId]);
            if (thisSlotName.Contains(ModuleBuilder.LeftArmSlotName))
            {
                DestroyArm(true);
                if(arm.HasTechType(techTypeInSlot))
                {
                    SpawnArm(mv, arm, true);
                    leftArmSlotID = slotId;
                }
            }
            else if (thisSlotName.Contains(ModuleBuilder.RightArmSlotName))
            {
                DestroyArm(false);
                if (arm.HasTechType(techTypeInSlot))
                {
                    SpawnArm(mv, arm, false);
                    rightArmSlotID = slotId;
                }
            }
            else
            {
                Logger.Warn("Can't update arms for a non-arm slot. How did we get here?");
                return;
            }
            mv.Arms.originalLeftArm?.SetActive(GetComponent<VFArmsManager>().leftArm == null);
            mv.Arms.originalRightArm?.SetActive(GetComponent<VFArmsManager>().rightArm == null);
        }
        public void ShowArms(bool isShown)
        {
            ModVehicle mv = GetComponent<ModVehicle>();
            if(isShown)
            {
                leftArm?.SetActive(true);
                rightArm?.SetActive(true);
                mv.Arms.originalLeftArm?.SetActive(leftArm == null);
                mv.Arms.originalRightArm?.SetActive(rightArm == null);
            }
            else
            {
                leftArm?.SetActive(false);
                rightArm?.SetActive(false);
                mv.Arms.originalLeftArm?.SetActive(false);
                mv.Arms.originalRightArm?.SetActive(false);
            }
        }
        public void SpawnArm(ModVehicle mv, ModVehicleArm arm, bool isLeft)
        {
            GameObject armPrefab = arm.armPrefab;
            if (isLeft && leftArm == null)
            {
                leftArm = UnityEngine.Object.Instantiate<GameObject>(armPrefab);
                leftArm.transform.SetParent(mv.transform);
                leftArm.transform.localRotation = Quaternion.identity;
                if (mv.Arms.leftArmPlacement != null)
                {
                    leftArm.transform.localPosition = mv.Arms.leftArmPlacement.localPosition;
                    leftArm.transform.localRotation = mv.Arms.leftArmPlacement.localRotation;
                }
                else
                {
                    if(mv is Drone)
                    {
                        leftArm.transform.localPosition = (mv as Drone).CameraLocation.localPosition
                            - Vector3.right;
                    }
                    else
                    {
                        Vector3 worldPositionOfC = mv.playerPosition.transform.position;
                        Vector3 localPositionOfCInA = mv.transform.InverseTransformPoint(worldPositionOfC);
                        leftArm.transform.localPosition = localPositionOfCInA
                            - Vector3.right;
                    }
                }
                leftArm.name = "LeftArm";
            }
            else if (!isLeft && rightArm == null)
            {
                rightArm = UnityEngine.Object.Instantiate<GameObject>(armPrefab);
                rightArm.transform.SetParent(mv.transform);
                rightArm.transform.localRotation = Quaternion.identity;
                rightArm.transform.localScale = new Vector3(
                    -rightArm.transform.localScale.x,
                    rightArm.transform.localScale.y,
                    rightArm.transform.localScale.z
                );
                if (mv.Arms.rightArmPlacement != null)
                {
                    rightArm.transform.localPosition = mv.Arms.rightArmPlacement.localPosition;
                    rightArm.transform.localRotation = mv.Arms.rightArmPlacement.localRotation;
                }
                else
                {
                    if (mv is Drone)
                    {
                        leftArm.transform.localPosition = (mv as Drone).CameraLocation.localPosition
                            + Vector3.right;
                    }
                    else
                    {
                        Vector3 worldPositionOfC = mv.playerPosition.transform.position;
                        Vector3 localPositionOfCInA = mv.transform.InverseTransformPoint(worldPositionOfC);
                        rightArm.transform.localPosition = localPositionOfCInA
                            + Vector3.right;
                    }
                }
                rightArm.name = "RightArm";
            }
        }
        public void DestroyArm(bool isLeft)
        {
            if (isLeft)
            {
                leftArm = null;
                leftArmSlotID = 0;
                UnityEngine.Object.DestroyImmediate(transform.Find("LeftArm")?.gameObject);
            }
            else
            {
                rightArm = null;
                rightArmSlotID = 0;
                UnityEngine.Object.DestroyImmediate(transform.Find("RightArm")?.gameObject);
            }
        }

        public ArmActionParams GetArmActionParams(bool isLeft)
        {
            ModVehicle mv = GetComponent<ModVehicle>();
            int slotID = isLeft ? mv.GetSlotIndex(ModuleBuilder.LeftArmSlotName) : mv.GetSlotIndex(ModuleBuilder.RightArmSlotName);
            mv.GetQuickSlotType(slotID, out TechType techType);
            return new ArmActionParams
            {
                vehicle = mv,
                slotID = slotID,
                techType = techType,
                arm = isLeft ? GetComponent<VFArmsManager>().leftArm : GetComponent<VFArmsManager>().rightArm
            };
        }
        public void DoArmDown(bool isLeft)
        {
            ArmActionParams param = GetArmActionParams(isLeft);
            if (GameInput.GetButtonDown(GameInput.Button.AltTool))
            {
                UpgradeRegistrar.OnArmAltActions.ForEach(x => x(param));
            }
            else
            {
                UpgradeRegistrar.OnArmDownActions.ForEach(x => x(param));
            }
        }
        public void DoArmUp(bool isLeft)
        {
            ArmActionParams param = GetArmActionParams(isLeft);
            UpgradeRegistrar.OnArmUpActions.ForEach(x => x(param));
        }
        public void DoArmHeld(bool isLeft)
        {
            ArmActionParams param = GetArmActionParams(isLeft);
            UpgradeRegistrar.OnArmHeldActions.ForEach(x => x(param));
        }
    }
}
