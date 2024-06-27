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
        public Sprite unlock;
        public VehicleAssets(GameObject imodel, Atlas.Sprite iping, Atlas.Sprite icrafter, GameObject ifragment, Sprite iunlock)
        {
            model = imodel;
            ping = iping;
            crafter = icrafter;
            fragment = ifragment;
            unlock = iunlock;
        }
    }
    public class AssetBundleInterface
    {
        private string bundleName;
        private AssetBundle bundle;
        private System.Object[] objectArray;
        public AssetBundleInterface(string bundlePath)
        {
            bundleName = bundlePath;
            try
            {
                bundle = AssetBundle.LoadFromFile(bundlePath);
            }
            catch (Exception e)
            {
                Logger.Error("AssetBundleInterface failed to load AssetBundle with the path: " + bundlePath);
                Logger.Error("Make sure the name is correct.");
                Logger.Error(e.Message);
                return;
            }
            objectArray = bundle.LoadAllAssets();
        }
        public SpriteAtlas GetSpriteAtlas(string spriteAtlasName)
        {
            foreach (System.Object obj in objectArray)
            {
                if (obj.ToString().Contains(spriteAtlasName))
                {
                    try
                    {
                        return (SpriteAtlas)obj;
                    }
                    catch (Exception e)
                    {
                        Logger.Error("In AssetBundle " + bundleName + ", failed to get Sprite Atlas " + spriteAtlasName);
                        Logger.Error(e.Message);
                        return null;
                    }
                }
            }
            Logger.Error("In AssetBundle " + bundleName + ", failed to get Sprite Atlas " + spriteAtlasName);
            return null;
        }
        public Atlas.Sprite GetSprite(string spriteAtlasName, string spriteName)
        {
            SpriteAtlas thisAtlas = GetSpriteAtlas(spriteAtlasName);
            try
            {
                Sprite ping = thisAtlas.GetSprite(spriteName);
                return new Atlas.Sprite(ping);
            }
            catch (Exception e)
            {
                Logger.Error("In AssetBundle " + bundleName + ", failed to get Sprite " + spriteName + " from Sprite Atlas " + spriteAtlasName);
                Logger.Error(e.Message);
                return null;
            }
        }
        public Sprite GetRawSprite(string spriteAtlasName, string spriteName)
        {
            SpriteAtlas thisAtlas = GetSpriteAtlas(spriteAtlasName);
            try
            {
                return thisAtlas.GetSprite(spriteName);
            }
            catch (Exception e)
            {
                Logger.Error("In AssetBundle " + bundleName + ", failed to get Sprite " + spriteName + " from Sprite Atlas " + spriteAtlasName);
                Logger.Error(e.Message);
                return null;
            }
        }
        public GameObject GetGameObject(string gameObjectName)
        {
            foreach (System.Object obj in objectArray)
            {
                if (obj.ToString().Contains(gameObjectName))
                {
                    return (GameObject)obj;
                }
            }
            Logger.Error("In AssetBundle " + bundleName + ", failed to get GameObject " + gameObjectName);
            return null;
        }
        public static VehicleAssets GetVehicleAssetsFromBundle(string bundlePath, string modelName, string spriteAtlasName = "", string pingSpriteName = "", string crafterSpriteName = "", string fragmentName = "", string unlockName = "")
        {
            AssetBundleInterface abi = new AssetBundleInterface(bundlePath);
            GameObject model = abi.GetGameObject(modelName);
            Atlas.Sprite ping = abi.GetSprite(spriteAtlasName, pingSpriteName);
            Atlas.Sprite crafter = abi.GetSprite(spriteAtlasName, crafterSpriteName);
            GameObject fragment = abi.GetGameObject(fragmentName);
            Sprite unlock = abi.GetRawSprite(spriteAtlasName, unlockName);
            return new VehicleAssets(model, ping, crafter, fragment, unlock);
        }
    }
}
