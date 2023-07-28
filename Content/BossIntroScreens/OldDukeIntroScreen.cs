using CalamityMod.NPCs.OldDuke;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace InfernumMode.Content.BossIntroScreens
{
    public class OldDukeIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => new(completionRatio =>
        {
            float limeColorInterpolant = Utils.GetLerpValue(0.77f, 1f, Sin(AnimationCompletion * -Pi * 4f + completionRatio * Pi) * 0.5f + 0.5f);
            Color skinColor = new(113, 90, 71);
            Color irradiatedColor = new(170, 216, 15);
            return Color.Lerp(skinColor, irradiatedColor, limeColorInterpolant);
        });

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;

        public override string TextToDisplay
        {
            get
            {
                if (IntroScreenManager.ShouldDisplayJokeIntroText)
                    return "Speed Demon\nThe Old Duke";

                return "Sulphuric Terror\nThe Old Duke";
            }
        }

        public override bool ShouldBeActive() => NPC.AnyNPCs(ModContent.NPCType<OldDuke>());

        // Sounds are played in the Old Duke's AI.
        public override SoundStyle? SoundToPlayWithTextCreation => null;
    }
}