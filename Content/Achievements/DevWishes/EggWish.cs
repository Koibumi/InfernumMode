﻿using InfernumMode.Content.Items.Weapons.Melee;
using InfernumMode.Content.Tiles.Wishes;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace InfernumMode.Content.Achievements.DevWishes
{
    public class EggWish : Achievement
    {
        public override string LocalizationCategory => "Achievements.Wishes";
        
        public override void Initialize()
        {
            TotalCompletion = 1;
            PositionInMainList = 14;
            UpdateCheck = AchievementUpdateCheck.TileBreak;
            IsDevWish = true;
        }

        public override void ExtraUpdate(Player player, int extraInfo)
        {
            if (extraInfo == ModContent.TileType<EggSwordShrine>() && NPC.downedGolemBoss)
                CurrentCompletion++;
        }

        public override void OnCompletion(Player player)
        {
            WishCompletionEffects(player, ModContent.ItemType<CallUponTheEggs>());
        }

        public override void SaveProgress(TagCompound tag)
        {
            tag["EggCurrentCompletion"] = CurrentCompletion;
            tag["EggDoneCompletionEffects"] = DoneCompletionEffects;
        }

        public override void LoadProgress(TagCompound tag)
        {
            CurrentCompletion = tag.Get<int>("EggCurrentCompletion");
            DoneCompletionEffects = tag.Get<bool>("EggDoneCompletionEffects");
        }
    }
}
