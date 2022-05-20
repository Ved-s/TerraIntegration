using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Components;
using TerraIntegration.Variables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraIntegration
{
    public static class Networking
    {
        public static TerraIntegration Mod => ModContent.GetInstance<TerraIntegration>();

        private static string[] MessageTypeNames = Enum.GetNames(typeof(NetMessageType));

        public static void HandlePacket(BinaryReader reader, int whoAmI) 
        {
            ushort type = reader.ReadUInt16();
            if (type >= MessageTypeNames.Length)
            {
                LogWarn("Unrecognized netmessage type: {0}", type);
                return;
            }
            NetMessageType msgType = (NetMessageType)type;

            bool broadcast = false;

            long startPos = reader.BaseStream.Position;

            switch (msgType)
            {
                case NetMessageType.ComponentDataSync:
                    ReceiveComponentDataSync(reader);
                    break;

                case NetMessageType.ComponentDataRequest:
                    ReceiveComponentDataRequest(reader, whoAmI);
                    break;

                case NetMessageType.ComponentPacket:
                    ReceiveComponentPacket(reader, whoAmI, ref broadcast);
                    break;

                case NetMessageType.VariablePacket:
                    ReceiveVariablePacket(reader, whoAmI, ref broadcast);
                    break;

                case NetMessageType.ComponentVariable:
                    ReceiveComponentVariable(reader);
                    broadcast = true;
                    return;

                case NetMessageType.ComponentFrequency:
                    ReceiveComponentFrequency(reader);
                    broadcast = true;
                    break;

                default:
                    LogWarn("Unhandled netmessage: {0}", msgType);
                    break;
            }

            if (broadcast && Main.netMode == NetmodeID.Server)
            {
                long size = reader.BaseStream.Position - startPos;
                reader.BaseStream.Seek(startPos, SeekOrigin.Begin);

                byte[] data = reader.ReadBytes((int)size);

                ModPacket packet = CreatePacket(msgType);
                packet.Write(data);
                packet.Send(-1, whoAmI);
            }
            
        }

        public static ModPacket CreatePacket(NetMessageType type) 
        {
            ModPacket pack = Mod.GetPacket();
            pack.Write((ushort)type);
            return pack;
        }

        public static void SendComponentDataSync(List<Point16> positions = null, int clientId = -1) 
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ComponentWorld world = ModContent.GetInstance<ComponentWorld>();

            List<(Point16, ComponentData)> send = new();

            IEnumerable<KeyValuePair<Point16, ComponentData>> ienum;

            if (positions is null)
                ienum = world.ComponentData;
            else
                ienum = positions
                    .Where(p => world.ComponentData.ContainsKey(p))
                    .Select(p => new KeyValuePair<Point16, ComponentData>(p, world.ComponentData[p]));

            foreach (KeyValuePair<Point16, ComponentData> pair in ienum)
            {
                if (
                    pair.Value is UnloadedComponentData 
                    || pair.Value.Component is null
                    || !pair.Value.Component.ShouldSyncData(pair.Value)) continue;
                send.Add((pair.Key, pair.Value));
            }
            ModPacket pack = CreatePacket(NetMessageType.ComponentDataSync);
            pack.Write((ushort)send.Count);
            for (int i = 0; i < send.Count; i++)
            {
                var (pos, cd) = send[i];
                pack.Write(pos.X);
                pack.Write(pos.Y);
                cd.Component.NetSendData(pack, cd);
            }
            pack.Send(clientId);
        }
        private static void ReceiveComponentDataSync(BinaryReader reader) 
        {
            ComponentWorld world = ModContent.GetInstance<ComponentWorld>();

            ushort count = reader.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                Point16 pos = new(reader.ReadInt16(), reader.ReadInt16());
                ComponentData data = Component.NetReceiveData(reader);
                
                if (data is null)
                {
                    LogWarn("Cannot receive data for component at {0}, {1}", pos.X, pos.Y);
                    continue;
                }
                data.Component?.OnLoaded(pos);
                world.ComponentData[pos] = data;
            }
        }

        public static void SendComponentDataRequest(List<Point16> positions = null) 
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;

            ModPacket pack = CreatePacket(NetMessageType.ComponentDataRequest);
            if (positions is null)
            {
                pack.Write(ushort.MaxValue);
                pack.Send();
                return;
            }

            pack.Write((ushort)positions.Count);
            for (int i = 0; i < positions.Count; i++)
            {
                pack.Write(positions[i].X);
                pack.Write(positions[i].Y);
            }
            pack.Send();
        }
        private static void ReceiveComponentDataRequest(BinaryReader reader, int whoAmI)
        {
            ushort count = reader.ReadUInt16();
            List<Point16> positions = null;
            if (count < ushort.MaxValue)
                for (int i = 0; i < count; i++)
                    positions.Add(new(reader.ReadInt16(), reader.ReadInt16()));

            SendComponentDataSync(positions, whoAmI);
        }

        //public static void SendComponentCustomData<TDataType>(Point16 pos, Component<TDataType> component, int clientId = -1) where TDataType : ComponentData, new()
        //{
        //    ModPacket pack = CreatePacket(NetMessageType.ComponentCustomDataSync);
        //    pack.Write(pos.X);
        //    pack.Write(pos.Y);
        //    component.SendCustomData(component.GetData(pos), pack);
        //    pack.Send(clientId);
        //}
        //private static void ReceiveComponentCustomData(BinaryReader reader, int whoAmI) 
        //{
        //
        //}

        public static ModPacket CreateComponentPacket(string component, Point16 pos, ushort messageType)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            ModPacket pack = CreatePacket(NetMessageType.ComponentPacket);
            pack.Write(component);
            pack.Write(messageType);
            pack.Write(pos.X);
            pack.Write(pos.Y);
            return pack;
        }
        private static void ReceiveComponentPacket(BinaryReader reader, int whoAmI, ref bool broadcast) 
        {
            string component = reader.ReadString();
            ushort type = reader.ReadUInt16();
            Point16 pos = new(reader.ReadInt16(), reader.ReadInt16());
            if (!Component.ByTypeName.TryGetValue(component, out Component c))
            {
                LogWarn("Message for unregistered component {0}", component);
                return;
            }
            if (!c.HandlePacket(pos, type, reader, whoAmI, ref broadcast))
            {
                LogWarn("Unhandled message {0} for component {1}", type, component);
                return;
            }
        }

        public static ModPacket CreateVariablePacket(string message, Point16 pos, ushort messageType)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            ModPacket pack = CreatePacket(NetMessageType.VariablePacket);
            pack.Write(message);
            pack.Write(messageType);
            pack.Write(pos.X);
            pack.Write(pos.Y);
            return pack;
        }
        private static void ReceiveVariablePacket(BinaryReader reader, int whoAmI, ref bool broadcast)
        {
            string component = reader.ReadString();
            ushort type = reader.ReadUInt16();
            Point16 pos = new(reader.ReadInt16(), reader.ReadInt16());
            if (!Variable.ByTypeName.TryGetValue(component, out Variable v))
            {
                LogWarn("Message for unregistered variable {0}", component);
                return;
            }
            v.HandlePacket(pos, type, reader, whoAmI, ref broadcast);
        }

        public static void SendComponentVariable(Point16 pos, int slot) 
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ComponentWorld world = ModContent.GetInstance<ComponentWorld>();
            ComponentData data = world.GetDataOrNull(pos);
            if (!data.Variables.TryGetValue(slot, out Items.Variable var))
                return;

            Variable v = var.Var;

            ModPacket pack = CreatePacket(NetMessageType.ComponentVariable);
            pack.Write(pos.X);
            pack.Write(pos.Y);
            pack.Write(slot);

            if (v is null)
                pack.Write("");
            else
                v.SaveData(pack);
            pack.Send();
        }
        private static void ReceiveComponentVariable(BinaryReader reader) 
        {
            ComponentWorld world = ModContent.GetInstance<ComponentWorld>();

            Point16 pos = new(reader.ReadInt16(), reader.ReadInt16());
            int slot = reader.ReadInt32();
            Variable v = Variable.LoadData(reader);

            ComponentData data = world.GetDataOrNull(pos);
            data?.SetVariable(slot, v);
        }

        public static void SendComponentFrequency(Point16 pos)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ComponentWorld world = ModContent.GetInstance<ComponentWorld>();
            ComponentData data = world.GetDataOrNull(pos);
            if (data is null) return;

            ModPacket p = CreatePacket(NetMessageType.ComponentFrequency);
            p.Write(pos.X);
            p.Write(pos.Y);
            p.Write(data.UpdateFrequency);
            p.Send();
        }
        private static void ReceiveComponentFrequency(BinaryReader reader) 
        {
            ComponentWorld world = ModContent.GetInstance<ComponentWorld>();

            Point16 pos = new(reader.ReadInt16(), reader.ReadInt16());
            ushort freq = reader.ReadUInt16();

            ComponentData data = world.GetDataOrNull(pos);
            if (data is null) return;
            data.Component?.SetUpdates(pos, freq > 0);
            data.UpdateFrequency = freq;
        }

        private static void LogWarn(string format, params object[] args) 
        {
            if (Main.dedServ)
            {
                Console.Write($"[{Mod.Name}/Warn] ");
                Console.WriteLine(format, args);
                return;
            }
            Mod.Logger.WarnFormat(format, args);
        }
    }

    public enum NetMessageType : ushort
    {
        ComponentDataSync,
        ComponentDataRequest,
        //ComponentCustomDataSync,
        //ComponentCustomDataRequest,
        ComponentPacket,
        VariablePacket,
        ComponentVariable,
        ComponentFrequency
    }
}
