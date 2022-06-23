using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernumMode.Systems
{
    public class NPCSyncHijackSystem : ModSystem
    {
        public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        {
            if (msgType == MessageID.SyncNPC)
            {
                NPC npc = Main.npc[number];
                if (!npc.active)
                    return base.HijackSendData(whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);

                ModPacket packet = InfernumMode.Instance.GetPacket();
                packet.Write((short)InfernumPacketType.SendExtraNPCData);
                packet.Write(npc.whoAmI);
                packet.Write(npc.realLife);
                packet.Write(npc.Infernum().TotalAISlotsInUse);
                packet.Write(npc.Infernum().arenaRectangle.X);
                packet.Write(npc.Infernum().arenaRectangle.Y);
                packet.Write(npc.Infernum().arenaRectangle.Width);
                packet.Write(npc.Infernum().arenaRectangle.Height);
                for (int i = 0; i < npc.Infernum().ExtraAI.Length; i++)
                {
                    if (!npc.Infernum().HasAssociatedAIBeenUsed[i])
                        continue;
                    packet.Write(i);
                    packet.Write(npc.Infernum().ExtraAI[i]);
                }
                packet.Send();
            }
            return base.HijackSendData(whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
        }
    }
}