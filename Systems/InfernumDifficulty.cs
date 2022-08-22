using CalamityMod.Systems;
using InfernumMode.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static CalamityMod.Systems.DifficultyModeSystem;

namespace InfernumMode.Systems
{
    public class InfernumDifficulty : DifficultyMode
    {
        public override bool Enabled
        {
            get => WorldSaveSystem.InfernumMode;
            set => WorldSaveSystem.InfernumMode = value;
        }

        private Asset<Texture2D> _texture;
        public override Asset<Texture2D> Texture
        {
            get
            {
                if (_texture == null)
                    _texture = ModContent.Request<Texture2D>("InfernumMode/ExtraTextures/InfernumIcon");

                return _texture;
            }
        }

        public override string ExpandedDescription
        {
            get
            {
                return ("[c/B32E81:Many major foes will be vastly different, having more challenging AI.] \n" +
                        "[c/B32E81:Adrenaline takes considerably longer to charge.] \n" +
                        "[c/FF0055:Adaptability is imperative.]");
            }
        }

        public InfernumDifficulty()
        {
            DifficultyScale = 99999999f;
            Name = "Infernum";
            ShortDescription = "[c/B32E81:A distinct challenge for the those who seek something vastly different yet also more demanding than Death mode.]";

            ActivationTextKey = "Mods.InfernumMode.InfernumText";
            DeactivationTextKey = "Mods.InfernumMode.InfernumText2";

            ActivationSound = InfernumSoundRegistry.AresLaughSound;

            ChatTextColor = Color.DarkRed;

            MostAlternateDifficulties = 1;
            Difficulties = new DifficultyMode[] { new NoDifficulty(), new RevengeanceDifficulty(), new DeathDifficulty(), new WhereMalice(), this };
            Difficulties = Difficulties.OrderBy(d => d.DifficultyScale).ToArray();

            DifficultyTiers = new List<DifficultyMode[]>();
            float currentTier = -1;
            int tierIndex = -1;

            for (int i = 0; i < Difficulties.Length; i++)
            {
                //if we are at a new tier, create a new list of difficulties at that tier.
                if (currentTier != Difficulties[i].DifficultyScale)
                {
                    DifficultyTiers.Add(new DifficultyMode[] { Difficulties[i] });
                    currentTier = Difficulties[i].DifficultyScale;
                    tierIndex++;
                }

                //if the tier already exists, just add it to the list of other difficulties at that tier.
                else
                {
                    //ugly
                    DifficultyTiers[tierIndex] = DifficultyTiers[tierIndex].Append(Difficulties[i]).ToArray();

                    MostAlternateDifficulties = Math.Max(DifficultyTiers[tierIndex].Length, MostAlternateDifficulties);
                }
            }
        }

        public override int FavoredDifficultyAtTier(int tier)
        {
            DifficultyMode[] tierList = DifficultyTiers[tier];

            for (int i = 0; i < tierList.Length; i++)
            {
                if (tierList[i].Name == "Death")
                    return i;
            }

            return 0;
        }
    }
}