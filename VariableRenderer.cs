using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public static class VariableRenderer
    {
        public static Dictionary<string, Texture2D> AssetCache = new();

        public static void Unload() 
        {
            AssetCache.Clear();
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

            if (returnType is not null && Values.VariableValue.ByType.TryGetValue(returnType, out var val))
            {
                if (val.SpriteSheet is not null)
                {
                    texture = GetAsset(val.SpriteSheet.Texture);
                    frame = new(
                        val.SpritesheetPos.X * val.SpriteSheet.SpriteSize.X,
                        val.SpritesheetPos.Y * val.SpriteSheet.SpriteSize.Y,
                        val.SpriteSheet.SpriteSize.X,
                        val.SpriteSheet.SpriteSize.Y);
                }
                else if (val.Texture is not null) 
                {
                    texture = GetAsset(val.Texture);
                    if (texture is not null)
                        frame = new(0, 0, texture.Width, texture.Height);
                }

                if (texture is not null)
                {
                    Vector2 scale = size / frame.Size();

                    spriteBatch.Draw(texture, pos, frame, color, rotation, origin, scale, SpriteEffects.None, 0);
                }
            }

            if (type is not null && Variables.Variable.ByTypeName.TryGetValue(type, out var var))
            {
                if (var.SpriteSheet is not null)
                {
                    texture = GetAsset(var.SpriteSheet.Texture);
                    frame = new(
                        var.SpritesheetPos.X * var.SpriteSheet.SpriteSize.X,
                        var.SpritesheetPos.Y * var.SpriteSheet.SpriteSize.Y,
                        var.SpriteSheet.SpriteSize.X,
                        var.SpriteSheet.SpriteSize.Y);
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

                    spriteBatch.Draw(texture, pos, frame, color, rotation, origin, scale, SpriteEffects.None, 0);
                }
            }
        }
    }
}
