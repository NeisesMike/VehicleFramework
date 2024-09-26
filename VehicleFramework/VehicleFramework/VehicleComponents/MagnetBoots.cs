using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.VehicleComponents
{
    public class MagnetBoots : MonoBehaviour
    {
        private Transform serializerObject;
        public struct MagnetStruct
        {
            public Transform target;
            public Vector3 location;
        }
        private ModVehicle mv;
        private Vector3 attachmentOffset = Vector3.zero;
        private bool IsAttached = false;
        private Transform attachedPlatform = null;
        public Action Attach = null;
        public Action Detach = null;
        public float MagnetDistance = 5f;
        public float AttachDistance = 2f;
        public bool recharges = true;
        public float rechargeRate = 0.5f; // transfer 0.5 energy per second
        public void HandleAttachment(bool isAttached, Transform platform = null)
        {
            IsAttached = isAttached;
            mv.useRigidbody.isKinematic = isAttached;
            mv.collisionModel.SetActive(!isAttached);
            mv.useRigidbody.detectCollisions = !isAttached;
            if (isAttached)
            {
                Attach?.Invoke();
                attachedPlatform = platform;
                if(attachedPlatform.GetComponent<ModVehicle>())
                {
                    attachedPlatform.GetComponent<ModVehicle>().useRigidbody.mass += mv.useRigidbody.mass;
                }
            }
            else
            {
                Detach?.Invoke();
                mv.transform.SetParent(serializerObject);
                if (attachedPlatform?.GetComponent<ModVehicle>())
                {
                    attachedPlatform.GetComponent<ModVehicle>().useRigidbody.mass -= mv.useRigidbody.mass;
                }
                attachedPlatform = null;
            }
        }
        public void Start()
        {
            mv = GetComponent<ModVehicle>();
            serializerObject = transform.parent.GetComponent<StoreInformationIdentifier>().transform;
        }
        public void Update()
        {
            TryMagnets(CheckControls(), CheckPlacement());
            if(mv.IsPlayerControlling() && IsAttached)
            {
                HandleAttachment(false);
            }
            UpdatePosition();
            UpdateRecharge();
        }
        public bool CheckControls()
        {
            return mv.IsPlayerControlling()
                && GameInput.GetKeyDown(MainPatcher.VFConfig.magnetBoots);
        }
        public MagnetStruct CheckPlacement()
        {
            RaycastHit[] allHits = Physics.RaycastAll(transform.position, -transform.up, MagnetDistance);
            var orderedHits = allHits.OrderBy(x => (x.point - transform.position).sqrMagnitude);
            foreach (var hit in orderedHits)
            {
                Base baseTarget = UWE.Utils.GetComponentInHierarchy<Base>(hit.collider.gameObject);
                SubControl cyclopsTarget = UWE.Utils.GetComponentInHierarchy<SubControl>(hit.collider.gameObject);
                ModVehicle mvTarget = UWE.Utils.GetComponentInHierarchy<ModVehicle>(hit.collider.gameObject);
                if (baseTarget != null)
                {
                    return new MagnetStruct
                    {
                        target = baseTarget.transform,
                        location = hit.point
                    };
                }
                else if (cyclopsTarget != null)
                {
                    return new MagnetStruct
                    {
                        target = cyclopsTarget.transform,
                        location = hit.point
                    };
                }
                else if (mvTarget != null)
                {
                    return new MagnetStruct
                    {
                        target = mvTarget.transform,
                        location = hit.point
                    };
                }
            }
            return default;
        }
        public void TryMagnets(bool isKeyed, MagnetStruct magnetData)
        {
            if(isKeyed)
            {
                if (magnetData.target)
                {
                    ParentSelfToTarget(magnetData);
                    Player.main.ExitLockedMode();
                }
                else
                {
                    Logger.Output("No attachable surface nearby.");
                }
            }
        }
        public void ParentSelfToTarget(MagnetStruct magnetData)
        {
            Vector3 hitLocation = magnetData.location;
            Vector3 myLocation = transform.position;
            float currentDistance = Vector3.Distance(hitLocation, myLocation);
            float ratio = AttachDistance / currentDistance;
            mv.transform.position = Vector3.Lerp(myLocation, hitLocation, ratio);
            mv.transform.SetParent(magnetData.target);
            attachmentOffset = transform.localPosition;
            HandleAttachment(true, magnetData.target);
        }
        public void UpdatePosition()
        {
            if(IsAttached)
            {
                transform.localPosition = attachmentOffset;
            }
        }
        public void UpdateRecharge()
        {
            if (IsAttached && attachedPlatform)
            {
                float consumePerFrame = rechargeRate * Time.deltaTime;
                float canConsume = mv.energyInterface.TotalCanConsume(out _);
                if (canConsume > consumePerFrame)
                {
                    Base baseTarget = UWE.Utils.GetComponentInHierarchy<Base>(attachedPlatform.gameObject);
                    SubRoot subRootTarget = attachedPlatform.GetComponent<SubRoot>();
                    ModVehicle mvTarget = attachedPlatform.GetComponent<ModVehicle>();
                    if (baseTarget)
                    {
                        baseTarget.GetComponent<BasePowerRelay>().ConsumeEnergy(consumePerFrame, out float trulyConsumed);
                        mv.energyInterface.AddEnergy(trulyConsumed);
                    }
                    else if (mvTarget)
                    {
                        float trulyConsumed = mvTarget.energyInterface.ConsumeEnergy(consumePerFrame);
                        mv.energyInterface.AddEnergy(trulyConsumed);
                    }
                    else if (subRootTarget)
                    {
                        subRootTarget.powerRelay.ConsumeEnergy(consumePerFrame, out float trulyConsumed);
                        mv.energyInterface.AddEnergy(trulyConsumed);
                    }
                }
            }
        }
    }
}
