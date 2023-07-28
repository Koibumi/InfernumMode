using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.SlimeGod;
using Terraria.ModLoader;
using static CalamityMod.UI.BossHealthBarManager;
using static Terraria.ModLoader.ModContent;

namespace InfernumMode.Core.GlobalInstances.Systems
{
    public class CalamityBossHPBarChangesSystem : ModSystem
    {
        public static void PerformBarChanges()
        {
            OneToMany.Remove(NPCType<EbonianSlimeGod>());
            OneToMany.Remove(NPCType<CrimulanSlimeGod>());

            OneToMany.Remove(NPCType<CeaselessVoid>());
            OneToMany.Remove(NPCType<DarkEnergy>());

            BossExclusionList.Remove(NPCType<SlimeGodCore>());
            EntityExtensionHandler.Remove(NPCType<CeaselessVoid>());
        }

        public static void UndoBarChanges()
        {
            int[] slimeGods = new int[] { NPCType<EbonianSlimeGod>(), NPCType<SplitEbonianSlimeGod>(), NPCType<CrimulanSlimeGod>(), NPCType<SplitCrimulanSlimeGod>() };
            OneToMany[NPCType<EbonianSlimeGod>()] = slimeGods;
            OneToMany[NPCType<CrimulanSlimeGod>()] = slimeGods;

            int[] ceaselessVoid = new int[] { NPCType<CeaselessVoid>(), NPCType<DarkEnergy>() };
            OneToMany[NPCType<CeaselessVoid>()] = ceaselessVoid;
            OneToMany[NPCType<DarkEnergy>()] = ceaselessVoid;

            if (BossExclusionList.Contains(NPCType<SlimeGodCore>()))
                BossExclusionList.Add(NPCType<SlimeGodCore>());

            EntityExtensionHandler[NPCType<CeaselessVoid>()] = new BossEntityExtension("Dark Energy", NPCType<DarkEnergy>());
        }
    }
}