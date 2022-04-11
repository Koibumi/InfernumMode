using CalamityMod.Events;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.Particles;
using InfernumMode.Balancing;
using InfernumMode.BehaviorOverrides.BossAIs.Cryogen;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon.Athena;
using InfernumMode.BehaviorOverrides.BossAIs.MoonLord;
using InfernumMode.BehaviorOverrides.BossAIs.Twins;
using InfernumMode.BossIntroScreens;
using InfernumMode.BossRush;
using InfernumMode.ILEditingStuff;
using InfernumMode.Items;
using InfernumMode.OverridingSystem;
using InfernumMode.Skies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode
{
	public class InfernumMode : Mod
    {
        internal static InfernumMode Instance = null;
        internal static Mod CalamityMod = null;
        internal static bool CanUseCustomAIs => (!BossRushEvent.BossRushActive || BossRushApplies) && PoDWorld.InfernumMode;

        internal static bool BossRushApplies => true;

        internal static readonly Color HiveMindSkyColor = new(53, 42, 81);

        public static float BlackFade = 0f;

        public override void Load()
        {
            Instance = this;
            CalamityMod = ModLoader.GetMod("CalamityMod");

            OverridingListManager.Load();
            BalancingChangesManager.Load();
            HookManager.Load();

            Filters.Scene["InfernumMode:HiveMind"] = new Filter(new HiveMindScreenShaderData("FilterMiniTower").UseColor(HiveMindSkyColor).UseOpacity(0.6f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:HiveMind"] = new HiveMindSky();

            Filters.Scene["InfernumMode:Dragonfolly"] = new Filter(new DragonfollyScreenShaderData("FilterMiniTower").UseColor(Color.Red).UseOpacity(0.6f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:Dragonfolly"] = new DragonfollySky();

            Filters.Scene["InfernumMode:Deus"] = new Filter(new DeusScreenShaderData("FilterMiniTower").UseColor(Color.Lerp(Color.Purple, Color.Black, 0.75f)).UseOpacity(0.24f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:Deus"] = new DeusSky();

            Filters.Scene["InfernumMode:OldDuke"] = new Filter(new OldDukeScreenShaderData("FilterMiniTower").UseColor(Color.Lerp(Color.Lime, Color.Black, 0.9f)).UseOpacity(0.6f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:OldDuke"] = new OldDukeSky();

            // Manually invoke the attribute constructors to get the marked methods cached.
            foreach (var type in typeof(InfernumMode).Assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(Utilities.UniversalBindingFlags))
                    method.GetCustomAttributes(false);
            }

            IntroScreenManager.Load();
            NPCBehaviorOverride.LoadAll();
            ProjectileBehaviorOverride.LoadAll();

            if (Main.netMode != NetmodeID.Server)
            {
                CryogenBehaviorOverride.SetupCustomBossIcon();

                Ref<Effect> portalShader = new(Assets.Request<Effect>("Effects/DoGPortalShader", AssetRequestMode.ImmediateLoad).Value);
                Filters.Scene["Infernum:DoGPortal"] = new Filter(new ScreenShaderData(portalShader, "ScreenPass"), EffectPriority.High);
                Filters.Scene["Infernum:DoGPortal"].Load();

                Ref<Effect> aewPsychicEnergyShader = new(Assets.Request<Effect>("Effects/AEWPsychicDistortionShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:AEWPsychicEnergy"] = new MiscShaderData(aewPsychicEnergyShader, "DistortionPass");

                Ref<Effect> gradientShader = new(Assets.Request<Effect>("Effects/GradientWingShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:GradientWingShader"] = new MiscShaderData(gradientShader, "GradientPass");

                Ref<Effect> cyclicHueShader = new(Assets.Request<Effect>("Effects/CyclicHueShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:CyclicHueShader"] = new MiscShaderData(cyclicHueShader, "OutlineShader");

                Ref<Effect> pristineArmorShader = new(Assets.Request<Effect>("Effects/PristineArmorShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:PristineArmorShader"] = new MiscShaderData(pristineArmorShader, "PristinePass");

                Ref<Effect> dukeTornadoShader = new(Assets.Request<Effect>("Effects/DukeTornado", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:DukeTornado"] = new MiscShaderData(dukeTornadoShader, "TrailPass");

                Ref<Effect> tentacleFleshShader = new(Assets.Request<Effect>("Effects/TentacleTexture", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:WoFTentacleTexture"] = new MiscShaderData(tentacleFleshShader, "TrailPass");

                Ref<Effect> bloodGeyserShader = new(Assets.Request<Effect>("Effects/BloodGeyser", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:WoFGeyserTexture"] = new MiscShaderData(bloodGeyserShader, "TrailPass");

                Ref<Effect> shadowflameShader = new(Assets.Request<Effect>("Effects/Shadowflame", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:Fire"] = new MiscShaderData(shadowflameShader, "TrailPass");

                Ref<Effect> brainPsychicShader = new(Assets.Request<Effect>("Effects/BrainPsychicShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:BrainPsychic"] = new MiscShaderData(brainPsychicShader, "TrailPass");

                Ref<Effect> cultistDeathAnimationShader = new(Assets.Request<Effect>("Effects/CultistDeathAnimation", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:CultistDeath"] = new MiscShaderData(cultistDeathAnimationShader, "DeathPass");

                Ref<Effect> flameTrailShader = new(Assets.Request<Effect>("Effects/TwinsFlameTail", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:TwinsFlameTrail"] = new MiscShaderData(flameTrailShader, "TrailPass");

                Ref<Effect> aresLightningArcShader = new(Assets.Request<Effect>("Effects/AresLightningArcShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:AresLightningArc"] = new MiscShaderData(aresLightningArcShader, "TrailPass");

                Ref<Effect> ghostlyShader = new(Assets.Request<Effect>("Effects/EidolicWailRingShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:PolterghastEctoplasm"] = new MiscShaderData(ghostlyShader, "BurstPass");

                ghostlyShader = new Ref<Effect>(Assets.Request<Effect>("Effects/NecroplasmicRoarShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:NecroplasmicRoar"] = new MiscShaderData(ghostlyShader, "BurstPass");

                Ref<Effect> backgroundShader = new(Assets.Request<Effect>("Effects/MoonLordBGDistortionShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:MoonLordBGDistortion"] = new MiscShaderData(backgroundShader, "DistortionPass");

                Ref<Effect> introShader = new(Assets.Request<Effect>("Effects/MechIntroLetterShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:MechsIntro"] = new MiscShaderData(introShader, "LetterPass");

                introShader = new Ref<Effect>(Assets.Request<Effect>("Effects/SCalIntroLetterShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:SCalIntro"] = new MiscShaderData(introShader, "LetterPass");

                Ref<Effect> hologramShader = new(Assets.Request<Effect>("Effects/HologramShader", AssetRequestMode.ImmediateLoad).Value);
                GameShaders.Misc["Infernum:Hologram"] = new MiscShaderData(hologramShader, "HologramPass");

                OverrideMusicBox(ItemID.MusicBoxBoss3, MusicLoader.GetMusicSlot(this, "Sounds/Music/Boss3"), TileID.MusicBoxes, 36 * 12);
                OverrideMusicBox(ItemID.MusicBoxLunarBoss, MusicLoader.GetMusicSlot(this, "Sounds/Music/MoonLord"), TileID.MusicBoxes, 36 * 32);
            }

            if (BossRushApplies)
                BossRushChanges.Load();

            if (Main.netMode != NetmodeID.Server)
                GeneralParticleHandler.LoadModParticleInstances(this);
        }

        internal static IDictionary<int, int> SoundLoaderMusicToItem => (IDictionary<int, int>)typeof(SoundLoader).GetField("musicToItem", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        internal static IDictionary<int, int> SoundLoaderItemToMusic => (IDictionary<int, int>)typeof(SoundLoader).GetField("itemToMusic", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        internal static IDictionary<int, IDictionary<int, int>> SoundLoaderTileToMusic => (IDictionary<int, IDictionary<int, int>>)typeof(SoundLoader).GetField("tileToMusic", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

        public static void OverrideMusicBox(int itemType, int musicSlot, int tileType, int tileFrameY)
        {
            SoundLoaderMusicToItem[musicSlot] = itemType;
            SoundLoaderItemToMusic[itemType] = musicSlot;
            if (!SoundLoaderTileToMusic.ContainsKey(tileType))
                SoundLoaderTileToMusic[tileType] = new Dictionary<int, int>();

            SoundLoaderTileToMusic[tileType][tileFrameY] = musicSlot;
        }

        public override void UpdateMusic(ref int music, ref MusicPriority priority)
        {
            if (NPC.AnyNPCs(NPCID.EyeofCthulhu))
            {
                music = MusicLoader.GetMusicSlot(this, "Sounds/Music/EyeOfCthulhu");
                priority = MusicPriority.BossLow;
            }

            if (NPC.AnyNPCs(NPCID.SkeletronHead))
            {
                music = MusicLoader.GetMusicSlot(this, "Sounds/Music/Boss3");
                priority = MusicPriority.BossLow;
            }

            if (NPC.AnyNPCs(NPCID.SkeletronPrime) || NPC.AnyNPCs(NPCID.Retinazer) || NPC.AnyNPCs(NPCID.Spazmatism) || NPC.AnyNPCs(NPCID.TheDestroyer))
            {
                music = MusicLoader.GetMusicSlot(this, "Sounds/Music/MechBosses");
                priority = MusicPriority.BossLow;
            }

            if (NPC.AnyNPCs(NPCID.DukeFishron))
            {
                music = MusicLoader.GetMusicSlot(this, "Sounds/Music/DukeFishron");
                priority = MusicPriority.BossMedium;
            }

            if (NPC.AnyNPCs(NPCID.CultistBoss))
            {
                music = MusicLoader.GetMusicSlot(this, "Sounds/Music/LunaticCultist");
                priority = MusicPriority.BossMedium;
            }

            int moonLordIndex = NPC.FindFirstNPC(NPCID.MoonLordCore);
            if (moonLordIndex != -1)
            {
                NPC moonLord = Main.npc[moonLordIndex];

                music = MusicLoader.GetMusicSlot(this, "Sounds/Music/MoonLord");
                if (moonLord.Infernum().ExtraAI[10] < MoonLordCoreBehaviorOverride.IntroSoundLength)
                    music = 0;
                Main.musicFade[Main.curMusic] = 1f;
                priority = MusicPriority.BossHigh;
            }

            bool areExoMechsAround = NPC.AnyNPCs(ModContent.NPCType<AresBody>()) ||
                NPC.AnyNPCs(ModContent.NPCType<ThanatosHead>()) ||
                NPC.AnyNPCs(ModContent.NPCType<Apollo>()) ||
                NPC.AnyNPCs(ModContent.NPCType<AthenaNPC>());

            if (areExoMechsAround)
            {
                music = MusicLoader.GetMusicSlot(this, "Sounds/Music/ExoMechBosses");
                priority = MusicPriority.BossHigh;
            }
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) => NetcodeHandler.ReceivePacket(this, reader, whoAmI);

        public override void AddRecipes() => RecipeUpdates.Update();

        public override object Call(params object[] args)
        {
            return InfernumModCalls.Call(args);
        }

        public override void Unload()
        {
            IntroScreenManager.Unload();
            OverridingListManager.Unload();
            BalancingChangesManager.Unload();
            HookManager.Unload();
            Instance = null;
            CalamityMod = null;
        }

        public override void PreUpdateEntities()
        {
            BlackFade = MathHelper.Clamp(BlackFade - 0.025f, 0f, 1f);
            NetcodeHandler.Update();
            TwinsAttackSynchronizer.DoUniversalUpdate();
            TwinsAttackSynchronizer.PostUpdateEffects();
        }
    }
}