using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerraIntegration.Basic;
using TerraIntegration.DataStructures;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerraIntegration.Components
{
    public class ChatWriter : Component
    {
        public override string TypeName => "chatw";
        public override string TypeDefaultDisplayName => "Chat Writer";
        public override string TypeDefaultDescription => "Announcement box, but with\nvariable message text";

        public override bool CanHaveVariables => true;
        public override Point16 DefaultSize => new(2, 2);

        public ChatWriter()
        {
            VariableInfo = new ComponentVariableInfo[]
            {
                new()
                {
                    VariableName = "Message",
                    VariableSlot = "msg",
                    AcceptVariableReturnTypes = new[] { typeof(Values.String) },
                    VariableDescription = "Message to print in chat"
                },
                new()
                {
                    VariableName = "Print message",
                    VariableSlot = "evt",
                    AcceptVariableTypes = new[] { "eventsub" },
                    VariableDescription = "Print message on this event"
                }
            };
        }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;

            var newTile = TileObjectData.newTile;

            newTile.Height = 2;
            newTile.Width = 2;

            newTile.CoordinateHeights = new[] { 16, 16 };
            newTile.CoordinateWidth = 16;
            newTile.CoordinatePadding = 0;
            newTile.CoordinatePaddingFix = new(2, 0);
            newTile.StyleHorizontal = true;
            newTile.UsesCustomCanPlace = true;

            TileObjectData.newAlternate.CopyFrom(newTile);
            TileObjectData.newAlternate.AnchorLeft = new(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
            TileObjectData.addAlternate(1);

            TileObjectData.newAlternate.CopyFrom(newTile);
            TileObjectData.newAlternate.AnchorRight = new(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
            TileObjectData.addAlternate(2);

            TileObjectData.newAlternate.CopyFrom(newTile);
            TileObjectData.newAlternate.AnchorTop = new(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
            TileObjectData.addAlternate(3);

            TileObjectData.newAlternate.CopyFrom(newTile);
            TileObjectData.newAlternate.AnchorBottom = new(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
            TileObjectData.addAlternate(4);

            newTile.AnchorWall = true;
            TileObjectData.addTile(Type);

            ItemDrop = ModContent.ItemType<Items.ComponentItems.ChatWriter>();
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Tile t = Framing.GetTileSafely(i, j);
            noItem = t.TileFrameX % 34 != 0 || t.TileFrameY != 0;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            base.PostDraw(i, j, spriteBatch);

            Vector2 screen = new Vector2(i, j) * 16 + new Vector2(Main.offScreenRange) - Main.screenPosition;

            Texture2D tex = TextureAssets.Tile[Type].Value;
            ComponentData data = GetData(new(i, j));

            Color c = data.LastErrors.Count == 0 ? Color.Lime : Color.OrangeRed;
            Tile t = Main.tile[i, j];

            Rectangle glowFrame = new(t.TileFrameX, t.TileFrameY + 34, 16, 16);
            spriteBatch.Draw(tex, screen, glowFrame, c);
        }

        public override void OnPlaced(Point16 pos)
        {
            base.OnPlaced(pos);
            World.DefineMultitile(new(pos.X, pos.Y, 2, 2));
        }
        public override void OnLoaded(Point16 pos)
        {
            Tile t = Framing.GetTileSafely(pos);
            if (t.TileFrameY != 0 || t.TileFrameX % 34 != 0)
                return;

            base.OnLoaded(pos);
            World.DefineMultitile(new(pos.X, pos.Y, 2, 2));
        }
        public override void OnEvent(Point16 pos, string variableSlot)
        {
            if (Networking.Client) return;
            if (variableSlot == "evt" && !Main.AnnouncementBoxDisabled)
            {
                ComponentData data = GetData(pos);
                Variable msg = data.GetVariable("msg");
                if (msg is null)
                    return;
                data.LastErrors.Clear();
                VariableValue value = msg.GetValue(data.System, data.LastErrors);
                data.SyncErrors();
                if (value is not Values.String str)
                    return;

                Vector2 center = pos.ToVector2() * 16 + new Vector2(16);

                if (Networking.SinglePlayer)
                {
                    if (CheckRange(Main.LocalPlayer, center))
                        Main.NewTextMultiline(str.Value, false, Color.White, 460);
                }
                else if (Networking.Server)
                {
                    if (Main.AnnouncementBoxRange < 0)
                        NetMessage.SendData(MessageID.SmartTextMessage, -1, -1, NetworkText.FromLiteral(str.Value), 255, 255, 255, 255, 460, 0, 0);
                    else for (int i = 0; i < 255; i++)
                            if (Main.player[i].active && CheckRange(Main.player[i], center))
                                NetMessage.SendData(MessageID.SmartTextMessage, i, -1, NetworkText.FromLiteral(str.Value), 255, 255, 255, 255, 460, 0, 0);
                }
            }
        }

        public bool CheckRange(Player player, Vector2 pos)
        {
            if (Main.AnnouncementBoxRange < 0)
                return true;

            return player.Center.Distance(pos) < Main.AnnouncementBoxRange;
        }
    }
}
