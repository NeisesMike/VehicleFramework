using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class UpgradeProxy : MonoBehaviour
    {
        public List<Transform> proxies = new List<Transform>();
        public List<VehicleUpgradeConsoleInput.Slot> slots = null;

        public void Awake()
        {
            StartCoroutine(GetSeamothBitsASAP());
        }

        public IEnumerator GetSeamothBitsASAP()
        {
            if (SeamothHelper.request.Get() is null)
            {
                yield return UWE.CoroutineHost.StartCoroutine(SeamothHelper.EnsureSeamoth());
            }

            slots = new List<VehicleUpgradeConsoleInput.Slot>();
            GameObject module = SeamothHelper.Seamoth.transform.Find("Model/Submersible_SeaMoth/Submersible_seaMoth_geo/engine_console_key_02_geo").gameObject;
            for (int i = 0; i < proxies.Count; i++)
            {
                foreach (Transform tran in proxies[i])
                {
                    GameObject.Destroy(tran.gameObject);
                }

                GameObject model = GameObject.Instantiate(module, proxies[i]);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;

                VehicleUpgradeConsoleInput.Slot slot;
                slot.id = "ModVehicleModule" + i;
                slot.model = model;
                slots.Add(slot);
            }
            GetComponentInParent<ModVehicle>().GetComponentInChildren<VehicleUpgradeConsoleInput>().slots = slots.ToArray();
        }

    }
}
