using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.MiscComponents
{
    public class UpgradeProxy : MonoBehaviour
    {
        public List<Transform> proxies = new();
        public List<VehicleUpgradeConsoleInput.Slot> slots = new();

        public void Awake()
        {
            Admin.SessionManager.StartCoroutine(GetSeamothBitsASAP());
        }

        public IEnumerator GetSeamothBitsASAP()
        {
            yield return Admin.SessionManager.StartCoroutine(SeamothHelper.EnsureSeamoth());
            GameObject module = SeamothHelper.Seamoth.transform.Find("Model/Submersible_SeaMoth/Submersible_seaMoth_geo/engine_console_key_02_geo").gameObject;
            for (int i = 0; i < proxies.Count; i++)
            {
                foreach (Transform tran in proxies[i])
                {
                    Destroy(tran.gameObject);
                }
                GameObject model = Instantiate(module, proxies[i]);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                VehicleUpgradeConsoleInput.Slot slot;
                slot.id = ModuleBuilder.ModVehicleModulePrefix + i;
                slot.model = model;
                slots.Add(slot);
            }
            GetComponentInChildren<VehicleUpgradeConsoleInput>().slots = slots.ToArray();
        }

    }
}
