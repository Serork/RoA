﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Wreath;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Utilities.Extensions;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Forms;
sealed class BaseFormHandler : ModPlayer {
    public record FormInfo {
        public BaseForm BaseForm { get; init; }
        public Predicate<Player> ShouldBeActive { get; init; }

        public FormInfo(BaseForm baseForm, Predicate<Player> shouldBeActive = null) {
            BaseForm = baseForm;
            ShouldBeActive = shouldBeActive;
        }

        public BaseFormBuff MountBuff => BaseForm.MountBuff;
    }

    private PlayerDrawLayer[] _layers;
    private bool _shouldClear;

    private static readonly Dictionary<Type, FormInfo> _formsByType = [];
    private FormInfo _currentForm;
    private bool _shouldBeActive;
    private bool _startDecreasingWreath;

    public static IReadOnlyCollection<FormInfo> Forms => _formsByType.Values;
    public FormInfo CurrentForm => _currentForm;
    public bool IsInDruidicForm => CurrentForm != null;
    public bool HasDruidArmorSet => _shouldBeActive == true;

    public bool UsePlayerSpeed { get; internal set; }

    public string Serialize() => CurrentForm.BaseForm.GetType().FullName;
    public FormInfo Deserialize(string typeName) => _formsByType[Type.GetType(typeName)];

