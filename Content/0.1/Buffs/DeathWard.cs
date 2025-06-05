using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Dusts;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class DeathWard : ModBuff {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Death Ward");
        // Description.SetDefault("Your next fatal hit will be prevented");
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<BehelitPlayer>().behelitPotion = true;
}

sealed class BehelitPlayer : ModPlayer {
    private class BehelitVisualEffectOnPlayer : ILoadable {
        void ILoadable.Load(Mod mod) {
            On_LegacyPlayerRenderer.DrawPlayerFull += On_LegacyPlayerRenderer_DrawPlayerFull;
            On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += On_PlayerDrawLayers_DrawPlayer_RenderAllLayers;
        }

        private void On_PlayerDrawLayers_DrawPlayer_RenderAllLayers(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawinfo) {
            var drawPlayer = drawinfo.drawPlayer;
            if (drawPlayer.active) {
                var handler = drawPlayer.GetModPlayer<BehelitPlayer>();
                if (handler.behelitPotion && drawinfo.shadow == handler.ghostFade) {
                    for (int i = 0; i < drawinfo.DrawDataCache.Count; i++) {
                        DrawData value = drawinfo.DrawDataCache[i];
                        value.color = Color.Lerp(value.color, Color.Red, 0.5f);
                        value.color.A = (byte)((float)(int)value.color.A * 0.2f);
                        drawinfo.DrawDataCache[i] = value;
                    }
                }
            }

            orig(ref drawinfo);
        }

        private void On_LegacyPlayerRenderer_DrawPlayerFull(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Terraria.Graphics.Camera camera, Player drawPlayer) {
            var handler = drawPlayer.GetModPlayer<BehelitPlayer>();
            if (handler.behelitPotion) {
                SpriteBatch spriteBatch = camera.SpriteBatch;
                SamplerState samplerState = camera.Sampler;
                if (drawPlayer.mount.Active && drawPlayer.fullRotation != 0f)
                    samplerState = LegacyPlayerRenderer.MountedSamplerState;

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, camera.Rasterizer, null, camera.GameViewMatrix.TransformationMatrix);
                //if (Main.gamePaused)
                //    drawPlayer.PlayerFrame();

                Vector2 position = default(Vector2);
                if (!drawPlayer.ghost) {
                    if (!drawPlayer.invis) {
                        _ = drawPlayer.position;
                        if (!Main.gamePaused)
                            handler.ghostFade += handler.ghostDir * 0.03f;

                        if ((double)handler.ghostFade < 0.05) {
                            handler.ghostDir = 1f;
                            handler.ghostFade = 0.15f;
                        }
                        else if ((double)handler.ghostFade > 0.95) {
                            handler.ghostDir = -1f;
                            handler.ghostFade = 0.95f;
                        }

                        float num2 = handler.ghostFade * 5f;
                        for (int l = 0; l < 4; l++) {
                            float num3;
                            float num4;
                            switch (l) {
                                default:
                                    num3 = num2;
                                    num4 = 0f;
                                    break;
                                case 1:
                                    num3 = 0f - num2;
                                    num4 = 0f;
                                    break;
                                case 2:
                                    num3 = 0f;
                                    num4 = num2;
                                    break;
                                case 3:
                                    num3 = 0f;
                                    num4 = 0f - num2;
                                    break;
                            }

                            position = new Vector2(drawPlayer.position.X + num3, drawPlayer.position.Y + drawPlayer.gfxOffY + num4);
                            self.DrawPlayer(camera, drawPlayer, position, drawPlayer.fullRotation, drawPlayer.fullRotationOrigin, handler.ghostFade);
                        }
                    }
                }

                spriteBatch.End();
            }

            orig(self, camera, drawPlayer);
        }

        void ILoadable.Unload() { }
    }


    public bool behelitPotion;

    private float ghostFade;
    private float ghostDir;

    public override void ResetEffects() => behelitPotion = false;

    public override bool FreeDodge(Player.HurtInfo info) {
        if (behelitPotion && Player.statLife <= info.Damage) {
            int time = Player.longInvince ? 80 : 40;
            DeathWardImmune(time);

            Player.statLife += info.Damage;
            Player.HealEffect(info.Damage, true);
            Player.ClearBuff(ModContent.BuffType<DeathWard>());
            //Player.AddBuff(BuffID.PotionSickness, 3600);

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new DeathWardImmuneTimePacket(Player, time));
            }

            return true;
        }

        return base.FreeDodge(info);
    }

    public void DeathWardImmune(int time) {
        Player.SetImmuneTimeForAllTypes(time);
        SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact with { Volume = 1.5f }, Player.Center);
        for (int i = 0; i < 50; i++) {
            int dust = Dust.NewDust(Player.position - new Vector2(20f, 20f), 40, 40, ModContent.DustType<DeathWardDust>(), 0f, -2f, 0, default);
            Main.dust[dust].velocity.X *= Main.rand.NextFloat(-8f, 8f);
            Main.dust[dust].velocity.Y *= Main.rand.NextFloat(-8f, 8f);
            Main.dust[dust].velocity *= 0.75f;
            Main.dust[dust].scale = Main.rand.NextFloat(2f, 3f) * 0.8f;
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = true;
        }
    }
}