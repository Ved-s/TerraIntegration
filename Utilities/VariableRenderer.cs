using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using TerraIntegration;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TerraIntegration.Utilities
{
    public static class VariableRenderer
    {
        public static Dictionary<string, Texture2D> AssetCache = new();

        public static Dictionary<Type, SpriteSheetPos> TypeSpritesheetOverrides = new();

        public static void Unload()
        {
            AssetCache.Clear();
            TypeSpritesheetOverrides.Clear();
        }

        public static Texture2D GetAsset(string path)
        {
            if (AssetCache.TryGetValue(path, out Texture2D tex))
                return tex;

            if (!ModContent.HasAsset(path))
                return null;

            tex = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
            AssetCache[path] = tex;
            return tex;
        }

        public static void DrawVariableOverlay(SpriteBatch spriteBatch, bool drawVariable, Type returnType, string type, Vector2 pos, Vector2 size, Color color, float rotation, Vector2 origin)
        {
            if (Main.dedServ) return;

            if (drawVariable)
            {
                Asset<Texture2D> variable = TextureAssets.Item[ModContent.ItemType<Items.Variable>()];

                if (!variable.IsLoaded)
                    variable.Wait();

                Vector2 scale = size / variable.Size();

                spriteBatch.Draw(variable.Value, pos, null, color, rotation, origin, scale, SpriteEffects.None, 0);
            }

            Rectangle frame = default;
            Texture2D texture = null;

            Queue<Type> drawTypes = new();
            drawTypes.Enqueue(returnType);

            while (drawTypes.Count > 0)
            {
                Type drawType = drawTypes.Dequeue();

                if (drawType is null) continue;

                if (drawType.IsGenericType)
                {
                    foreach (Type subtype in drawType.GetGenericArguments())
                        drawTypes.Enqueue(subtype);
                    drawType = drawType.GetGenericTypeDefinition();
                }

                SpriteSheetPos spriteSheetPos = default;
                string drawTexture = null;

                bool hasPos = false;

                if (TypeSpritesheetOverrides.TryGetValue(drawType, out SpriteSheetPos ssp))
                {
                    spriteSheetPos = ssp;
                    hasPos = true;
                }

                else if (drawType is not null && VariableValue.ByType.TryGetValue(drawType, out var val))
                {
                    spriteSheetPos = val.SpriteSheetPos;
                    if (spriteSheetPos.SpriteSheet is null)
                        spriteSheetPos.SpriteSheet = val.DefaultSpriteSheet;
                    drawTexture = val.Texture;
                    hasPos = true;
                }

                if (hasPos)
                {
                    SpriteSheet ss = spriteSheetPos.SpriteSheet;
                    if (ss is not null)
                    {
                        texture = GetAsset(ss.Texture);
                        frame = new(
                            spriteSheetPos.X * ss.SpriteSize.X,
                            spriteSheetPos.Y * ss.SpriteSize.Y,
                            ss.SpriteSize.X,
                            ss.SpriteSize.Y);
                    }
                    else if (drawTexture is not null)
                    {
                        texture = GetAsset(drawTexture);
                        if (texture is not null)
                            frame = new(0, 0, texture.Width, texture.Height);
                    }

                    if (texture is not null)
                    {
                        Vector2 scale = size / frame.Size();

                        spriteBatch.Draw(texture, pos, frame, color, rotation, origin, scale, SpriteEffects.None, 0);
                    }
                }
            }

            if (type is not null && Variable.ByTypeName.TryGetValue(type, out var var))
            {
                SpriteSheet ss = var.SpriteSheetPos.SpriteSheet ?? var.DefaultSpriteSheet;
                if (ss is not null)
                {
                    texture = GetAsset(ss.Texture);
                    frame = new(
                        var.SpriteSheetPos.X * ss.SpriteSize.X,
                        var.SpriteSheetPos.Y * ss.SpriteSize.Y,
                        ss.SpriteSize.X,
                        ss.SpriteSize.Y);
                }
                else if (var.Texture is not null)
                {
                    texture = GetAsset(var.Texture);
                    if (texture is not null)
                        frame = new(0, 0, texture.Width, texture.Height);
                }

                if (texture is null && var is ComponentProperty prop
                    && Component.ByTypeName.TryGetValue(prop.ComponentType, out Component com))
                {
                    SpriteSheet sheet = com.DefaultPropertySpriteSheet;

                    if (sheet is not null)
                    {
                        texture = GetAsset(sheet.Texture);
                        frame = new(
                            com.DefaultPropertySpriteSheetPos.X * sheet.SpriteSize.X,
                            com.DefaultPropertySpriteSheetPos.Y * sheet.SpriteSize.Y,
                            sheet.SpriteSize.X,
                            sheet.SpriteSize.Y);
                    }
                    else if (com.DefaultPropertyTexture is not null)
                    {
                        texture = GetAsset(com.DefaultPropertyTexture);
                        if (texture is not null)
                            frame = new(0, 0, texture.Width, texture.Height);
                    }
                }

                if (texture is not null)
                {
                    Vector2 scale = size / frame.Size();

                    spriteBatch.Draw(texture, pos, frame, color, rotation, origin, scale, SpriteEffects.None, 0);
                }
            }
        }
    }
}
