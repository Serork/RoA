﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Utilities.Extensions;
using RoA.Content.Buffs;
using RoA.Core.Utility;

using System;
using System.Reflection;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Face)]
sealed class RavencallersCloak : ModItem {
	public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Ravencaller's Cloak");
        //Tooltip.SetDefault("While at full health, enemies are less likely to target you");
        if (Main.netMode != NetmodeID.Server) {
            int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Face);
            ArmorIDs.Face.Sets.PreventHairDraw[equipSlot] = true;
            ArmorIDs.Face.Sets.OverrideHelmet[equipSlot] = true;
        }

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 24; int height = width;
		Item.Size = new Vector2(width, height);

		Item.value = Item.sellPrice(silver: 80);
		Item.rare = ItemRarityID.Green;
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual) {
        RavencallerPlayer data = player.GetModPlayer<RavencallerPlayer>();
        data.RavencallersCloak = true;
        data.RavencallersCloakVisible = !hideVisual;
	}

    private class RavencallerPlayer : ModPlayer {
        private const float RESETTIME = 300f;

        private static MethodInfo? _drawPlayerInternal;

        private struct OldPositionInfo {
            public Vector2 Position, Velocity;
            public float Rotation;
            public Vector2 RotationOrigin;
            public int Direction;
            public Rectangle HeadFrame, BodyFrame, LegFrame;
            public int WingFrame;
            public float GfxOffY;
        }

        private bool _resetted = false;
        private OldPositionInfo[] _oldPositionInfos = new OldPositionInfo[20];
        private float _resetTime;
        private float _opacity;

        public bool RavencallersCloak { get; set; }
        public bool RavencallersCloakVisible { get; set; }

        public int ItemType => ModContent.ItemType<RavencallersCloak>();
        public int CloakFaceId => EquipLoader.GetEquipSlot(Mod, ItemLoader.GetItem(ItemType).Name, EquipType.Face);
        public bool ReceivedDamage => _resetTime > 0f;
        public bool Available => RavencallersCloak && !ReceivedDamage;
        public bool AvailableForNPCs => Available;

        public override void ResetEffects() {
            RavencallersCloak = RavencallersCloakVisible = false;
        }

        public override void OnHurt(Player.HurtInfo info) {
            _resetTime = RESETTIME;
            OldPositionInfo[] playerOldPositions = _oldPositionInfos;
            OldPositionInfo lastPositionInfo = playerOldPositions[^1];
            for (int i = 0; i < 25; i++) {
                int dust = Dust.NewDust(lastPositionInfo.Position, Player.width, Player.height, 240, lastPositionInfo.Velocity.X * 0.4f, lastPositionInfo.Velocity.Y * 0.4f, 110, default, 1.7f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity.X *= 4f;
                Main.dust[dust].velocity.Y *= 4f;
                Main.dust[dust].velocity = (Main.dust[dust].velocity + Player.velocity) / 2f;
            }

            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 0.5f, PitchVariance = 0.5f }, lastPositionInfo.Position);
            ResetPositions();
        }

        public void ResetPositions() {
            for (int j = 0; j < _oldPositionInfos.Length; j++) {
                _oldPositionInfos[j].Position = Vector2.Zero;
            }
        }

        public override void Load() {
            ResetPositions();

            if (Main.netMode == NetmodeID.Server) {
                return;
            }
            On_LegacyPlayerRenderer.DrawPlayerFull += On_LegacyPlayerRenderer_DrawPlayerFull;
            On_PlayerHeadDrawRenderTargetContent.DrawTheContent += On_PlayerHeadDrawRenderTargetContent_DrawTheContent;

            _drawPlayerInternal = typeof(LegacyPlayerRenderer).GetMethod("DrawPlayerInternal", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void Unload() {
            _drawPlayerInternal = null;
        }

        private void On_PlayerHeadDrawRenderTargetContent_DrawTheContent(On_PlayerHeadDrawRenderTargetContent.orig_DrawTheContent orig, PlayerHeadDrawRenderTargetContent self, SpriteBatch spriteBatch) {
            Player player = typeof(PlayerHeadDrawRenderTargetContent).GetFieldValue<Player>("_player", self);
            if (player != null && !player.ShouldNotDraw) {
                bool ravenCloak = player.face == CloakFaceId;
                if (ravenCloak) {
                    int head = player.head;
                    int face = player.face;
                    player.head = 0;
                    player.face = 0;
                    Main.PlayerRenderer.DrawPlayerHead(Main.Camera, player, new Vector2(84 * 0.5f, 84f * 0.5f));
                    player.head = head;
                    player.face = face;
                }
                else {
                    orig(self, spriteBatch);
                }
            }
        }

        private void On_LegacyPlayerRenderer_DrawPlayerFull(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Terraria.Graphics.Camera camera, Player drawPlayer) {
            RavencallerPlayer data = drawPlayer.GetModPlayer<RavencallerPlayer>();
            if (!data.Available || !drawPlayer.active || Main.gameMenu) {
                orig(self, camera, drawPlayer);
                _opacity = 0f;
                return;
            }

            int direction = drawPlayer.direction;
            Rectangle headFrame = drawPlayer.headFrame;
            Rectangle bodyFrame = drawPlayer.bodyFrame;
            Rectangle legFrame = drawPlayer.legFrame;
            int wingFrame = drawPlayer.wingFrame;
            int itemAnimation = drawPlayer.itemAnimation;
            int itemTime = drawPlayer.itemTime;
            int face = drawPlayer.face;
            float stealth = drawPlayer.stealth;
            int head = drawPlayer.head;
            bool shroomiteStealth = drawPlayer.shroomiteStealth;
            float gfxOffY = drawPlayer.gfxOffY;
            Color skinColor = drawPlayer.skinColor;
            Vector2 velocity = drawPlayer.velocity;
            if (!drawPlayer.ShouldNotDraw && !drawPlayer.dead) {
                OldPositionInfo[] playerOldPositions = data._oldPositionInfos;
                OldPositionInfo lastPositionInfo = playerOldPositions[^1];
                if (lastPositionInfo.Position != Vector2.Zero) {
                    if (_opacity < 1f) {
                        _opacity += 0.025f;
                    }
                    drawPlayer.velocity = lastPositionInfo.Velocity;
                    drawPlayer.direction = lastPositionInfo.Direction;
                    drawPlayer.headFrame = lastPositionInfo.HeadFrame;
                    drawPlayer.bodyFrame = lastPositionInfo.BodyFrame;
                    drawPlayer.legFrame = lastPositionInfo.LegFrame;
                    drawPlayer.wingFrame = lastPositionInfo.WingFrame;
                    drawPlayer.itemAnimation = drawPlayer.itemTime = 0;
                    drawPlayer.head = 0;
                    drawPlayer.face = CloakFaceId;
                    drawPlayer.shroomiteStealth = true;
                    drawPlayer.stealth = 0.5f * _opacity;
                    drawPlayer.gfxOffY = lastPositionInfo.GfxOffY;
                    drawPlayer.skinColor = Color.Transparent;
                    SamplerState samplerState = camera.Sampler;
                    if (drawPlayer.mount.Active && drawPlayer.fullRotation != 0f) {
                        samplerState = LegacyPlayerRenderer.MountedSamplerState;
                    }
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, camera.Rasterizer, null, camera.GameViewMatrix.TransformationMatrix);
                    _drawPlayerInternal.Invoke(self, [camera, drawPlayer, lastPositionInfo.Position, lastPositionInfo.Rotation, lastPositionInfo.RotationOrigin, -1f, 1f, 1f, false]);
                    //self.DrawPlayer(camera, drawPlayer, lastPositionInfo.Position, lastPositionInfo.Rotation, lastPositionInfo.RotationOrigin, 0f);
                    Main.spriteBatch.End();
                }
            }
            drawPlayer.direction = direction;
            drawPlayer.headFrame = headFrame;
            drawPlayer.bodyFrame = bodyFrame;
            drawPlayer.legFrame = legFrame;
            drawPlayer.wingFrame = wingFrame;
            drawPlayer.itemAnimation = itemAnimation;
            drawPlayer.itemTime = itemTime;
            drawPlayer.face = face;
            drawPlayer.stealth = stealth;
            drawPlayer.head = head;
            drawPlayer.shroomiteStealth = shroomiteStealth;
            drawPlayer.gfxOffY = gfxOffY;
            drawPlayer.skinColor = skinColor;
            drawPlayer.velocity = velocity;
            orig(self, camera, drawPlayer);
        }

        public override void PostUpdate() {
            int buffType = ModContent.BuffType<RavencallersCloakBuff>();
            if (RavencallersCloak || Main.mouseItem.type == ItemType) {
                if (ReceivedDamage) {
                    _resetTime -= 1f;
                    return;
                }

                Player.AddBuff(buffType, 10);

                _resetted = false;

                for (int num2 = _oldPositionInfos.Length - 1; num2 > 0; num2--) {
                    _oldPositionInfos[num2] = _oldPositionInfos[num2 - 1];
                }
                _oldPositionInfos[0].Position = Player.position;
                _oldPositionInfos[0].Velocity = Player.velocity;
                _oldPositionInfos[0].Rotation = Player.fullRotation;
                _oldPositionInfos[0].RotationOrigin = Player.fullRotationOrigin;
                _oldPositionInfos[0].Direction = Player.direction;
                _oldPositionInfos[0].HeadFrame = Player.headFrame;
                _oldPositionInfos[0].BodyFrame = Player.bodyFrame;
                _oldPositionInfos[0].LegFrame = Player.legFrame;
                _oldPositionInfos[0].WingFrame = Player.wingFrame;
            }
            else {
                if (!_resetted) {
                    if (Player.FindBuff(buffType, out int buffIndex)) {
                        Player.DelBuff(buffIndex);
                    }

                    ResetPositions();
                    _resetted = true;
                }
            }
        }

        private class NPCTargetSystem : GlobalNPC {
            private readonly static Vector2[] _before = new Vector2[256];

            public override bool PreAI(NPC npc) {
                foreach (Player player in Main.ActivePlayers) {
                    RavencallerPlayer data = player.GetModPlayer<RavencallerPlayer>();
                    if (data.AvailableForNPCs) {
                        _before[player.whoAmI] = player.Center;
                        OldPositionInfo[] playerOldPositions = data._oldPositionInfos;
                        OldPositionInfo lastPositionInfo = playerOldPositions[^1];
                        Vector2 position = lastPositionInfo.Position;
                        if (position != Vector2.Zero) {
                            player.Center = lastPositionInfo.Position;
                        }

                        return true;
                    }
                }

                return base.PreAI(npc);
            }

            public override void PostAI(NPC npc) {
                foreach (Player player in Main.ActivePlayers) {
                    RavencallerPlayer data = player.GetModPlayer<RavencallerPlayer>();
                    if (data.AvailableForNPCs) {
                        player.Center = _before[player.whoAmI];
                    }
                }
            }
        }
    }
}