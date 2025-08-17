using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.StorageComponents
{
    public class BatteryProxy : MonoBehaviour
    {
        public Transform proxy = null;
        public EnergyMixin mixin = null;

        public void Awake()
        {
            Admin.Utils.StartCoroutine(GetSeamothBitsASAP());
        }
        public IEnumerator GetSeamothBitsASAP()
        {
            if (proxy is null || mixin is null)
            {
                // reload reload condition ?
                // no...
                yield break;
            }
            yield return Admin.Utils.StartCoroutine(SeamothHelper.EnsureSeamoth());
            var seamothEnergyMixin = SeamothHelper.Seamoth.GetComponent<EnergyMixin>();

            mixin.batteryModels = new EnergyMixin.BatteryModels[seamothEnergyMixin.batteryModels.Length];
            for (int i = 0; i < seamothEnergyMixin.batteryModels.Length; i++)
            {
                var but = seamothEnergyMixin.batteryModels[i];
                EnergyMixin.BatteryModels mod = new EnergyMixin.BatteryModels
                {
                    model = GameObject.Instantiate(but.model),
                    techType = but.techType
                };
                mixin.batteryModels[i] = mod;
            }

            foreach (Transform tran in proxy)
            {
                GameObject.Destroy(tran.gameObject);
            }
            for (int i = 0; i < mixin.batteryModels.Length; i++)
            {
                mixin.batteryModels[i].model.SetActive(true);
                var model = GameObject.Instantiate(mixin.batteryModels[i].model, proxy);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                if (model.name.ToLower().Contains("ion"))
                {
                    model.transform.localScale *= 0.01f;
                }
                mixin.batteryModels[i].model = model;
            }
        }
    }
}
