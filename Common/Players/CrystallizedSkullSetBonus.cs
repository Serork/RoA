using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ReLogic.Content;

using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    //public static ushort BUFFTIMEMAX => 0; // we use 5 minutes if BUFFTIMEMAX value is zero
    public static ushort USECHECKTIME => 0; // we use useTime * 1.5 if USECHECKTIME value is zero

    private struct CrystalInfo(Vector2 offset, bool secondFrame, float extraRotation = 0f, Color? color = null, byte? colorIndex = null) {
        private float _opacity = 0f;

        public readonly Vector2 Offset = offset;
        public readonly bool SecondFrame = secondFrame;
        public readonly float ExtraRotation = extraRotation;
        public readonly Color Color = color ?? Color.White;
        public readonly byte ColorIndex = colorIndex ?? 0;

        public float Opacity {
            readonly get => _opacity;
            set => _opacity = MathUtils.Clamp01(value);    
        }
    }

    private static Asset<Texture2D> _crystalOnPlayerTexture = null!;

    private bool _initializingCrystals = true;
    private CrystalInfo[] _crystalData = null!;
    private ushort _stoppedUsingManaFor;
    private float _crystalAlphaOpacity;

    public bool ShouldDrawCrystals() => Player.statMana < 0 && !_initializingCrystals;
    public ushort GetUseCheckTime() => USECHECKTIME == 0 ? Math.Max((ushort)30, (ushort)(Player.itemAnimationMax * 1.5f)) : USECHECKTIME;

    public bool ApplyCrystallizedSkullSetBonus;

    public override void PostItemCheck() {
        if (!ApplyCrystallizedSkullSetBonus) {
            return;
        }

        Player self = Player;
        float target = 1f;
        if (!(!self.HasBuff<Crystallized>() && self.manaRegenDelay > 0f)) {
            target = 0f;
        }
        _crystalAlphaOpacity = Helper.Approach(_crystalAlphaOpacity, target, TimeSystem.LogicDeltaTime);

        if (self.statMana < 0 && !self.ItemAnimationActive) {
            ref ushort stoppedUsingManaFor = ref self.GetCommon()._stoppedUsingManaFor;
            if (stoppedUsingManaFor > 0) {
                stoppedUsingManaFor--;
                if (stoppedUsingManaFor <= 0) {
                    self.AddBuff<Crystallized>(2/*(int)((BUFFTIMEMAX == 0 ? 3000 : BUFFTIMEMAX) * (Math.Abs(self.statMana) / (float)self.statManaMax2))*/);
                }
            }
        }
        //if (self.statMana >= 0 && self.FindBuff<Crystallized>(out int buffIndex) && self.buffTime[buffIndex] >= 2) {
        //    self.buffTime[buffIndex] = 1;
        //}
    }

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _crystalOnPlayerTexture = ModContent.Request<Texture2D>(ResourceManager.FriendlyMiscProjectiles + "ManaCrystal");
    }

    public partial void CrystallizedSkullLoad() {
        On_Player.UpdateManaRegen += On_Player_UpdateManaRegen;
        On_Player.CheckMana_int_bool_bool += On_Player_CheckMana_int_bool_bool;
        On_Player.CheckMana_Item_int_bool_bool += On_Player_CheckMana_Item_int_bool_bool;
        On_Player.ApplyLifeAndOrMana += On_Player_ApplyLifeAndOrMana;

        ExtraDrawLayerSupport.PreBackpackDrawEvent += ExtraDrawLayerSupport_PreBackpackDrawEvent;
        PreUpdateEvent += PlayerCommon_PreUpdateEvent;

        On_PlayerDrawSet.BoringSetup_2 += On_PlayerDrawSet_BoringSetup_2;
    }

    private void On_PlayerDrawSet_BoringSetup_2(On_PlayerDrawSet.orig_BoringSetup_2 orig, ref PlayerDrawSet self, Player player, List<DrawData> drawData, List<int> dust, List<int> gore, Vector2 drawPosition, float shadowOpacity, float rotation, Vector2 rotationOrigin) {
        bool reset = false;
        if (((player.HasBuff<Crystallized>() && player.statMana < 0) || (player.mount.Type == MountID.Drill))) {
            player.noItems = false;
            reset = true;
        }

        orig(ref self, player, drawData, dust, gore, drawPosition, shadowOpacity, rotation, rotationOrigin);

        if (reset) {
            player.noItems = true;
        }
    }

    private void On_Player_ApplyLifeAndOrMana(On_Player.orig_ApplyLifeAndOrMana orig, Player self, Item item) {
        if (self.GetCommon().ApplyCrystallizedSkullSetBonus) {
            int previousMana = self.statMana;
            orig(self, item);
            //if (previousMana < 0 && !self.HasBuff<Crystallized>()) {
            //    self.AddBuff<Crystallized>((int)(BUFFTIMEMAX * (Math.Abs(previousMana) / (float)self.statManaMax2)));
            //}

            return;
        }

        orig(self, item);
    }

    private static void PlayerCommon_PreUpdateEvent(Player player) {
        var handler = player.GetCommon();
        void breakCrystal(int index) {
            ref var info = ref handler._crystalData[index];
            if (info.Opacity == 0f) {
                return;
            }

            // here we spawn dusts and gores
            SoundEngine.PlaySound(SoundID.Item27, player.Center);
            if (!Main.dedServ) {
                int goreCount = (int)(info.Opacity * 6);
                for (int i = 0; i < goreCount; i++) {
                    float rotation = info.ExtraRotation;
                    int direction = (int)(player.direction * player.gravDir);
                    bool facedLeft = direction == -1;
                    bool reversedGravity = player.gravDir < 0f;
                    if (facedLeft) {
                        rotation = MathHelper.TwoPi - rotation;
                        if (reversedGravity) {
                            rotation += MathHelper.Pi;
                        }
                    }
                    else {
                        if (reversedGravity) {
                            rotation += MathHelper.Pi;
                        }
                    }
                    if (index == 2) {
                        rotation -= 0.15f * player.direction;
                    }
                    if (index == 1) {
                        rotation += 0.075f * player.direction;
                    }
                    if (index == 0) {
                        rotation += 0.3f * player.direction;
                    }
                    float y = -player.width * 2f * Main.rand.NextFloat(0.5f, 1.375f);
                    Vector2 vector3 = (new Vector2(0f, y) + info.Offset * info.Opacity * new Vector2(direction, 1f)).RotatedBy(rotation);
                    int currentIndex = i + 1;
                    float progress = currentIndex / goreCount;
                    Vector2 gorePosition = player.GetPlayerCorePoint() + Main.rand.RandomPointInArea(6f) + vector3 +
                        (Vector2.UnitY * 42f * 0.75f).RotatedBy(rotation);
                    int gore = Gore.NewGore(player.GetSource_Misc("manacrystalgore"),
                        gorePosition,
                        Vector2.One.RotatedBy(currentIndex * MathHelper.TwoPi / goreCount) * 2f, ModContent.Find<ModGore>(RoA.ModName + $"/ManaCrystalGore").Type, 1f);
                    Main.gore[gore].velocity *= 0.5f;
                    Main.gore[gore].frameCounter = info.ColorIndex;
                    gorePosition = player.GetPlayerCorePoint() + Main.rand.RandomPointInArea(6f) + vector3 + (Vector2.UnitY * 42f * 0.75f).RotatedBy(rotation);

                    Color color = info.Color;
                    for (int k = 0; k < 4; k++) {
                        int dustType = ModContent.DustType<TintableDustGlow>();
                        Dust dust = Dust.NewDustPerfect(gorePosition, dustType, Vector2.One.RotatedBy(currentIndex * MathHelper.TwoPi / goreCount) * 2f,
                            0, color * 0.9f, 1f + Main.rand.NextFloatRange(0.1f));
                        dust.velocity *= 0.5f;
                        dust.customData = 1f;
                        dust.noLightEmittence = true;
                        dust.noGravity = true;
                        dust.fadeIn = 1f * Main.rand.NextFloat(0.25f, 1f);

                        if (Main.rand.NextBool(3)) {
                            continue;
                        }
                        dustType = ModContent.DustType<ManaCrystalShard>();
                        dust = Dust.NewDustPerfect(gorePosition, dustType, Vector2.One.RotatedBy(currentIndex * MathHelper.TwoPi / goreCount) * 2f,
                            0, color, 1f + Main.rand.NextFloatRange(0.1f));
                        dust.velocity *= 0.5f;
                    }
                }
            }

            info.Opacity = 0f;
        }

        int max = 3;
        if (!handler.ApplyCrystallizedSkullSetBonus) {
            return;
        }

        if (player.statMana >= 0) {
            if (!handler._initializingCrystals) {
                handler._initializingCrystals = true;
            }
        }
        else {
            if (handler._initializingCrystals) {
                handler._initializingCrystals = false;

                handler._crystalData = new CrystalInfo[max];
                Color[] colors = [
                            Color.Lerp(new Color(227, 170, 230), new Color(175, 89, 192), 0.5f),
                            Color.Lerp(new Color(210, 182, 241), new Color(164, 109, 224), 0.5f),
                            Color.Lerp(new Color(184, 200, 241), new Color(133, 148, 186), 0.5f)
                            ];
                List<int> taken = [];
                for (int i = 0; i < max; i++) {
                    int colorIndex = Main.rand.Next(max);
                    while (taken.Contains(colorIndex)) {
                        colorIndex = Main.rand.Next(max);
                    }
                    taken.Add(colorIndex);
                    handler._crystalData[i] = new CrystalInfo(
                        Main.rand.RandomPointInArea(4f) - Vector2.UnitY * player.height * 0.6f,
                        Main.rand.NextBool(),
                        MathHelper.Lerp(-MathHelper.PiOver4, MathHelper.PiOver4, (float)i / max + Main.rand.NextFloatRange(0.05f) + (float)i / max / 2f),
                        colors[colorIndex],
                        (byte)colorIndex);
                }
            }
            else {
                for (int i = 0; i < max; i++) {
                    if (player.manaRegenDelay > 0f && i > 0 && handler._crystalData[i - 1].Opacity <= 0f) {
                        continue;
                    }
                    float progress = (float)Math.Abs(player.statMana) / player.statManaMax2;
                    float to = MathF.Min(max, i + 1) / (float)max,
                          from = i / (float)max;
                    float opacity = Utils.GetLerpValue(from, to, progress, true) * 1.5f;
                    opacity = MathUtils.Clamp01(opacity);
                    if (player.manaRegenDelay <= 0f) {
                        opacity = handler._crystalData[i].Opacity;
                        if (progress < from + 0.01f) {
                            breakCrystal(i);
                            opacity = 0f;
                        }
                    }
                    handler._crystalData[i].Opacity = MathHelper.Lerp(handler._crystalData[i].Opacity, opacity, 0.25f);
                }
            }
        }
    }

    private static void ExtraDrawLayerSupport_PreBackpackDrawEvent(ref PlayerDrawSet drawinfo) {
        var player = drawinfo.drawPlayer;
        var handler = player.GetCommon();
        if (!handler.ApplyCrystallizedSkullSetBonus || !handler.ShouldDrawCrystals()) {
            return;
        }
        
        if (!player.IsAlive()) {
            return;
        }

        Texture2D texture = _crystalOnPlayerTexture.Value;
        int max = handler._crystalData.Length;
        for (int i = 0; i < max; i++) {
            var info = handler._crystalData[i];
            Color color = info.Color;
            color.A = (byte)MathHelper.Lerp(255, 188, Helper.Wave(0f, handler._crystalAlphaOpacity, 12.5f, i * max));
            color = drawinfo.drawPlayer.GetImmuneAlphaPure(color, (float)drawinfo.shadow);
            SpriteFrame spriteFrame = new(1, 2, 0, (byte)info.SecondFrame.ToInt());
            Rectangle sourceRectangle = spriteFrame.GetSourceRectangle(texture);
            float rotation = info.ExtraRotation;
            int direction = (int)(player.direction * player.gravDir);
            bool facedLeft = direction == -1;
            bool reversedGravity = player.gravDir < 0f;
            if (facedLeft) {
                rotation = MathHelper.TwoPi - rotation;
                if (reversedGravity) {
                    rotation += MathHelper.Pi;
                }
            }
            else {
                if (reversedGravity) {
                    rotation += MathHelper.Pi;
                }
            }
            float y = -drawinfo.drawPlayer.width * 0.5f;
            Vector2 vector3 = (new Vector2(0f, y) + info.Offset * new Vector2(direction, 1f)).RotatedBy(rotation);
            Vector2 position = drawinfo.Position - Main.screenPosition + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.width / 2, drawinfo.drawPlayer.height - drawinfo.drawPlayer.bodyFrame.Height / 2) + new Vector2(0f, -4f);
            Vector2 origin = sourceRectangle.BottomCenter();
            position = position.Floor() + vector3 + (Vector2.UnitY * origin.Y * 0.75f).RotatedBy(rotation);
            Vector2 scale = new(1f, info.Opacity);
            SpriteEffects effect = facedLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (reversedGravity) {
                if (!facedLeft) {
                    effect = SpriteEffects.None;
                }
                else {
                    effect |= SpriteEffects.FlipHorizontally;
                }
                position.Y += 16f;
            }
            DrawData item = new(texture, position + player.MovementOffset(), sourceRectangle, color * 0.9f, rotation, origin, scale, effect);
            drawinfo.DrawDataCache.Add(item);
        }
    }

    private static bool On_Player_CheckMana_Item_int_bool_bool(On_Player.orig_CheckMana_Item_int_bool_bool orig, Player self, Item item, int amount, bool pay, bool blockQuickMana) {
        if (self.GetCommon().ApplyCrystallizedSkullSetBonus) {
            if (self.statMana > 0) {
                if (amount <= -1)
                    amount = self.GetManaCost(item);

                if (self.statMana >= 0/*amount*/) {
                    if (pay) {
                        CombinedHooks.OnConsumeMana(self, item, amount);
                        self.statMana -= amount;
                    }

                    self.GetCommon()._stoppedUsingManaFor = self.GetCommon().GetUseCheckTime();

                    return true;
                }

                if (blockQuickMana)
                    return false;

                CombinedHooks.OnMissingMana(self, item, amount);
                if ((self.statManaMax2 - Math.Abs(self.statMana)) < amount && self.manaFlower)
                    self.QuickMana();

                if (self.statMana >= 0/*amount*/) {
                    if (pay) {
                        CombinedHooks.OnConsumeMana(self, item, amount);
                        self.statMana -= amount;
                    }
                    self.GetCommon()._stoppedUsingManaFor = self.GetCommon().GetUseCheckTime();

                    return true;
                }

                return false;
            }

            if (amount <= -1)
                amount = self.GetManaCost(item);

            if ((self.statManaMax2 - Math.Abs(self.statMana)) >= amount) {
                if (pay) {
                    CombinedHooks.OnConsumeMana(self, item, amount);
                    self.statMana -= amount;
                }
                self.GetCommon()._stoppedUsingManaFor = self.GetCommon().GetUseCheckTime();

                return true;
            }

            if (blockQuickMana)
                return false;

            CombinedHooks.OnMissingMana(self, item, amount);
            if ((self.statManaMax2 - Math.Abs(self.statMana)) < amount && self.manaFlower)
                self.QuickMana();

            if ((self.statManaMax2 - Math.Abs(self.statMana)) >= amount) {
                if (pay) {
                    CombinedHooks.OnConsumeMana(self, item, amount);
                    self.statMana -= amount;
                }
                self.GetCommon()._stoppedUsingManaFor = self.GetCommon().GetUseCheckTime();

                return true;
            }

            return false;
        }

        return orig(self, item, amount, pay, blockQuickMana);
    }

    private static bool On_Player_CheckMana_int_bool_bool(On_Player.orig_CheckMana_int_bool_bool orig, Player self, int amount, bool pay, bool blockQuickMana) {
        if (self.GetCommon().ApplyCrystallizedSkullSetBonus) {
            int num;
            if (self.statMana > 0) {
                num = (int)((float)amount * self.manaCost);
                if (self.statMana >= 0/*num*/) {
                    if (pay) {
                        self.statMana -= num;
                    }
                    self.GetCommon()._stoppedUsingManaFor = self.GetCommon().GetUseCheckTime();

                    return true;
                }

                if (self.manaFlower && !blockQuickMana) {
                    self.QuickMana();
                    if (self.statMana >= 0/*num*/) {
                        if (pay) {
                            self.statMana -= num;
                        }
                        self.GetCommon()._stoppedUsingManaFor = self.GetCommon().GetUseCheckTime();

                        return true;
                    }

                    return false;
                }

                return false;
            }

            num = (int)((float)amount * self.manaCost);
            if ((self.statManaMax2 - Math.Abs(self.statMana)) >= num) {
                if (pay) {
                    self.statMana -= num;
                }
                self.GetCommon()._stoppedUsingManaFor = self.GetCommon().GetUseCheckTime();

                return true;
            }

            if (self.manaFlower && !blockQuickMana) {
                self.QuickMana();
                if ((self.statManaMax2 - Math.Abs(self.statMana)) >= num) {
                    if (pay) {
                        self.statMana -= num;
                    }
                    self.GetCommon()._stoppedUsingManaFor = self.GetCommon().GetUseCheckTime();

                    return true;
                }

                return false;
            }
        }

        return orig(self, amount, pay, blockQuickMana);
    }

    private static void On_Player_UpdateManaRegen(On_Player.orig_UpdateManaRegen orig, Player self) {
        //if (self.statMana < 0) {
        if (self.statMana < -self.statManaMax2) {
            self.statMana = -self.statManaMax2;
        }
        if (self.nebulaLevelMana > 0) {
            int num = 6;
            self.nebulaManaCounter += self.nebulaLevelMana;
            if (self.nebulaManaCounter >= num) {
                self.nebulaManaCounter -= num;
                self.statMana++;
                if (self.statMana >= self.statManaMax2)
                    self.statMana = self.statManaMax2;
            }
        }
        else {
            self.nebulaManaCounter = 0;
        }

        if (self.manaRegenDelay > 0f) {
            //if (self.statMana < 0 && self.manaRegenDelay < self.maxRegenDelay / 2 && !self.HasBuff<Crystallized>()) {
            //    self.AddBuff<Crystallized>((int)(BUFFTIMEMAX * (Math.Abs(self.statMana) / (float)self.statManaMax2)));
            //}

            self.manaRegenDelay -= 1f;
            self.manaRegenDelay -= self.manaRegenDelayBonus;
            if (self.IsStandingStillForSpecialEffects || self.grappling[0] >= 0 || self.manaRegenBuff)
                self.manaRegenDelay -= 1f;

            if (self.usedArcaneCrystal)
                self.manaRegenDelay -= 0.05f;
        }

        if (self.manaRegenBuff && self.manaRegenDelay > 20f)
            self.manaRegenDelay = 20f;

        if (self.manaRegenDelay <= 0f) {
            self.manaRegenDelay = 0f;
            self.manaRegen = self.statManaMax2 / 3 + 1 + self.manaRegenBonus;
            if (self.IsStandingStillForSpecialEffects || self.grappling[0] >= 0 || self.manaRegenBuff)
                self.manaRegen += self.statManaMax2 / 3;

            if (self.usedArcaneCrystal)
                self.manaRegen += self.statManaMax2 / 50;

            float num2 = (float)(MathF.Max(((float)self.statMana / (float)self.statManaMax2), 1f - Math.Abs(self.statMana) / (float)self.statManaMax2)) * 0.8f + 0.2f;

            if (self.manaRegenBuff)
                num2 = 1f;

            self.manaRegen = (int)((double)((float)self.manaRegen * num2) * 1.15);
        }
        else {
            self.manaRegen = 0;
        }

        self.manaRegenCount += self.manaRegen;
        while (self.manaRegenCount >= 120) {
            bool flag = false;
            self.manaRegenCount -= 120;
            if (self.statMana < self.statManaMax2) {
                self.statMana++;
                flag = true;
            }

            if (self.statMana < self.statManaMax2)
                continue;

            if (self.whoAmI == Main.myPlayer && flag) {
                SoundEngine.PlaySound(SoundID.MaxMana);
                for (int i = 0; i < 5; i++) {
                    int num3 = Dust.NewDust(self.position, self.width, self.height, 45, 0f, 0f, 255, default(Color), (float)Main.rand.Next(20, 26) * 0.1f);
                    Main.dust[num3].noLight = true;
                    Main.dust[num3].noGravity = true;
                    Main.dust[num3].velocity *= 0.5f;
                }
            }

            self.statMana = self.statManaMax2;
        }

        return;
        //}

        //orig(self);
    }
}
