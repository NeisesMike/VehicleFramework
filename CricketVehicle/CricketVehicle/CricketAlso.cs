using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework;
using System.IO;
using System.Reflection;

using UnityEngine.U2D;
using VehicleFramework.VehicleParts;
using VehicleFramework.VehicleTypes;

namespace CricketVehicle
{
    // This file contains parts of the Cricket above and beyond what is required for Vehicle Framework.
    // In other words, the Cricket is a submersible, but it's also more than that.
    // Specifically, it can also haul containers.
    // Those methods are implemented here.
    public partial class Cricket : Submersible
    {
        public Transform ContainerMountPoint
        {
            get
            {
                return transform.Find("ContainerMountPoint");
            }
        }
        public CricketContainer currentMountedContainer;
        public GameObject fabricator
        {
            get
            {
                return transform.Find("Fabricator").gameObject;
            }
        }

        public override void Awake()
        {
            base.Awake();
            //SetupFabricator();
            fabricator.SetActive(false);
        }
        public void SetupFabricator()
        {
            GhostCrafter crafter;
            crafter = fabricator.EnsureComponent<Fabricator>();

            CraftTree.Type mycraftTree = new CraftTree.Type();
            Nautilus.Handlers.CraftTreeHandler.AddCraftingNode(mycraftTree, MainPatcher.cricketContainerTT, "");

            crafter.craftTree = mycraftTree;
            crafter.handOverText = "Use Cricket Fabricator";
            crafter.ghost.itemSpawnPoint = ContainerMountPoint;
        }
        private bool waitingForButtonRelease = false;
        public override void Update()
        {
            base.Update();
            if (currentMountedContainer != null)
            {
                currentMountedContainer.transform.localPosition = Vector3.zero;
                currentMountedContainer.transform.localRotation = Quaternion.identity;
            }
            if (!IsPlayerDry || !IsPowered())
            {
                return;
            }



            if (Input.GetKey(MainPatcher.config.attach))
            {
                if (currentMountedContainer is null && !waitingForButtonRelease)
                {
                    ShowAttachmentStatus();
                }
            }
            if (Input.GetKeyUp(MainPatcher.config.attach))
            {
                if (currentMountedContainer is null && !waitingForButtonRelease)
                {
                    TryAttachContainer();
                }
                else if(waitingForButtonRelease)
                {
                    waitingForButtonRelease = false;
                }
            }
            else if (Input.GetKeyDown(MainPatcher.config.attach))
            {
                if (currentMountedContainer != null)
                {
                    TryDetachContainer();
                    waitingForButtonRelease = true;
                }
            }



        }
        public bool HasContainerAttached
        {
            get
            {
                foreach (Transform tran in ContainerMountPoint)
                {
                    if (tran.name.Contains("CricketContainer"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public void TryAttachContainer()
        {
            if (HasContainerAttached)
            {
                return;
            }
            CricketContainer container = CricketContainerManager.main.FindNearestCricketContainer(ContainerMountPoint.transform.position);
            if (!ValidateAttachment(container))
            {
                Logger.Log("Container Attachment Request was Invalid");
                return;
            }
            AttachContainer(container);
            string msg = container.storageContainer.storageLabel + " Attached";
            Nautilus.Utility.BasicText message = new Nautilus.Utility.BasicText(500, 0);
            message.ShowMessage(msg, 1);
        }
        public bool ValidateAttachment(CricketContainer container)
        {
            if (container is null)
            {
                return false;
            }
            if (Vector3.Distance(ContainerMountPoint.position, container.transform.position) > container.marginOfError)
            {
                return false;
            }
            GameObject zone = ContainerMountPoint.Find("DangerZone").gameObject;
            Bounds spaceToEnsureIsClear = zone.GetComponent<Collider>().bounds;
            Vector3 center = spaceToEnsureIsClear.center;
            Vector3 halfExtents = spaceToEnsureIsClear.extents;
            Collider[] hitColliders = Physics.OverlapBox(center, halfExtents);
            foreach (Collider col in hitColliders)
            {
                if (col.GetComponentInParent<Cricket>() == this || col.GetComponent<Cricket>() == this || col.isTrigger)
                {
                    continue;
                }
                // if name is not player or cricket, return false
                return false;
            }
            return true;
        }
        public void AttachContainer(CricketContainer container)
        {
            container.transform.SetParent(ContainerMountPoint);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;

            container.transform.Find("FloatCollider").gameObject.SetActive(false);
            container.transform.Find("AttachedCollider").gameObject.SetActive(false);

            currentMountedContainer = container;
        }
        public void TryDetachContainer()
        {
            if (!HasContainerAttached)
            {
                return;
            }
            currentMountedContainer.transform.SetParent(null);
            currentMountedContainer.transform.Find("FloatCollider").gameObject.SetActive(true);
            currentMountedContainer.transform.Find("AttachedCollider").gameObject.SetActive(false);
            string msg = currentMountedContainer.storageContainer.storageLabel + " Detached";
            currentMountedContainer = null;

            Nautilus.Utility.BasicText message = new Nautilus.Utility.BasicText(500, 0);
            message.ShowMessage(msg, 1);

        }

        public void ShowAttachmentStatus()
        {
            CricketContainer container = CricketContainerManager.main.FindNearestCricketContainer(ContainerMountPoint.transform.position);
            if (container is null)
            {
                return;
            }
            float distance = Vector3.Distance(container.transform.position, ContainerMountPoint.position);
            if (distance > 10)
            {
                return;
            }
            string distanceString = distance.ToString();
            if (distance > container.marginOfError)
            {
                string msg = "Container is " + distanceString + " meters away.";
                Nautilus.Utility.BasicText message = new Nautilus.Utility.BasicText(500, 0);
                message.ShowMessage(msg, 2 * Time.deltaTime);
            }
            else if(!ValidateAttachment(container))
            {
                string msg = "Something is blocking the way.";
                Nautilus.Utility.BasicText message = new Nautilus.Utility.BasicText(500, 0);
                message.ShowMessage(msg, 2 * Time.deltaTime);
            }
            else
            {
                string msg = "Ready";
                Nautilus.Utility.BasicText message = new Nautilus.Utility.BasicText(500, 0);
                message.ShowMessage(msg, 2 * Time.deltaTime);
            }

        }

        public override void PlayerExit()
        {
            base.PlayerExit();
            if (!HasContainerAttached)
            {
                return;
            }
            currentMountedContainer.transform.Find("AttachedCollider").gameObject.SetActive(true);
        }
        public override void PlayerEntry()
        {
            base.PlayerEntry();
            if (!HasContainerAttached)
            {
                return;
            }
            currentMountedContainer.transform.Find("AttachedCollider").gameObject.SetActive(false);
        }
    }
}
