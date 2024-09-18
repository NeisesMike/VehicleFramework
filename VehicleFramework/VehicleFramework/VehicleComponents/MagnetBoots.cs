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
        public struct MagnetStruct
        {
            public Transform target;
            public Vector3 location;
        }
        private ModVehicle mv;
        private Vector3 attachmentOffset = Vector3.zero;
        private bool IsAttached = false;
        public Action Attach = null;
        public Action Detach = null;
        public float MagnetDistance = 5f;
        public float AttachDistance = 2f;
        public void HandleAttachment(bool isAttached)
        {
            IsAttached = isAttached;
            mv.useRigidbody.isKinematic = isAttached;
            mv.collisionModel.SetActive(!isAttached);
            mv.useRigidbody.detectCollisions = !isAttached;
            if (isAttached)
            {
                Attach?.Invoke();
            }
            else
            {
                Detach?.Invoke();
                mv.transform.SetParent(null);
            }
        }
        public void Start()
        {
            mv = GetComponent<ModVehicle>();
        }
        public void Update()
        {
            TryMagnets(CheckControls(), CheckPlacement());
            if(mv.IsPlayerControlling())
            {
                HandleAttachment(false);
            }
            UpdatePosition();
        }
        public bool CheckControls()
        {
            return GameInput.GetKeyDown(MainPatcher.VFConfig.magnetBoots);
        }
        public MagnetStruct CheckPlacement()
        {
            RaycastHit[] allHits = Physics.RaycastAll(transform.position, -transform.up, MagnetDistance);
            var meaningfulHits = allHits.Where(hit => UWE.Utils.GetComponentInHierarchy<Base>(hit.collider.gameObject) || UWE.Utils.GetComponentInHierarchy<SubRoot>(hit.collider.gameObject));
            if (meaningfulHits.Count() > 0)
            {
                Transform finalTarget = UWE.Utils.GetComponentInHierarchy<Base>(meaningfulHits.First().collider.gameObject)?.transform
                ?? UWE.Utils.GetComponentInHierarchy<SubRoot>(meaningfulHits.First().collider.gameObject).transform;
                return new MagnetStruct
                {
                    target = finalTarget,
                    location = meaningfulHits.First().point
                };
            }
            else
            {
                return default;
            }
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
            HandleAttachment(true);
        }
        public void UpdatePosition()
        {
            if(IsAttached)
            {
                transform.localPosition = attachmentOffset;
            }
        }
    }
}
