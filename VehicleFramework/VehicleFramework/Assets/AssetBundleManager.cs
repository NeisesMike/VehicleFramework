using System;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Reflection;
using UnityEngine.U2D;

namespace VehicleFramework.Assets
{
    public struct VehicleAssets
    {
        public GameObject? model;
        public Sprite? ping;
        public Sprite? crafter;
        public GameObject? fragment;
        public Sprite? unlock;
        public AssetBundleInterface? abi;
        public VehicleAssets(GameObject imodel, Sprite iping, Sprite icrafter, GameObject ifragment, Sprite iunlock)
        {
            model = imodel;
            ping = iping;
            crafter = icrafter;
            fragment = ifragment;
            unlock = iunlock;
            abi = null;
        }
        public readonly void Close()
        {
            abi?.CloseBundle();
        }
    }
    public class AssetBundleInterface
    {
        internal string bundleName;
        internal AssetBundle bundle = null!;
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
            if(bundle == null)
            {
                throw Admin.SessionManager.Fatal($"AssetBundleInterface failed to load AssetBundle with the path: {bundlePath}. The bundle == null.");
            }
        }
        internal SpriteAtlas? GetSpriteAtlas(string spriteAtlasName)
        {
            try
            {
                return bundle.LoadAsset<SpriteAtlas>(spriteAtlasName);
            }
            catch
            {
                try
                {
                    return bundle.LoadAsset<SpriteAtlas>($"{spriteAtlasName}.spriteatlas");
                }
                catch (Exception e)
                {
                    Logger.LogException($"AssetBundle {bundleName} failed to get Sprite Atlas: {spriteAtlasName}.", e);
                    return null;
                }
            }
        }
        internal Sprite? GetSprite(string spriteAtlasName, string spriteName)
        {
            SpriteAtlas? thisAtlas = GetSpriteAtlas(spriteAtlasName);
            try
            {
                return thisAtlas?.GetSprite(spriteName);
            }
            catch (Exception e)
            {
                Logger.LogException($"In AssetBundle {bundleName}, failed to get Sprite {spriteName} from Sprite Atlas {spriteAtlasName}.", e);
                return null;
            }
        }
        internal GameObject? GetGameObject(string gameObjectName)
        {
            try
            {
                return bundle.LoadAsset<GameObject>(gameObjectName);
            }
            catch
            {
                try
                {
                    return bundle.LoadAsset<GameObject>($"{gameObjectName}.prefab");
                }
                catch (Exception e)
                {
                    Logger.LogException($"AssetBundle {bundleName} failed to get Sprite Atlas: {gameObjectName}.", e);
                    return null;
                }
            }
        }
        internal AudioClip? GetAudioClip(string prefabName, string clipName)
        {
            return GetGameObject(prefabName)?
                .GetComponents<AudioSource>()
                .Select(x => x.clip)
                .Where(x => x.name == clipName)
                .FirstOrDefault();
        }
        public static VehicleAssets GetVehicleAssetsFromBundle(string bundleName, string modelName = "", string spriteAtlasName = "", string pingSpriteName = "", string crafterSpriteName = "", string fragmentName = "", string unlockName = "")
        {
            string directoryPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string bundlePath = Path.Combine(directoryPath, bundleName);
            AssetBundleInterface abi = new(bundlePath);
            VehicleAssets result = new()
            {
                abi = abi
            };
            if (modelName != "")
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
                    result.unlock = abi.GetSprite(spriteAtlasName, unlockName);
                }
            }
            if (fragmentName != "")
            {
                result.fragment = abi.GetGameObject(fragmentName);
            }
            return result;
        }
        public static GameObject? LoadAdditionalGameObject(AssetBundleInterface abi, string modelName)
        {
            return abi.GetGameObject(modelName);
        }
        public static Sprite? LoadAdditionalSprite(AssetBundleInterface abi, string SpriteAtlasName, string SpriteName)
        {
            return abi.GetSprite(SpriteAtlasName, SpriteName);
        }
        public static AudioClip? LoadAudioClip(AssetBundleInterface abi, string prefabName, string clipName)
        {
            return abi.GetAudioClip(prefabName, clipName);
        }
        public void CloseBundle()
        {
            bundle.Unload(false);
        }
    }
}
