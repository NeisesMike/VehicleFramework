using System;
using System.Collections;
using System.Linq;
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
        private ModVehicle mv => GetComponent<ModVehicle>();
        private Vector3 attachmentOffset = Vector3.zero;
        private bool _isAttached = false;
        public bool IsAttached
        {
            get
            {
                return _isAttached;
            }
            private set
            {
                _isAttached = value;
            }
        }
        private Transform attachedPlatform = null;
        public Action Attach = null;
        public Action Detach = null;
        public float MagnetDistance = 1f;
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
            if(mv is VehicleTypes.Submarine)
            {
                ErrorMessage.AddWarning("Submarines cannot use MagnetBoots!");
                DestroyImmediate(this);
                return;
            }
            if(mv?.BoundingBoxCollider == null)
            {
                ErrorMessage.AddWarning("This vehicle requires a BoundingBoxCollider to use MagnetBoots!");
                DestroyImmediate(this);
                return;
            }
            UWE.CoroutineHost.StartCoroutine(FindStoreInfoIdentifier());
            if (!mv.docked)
            {
                TryMagnets(true, CheckPlacement(), false);
            }
        }
        public IEnumerator FindStoreInfoIdentifier()
        {
            while (serializerObject == null)
            {
                serializerObject = transform.parent?.GetComponent<StoreInformationIdentifier>()?.transform;
                yield return null;
            }
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
                && GameInput.GetKeyDown(MainPatcher.VFConfig.MagnetBootsButton.Value.MainKey);
        }
        public MagnetStruct CheckPlacement()
        {
            RaycastHit[] allHits = Physics.BoxCastAll(mv.BoundingBoxCollider.bounds.center, mv.GetBoundingDimensions() / 2f, -transform.up, transform.rotation, MagnetDistance);
            var orderedHits = allHits
                .Where(x=>!x.collider.transform.IsGameObjectAncestor(gameObject))
                .OrderBy(x => (x.point - transform.position).sqrMagnitude);
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
        public void TryMagnets(bool isKeyed, MagnetStruct magnetData, bool verbose=true)
        {
            if(isKeyed)
            {
                if (magnetData.target)
                {
                    ParentSelfToTarget(magnetData);
                    mv.DeselectSlots();
                }
                else if(verbose)
                {
                    Logger.PDANote("No attachable surface nearby.");
                }
            }
        }
        public void ParentSelfToTarget(MagnetStruct magnetData)
        {
            transform.SetParent(magnetData.target);
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
