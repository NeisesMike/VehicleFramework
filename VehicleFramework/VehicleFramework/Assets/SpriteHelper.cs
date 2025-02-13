using UnityEngine;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace VehicleFramework.Assets
{
    public static class SpriteHelper
    {
        internal static Atlas.Sprite GetSpriteInternal(string name)
        {
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fullPath = Path.Combine(modPath, "Sprites", name);
            return GetSpriteGeneric(fullPath);
        }
        public static Atlas.Sprite GetSprite(string relativePath)
        {
            string modPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string fullPath = Path.Combine(modPath, relativePath);
            return GetSpriteGeneric(fullPath);
        }
        public static Sprite GetSpriteRaw(string relativePath)
        {
            string modPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string fullPath = Path.Combine(modPath, relativePath);
            return GetSpriteGenericRaw(fullPath);
        }
        private static Atlas.Sprite GetSpriteGeneric(string fullPath)
        {
            Sprite innerSprite = GetSpriteGenericRaw(fullPath);
            if (innerSprite != null)
            {
                return new Atlas.Sprite(innerSprite);
            }
            else return null;
        }
        private static Sprite GetSpriteGenericRaw(string fullPath)
        {
            try
            {
                byte[] spriteBytes = System.IO.File.ReadAllBytes(fullPath);
                Texture2D SpriteTexture = new Texture2D(128, 128);
                SpriteTexture.LoadImage(spriteBytes);
                return Sprite.Create(SpriteTexture, new Rect(0.0f, 0.0f, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            catch
            {
                Logger.Warn($"Could not find file {fullPath}. Returning null Sprite.");
                return null;
            }
        }
        public static Sprite CreateSpriteFromAtlasSprite(Atlas.Sprite sprite)
        {
            Texture2D texture = sprite.texture;
            return Sprite.Create(texture, new Rect(0f, 0f, (float)texture.width, (float)texture.height), Vector2.one * 0.5f);
        }

        internal static readonly List<(string, PingType, Atlas.Sprite)> PingSprites = new List<(string, PingType, Atlas.Sprite)>();
        public static void RegisterPingSprite(string name, PingType pt, Atlas.Sprite pingSprite)
        {
            PingSprites.Add((name, pt, pingSprite));
        }
    }
}
