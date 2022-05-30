using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraIntegration.Basic;
using TerraIntegration.Components;
using TerraIntegration.DataStructures;
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

        public static bool SinglePlayer => Main.netMode == NetmodeID.SinglePlayer;
        public static bool Server => Main.netMode == NetmodeID.Server;
        public static bool Client => Main.netMode == NetmodeID.MultiplayerClient;

        public static void HandlePacket(BinaryReader reader, int whoAmI) 
        {
            ushort type = reader.ReadUInt16();
            if (type >= MessageTypeNames.Length)
            {
                LogWarn("Unrecognized netmessage type: {0}", type);
                return;
            }
            NetMessageType msgType = (NetMessageType)type;

            try
            {
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
            catch (NetworkException) { throw; }
            catch (Exception e)
            {
                throw new NetworkException($"Exception in receiving {msgType} packet", e);
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

            Statistics.LogMessage($"[Net] Sending {positions?.Count.ToString() ?? "all"} component data");

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
            Statistics.LogMessage($"[Net] Receiving {count} component data");
            for (int i = 0; i < count; i++)
            {
                Point16 pos = new(reader.ReadInt16(), reader.ReadInt16());
                ComponentData data = Component.NetReceiveData(reader, pos);
                
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

            Statistics.LogMessage($"[Net] Sending {positions?.Count.ToString() ?? "all"} component data request");

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
            Statistics.LogMessage($"[Net] Receiving {(count == ushort.MaxValue ? "all" : count.ToString())} component data request");
            List<Point16> positions = null;
            if (count < ushort.MaxValue)
                for (int i = 0; i < count; i++)
                    positions.Add(new(reader.ReadInt16(), reader.ReadInt16()));

            SendComponentDataSync(positions, whoAmI);
        }

        public static ModPacket CreateComponentPacket(string component, Point16 pos, ushort messageType)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            Statistics.LogMessage($"[Net] Sending {component} component packet at {pos}, type {messageType}");

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

            Statistics.LogMessage($"[Net] Receiving {component} component packet at {pos}, type {type}");

            if (!Component.ByTypeName.TryGetValue(component, out Component c))
            {
                LogWarn("Message for unregistered component {0}", component);
                return;
            }
            try
            {
                if (!c.HandlePacket(pos, type, reader, whoAmI, ref broadcast))
                {
                    LogWarn("Unhandled message {0} for component {1}", type, component);
                    return;
                }
            }
            catch (Exception e)
            {
                throw new NetworkException($"Exception while receiving message {type} for component {component} at {pos}", e);
            }
        }

        public static ModPacket CreateVariablePacket(string variable, Point16 pos, ushort messageType)
        {
            if (variable is null)
                throw new ArgumentNullException(nameof(variable));

            Statistics.LogMessage($"[Net] Sending {variable} variable packet at {pos}, type {messageType}");

            ModPacket pack = CreatePacket(NetMessageType.VariablePacket);
            pack.Write(variable);
            pack.Write(messageType);
            pack.Write(pos.X);
            pack.Write(pos.Y);
            return pack;
        }
        private static void ReceiveVariablePacket(BinaryReader reader, int whoAmI, ref bool broadcast)
        {
            string variable = reader.ReadString();
            ushort type = reader.ReadUInt16();
            Point16 pos = new(reader.ReadInt16(), reader.ReadInt16());

            Statistics.LogMessage($"[Net] Receivnig {variable} variable packet at {pos}, type {type}");

            if (!Variable.ByTypeName.TryGetValue(variable, out Variable v))
            {
                LogWarn("Message for unregistered variable {0}", variable);
                return;
            }
            v.HandlePacket(pos, type, reader, whoAmI, ref broadcast);
        }

        public static void SendComponentVariable(Point16 pos, string slot) 
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            Statistics.LogMessage($"[Net] Sending variable at {pos}, slot {slot}");

            ComponentWorld world = ModContent.GetInstance<ComponentWorld>();
            ComponentData data = world.GetDataOrNull(pos);
            if (!data.Variables.TryGetValue(slot, out Items.Variable var))
                return;

            Variable v = var?.Var;

            ModPacket pack = CreatePacket(NetMessageType.ComponentVariable);
            pack.Write(pos.X);
            pack.Write(pos.Y);
            pack.Write(slot);

            Variable.SaveData(v, pack);
            pack.Send();
        }
        private static void ReceiveComponentVariable(BinaryReader reader) 
        {
            ComponentWorld world = ModContent.GetInstance<ComponentWorld>();

            Point16 pos = new(reader.ReadInt16(), reader.ReadInt16());
            string slot = reader.ReadString();

            Statistics.LogMessage($"[Net] Receiving variable at {pos}, slot {slot}");

            Variable v = Variable.LoadData(reader);

            ComponentData data = world.GetDataOrNull(pos);
            data?.SetVariable(slot, v);
        }

        public static void SendComponentFrequency(Point16 pos)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            Statistics.LogMessage($"[Net] Sending component frequency at {pos}");

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

            Statistics.LogMessage($"[Net] Receiving component frequency at {pos}");

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
            else
            {
                Statistics.LogMessage("[Net] " + string.Format(format, args));
                Mod.Logger.WarnFormat(format, args);
            }
        }
    }

    public enum NetMessageType : ushort
    {
        ComponentDataSync,
        ComponentDataRequest,
        ComponentPacket,
        VariablePacket,
        ComponentVariable,
        ComponentFrequency
    }

    public class NetworkException : Exception
    {
        public NetworkException(string message, Exception inner) : base(message, inner) { }
    }
}
