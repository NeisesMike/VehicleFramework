using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.VehicleBuilding
{
    internal class UpgradeSlotListener : MonoBehaviour
    {
        private readonly List<GameObject> trackedSlots = new();
        private static UpgradeSlotListener privilegedListener = null!;

        private void Awake()
        {
            trackedSlots.Add(gameObject);
        }
        private void OnEnable()
        {
            if(this != privilegedListener)
            {
                Bump();
            }
        }
        private void OnDisable()
        {
            if (this != privilegedListener)
            {
                Bump();
            }
        }
        internal void Privilege()
        {
            privilegedListener = this;
        }

        private static void Bump()
        {
            int activeSlots =
             privilegedListener.transform.parent.GetComponentsInChildren<UpgradeSlotListener>(true)
                .Where(x => x != privilegedListener)
                .Where(x => x.gameObject.activeInHierarchy)
                .Count();

            if (activeSlots == 0)
            {
                // reset it to normal
                FunnyEnable(false);
            }
            else if (activeSlots > 0)
            {
                if (!privilegedListener.gameObject.activeInHierarchy)
                {
                    // show only the background image
                    FunnyEnable(true);
                }
            }
        }
        private static void FunnyEnable(bool enabled)
        {
            privilegedListener.GetComponent<uGUI_EquipmentSlot>().enabled = !enabled;
            privilegedListener.transform.Find("Background").gameObject.SetActive(!enabled);
            privilegedListener.gameObject.SetActive(enabled);
        }
    }
}
