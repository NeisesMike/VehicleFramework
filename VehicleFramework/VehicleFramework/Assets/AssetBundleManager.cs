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
        public AssetBundleInterface abi;
        public VehicleAssets(GameObject imodel, Atlas.Sprite iping, Atlas.Sprite icrafter, GameObject ifragment, Sprite iunlock)
        {
            model = imodel;
            ping = iping;
            crafter = icrafter;
            fragment = ifragment;
            unlock = iunlock;
            abi = null;
        }
        public void Close()
        {
            abi.CloseBundle();
        }
    }
    public class AssetBundleInterface
    {
        internal string bundleName;
        internal AssetBundle bundle;
        internal System.Object[] objectArray;
        internal AssetBundleInterface(string bundlePath)
        {
            bundleName = bundlePath;
            try
            {
                bundle = AssetBundle.LoadFromFile(bundlePath);
            }
            catch (Exception e)
            {
                Logger.LogException($"AssetBundleInterface failed to load AssetBundle with the path: {bundlePath}. Make sure the name is correct.", e);
                return;
            }
            objectArray = bundle.LoadAllAssets();
        }
        internal SpriteAtlas GetSpriteAtlas(string spriteAtlasName)
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
                        Logger.LogException($"AssetBundle {bundleName} failed to get Sprite Atlas: {spriteAtlasName}.", e);
                        return null;
                    }
                }
            }
            Logger.Error($"AssetBundle {bundleName} failed to get Sprite Atlas: {spriteAtlasName}.");
            return null;
        }
        internal Atlas.Sprite GetSprite(string spriteAtlasName, string spriteName)
        {
            SpriteAtlas thisAtlas = GetSpriteAtlas(spriteAtlasName);
            try
            {
                Sprite ping = thisAtlas.GetSprite(spriteName);
                return new Atlas.Sprite(ping);
            }
            catch (Exception e)
            {
                Logger.LogException($"In AssetBundle {bundleName}, failed to get Sprite {spriteName} from Sprite Atlas {spriteAtlasName}.", e);
                return null;
            }
        }
        internal Sprite GetRawSprite(string spriteAtlasName, string spriteName)
        {
            SpriteAtlas thisAtlas = GetSpriteAtlas(spriteAtlasName);
            try
            {
                return thisAtlas.GetSprite(spriteName);
            }
            catch (Exception e)
            {
                Logger.LogException($"In AssetBundle {bundleName}, failed to get Sprite {spriteName} from Sprite Atlas {spriteAtlasName}.", e);
                return null;
            }
        }
        internal GameObject GetGameObject(string gameObjectName)
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
        internal AudioClip GetAudioClip(string prefabName, string clipName)
        {
            foreach (System.Object obj in objectArray)
            {
                if (obj.ToString().Contains(prefabName))
                {
                    GameObject thisGO = (GameObject)obj;
                    var sources = thisGO.GetComponents<AudioSource>();
                    foreach(AudioSource source in sources)
                    {
                        if(source.clip.name == clipName)
                        {
                            return source.clip;
                        }
                    }
                }
            }
            Logger.Error("In AssetBundle " + bundleName + ", failed to get AudioClip " + clipName + " from prefab " + prefabName);
            return null;
        }
        public static VehicleAssets GetVehicleAssetsFromBundle(string bundleName, string modelName = "", string spriteAtlasName = "", string pingSpriteName = "", string crafterSpriteName = "", string fragmentName = "", string unlockName = "")
        {
            string directoryPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string bundlePath = Path.Combine(directoryPath, bundleName);
            AssetBundleInterface abi = new AssetBundleInterface(bundlePath);
            VehicleAssets result = new VehicleAssets();
            result.abi = abi;
            if(modelName != "")
            {
                result.model = abi.GetGameObject(modelName);
            }
            if (spriteAtlasName != "")
            {
                if (pingSpriteName != "")
                {
                    result.ping = abi.GetSprite(spriteAtlasName, pingSpriteName);
                }
                if (crafterSpriteName != "")
                {
                    result.crafter = abi.GetSprite(spriteAtlasName, crafterSpriteName);
                }
                if (unlockName != "")
                {
                    result.unlock = abi.GetRawSprite(spriteAtlasName, unlockName);
                }
            }
            if (fragmentName != "")
            {
                result.fragment = abi.GetGameObject(fragmentName);
            }
            return result;
        }
        public static GameObject LoadAdditionalGameObject(AssetBundleInterface abi, string modelName)
        {
            return abi.GetGameObject(modelName);
        }
        public static Atlas.Sprite LoadAdditionalSprite(AssetBundleInterface abi, string SpriteAtlasName, string SpriteName)
        {
            return abi.GetSprite(SpriteAtlasName, SpriteName);
        }
        public static Sprite LoadAdditionalRawSprite(AssetBundleInterface abi, string SpriteAtlasName, string SpriteName)
        {
            return abi.GetRawSprite(SpriteAtlasName, SpriteName);
        }
        public static AudioClip LoadAudioClip(AssetBundleInterface abi, string prefabName, string clipName)
        {
            return abi.GetAudioClip(prefabName, clipName);
        }
        public void CloseBundle()
        {
            bundle.Unload(false);
        }
    }
}
