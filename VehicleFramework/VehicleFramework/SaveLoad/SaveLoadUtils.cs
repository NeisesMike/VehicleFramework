using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.SaveLoad
{
    internal static class SaveLoadUtils
    {
        internal static bool IsNameUniqueAmongSiblings(Transform tran)
        {
            foreach (Transform tr in tran.parent)
            {
                if (tr == tran)
                {
                    continue;
                }
                if (tr.name == tran.name)
                {
                    return false;
                }
            }
            return true;
        }
        internal static void EnsureUniqueNameAmongSiblings(Transform tran)
        {
            if (IsNameUniqueAmongSiblings(tran))
            {
                return;
            }
            else
            {
                string targetName = tran.name += "0";
                Logger.Warn($"SaveLoadUtils Warning: The name of this transform is being changed from {tran.name} to {targetName} in order to ensure its name is unique among its siblings. This is important for saving and loading its data correctly.");
                tran.name = targetName;
            }
        }
        internal static string GetTransformPath(Transform root, Transform target)
        {
            if (target == root)
            {
                return "root";
            }
            string result = target.name;
            Transform index = target.parent;
            while (index != root)
            {
                result = $"{index.name}-{result}";
                index = index.parent;
            }
            return result;
        }
        internal static string GetSaveFileName(Transform root, Transform target, string fileSuffix)
        {
            return $"{GetTransformPath(root, target)}-{fileSuffix}";
        }
    }
}
