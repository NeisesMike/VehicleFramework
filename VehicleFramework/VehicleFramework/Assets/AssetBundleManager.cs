using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Reflection;
using UnityEngine.U2D;

namespace VehicleFramework.Assets
{
    public struct VehicleAssets
    {
        public GameObject model;
        public Atlas.Sprite ping;
        public Atlas.Sprite crafter;
        public GameObject fragment;
        public VehicleAssets(GameObject imodel, Atlas.Sprite iping, Atlas.Sprite icrafter, GameObject ifragment)
        {
            model = imodel;
            ping = iping;
            crafter = icrafter;
            fragment = ifragment;
        }
    }
    public static class AssetBundleManager
    {
        public static Atlas.Sprite GetSprite(System.Object[] arr, string bundleName, string spriteAtlasName, string spriteName)
        {
            foreach (System.Object obj in arr)
            {
                if (obj.ToString().Contains(spriteAtlasName))
                {
                    try
                    {
                        SpriteAtlas thisAtlas = (SpriteAtlas)obj;
                        Sprite ping = thisAtlas.GetSprite(spriteName);
                        return new Atlas.Sprite(ping);
                    }
                    catch(Exception e)
                    {
                        Logger.Error("In AssetBundle " + bundleName + ", failed to get Sprite " + spriteName + " from Sprite Atlas " + spriteAtlasName);
                        Logger.Error(e.Message);
                        return null;
                    }
                }
            }
            Logger.Error("In AssetBundle " + bundleName + ", failed to get Sprite " + spriteName + " from Sprite Atlas " + spriteAtlasName);
            return null;
        }
        public static GameObject GetGameObject(System.Object[] arr, string bundleName, string gameObjectName)
        {
            foreach (System.Object obj in arr)
            {
                if (obj.ToString().Contains(gameObjectName))
                {
                    return (GameObject)obj;
                }
            }
            Logger.Error("In AssetBundle " + bundleName + ", failed to get GameObject " + gameObjectName);
            return null;
        }

        public static VehicleAssets GetVehicleAssetsFromBundle(string bundlePath, string modelName, string spriteAtlasName = "", string pingSpriteName = "", string crafterSpriteName = "", string fragmentName = "")
        {
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(bundlePath);
            if (myLoadedAssetBundle == null)
            {
                Logger.Error("Failed to load AssetBundle: " + bundlePath);
                return new VehicleAssets(null, null, null, null);
            }
            System.Object[] arr = myLoadedAssetBundle.LoadAllAssets();
            GameObject model = GetGameObject(arr, bundlePath, modelName);
            Atlas.Sprite ping = GetSprite(arr, bundlePath, spriteAtlasName, pingSpriteName);
            Atlas.Sprite crafter = GetSprite(arr, bundlePath, spriteAtlasName, crafterSpriteName);
            GameObject fragment = GetGameObject(arr, bundlePath, fragmentName);
            return new VehicleAssets ( model, ping, crafter, fragment );
        }
    }
}
