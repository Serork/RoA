using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class BloodbathLocketGlowGlowing : ModSystem {
    private Asset<Texture2D> _lothorGlowMaskTexture;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _lothorGlowMaskTexture = ModContent.Request<Texture2D>(GetType().Namespace.Replace(".", "/") + "/BloodbathLocket_Neck_Glow");
    }

    public override void Load() {
        On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += On_PlayerDrawLayers_DrawPlayer_RenderAllLayers;
    }

    private void On_PlayerDrawLayers_DrawPlayer_RenderAllLayers(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);

        if (!drawinfo.drawPlayer.GetModPlayer<BloodbathLocketGlowMaskHandler.BloodbathLocketGlowMaskHandler2>().ShouldDraw) {
            return;
        }

        Player player = drawinfo.drawPlayer;
        if (player.neck > 0) {
            var drawInfo = drawinfo;
            if (player.sleeping.isSleeping) {
                return;
            }
            if (!(player.dead || player.invis || player.ShouldNotDraw)) {
                bool wasSitting = player.sitting.isSitting;
                if (drawInfo.isSitting) {
                    player.sitting.isSitting = true;
                }
                player.sitting.GetSittingOffsetInfo(player, out Vector2 posOffset, out float seatYOffset);

                SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
                Main.spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
                Color color = Color.White;
                var spriteEffects = drawInfo.drawPlayer.direction == -1 ? SpriteEffects.None | SpriteEffects.FlipHorizontally : SpriteEffects.None;
                if (player.gravDir == -1f) {
                    spriteEffects |= SpriteEffects.FlipVertically;
                }
                for (float i2 = -MathHelper.Pi; i2 <= MathHelper.Pi; i2 += MathHelper.PiOver2) {
                    Main.spriteBatch.Draw(
                        _lothorGlowMaskTexture.Value,
                        (drawInfo.drawPlayer.mount.Active ? drawInfo.drawPlayer.position : drawInfo.Position) + (player.gravDir != 1 ? Vector2.UnitY * 6f : Vector2.Zero) + Vector2.UnitY * (-posOffset.Y + seatYOffset) - Main.screenPosition + new Vector2(0f, drawInfo.drawPlayer.mount.Active ? drawInfo.drawPlayer.gfxOffY : 0f)
                            +
                            Utils.RotatedBy(Utils.ToRotationVector2(i2), Main.GlobalTimeWrappedHourly * 10.0 * player.gravDir, new Vector2())
                            * Helper.Wave(0f, 3f, 12f, 0.5f),
                        drawInfo.drawPlayer.bodyFrame,
                        color * 0.5f,
                        0f,
                        new Vector2(10),
                        1f + Main.rand.NextFloatRange(0.05f),
                        spriteEffects,
                        0
                    );
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(in snapshot);
                player.sitting.isSitting = wasSitting;
            }
        }
    }
}


sealed class BloodbathLocketGlowMaskHandler : PlayerDrawLayer {
    private static Asset<Texture2D> glowTexture;

    internal class BloodbathLocketGlowMaskHandler2 : ModPlayer {
        public bool ShouldDraw;
    }

    public override void Load() => glowTexture = ModContent.Request<Texture2D>(GetType().Namespace.Replace(".", "/") + "/BloodbathLocket_Neck_Glow");
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.NeckAcc);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return true;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        drawInfo.drawPlayer.GetModPlayer<BloodbathLocketGlowMaskHandler2>().ShouldDraw = false;

        if (drawInfo.drawPlayer.neck != EquipLoader.GetEquipSlot(RoA.Instance, nameof(BloodbathLocket), EquipType.Neck)) {
            return;
        }

        if (drawInfo.shadow != 0f)
            return;

        drawInfo.drawPlayer.GetModPlayer<BloodbathLocketGlowMaskHandler2>().ShouldDraw = true;
    }
}

