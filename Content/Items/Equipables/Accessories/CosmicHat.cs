using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.UI;
using RoA.Content.Buffs;
using RoA.Content.Projectiles.Friendly.Miscellaneous;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Face)]
sealed class CosmicHat : ModItem, IMagicItemForVisuals {
    private static Asset<Texture2D> _faceGlow = null!,
                                    _faceMask = null!;

    public override void SetStaticDefaults() {
        ArmorIDs.Face.Sets.OverrideHelmet[Item.faceSlot] = true;
        ArmorIDs.Head.Sets.DrawHatHair[Item.faceSlot] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        if (Main.dedServ) {
            return;
        }

        _faceGlow = ModContent.Request<Texture2D>(Texture + "_Face_Glow");
        _faceMask = ModContent.Request<Texture2D>(Texture + "_Face_Mask");
    }

    private class CosmicHatFaceGlowing : PlayerDrawLayer {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FaceAcc);

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            if (drawInfo.hideEntirePlayer) {
                return;
            }

            Player player = drawInfo.drawPlayer;
            if (!player.active || player.invis) {
                return;
            }
            DrawHeadGlowMask(ref drawInfo);
        }

        private static void DrawHeadGlowMask(ref PlayerDrawSet drawInfo) {
            Player player = drawInfo.drawPlayer;
            if (player.face == EquipLoader.GetEquipSlot(RoA.Instance, typeof(CosmicHat).Name, EquipType.Face)) {
                Texture2D glowMaskTexture = _faceGlow.Value;
                Color glowMaskColor = Color.White * 0.9f;
                glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, drawInfo.shadow);
                DrawData drawData;
                var drawinfo = drawInfo;

                Texture2D maskTexture = _faceMask.Value;
                drawData = GetHeadGlowMask(ref drawInfo, maskTexture, glowMaskColor);
                //DrawColor color = Lighting.GetColor((int)((double)player.position.X + (double)player.width * 0.5) / 16, (int)(((double)player.position.Y + (double)player.height * 0.25) / 16.0));
                glowMaskColor = GameShaders.Hair.GetColor(12, player, Color.White * 0.9f);
                glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
                int head = drawInfo.drawPlayer.head;
                drawInfo.drawPlayer.head = 1;
                drawData.color = glowMaskColor;
                var shader = PlayerDrawHelper.PackShader(12, PlayerDrawHelper.ShaderConfiguration.HairShader);
                drawData.shader = shader;
                drawinfo.DrawDataCache.Add(drawData);

                drawinfo = drawInfo;
                drawData = GetHeadGlowMask(ref drawInfo, glowMaskTexture, glowMaskColor);
                glowMaskColor = Color.White;
                glowMaskColor = drawinfo.drawPlayer.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
                drawData.color = glowMaskColor;
                drawData.shader = drawinfo.cHead;
                drawinfo.DrawDataCache.Add(drawData);
            }
        }

        public static DrawData GetHeadGlowMask(ref PlayerDrawSet drawInfo, Texture2D glowMaskTexture, Color glowMaskColor) {
            Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
            bodyFrame.Width += 2;
            Vector2 helmetOffset = drawInfo.helmetOffset;
            if (drawInfo.drawPlayer.direction == -1) {
                helmetOffset.X -= 2f;
            }
            DrawData item = new(glowMaskTexture,
                helmetOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) +
                (float)(drawInfo.drawPlayer.width / 2)),
                (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height -
                (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect,
                bodyFrame, glowMaskColor, drawInfo.drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect) {
            };
            return item;
        }
    }

    private class CosmicHatHandler : ModPlayer {
        private float _timer, _timer2;
        private int _lastMana;

        public bool IsEffectActive;
        public bool IsEffectActive2;

        public override void Load() {
            On_Player.GetManaCost += On_Player_GetManaCost;
            On_Player.UpdateManaRegen += On_Player_UpdateManaRegen;
        }

        private void On_Player_UpdateManaRegen(On_Player.orig_UpdateManaRegen orig, Player self) {
            if (self.GetModPlayer<CosmicHatHandler>()._timer > 0f) {
                return;
            }

            orig(self);
        }

        private int On_Player_GetManaCost(On_Player.orig_GetManaCost orig, Player self, Item item) {
            if (self.GetModPlayer<CosmicHatHandler>()._timer > 0f) {
                return 0;
            }

            return orig(self, item);
        }

        public override void ResetEffects() {
            IsEffectActive = false;
        }

        public override void PostUpdateEquips() {
            if (!IsEffectActive) {
                return;
            }

            int cddebuff = ModContent.BuffType<CosmicHat_Cooldown>();
            int buff = ModContent.BuffType<CosmicHat_Buff>();
            Player player = Player;
            float min = 0.4f;
            if (_timer > 0f) {
                _timer--;
                if (_timer <= 0f) {
                    player.AddBuff(cddebuff, 420);
                }
                player.manaRegen = 0;
                player.manaRegenDelay = 0;
            }
            if (!IsEffectActive2 && _timer <= 0f) {
                if (!player.HasBuff(cddebuff) && (float)player.statMana < (float)player.statManaMax2 * min) {
                    IsEffectActive2 = true;
                    _timer = _timer2 = 180f;
                    _lastMana = player.statMana;

                    player.AddBuff(buff, (int)_timer2);

                    for (int i = 0; i < 3; i++) {
                        if (player.whoAmI == Main.myPlayer) {
                            Projectile.NewProjectile(player.GetSource_Misc("cosmichat"),
                                Player.Center.X, Player.Center.Y, 0f, 0f, ModContent.ProjectileType<CosmicMana>(), 0, 0f, player.whoAmI, i, 0f);
                        }
                        SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.5f, PitchVariance = 0.5f }, player.Center);
                    }
                }
            }
            else if (player.statMana > (float)player.statManaMax2 * min) {
                IsEffectActive2 = false;
            }
        }
    }

    public override void SetDefaults() {
        int width = 34; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 0, 50, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<CosmicHatHandler>().IsEffectActive = true;
    }
}
