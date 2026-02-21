using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Utilities.Extensions;
using RoA.Content.Buffs;
using RoA.Core.Utility;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.Graphics;
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
        int width = 24; int height = 34;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 1, 0, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        RavencallerPlayer data = player.GetModPlayer<RavencallerPlayer>();
        data.RavencallersCloak = true;
        data.RavencallersCloakVisible = !hideVisual;
    }

    // also see NPCTargetting.cs
    public sealed class RavencallerPlayer : ModPlayer {
        private const float RESETTIME = 300f;

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawPlayerInternal")]
        public extern static void LegacyPlayerRenderer_DrawPlayerInternal(LegacyPlayerRenderer playerRenderer, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow = 0f, float alpha = 1f, float scale = 1f, bool headOnly = false);

        public struct OldPositionInfo {
            public Vector2 Position, Velocity;
            public float Rotation;
            public Vector2 RotationOrigin;
            public int Direction;
            public Rectangle HeadFrame, BodyFrame, LegFrame;
            public int WingFrame;
            public float GfxOffY;
            public int Step;
            public float StepSpeed;
            public int MountFrame;
        }

        private bool _resetted = false, _resetted2 = false;
        public OldPositionInfo[] OldPositionInfos { get; private set; } = new OldPositionInfo[20];
        private float _resetTime;
        private float _opacity;

        public bool RavencallersCloak { get; set; }
        public bool RavencallersCloakVisible { get; set; }

        public int ItemType => ModContent.ItemType<RavencallersCloak>();
        public int CloakFaceId => EquipLoader.GetEquipSlot(Mod, ItemLoader.GetItem(ItemType).Name, EquipType.Face);
        public bool ReceivedDamage => _resetTime > 0f;
        public bool Available => RavencallersCloak && !ReceivedDamage;
        public bool AvailableForNPCs => Available && _opacity > 0.5f;

        public override void ResetEffects() {
            RavencallersCloak = RavencallersCloakVisible = false;
        }

        public override void OnHurt(Player.HurtInfo info) {
            _resetTime = RESETTIME;
            OldPositionInfo[] playerOldPositions = OldPositionInfos;
            OldPositionInfo lastPositionInfo = playerOldPositions[^1];
            for (int i = 0; i < 25; i++) {
                int dust = Dust.NewDust(lastPositionInfo.Position, Player.width, Player.height, 240, lastPositionInfo.Velocity.X * 0.4f, lastPositionInfo.Velocity.Y * 0.4f, 110, default, 1.7f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity.X *= 4f;
                Main.dust[dust].velocity.Y *= 4f;
                Main.dust[dust].velocity = (Main.dust[dust].velocity + Player.velocity) / 2f;
            }

            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 0.35f, Pitch = -0.35f, PitchVariance = Main.rand.NextFloat() * 0.1f, }, lastPositionInfo.Position);

            ResetPositions();
        }

        public void ResetPositions(bool resetPositions = true) {
            if (resetPositions) {
                for (int j = 0; j < OldPositionInfos.Length; j++) {
                    OldPositionInfos[j].Position = Vector2.Zero;
                }
            }
            _opacity = 0f;
        }

        public override void Load() {
            ResetPositions();

            On_Mount.SetMount += On_Mount_SetMount;
            On_Mount.Dismount += On_Mount_Dismount;
            On_LegacyPlayerRenderer.DrawPlayerFull += On_LegacyPlayerRenderer_DrawPlayerFull;
            On_PlayerHeadDrawRenderTargetContent.DrawTheContent += On_PlayerHeadDrawRenderTargetContent_DrawTheContent;
        }

        private void On_Mount_Dismount(On_Mount.orig_Dismount orig, Mount self, Player mountedPlayer) {
            if (self != null && self._active) {
                mountedPlayer.GetModPlayer<RavencallerPlayer>().ResetPositions(false);
            }
            orig(self, mountedPlayer);
        }

        private void On_Mount_SetMount(On_Mount.orig_SetMount orig, Mount self, int m, Player mountedPlayer, bool faceLeft) {
            if (self != null && !self._active) {
                mountedPlayer.GetModPlayer<RavencallerPlayer>().ResetPositions(false);
            }
            orig(self, m, mountedPlayer, faceLeft);
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
                    Main.PlayerRenderer.DrawPlayerHead(Main.Camera, player, new Vector2(84f * 0.5f, 84f * 0.5f));
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
            if (!data.Available || !drawPlayer.active || Main.gameMenu || drawPlayer.isDisplayDollOrInanimate) {
                orig(self, camera, drawPlayer);
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
            Vector2 position = drawPlayer.position;
            Vector2 velocity = drawPlayer.velocity;
            int step = drawPlayer.step;
            float stepSpeed = drawPlayer.stepSpeed;
            float fullRotation = drawPlayer.fullRotation;
            Vector2 fullRotationOrigin = drawPlayer.fullRotationOrigin;
            int mountFrame = drawPlayer.mount._frame;
            if (!drawPlayer.ShouldNotDraw && !drawPlayer.dead) {
                OldPositionInfo[] playerOldPositions = data.OldPositionInfos;
                OldPositionInfo lastPositionInfo = playerOldPositions[^1];
                if (lastPositionInfo.Position != Vector2.Zero) {
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
                    drawPlayer.stealth = 0.5f * data._opacity;
                    drawPlayer.gfxOffY = lastPositionInfo.GfxOffY;
                    drawPlayer.skinColor = Color.Transparent;
                    drawPlayer.step = lastPositionInfo.Step;
                    drawPlayer.stepSpeed = lastPositionInfo.StepSpeed;
                    if (!drawPlayer.mount.Active) {
                        //drawPlayer.fullRotation = lastPositionInfo.Rotation;
                        //drawPlayer.fullRotationOrigin = lastPositionInfo.RotationOrigin;
                    }
                    else {
                        drawPlayer.mount._frame = lastPositionInfo.MountFrame;
                    }
                    SamplerState samplerState = camera.Sampler;
                    if (drawPlayer.mount.Active && drawPlayer.fullRotation != 0f) {
                        samplerState = LegacyPlayerRenderer.MountedSamplerState;
                    }
                    bool flag = drawPlayer.legFrame.Y == 336;
                    float offsetY = -drawPlayer.HeightOffsetHitboxCenter;
                    if (flag) {
                        if (!drawPlayer.mount.Active) {
                            offsetY -= drawPlayer.height / 2f;
                            offsetY += 6f;
                        }
                        else {
                            offsetY = 0f;
                        }
                    }
                    else {

                    }
                    drawPlayer.position = lastPositionInfo.Position + Vector2.UnitY * offsetY;
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, camera.Rasterizer, null, camera.GameViewMatrix.TransformationMatrix);
                    LegacyPlayerRenderer_DrawPlayerInternal(self, camera, drawPlayer, drawPlayer.position + new Vector2(0f, drawPlayer.gfxOffY), drawPlayer.fullRotation, drawPlayer.fullRotationOrigin, -1f, 1f, 1f, false);
                    Main.spriteBatch.End();
                }
            }
            drawPlayer.position = position;
            drawPlayer.velocity = velocity;
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
            drawPlayer.step = step;
            drawPlayer.stepSpeed = stepSpeed;
            drawPlayer.fullRotation = fullRotation;
            drawPlayer.fullRotationOrigin = fullRotationOrigin;
            drawPlayer.mount._frame = mountFrame;
            orig(self, camera, drawPlayer);
        }

        public override void UpdateEquips() {
            int buffType = ModContent.BuffType<RavencallersCloakBuff>();
            if (RavencallersCloak || Main.mouseItem.type == ItemType) {
                if (ReceivedDamage) {
                    _resetTime -= 1f;
                    return;
                }

                if (!Player.FindBuff(buffType, out int buffIndex)) {
                    SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap with { Volume = 0.6f, Pitch = 0.2f }, Player.Center);
                }

                Player.AddBuff(buffType, 10);

                _resetted = false;

                if (!_resetted2) {
                    for (int num2 = OldPositionInfos.Length - 1; num2 > 0; num2--) {
                        OldPositionInfos[num2] = OldPositionInfos[num2 - 1];
                    }
                    OldPositionInfos[0].Position = Player.position;
                    OldPositionInfos[0].Velocity = Player.velocity;
                    OldPositionInfos[0].Rotation = Player.fullRotation;
                    OldPositionInfos[0].RotationOrigin = Player.fullRotationOrigin;
                    OldPositionInfos[0].Direction = Player.direction;
                    OldPositionInfos[0].HeadFrame = Player.headFrame;
                    OldPositionInfos[0].BodyFrame = Player.bodyFrame;
                    OldPositionInfos[0].LegFrame = Player.legFrame;
                    OldPositionInfos[0].WingFrame = Player.wingFrame;
                    OldPositionInfos[0].GfxOffY = Player.gfxOffY;
                    OldPositionInfos[0].Step = Player.step;
                    OldPositionInfos[0].StepSpeed = Player.stepSpeed;
                    OldPositionInfos[0].MountFrame = Player.mount._frame;
                }
                else {
                    _resetted2 = false;
                }

                if (Player.velocity.Length() > 0.5f) {
                    if (_opacity < 1f) {
                        _opacity += 0.035f;
                    }
                }
                else {
                    if (_opacity > 0f) {
                        _opacity -= 0.035f;
                    }
                }
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
    }
}