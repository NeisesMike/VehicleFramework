using System;
using System.Collections;
using System.Collections.Generic;
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
        private Vehicle MyVehicle => GetComponent<Vehicle>();
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
        public static int colliderPairsPerFrame = 20;

        private Coroutine CollisionHandling = null;
        private IEnumerator IgnoreCollisionWithHost(bool shouldIgnore)
        {
            if (MyVehicle == null || MyVehicle.useRigidbody == null || attachedPlatform == null) yield break;

            int ignoreCounter = 0;
            MyVehicle.useRigidbody.detectCollisions = !shouldIgnore;

            var attachedVehicleColliders = MyVehicle.GetComponentsInChildren<Collider>()
                .Where(x => x.GetComponentInParent<Player>() == null)
                .Where(x => x.enabled && x.gameObject.activeInHierarchy);
            var hostColliders = attachedPlatform.GetComponentsInChildren<Collider>()
                .Where(x => !attachedVehicleColliders.Contains(x))
                .Where(x => x.GetComponentInParent<Player>() == null)
                .Where(x => x.enabled && x.gameObject.activeInHierarchy);

            foreach (Collider left in attachedVehicleColliders)
            {
                foreach (Collider right in hostColliders)
                {
                    // Disallow the attached vehicle from colliding with the host vehicle.
                    // Hate this nested foreach loop, especially because some vehicles have a huge number of colliders.
                    Physics.IgnoreCollision(left, right, shouldIgnore);
                    Logger.Log($"{left.gameObject.name} and {right.gameObject.name}");
                    ignoreCounter++;
                    if (ignoreCounter >= colliderPairsPerFrame)
                    {
                        // only only a few collider pairs per frame, so it never chugs.
                        ignoreCounter = 0;
                        yield return null;
                    }
                }
            }
            // Once all collisions with the host are ignored, this vehicle begins to detect collisions again.
            MyVehicle.useRigidbody.detectCollisions = true;
            if(MyVehicle is ModVehicle mv)
            {
                // Why the hell does bounding box collider become enabled sometime after we detectCollisions:=true ???
                yield return new WaitUntil(() => mv.BoundingBoxCollider.enabled);
                mv.BoundingBoxCollider.enabled = false;
            }
            CollisionHandling = null;
        }
        public void HandleAttachment(bool isAttached, Transform platform = null)
        {
            MyVehicle.teleporting = isAttached; // a little hack to ensure the vehicle gets IsKinematic:=true every frame
            IsAttached = isAttached;
            if (isAttached)
            {
                if (platform == null) return;
                Attach?.Invoke();
                attachedPlatform = platform;
                if(CollisionHandling != null)
                {
                    UWE.CoroutineHost.StopCoroutine(CollisionHandling);
                }
                CollisionHandling = UWE.CoroutineHost.StartCoroutine(IgnoreCollisionWithHost(true));
                if (attachedPlatform.GetComponent<ModVehicle>())
                {
                    attachedPlatform.GetComponent<ModVehicle>().useRigidbody.mass += MyVehicle.useRigidbody.mass;
                }
            }
            else
            {
                Detach?.Invoke();
                if (CollisionHandling != null)
                {
                    UWE.CoroutineHost.StopCoroutine(CollisionHandling);
                }
                CollisionHandling = UWE.CoroutineHost.StartCoroutine(IgnoreCollisionWithHost(false));
                MyVehicle.transform.SetParent(serializerObject);
                if (attachedPlatform?.GetComponent<ModVehicle>())
                {
                    attachedPlatform.GetComponent<ModVehicle>().useRigidbody.mass -= MyVehicle.useRigidbody.mass;
                }
                attachedPlatform = null;
            }
            //MyVehicle.collisionModel.SetActive(!isAttached);
        }
        public void Start()
        {
            if(MyVehicle is ModVehicle mv)
            {
                if (mv is VehicleTypes.Submarine)
                {
                    ErrorMessage.AddWarning("Submarines cannot use MagnetBoots!");
                    DestroyImmediate(this);
                    return;
                }
                if (mv?.BoundingBoxCollider == null)
                {
                    ErrorMessage.AddWarning("This vehicle requires a BoundingBoxCollider to use MagnetBoots!");
                    DestroyImmediate(this);
                    return;
                }
            }
            UWE.CoroutineHost.StartCoroutine(FindStoreInfoIdentifier());
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
            if(IsPlayerControlling() && IsAttached)
            {
                HandleAttachment(false);
            }
            UpdatePosition();
            UpdateRecharge();
        }
        private bool IsPlayerControlling()
        {
            if(MyVehicle is ModVehicle mv)
            {
                return mv.IsPlayerControlling();
            }
            else
            {
                return MyVehicle == Player.main.currentMountedVehicle;
            }
        }
        public bool CheckControls()
        {
            return IsPlayerControlling()
                && GameInput.GetKeyDown(MainPatcher.VFConfig.MagnetBootsButton.Value.MainKey);
        }
        public MagnetStruct CheckPlacement()
        {
            RaycastHit[] allHits;
            if(MyVehicle is ModVehicle mv)
            {
                allHits = Physics.BoxCastAll(mv.BoundingBoxCollider.bounds.center, mv.GetBoundingDimensions() / 2f, -transform.up, transform.rotation, MagnetDistance);
            }
            else
            {
                allHits = Physics.BoxCastAll(MyVehicle.transform.position, Vector3.one, -transform.up, transform.rotation, MagnetDistance);
            }
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
                    MyVehicle.DeselectSlots();
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
                float canConsume = MyVehicle.energyInterface.TotalCanConsume(out _);
                if (canConsume > consumePerFrame)
                {
                    Base baseTarget = UWE.Utils.GetComponentInHierarchy<Base>(attachedPlatform.gameObject);
                    SubRoot subRootTarget = attachedPlatform.GetComponent<SubRoot>();
                    ModVehicle mvTarget = attachedPlatform.GetComponent<ModVehicle>();
                    if (baseTarget)
                    {
                        baseTarget.GetComponent<BasePowerRelay>().ConsumeEnergy(consumePerFrame, out float trulyConsumed);
                        MyVehicle.energyInterface.AddEnergy(trulyConsumed);
                    }
                    else if (mvTarget)
                    {
                        float trulyConsumed = mvTarget.energyInterface.ConsumeEnergy(consumePerFrame);
                        MyVehicle.energyInterface.AddEnergy(trulyConsumed);
                    }
                    else if (subRootTarget)
                    {
                        subRootTarget.powerRelay.ConsumeEnergy(consumePerFrame, out float trulyConsumed);
                        MyVehicle.energyInterface.AddEnergy(trulyConsumed);
                    }
                }
            }
        }

        private static List<MagnetBoots> previouslyAttached = new List<MagnetBoots>();
        internal static void DetachAll()
        {
            previouslyAttached = new List<MagnetBoots>();
            void DetachAndNotify(MagnetBoots boots)
            {
                if (boots.IsAttached)
                {
                    boots.HandleAttachment(false);
                    Logger.PDANote($"{boots.MyVehicle.GetName()} magnet boots have detached!", 3f);
                    previouslyAttached.Add(boots);
                }
            }
            VehicleManager.VehiclesInPlay
                .Where(x => x != null && x.GetComponent<MagnetBoots>() != null)
                .Select(x => x.GetComponent<MagnetBoots>())
                .ForEach(DetachAndNotify);
        }
        internal static void AttachAll()
        {
            previouslyAttached.ForEach(x => x.TryMagnets(true, x.CheckPlacement(), false));
            previouslyAttached.Clear();
        }
    }
}
