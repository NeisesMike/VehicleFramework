using UnityEngine;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace VehicleFramework.Assets
{
    public static class SpriteHelper
    {
        internal static Sprite GetSpriteInternal(string name)
        {
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fullPath = Path.Combine(modPath, "Sprites", name);
            return GetSpriteGeneric(fullPath);
        }
        public static Sprite GetSprite(string relativePath)
        {
            string modPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string fullPath = Path.Combine(modPath, relativePath);
            return GetSpriteGeneric(fullPath);
        }
        private static Sprite GetSpriteGeneric(string fullPath)
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
        internal static readonly List<(string, PingType, Sprite)> PingSprites = new List<(string, PingType, Sprite)>();
        public static void RegisterPingSprite(string name, PingType pt, Sprite pingSprite)
        {
            PingSprites.Add((name, pt, pingSprite));
        }
    }
}
