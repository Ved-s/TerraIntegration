using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.DataStructures;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TerraIntegration
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

        static List<Type> TypeTmp = new();

        public static void DrawVariable(SpriteBatch spriteBatch, Vector2 pos, Vector2 size, Color? color = null, float rotation = 0f, Vector2? origin = null)
        {
            TypeTmp.Clear();
            DrawVariableOverlay(spriteBatch, true, TypeTmp, null, pos, size, color, rotation, origin);
        }

        public static void DrawVariableOverlay(SpriteBatch spriteBatch, bool drawVariable, string variableType, Vector2 pos, Vector2 size, Color? color = null, float rotation = 0f, Vector2? origin = null)
        {
            TypeTmp.Clear();
            DrawVariableOverlay(spriteBatch, drawVariable, TypeTmp, variableType, pos, size, color, rotation, origin);
        }

        public static void DrawVariableOverlay(SpriteBatch spriteBatch, bool drawVariable, Type valueType, Vector2 pos, Vector2 size, Color? color = null, float rotation = 0f, Vector2? origin = null)
        {
            TypeTmp.Clear();
            TypeTmp.Add(valueType);
            DrawVariableOverlay(spriteBatch, drawVariable, TypeTmp, null, pos, size, color, rotation, origin);
        }

        public static void DrawVariableOverlay(SpriteBatch spriteBatch, bool drawVariable, Type valueType, string variableType, Vector2 pos, Vector2 size, Color? color = null, float rotation = 0f, Vector2? origin = null)
        {
            TypeTmp.Clear();
            TypeTmp.Add(valueType);
            DrawVariableOverlay(spriteBatch, drawVariable, TypeTmp, variableType, pos, size, color, rotation, origin);
        }

        public static void DrawVariableOverlay(SpriteBatch spriteBatch, bool drawVariable, ReturnValue? returnValue, string variableType, Vector2 pos, Vector2 size, Color? color = null, float rotation = 0f, Vector2? origin = null)
        {
            if (returnValue is null)
            {
                DrawVariableOverlay(spriteBatch, drawVariable, null as Type[], variableType, pos, size, color, rotation, origin);
                return;
            }

            TypeTmp.Clear();
            TypeTmp.Add(returnValue.Value.ValueType);
            TypeTmp.Add(returnValue.Value.SubTypeA);
            TypeTmp.Add(returnValue.Value.SubTypeB);
            DrawVariableOverlay(spriteBatch, drawVariable, TypeTmp, variableType, pos, size, color, rotation, origin);
        }

        public static void DrawVariableOverlay(SpriteBatch spriteBatch, bool drawVariable, IEnumerable<Type> valueTypes, string variableType, Vector2 pos, Vector2 size, Color? color = null, float rotation = 0f, Vector2? origin = null)
        {
            if (Main.dedServ) return;

            if (drawVariable)
            {
                Asset<Texture2D> variable = TextureAssets.Item[ModContent.ItemType<Items.Variable>()];

                if (!variable.IsLoaded)
                    variable.Wait();

                Vector2 scale = size / variable.Size();

                spriteBatch.Draw(variable.Value, pos, null, color ?? Color.White, rotation, origin ?? Vector2.Zero, scale, SpriteEffects.None, 0);
            }

            Rectangle frame = default;
            Texture2D texture = null;

            Queue<Type> drawTypes = new();
            if (valueTypes is not null)
                foreach (Type type in valueTypes)
                    drawTypes.Enqueue(type);

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

                else if (drawType is not null && Values.VariableValue.ByType.TryGetValue(drawType, out var val))
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

                        spriteBatch.Draw(texture, pos, frame, color ?? Color.White, rotation, origin ?? Vector2.Zero, scale, SpriteEffects.None, 0);
                    }
                }
            }

            if (variableType is not null && Variables.Variable.ByTypeName.TryGetValue(variableType, out var var))
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

                if (texture is null && var is Variables.ComponentProperty prop 
                    && Components.Component.ByTypeName.TryGetValue(prop.ComponentType, out Components.Component com))
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

                    spriteBatch.Draw(texture, pos, frame, color ?? Color.White, rotation, origin ?? Vector2.Zero, scale, SpriteEffects.None, 0);
                }
            }
        }
    }
}
