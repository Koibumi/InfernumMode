using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.Content.BehaviorOverrides.BossAIs.MoonLord.MoonLordCoreBehaviorOverride;
namespace InfernumMode.Core.Netcode.Packets
{
    public class SyncMoonlordPacket : BaseInfernumPacket
    {
        public override bool ResendFromServer => false;
        public override void Write(ModPacket packet, params object[] context)
        {
            packet.Write((int)context[0]);
            packet.Write((double)context[1]);
        }

        public override void Read(BinaryReader reader)
        {
            int n = reader.ReadInt32();
            double damage = reader.ReadDouble();
            HandleBodyPartDeathTriggers(Main.npc[n], damage);
        }
    }
}