using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BossIntroScreens
{
    public class SkeletronPrimeIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => Color.Silver;

        public override int AnimationTime => 210;

        public override bool TextShouldBeCentered => true;

        public override string TextToDisplay => "Contraption of Dread\nSkeletron Prime";

        public override Effect ShaderToApplyToLetters => GameShaders.Misc["Infernum:MechsIntro"].Shader;

        public override void PrepareShader(Effect shader)
        {
            shader.Parameters["uColor"].SetValue(new Vector3(1f, 0.34f, 0.09f));
            shader.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("InfernumMode/ExtraTextures/DiagonalGleam").Value;
        }

        public override bool ShouldBeActive() => NPC.AnyNPCs(NPCID.SkeletronPrime);

        public override SoundStyle? SoundToPlayWithTextCreation => new SoundStyle("InfernumMode/Sounds/Custom/ThanatosTransition");

        public override SoundStyle? SoundToPlayWithLetterAddition => SoundID.NPCHit4;

        public override bool CanPlaySound => LetterDisplayCompletionRatio(AnimationTimer) >= 1f;

        public override float LetterDisplayCompletionRatio(int animationTimer)
        {
            float completionRatio = Utils.GetLerpValue(TextDelayInterpolant, 0.92f, animationTimer / (float)AnimationTime, true);

            // If the completion ratio exceeds the point where the name is displayed, display all letters.
            int startOfLargeTextIndex = TextToDisplay.IndexOf('\n');
            int currentIndex = (int)(completionRatio * TextToDisplay.Length);
            if (currentIndex >= startOfLargeTextIndex)
                completionRatio = 1f;

            return completionRatio;
        }
    }
}