[AutoloadEquip(EquipType.Neck)]
sealed class BloodbathLocket : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Bloodbath Locket");
        // Tooltip.SetDefault("Increases damage by 1% for each enemy and 5% for each boss nearby, up to 25%");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = width;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.accessory = true;
        Item.expert = true;

        Item.value = Item.sellPrice(0, 2, 0, 0);
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
        SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
        Main.spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
        for (float i2 = -MathHelper.Pi; i2 <= MathHelper.Pi; i2 += MathHelper.PiOver2) {
            Texture2D glowMaskTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            Vector2 origin = glowMaskTexture.Size() / 2f;
            Color color = Color.White;
            spriteBatch.Draw(glowMaskTexture, Item.Center - Main.screenPosition +
                    Utils.RotatedBy(Utils.ToRotationVector2(i2), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                    * Helper.Wave(0f, 3f, 12f, 0.5f + whoAmI), null, color * 0.5f, rotation + Main.rand.NextFloatRange(0.05f), origin, 1f, SpriteEffects.None, 0f);
        }
        Lighting.AddLight(Item.Center, new Vector3(1f, 0.2f, 0.2f) * 0.35f * Helper.Wave(1f, 1.25f, 12f, 0.5f + whoAmI));
        if (Item.timeSinceItemSpawned % 12 == 0) {
            Vector2 center = Item.Center;
            Vector2 direction = Main.rand.NextVector2CircularEdge(Item.width * 0.6f, Item.height * 0.6f);
            float distance = 0.3f + Main.rand.NextFloat() * 0.5f;
            Vector2 velocity = new(0f, -Main.rand.NextFloat() * 0.3f - 1.5f);

            Dust dust = Dust.NewDustPerfect(center + direction * distance, DustID.RedTorch, velocity);
            dust.scale = 0.5f;
            dust.fadeIn = 1.1f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.alpha = 0;
        }
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(in snapshot);
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!TileHelper.DrawingTiles) {
            return;
        }

        SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot.sortMode, BlendState.Additive, Main.DefaultSamplerState, snapshot.depthStencilState, snapshot.rasterizerState, snapshot.effect, snapshot.transformationMatrix);
        for (float i2 = -MathHelper.Pi; i2 <= MathHelper.Pi; i2 += MathHelper.PiOver2) {
            Texture2D glowMaskTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            Vector2 origin2 = glowMaskTexture.Size() / 2f;
            Color color = Color.White;
            spriteBatch.Draw(glowMaskTexture, position +
                    Utils.RotatedBy(Utils.ToRotationVector2(i2), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                    * Helper.Wave(0f, 3f, 12f, 0.5f + Item.whoAmI), null, color * 0.5f, 0f + Main.rand.NextFloatRange(0.05f), origin2, scale, SpriteEffects.None, 0f);
        }
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(in snapshot);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<BloodbathLocketPlayer>().bloodbathLocket = true;
        if (!hideVisual) player.GetModPlayer<BloodbathLocketPlayer>().theWildEye = true;
    }

    private class BloodbathLocketEyes : ILoadable {
        private class EyesData {
            public Vector2 Position;
            public int TimeLeft, MaxTimeLeft;
            public float Rotation;
            public bool Extra;
        }

        private float _eyesTimer;
        private readonly EyesData[] _eyes = new EyesData[5];

        void ILoadable.Load(Mod mod) {
            for (int i = 0; i < _eyes.Length; i++) {
                _eyes[i] = new EyesData();
            }

            On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += On_PlayerDrawLayers_DrawPlayer_RenderAllLayers;
        }

        void ILoadable.Unload() { }

        private void On_PlayerDrawLayers_DrawPlayer_RenderAllLayers(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawInfo) {
            orig(ref drawInfo);

            Player player = drawInfo.drawPlayer;

            if (drawInfo.shadow != 0f || !player.active) {
                return;
            }

            BloodbathLocketPlayer handler = player.GetModPlayer<BloodbathLocketPlayer>();
            if (!handler.theWildEye) {
                return;
            }

            bool gameActive = !Main.gamePaused;
            int eyesCount = Math.Min(_eyes.Length, (int)(handler.bloodbathDamage / 0.05f) + 1);
            if (gameActive && eyesCount > 0 && ++_eyesTimer > 120f) {
                for (int i = 0; i < eyesCount; i++) {
                    EyesData eyes = _eyes[i];
                    if (eyes.TimeLeft > 0) {
                        continue;
                    }
                    eyes.MaxTimeLeft = eyes.TimeLeft = 210 + Main.rand.Next(60) + eyesCount * 60;
                    eyes.Position = player.Center + Main.rand.RandomPointInArea(100f);
                    eyes.Rotation = Main.rand.NextFloatRange(MathHelper.PiOver4 / 2f);
                    eyes.Extra = Main.rand.NextBool();
                    _eyesTimer = 0f;
                    break;
                }
            }
            SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(snapshot with { blendState = BlendState.Additive });
            for (int i = 0; i < eyesCount; i++) {
                EyesData eyes = _eyes[i];
                if (eyes is null) {
                    continue;
                }
                if (eyes.TimeLeft <= 0) {
                    continue;
                }
                if (gameActive) {
                    eyes.TimeLeft--;
                }
                Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.Textures + $"FlederEyes" + (eyes.Extra ? 2 : string.Empty)).Value;
                float edge = 30f;
                float opacity = Utils.GetLerpValue(0f, edge, eyes.TimeLeft, true) * Utils.GetLerpValue(eyes.MaxTimeLeft, eyes.MaxTimeLeft - edge, eyes.TimeLeft, true);
                for (float i2 = -MathHelper.Pi; i2 <= MathHelper.Pi; i2 += MathHelper.PiOver2) {
                    Main.spriteBatch.Draw(texture, eyes.Position - Main.screenPosition +
                            Utils.RotatedBy(Utils.ToRotationVector2(i2), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                            * Helper.Wave(0f, 3f, 12f, 0.5f + eyes.MaxTimeLeft * 0.25f), null,
                            Color.White.MultiplyAlpha(Helper.Wave(0.5f, 0.75f, 12f, 0.5f + eyes.MaxTimeLeft * 0.25f)) * opacity * 0.75f,
                            eyes.Rotation + Main.rand.NextFloatRange(0.05f),
                            Vector2.Zero, 1f, default, 0f);
                }
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(in snapshot);
        }
    }

    private class BloodbathLocketPlayer : ModPlayer {
        public bool bloodbathLocket, theWildEye;
        internal float bloodbathDamage { get; private set; }
        private const float bloodbathDamageMax = 0.25f;

        public override void ResetEffects() {
            bloodbathLocket = false;
            theWildEye = false;
            bloodbathDamage = 0;
        }

        public override void UpdateEquips() {
            float locketRadius = 1000;
            if (bloodbathLocket) {
                for (int i = 0; i < Main.npc.Length; i++) {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() && !npc.friendly && !npc.CountsAsACritter && Vector2.Distance(Player.Center, npc.Center) < locketRadius) {
                        if (npc.boss) bloodbathDamage += 0.05f;
                        else bloodbathDamage += 0.01f;
                        if (theWildEye) TheWildEye();
                    }
                }
                if (bloodbathDamage > bloodbathDamageMax) bloodbathDamage = bloodbathDamageMax;
                Player.GetDamage(DamageClass.Generic) += bloodbathDamage;
            }
        }

        public void TheWildEye() {
            //int index = 0 + Player.bodyFrame.Y / 56;
            //if (index >= Main.OffsetsPlayerHeadgear.Length) index = 0;

            //Vector2 vector2_1 = Vector2.Zero + Player.velocity;
            //if (Player.mount.Active && Player.mount.Cart) {
            //    int num = Math.Sign(Player.velocity.X);
            //    if (num == 0) num = Player.direction;
            //    vector2_1 = new Vector2(MathHelper.Lerp(0.0f, -8f, Player.fullRotation / 0.78f), MathHelper.Lerp(0.0f, 2f, Math.Abs(Player.fullRotation / 0.78f))).RotatedBy(Player.fullRotation, new Vector2());
            //    if (num == Math.Sign(Player.fullRotation))
            //        vector2_1 *= MathHelper.Lerp(1f, 0.6f, Math.Abs(Player.fullRotation / 0.78f));
            //}

            //Vector2 spinningpoint1 = new Vector2(3 * Player.direction - (Player.direction == 1 ? 1 : 0), -11.5f * Player.gravDir) + Vector2.UnitY * Player.gfxOffY + Player.Size / 2f + Main.OffsetsPlayerHeadgear[index];
            //Vector2 spinningpoint2 = new Vector2(3 * Player.shadowDirection[1] - (Player.direction == 1 ? 1 : 0), -11.5f * Player.gravDir) + Player.Size / 2f + Main.OffsetsPlayerHeadgear[index];
            //if (Player.fullRotation != 0.0) {
            //    spinningpoint1 = spinningpoint1.RotatedBy(Player.fullRotation, Player.fullRotationOrigin);
            //    spinningpoint2 = spinningpoint2.RotatedBy(Player.fullRotation, Player.fullRotationOrigin);
            //}

            //float num1 = 0.0f;
            //if (Player.mount.Active) num1 = Player.mount.PlayerOffset;

            //Vector2 vector2_2 = Player.position + spinningpoint1 + vector2_1;
            //vector2_2.Y -= num1 / 2f;

            //Vector2 vector2_3 = Player.oldPosition + spinningpoint2 + vector2_1;
            //vector2_3.Y -= num1 / 2f;

            //float num2 = 1f;
            //int num3 = (int)Vector2.Distance(vector2_2, vector2_3) / 3 + 1;
            //if (Vector2.Distance(vector2_2, vector2_3) % 3.0 != 0.0) ++num3;

            //DelegateMethods.v3_1 = Main.hslToRgb(Main.rgbToHsl(Player.eyeColor).X, 1f, 0.5f).ToVector3() * 0.5f * num2;
            //if (Player.velocity != Vector2.Zero)
            //    Utils.PlotTileLine(Player.Center, Player.Center + Player.velocity * 2f, 4f, DelegateMethods.CastLightOpen);
            //else
            //    Utils.PlotTileLine(Player.Left, Player.Right, 4f, DelegateMethods.CastLightOpen);

            //for (float num4 = 1f; num4 <= (double)num3; ++num4) {
            //    Dust dust = Main.dust[Dust.NewDust(Player.Center, 0, 0, ModContent.DustType<RedLineDust>(), 0.0f, 0.0f, 0, default, 1f)];
            //    dust.position = Vector2.Lerp(vector2_3, vector2_2, num4 / num3);
            //    dust.noGravity = true;
            //    dust.velocity = Vector2.Zero;
            //    dust.customData = Player;
            //    dust.scale = num2;
            //    dust.shader = GameShaders.Armor.GetSecondaryShader(Player.cYorai, Player);
            //}
        }
    }
}