    internal void InternalSetCurrentForm<T>(T formInstance) where T : FormInfo {
        if (formInstance != null) {
            _currentForm = formInstance;

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new SpawnFormPacket(Player, Serialize()));
            }
        }
    }

    public bool IsConsideredAs<T>() where T : BaseForm => IsInDruidicForm && CurrentForm.BaseForm.GetType().Equals(typeof(T));

    public bool ShouldFormBeActive<T>(T instance = null) where T : FormInfo {
        T formInstance = instance ?? GetForm<T>();
        bool flag = formInstance.ShouldBeActive != null;
        return (!flag && _shouldBeActive) || (flag && formInstance.ShouldBeActive(Player));
    }

    public static void KeepFormActive(Player player) => player.GetModPlayer<BaseFormHandler>().KeepFormActive();

    public static T GetFormInstance<T>() where T : BaseForm {
        if (_formsByType.TryGetValue(typeof(T), out FormInfo form)) {
            return (T)form.BaseForm;
        }

        return default;
    }

    public static T GetForm<T>() where T : FormInfo {
        if (_formsByType.TryGetValue(typeof(T), out FormInfo form)) {
            return (T)form;
        }

        return default;
    }

    public static void RegisterForm<T>(Predicate<Player> active = null) where T : BaseForm => _formsByType.TryAdd(typeof(T), new(ModContent.GetInstance<T>(), active));

    public static void ApplyForm<T>(Player player, T instance = null) where T : FormInfo {
        BaseFormHandler handler = player.GetModPlayer<BaseFormHandler>();
        if (handler.IsInDruidicForm) {
            return;
        }

        T formInstance = instance ?? GetForm<T>();
        if (!player.GetModPlayer<WreathHandler>().IsFull6) {
            player.GetModPlayer<WreathHandler>().SlowlyActivateForm(formInstance);
            return;
        }

        if (player.whoAmI == Main.myPlayer) {
            SoundEngine.PlaySound(formInstance.BaseForm.ApplySound, player.Center);
        }

        if (formInstance is null) {
            return;
        }

        player.GetModPlayer<WreathHandler>().Reset(true, 0.1f);
        player.AddBuffInStart(formInstance.MountBuff.Type, 3600);
        handler.InternalSetCurrentForm(formInstance);

        if (player.heldProj != -1) {
            Main.projectile[player.heldProj].Kill();
            player.heldProj = -1;
        }
        player.itemAnimation = player.itemTime = 0;
        player.reuseDelay = 0;
    }

    public static void ReleaseForm<T>(Player player, T instance = null) where T : FormInfo {
        BaseFormHandler handler = player.GetModPlayer<BaseFormHandler>();
        T formInstance = instance ?? GetForm<T>();
        if (formInstance != null) {
            player.ClearBuff(formInstance.MountBuff.Type);
            player.GetModPlayer<WreathHandler>().Reset(true);
            player.GetModPlayer<WreathHandler>().OnResetEffects();

            if (player.whoAmI == Main.myPlayer) {
                SoundEngine.PlaySound(formInstance.BaseForm.ReleaseSound, player.Center);
            }
        }
        handler.HardResetActiveForm();
    }

    public static void ToggleForm<T>(Player player, T instance = null) where T : FormInfo {
        if (player.ItemAnimationActive) {
            return;
        }

        BaseFormHandler handler = player.GetModPlayer<BaseFormHandler>();
        T formInstance = instance ?? GetForm<T>();
        if (!handler.ShouldFormBeActive(formInstance)) {
            return;
        }

        if (handler.IsInDruidicForm) {
            ReleaseForm(player, formInstance);

            return;
        }

        ApplyForm(player, formInstance);
    }

    public static void ToggleForm<T>(Player player) where T : BaseForm => ToggleForm(player, _formsByType[typeof(T)]);

    public override void ResetEffects() {
        ResetActiveForm();
    }

    public override void PostUpdateRunSpeeds() { }

    public override void PostUpdate() {
        UsePlayerSpeed = false;
    }

    public override void PostUpdateEquips() {
        ClearForm();
        MakePlayerUnavailableToUseItems();
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
        if (!IsInDruidicForm) {
            return;
        }
        SoundStyle? hurtSound = _currentForm.BaseForm.HurtSound;
        if (!hurtSound.HasValue) {
            return;
        }
        modifiers.DisableSound();
        SoundEngine.PlaySound(_currentForm.BaseForm.HurtSound, Player.Center);
    }

    private void KeepFormActive() => _shouldBeActive = true;

    private void ResetActiveForm() => _shouldBeActive = false;
    internal void HardResetActiveForm() => _currentForm = null;

    private void MakePlayerUnavailableToUseItems() {
        if (!IsInDruidicForm) {
            return;
        }

        _shouldClear = true;

        Player.controlUseItem = false;
    }

    private void ClearForm() {
        if (IsInDruidicForm && Player.mount.Active && Player.mount._type < MountID.Count) {
            ReleaseForm(Player, _currentForm);
        }

        if (Player.mount.Active && (!_shouldClear || !IsInDruidicForm || _shouldBeActive)) {
            return;
        }

        if (!Player.mount.Active || !ShouldFormBeActive(_currentForm)) {
            ReleaseForm(Player, _currentForm);
        }

        _shouldClear = false;
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
        if (!IsInDruidicForm) {
            return;
        }

        _layers = PlayerDrawLayerLoader.GetDrawLayers(drawInfo);
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo) {
        if (!IsInDruidicForm || _layers == null) {
            return;
        }

        foreach (PlayerDrawLayer layer in _layers) {
            if (layer.FullName.Contains("Terraria") && !layer.FullName.Contains("Mount")) {
                layer.Hide();
            }
        }
    }

    public override void Load() {
        On_TileObject.DrawPreview += On_TileObject_DrawPreview;
        On_Main.DrawInterface_40_InteractItemIcon += On_Main_DrawInterface_40_InteractItemIcon;
        On_Player.QuickMount += On_Player_QuickMount;
        On_Player.QuickMount_GetItemToUse += On_Player_QuickMount_GetItemToUse;
        On_Player.MakeFloorDust += On_Player_MakeFloorDust;
        On_PlayerHeadDrawRenderTargetContent.DrawTheContent += On_PlayerHeadDrawRenderTargetContent_DrawTheContent;
        On_Player.RotatedRelativePoint += On_Player_RotatedRelativePoint;
    }

    private Vector2 On_Player_RotatedRelativePoint(On_Player.orig_RotatedRelativePoint orig, Player self, Vector2 pos, bool reverseRotation, bool addGfxOffY) {
        BaseFormHandler handler = self.GetModPlayer<BaseFormHandler>();
        if (handler.IsInDruidicForm) {
            Vector2 result = pos;
            return result;
        }

        return orig(self, pos, reverseRotation, addGfxOffY);
    }

    private void On_PlayerHeadDrawRenderTargetContent_DrawTheContent(On_PlayerHeadDrawRenderTargetContent.orig_DrawTheContent orig, PlayerHeadDrawRenderTargetContent self, SpriteBatch spriteBatch) {
        Player player = typeof(PlayerHeadDrawRenderTargetContent).GetFieldValue<Player>("_player", self);
        if (player != null && !player.ShouldNotDraw) {
            BaseFormHandler handler = player.GetModPlayer<BaseFormHandler>();
            if (handler.IsInDruidicForm) {
                Texture2D texture = ModContent.Request<Texture2D>(handler.CurrentForm.BaseForm.Texture + "_Head").Value;
                Vector2 position = new(84f * 0.5f, 84f * 0.5f);
                position.X -= 6f;
                position.Y -= 4f;
                position.Y -= player.HeightMapOffset;
                if (position.Y == 38) {
                    position.Y = 48;
                }
                Vector2 origin = texture.Size() / 2f;
                spriteBatch.Draw(texture, position + origin - Vector2.UnitY * player.height / 2f, null, Color.White, 0f, origin, 1f, (SpriteEffects)(player.direction != 1).ToInt(), 0f);
            }
            else {
                orig(self, spriteBatch);
            }
        }
    }

    private void On_Player_MakeFloorDust(On_Player.orig_MakeFloorDust orig, Player self, bool Falling, int type, int paintColor) {
        if (type == 659 || type == 667) {
            bool flag = true;
            if (!Falling) {
                float num = Math.Abs(self.velocity.X) / 3f;
                if ((float)Main.rand.Next(100) > num * 50f)
                    flag = false;
            }

            if (!flag)
                return;

            Vector2 positionInWorld = new Vector2(self.position.X, self.position.Y + (float)self.height - 2f) + new Vector2((float)self.width * Main.rand.NextFloat(), 6f * Main.rand.NextFloat());
            Vector2 movementVector = Main.rand.NextVector2Circular(0.8f, 0.8f);
            if (movementVector.Y > 0f)
                movementVector.Y *= -1f;

            ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.ShimmerBlock, new ParticleOrchestraSettings {
                PositionInWorld = positionInWorld,
                MovementVector = movementVector
            }, self.whoAmI);
        }

        if (TileLoader.HasWalkDust(type)) { }
        else
        if (type != 147 && type != 25 && type != 53 && type != 189 && type != 0 && type != 123 && type != 57 && type != 112 && type != 116 && type != 196 && type != 193 && type != 195 && type != 197 && type != 199 && type != 229 && type != 234 && type != 371 && type != 460 && type != 666)
            return;

        int num2 = 1;
        if (Falling) {
            num2 = 20;
            if (type == 666)
                SoundEngine.PlaySound(SoundID.Item177, self.Center);
        }

        for (int i = 0; i < num2; i++) {
            bool flag2 = true;
            int num3 = 76;
            if (type == 666) {
                if (paintColor != 0)
                    break;

                num3 = 322;
            }

            if (type == 53)
                num3 = 32;

            if (type == 189)
                num3 = 16;

            if (type == 0)
                num3 = 0;

            if (type == 123)
                num3 = 53;

            if (type == 57)
                num3 = 36;

            if (type == 112)
                num3 = 14;

            if (type == 234)
                num3 = 122;

            if (type == 116)
                num3 = 51;

            if (type == 196)
                num3 = 108;

            if (type == 193)
                num3 = 4;

            if (type == 195 || type == 199)
                num3 = 5;

            if (type == 197)
                num3 = 4;

            if (type == 229)
                num3 = 153;

            if (type == 371)
                num3 = 243;

            if (type == 460)
                num3 = 108;

            if (type == 25)
                num3 = 37;

            if (num3 == 32 && Main.rand.Next(2) == 0)
                flag2 = false;

            if (num3 == 14 && Main.rand.Next(2) == 0)
                flag2 = false;

            if (num3 == 51 && Main.rand.Next(2) == 0)
                flag2 = false;

            if (num3 == 36 && Main.rand.Next(2) == 0)
                flag2 = false;

            if (num3 == 0 && Main.rand.Next(3) != 0)
                flag2 = false;

            // Patch note: num3 & flag2 are used below.
            if (num3 == 53 && Main.rand.Next(3) != 0)
                flag2 = false;

            Color newColor = default(Color);
            if (type == 193)
                newColor = new Color(30, 100, 255, 100);

            if (type == 197)
                newColor = new Color(97, 200, 255, 100);

            if (type == 460)
                newColor = new Color(100, 150, 130, 100);

            TileLoader.WalkDust(type, ref num3, ref flag2, ref newColor);

            if (!Falling) {
                float num4 = Math.Abs(self.velocity.X) / 3f;
                if ((float)Main.rand.Next(100) > num4 * 100f)
                    flag2 = false;
            }

            if (!flag2)
                continue;

            float num5 = self.velocity.X;
            if (num5 > 6f)
                num5 = 6f;

            if (num5 < -6f)
                num5 = -6f;

            if (!(self.velocity.X != 0f || Falling))
                continue;

            if (self.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
                num3 = self.mount._data.spawnDust;
            }

            int num6 = Dust.NewDust(new Vector2(self.position.X, self.position.Y + (float)self.height - 2f), self.width, 6, num3, 0f, 0f, 50, newColor);
            if (self.gravDir == -1f)
                Main.dust[num6].position.Y -= self.height + 4;

            if (num3 == 76) {
                Main.dust[num6].scale += (float)Main.rand.Next(3) * 0.1f;
                Main.dust[num6].noLight = true;
            }

            if (num3 == 16 || num3 == 108 || num3 == 153)
                Main.dust[num6].scale += (float)Main.rand.Next(6) * 0.1f;

            if (num3 == 37) {
                Main.dust[num6].scale += 0.25f;
                Main.dust[num6].alpha = 50;
            }

            if (num3 == 5)
                Main.dust[num6].scale += (float)Main.rand.Next(2, 8) * 0.1f;

            Main.dust[num6].noGravity = true;
            if (num3 == 322) {
                if (Main.rand.Next(4) == 0) {
                    Main.dust[num6].noGravity = false;
                    Main.dust[num6].scale *= 1.1f;
                }
                else {
                    Main.dust[num6].scale *= 1.2f;
                }
            }

            if (num2 > 1) {
                Main.dust[num6].velocity.X *= 1.2f;
                Main.dust[num6].velocity.Y *= 0.8f;
                Main.dust[num6].velocity.Y -= 1f;
                Main.dust[num6].velocity *= 0.8f;
                Main.dust[num6].scale += (float)Main.rand.Next(3) * 0.1f;
                Main.dust[num6].velocity.X = (Main.dust[num6].position.X - (self.position.X + (float)(self.width / 2))) * 0.2f;
                if (Main.dust[num6].velocity.Y > 0f)
                    Main.dust[num6].velocity.Y *= -1f;

                Main.dust[num6].velocity.X += num5 * 0.3f;
            }
            else {
                Main.dust[num6].velocity *= 0.2f;
            }

            Main.dust[num6].position.X -= num5 * 1f;
            if (self.gravDir == -1f)
                Main.dust[num6].velocity.Y *= -1f;
        }
    }

    private void On_Player_QuickMount(On_Player.orig_QuickMount orig, Player self) {
        if (Main.LocalPlayer.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
            return;
        }

        orig(self);
    }

    private Item On_Player_QuickMount_GetItemToUse(On_Player.orig_QuickMount_GetItemToUse orig, Player self) {
        if (Main.LocalPlayer.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
            return null;
        }

        return orig(self);
    }

    private void On_Main_DrawInterface_40_InteractItemIcon(On_Main.orig_DrawInterface_40_InteractItemIcon orig, Main self) {
        if (Main.player[Main.myPlayer].cursorItemIconID <= 0 && Main.LocalPlayer.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
            return;
        }

        orig(self);
    }

    private void On_TileObject_DrawPreview(On_TileObject.orig_DrawPreview orig, Microsoft.Xna.Framework.Graphics.SpriteBatch sb, TileObjectPreviewData op, Vector2 position) {
        if (Main.LocalPlayer.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
            return;
        }

        orig(sb, op, position);
    }
}
