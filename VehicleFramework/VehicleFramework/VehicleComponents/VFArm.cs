using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Admin;
using VehicleFramework.UpgradeTypes;

namespace VehicleFramework.VehicleComponents
{
    // This class is for Exosuit (Prawn) only.
    // This component is added to each arm GameObject.
    // Its methods are mostly passed through to ModVehicleArm
    public class VFArm : MonoBehaviour, IExosuitArm
    {
        internal static Dictionary<UpgradeTechTypes, GameObject> armPrefabs = new Dictionary<UpgradeTechTypes, GameObject>();
        internal static Dictionary<UpgradeTechTypes, ModVehicleArm> armLogics = new Dictionary<UpgradeTechTypes, ModVehicleArm>();
        private ModVehicleArm armDeclaration;
        public bool IsLeft { get; private set; }
        private Vehicle Vehicle => GetComponentInParent<Exosuit>();
        private int SlotID => IsLeft ? Vehicle.GetSlotIndex("ExosuitArmLeft") : Vehicle.GetSlotIndex("ExosuitArmRight");
        private TechType TechType
        {
            get
            {
                Vehicle.GetQuickSlotType(SlotID, out TechType techType);
                return techType;
            }
        }

        public void SetArmDecl(ModVehicleArm decl)
        {
            armDeclaration = decl;
        }

        GameObject IExosuitArm.GetGameObject()
        {
            return gameObject;
        }

        GameObject IExosuitArm.GetInteractableRoot(GameObject target) // target is whatever we're aiming at. What we return becomes Exosuit.activeTarget.
        {
            return armDeclaration.GetInteractableRoot(gameObject, Vehicle, target);
        }
            
        void IExosuitArm.SetSide(Exosuit.Arm arm)
        {
            if (arm == Exosuit.Arm.Right)
            {
                IsLeft = false;
                gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                IsLeft = true;
                gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }

        bool IExosuitArm.OnUseDown(out float cooldownDuration) // return true when the "use" happened
        {
            ArmActionParams armActionParams = new ArmActionParams
            {
                vehicle = Vehicle,
                slotID = SlotID,
                techType = TechType,
                arm = gameObject
            };
            return armDeclaration.OnArmDown(armActionParams, out cooldownDuration);
        }

        bool IExosuitArm.OnUseHeld(out float cooldownDuration) // return true when the "hold" happened
        {
            ArmActionParams armActionParams = new ArmActionParams
            {
                vehicle = Vehicle,
                slotID = SlotID,
                techType = TechType,
                arm = gameObject
            };
            return armDeclaration.OnArmHeld(armActionParams, out cooldownDuration);
        }

        bool IExosuitArm.OnUseUp(out float cooldownDuration) // return true when the "stop using" happened
        {
            ArmActionParams armActionParams = new ArmActionParams
            {
                vehicle = Vehicle,
                slotID = SlotID,
                techType = TechType,
                arm = gameObject
            };
            return armDeclaration.OnArmUp(armActionParams, out cooldownDuration);
        }

        bool IExosuitArm.OnAltDown() // return true when the "alt use" happened
        {
            ArmActionParams armActionParams = new ArmActionParams
            {
                vehicle = Vehicle,
                slotID = SlotID,
                techType = TechType,
                arm = gameObject
            };
            return armDeclaration.OnArmAltUse(armActionParams);
        }

        void IExosuitArm.Update(ref Quaternion aimDirection) // happens every frame the player is inside and has control over the exosuit
        {
            armDeclaration.Update(gameObject, Vehicle, ref aimDirection);
        }

        void IExosuitArm.ResetArm() // called OnPilotExit
        {
            armDeclaration.OnPilotExit(gameObject, Vehicle);
        }

        public static GameObject GetArmPrefab(TechType techType)
        {
            foreach (KeyValuePair<UpgradeTechTypes, GameObject> arms in armPrefabs)
            {
                if (arms.Key.forModVehicle == techType)
                {
                    return arms.Value;
                }
                else if (arms.Key.forSeamoth == techType)
                {
                    return arms.Value;
                }
                else if (arms.Key.forExosuit == techType)
                {
                    return arms.Value;
                }
                else if (arms.Key.forCyclops == techType)
                {
                    return arms.Value;
                }
            }
            return null;
        }
        public static ModVehicleArm GetModVehicleArm(TechType techType)
        {
            foreach (KeyValuePair<UpgradeTechTypes, ModVehicleArm> arms in armLogics)
            {
                if (arms.Key.forModVehicle == techType)
                {
                    return arms.Value;
                }
                else if (arms.Key.forSeamoth == techType)
                {
                    return arms.Value;
                }
                else if (arms.Key.forExosuit == techType)
                {
                    return arms.Value;
                }
                else if (arms.Key.forCyclops == techType)
                {
                    return arms.Value;
                }
            }
            return null;
        }
    }
}
