using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.UpgradeTypes;

namespace VehicleFramework.VehicleComponents
{
    public class VFArmsManager : MonoBehaviour
    {
        public GameObject leftArm;
        public GameObject rightArm;
        internal int leftArmSlotID = 0;
        internal int rightArmSlotID = 0;

        public IEnumerator UpdateArms(ModVehicleArm arm, int slotId)
        {
            ModVehicle mv = GetComponent<ModVehicle>();
            string thisSlotName = mv.slotIDs[slotId];
            TechType techTypeInSlot = mv.modules.GetTechTypeInSlot(mv.slotIDs[slotId]);
            if (thisSlotName.Contains(ModuleBuilder.LeftArmSlotName))
            {
                DestroyArm(true);
                if (techTypeInSlot == arm.TechType)
                {
                    yield return UWE.CoroutineHost.StartCoroutine(SpawnArm(mv, arm, true));
                    leftArmSlotID = slotId;
                }
            }
            else if (thisSlotName.Contains(ModuleBuilder.RightArmSlotName))
            {
                DestroyArm(false);
                if (techTypeInSlot == arm.TechType)
                {
                    yield return UWE.CoroutineHost.StartCoroutine(SpawnArm(mv, arm, false));
                    rightArmSlotID = slotId;
                }
            }
            else
            {
                Logger.Warn("Can't update arms for a non-arm slot. How did we get here?");
                yield break;
            }
            mv.Arms.originalLeftArm?.SetActive(GetComponent<VFArmsManager>().leftArm == null);
            mv.Arms.originalRightArm?.SetActive(GetComponent<VFArmsManager>().rightArm == null);
        }
        public IEnumerator SpawnArm(ModVehicle mv, ModVehicleArm arm, bool isLeft)
        {
            TaskResult<GameObject> armRequest = new TaskResult<GameObject>();
            yield return UWE.CoroutineHost.StartCoroutine(arm.GetArmPrefab(armRequest));
            GameObject armPrefab = armRequest.Get();
            if(armPrefab == null)
            {
                Logger.Error("VFArmsManager Error: GetArmPrefab returned a null GameObject instead of a valid arm.");
                yield break;
            }

            if (isLeft && leftArm == null)
            {
                leftArm = UnityEngine.Object.Instantiate<GameObject>(armPrefab);
                leftArm.transform.parent = mv.transform;
                leftArm.transform.localRotation = Quaternion.identity;
                if (mv.Arms.leftArmPlacement != null)
                {
                    leftArm.transform.localPosition = mv.Arms.leftArmPlacement.localPosition;
                    leftArm.transform.localRotation = mv.Arms.leftArmPlacement.localRotation;
                }
                else
                {
                    leftArm.transform.localPosition =
                        (mv is VehicleFramework.VehicleTypes.Drone) ?
                        (mv as VehicleFramework.VehicleTypes.Drone).CameraLocation.localPosition :
                        mv.playerPosition.transform.localPosition;
                    leftArm.transform.localPosition -= mv.transform.right;
                }
                leftArm.name = "LeftArm";
            }
            else if (!isLeft && rightArm == null)
            {
                rightArm = UnityEngine.Object.Instantiate<GameObject>(armPrefab);
                rightArm.transform.parent = mv.transform;
                rightArm.transform.localRotation = Quaternion.identity;
                rightArm.transform.localScale = new Vector3(-1, 1, 1);
                if (mv.Arms.rightArmPlacement != null)
                {
                    rightArm.transform.localPosition = mv.Arms.rightArmPlacement.localPosition;
                    rightArm.transform.localRotation = mv.Arms.rightArmPlacement.localRotation;
                }
                else
                {
                    rightArm.transform.localPosition =
                        (mv is VehicleFramework.VehicleTypes.Drone) ?
                        (mv as VehicleFramework.VehicleTypes.Drone).CameraLocation.localPosition :
                        mv.playerPosition.transform.localPosition;
                    rightArm.transform.localPosition += mv.transform.right;
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
    }
}
