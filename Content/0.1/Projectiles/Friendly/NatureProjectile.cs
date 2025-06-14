using RoA.Common.Druid;
using RoA.Content.Items;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Projectiles.Friendly;

abstract class FormProjectile : NatureProjectile {
    protected override void SafeSetDefaults3() {
        ShouldChargeWreathOnDamage = false;
    }
}

sealed class CrossmodNatureProjectileHandler : GlobalProjectile {
    internal float _wreathFillingFine;
    internal bool _syncAttachedNatureWeapon;

    public Item? AttachedNatureWeapon { get; internal set; } = null;

    public bool ShouldChargeWreathOnDamage { get; internal set; } = true;
    public bool ShouldApplyAttachedNatureWeaponCurrentDamage { get; internal set; } = true;

    public float WreathFillingFine {
        get => _wreathFillingFine;
        internal set {
            _wreathFillingFine = Math.Clamp(value, -1f, 1f);
        }
    }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => CrossmodNatureContent.IsProjectileNature(entity);

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
        NatureProjectile.NatureProjectileSendExtraAI(projectile, binaryWriter);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
        NatureProjectile.NatureProjectileReceiveExtraAI(projectile, binaryReader);
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source) {
        NatureProjectile.NatureProjectileSetItem(projectile);
    }

    public override void PostAI(Projectile projectile) {
        NatureProjectile.NatureProjectilePostAI(projectile);
    }
}

abstract class NatureProjectile : ModProjectile {
    private float _wreathFillingFine;
    private bool _syncAttachedNatureWeapon;

    public Item AttachedNatureWeapon { get; private set; } = null!;

    public bool ShouldChargeWreathOnDamage { get; protected set; } = true;
    public bool ShouldApplyAttachedNatureWeaponCurrentDamage { get; protected set; } = true;

    public float WreathFillingFine {
        get => _wreathFillingFine;
        protected set {
            _wreathFillingFine = Math.Clamp(value, -1f, 1f);
        }
    }

    public static void SetNatureValues(Projectile projectile, bool shouldChargeWreath = true, bool shouldApplyAttachedItemDamage = true, float wreathFillingFine = 0f) {
        if (projectile.ModProjectile is NatureProjectile natureProjectile) {
            natureProjectile.ShouldChargeWreathOnDamage = shouldChargeWreath;
            natureProjectile.ShouldApplyAttachedNatureWeaponCurrentDamage = shouldApplyAttachedItemDamage;
            natureProjectile.WreathFillingFine = wreathFillingFine;

            return;
        }

        CrossmodNatureProjectileHandler handler = projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
        handler.ShouldChargeWreathOnDamage = shouldChargeWreath;
        handler.ShouldApplyAttachedNatureWeaponCurrentDamage = shouldApplyAttachedItemDamage;
        handler.WreathFillingFine = wreathFillingFine;
    }

    public static void NatureProjectileSendExtraAI(Projectile projectile, BinaryWriter writer) {
        if (projectile.ModProjectile is NatureProjectile natureProjectile) {
            if (natureProjectile.ShouldChargeWreathOnDamage) {
                if (natureProjectile is not FormProjectile) {
                    writer.Write(natureProjectile._syncAttachedNatureWeapon);
                    if (natureProjectile._syncAttachedNatureWeapon) {
                        writer.Write(natureProjectile.WreathFillingFine);
                        ItemIO.Send(natureProjectile.AttachedNatureWeapon, writer, true);
                    }
                }
            }
            return;
        }

        if (CrossmodNatureContent.IsProjectileNature(projectile)) {
            CrossmodNatureProjectileHandler handler = projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
            if (handler.ShouldChargeWreathOnDamage) {
                writer.Write(handler._syncAttachedNatureWeapon);
                if (handler._syncAttachedNatureWeapon) {
                    writer.Write(handler.WreathFillingFine);
                    ItemIO.Send(handler.AttachedNatureWeapon, writer, true);
                }
            }
        }
    }

    public static void NatureProjectileReceiveExtraAI(Projectile projectile, BinaryReader reader) {
        if (projectile.ModProjectile is NatureProjectile natureProjectile) {
            if (natureProjectile.ShouldChargeWreathOnDamage) {
                if (natureProjectile is not FormProjectile) {
                    natureProjectile._syncAttachedNatureWeapon = reader.ReadBoolean();
                    if (natureProjectile._syncAttachedNatureWeapon) {
                        natureProjectile.WreathFillingFine = reader.ReadSingle();
                        natureProjectile.AttachedNatureWeapon = ItemIO.Receive(reader, true);
                    }
                }
            }
            return;
        }

        if (CrossmodNatureContent.IsProjectileNature(projectile)) {
            CrossmodNatureProjectileHandler handler = projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
            if (handler.ShouldChargeWreathOnDamage) {
                handler._syncAttachedNatureWeapon = reader.ReadBoolean();
                if (handler._syncAttachedNatureWeapon) {
                    handler.WreathFillingFine = reader.ReadSingle();
                    handler.AttachedNatureWeapon = ItemIO.Receive(reader, true);
                }
            }
        }
    }

    public sealed override void SendExtraAI(BinaryWriter writer) {
        NatureProjectileSendExtraAI(Projectile, writer);

        SafeSendExtraAI(writer);
    }

    protected virtual void SafeSendExtraAI(BinaryWriter writer) { }

    public sealed override void ReceiveExtraAI(BinaryReader reader) {
        NatureProjectileReceiveExtraAI(Projectile, reader);

        SafeReceiveExtraAI(reader);
    }

    protected virtual void SafeReceiveExtraAI(BinaryReader reader) { }

    public static void NatureProjectileSetItem(Projectile projectile, Item item = null) {
        if (projectile.ModProjectile is NatureProjectile natureProjectile) {
            if (natureProjectile is not FormProjectile && projectile.owner == Main.myPlayer) {
                if (natureProjectile.AttachedNatureWeapon != null) {
                    projectile.netUpdate = true;
                    goto setWreathFillingRateFine;
                }
                if (natureProjectile.ShouldChargeWreathOnDamage) {
                    item ??= projectile.GetOwnerAsPlayer().GetSelectedItem();
                    if (!(item.IsEmpty() || !item.IsANatureWeapon())) {
                        natureProjectile.AttachedNatureWeapon = item;
                        natureProjectile._syncAttachedNatureWeapon = true;
                        projectile.netUpdate = true;
                        goto setWreathFillingRateFine;
                    }
                }
            setWreathFillingRateFine:
                if (natureProjectile.AttachedNatureWeapon != null) {
                    float fillingRate = NatureWeaponHandler.GetFillingRate(natureProjectile.AttachedNatureWeapon);
                    natureProjectile.WreathFillingFine = fillingRate <= 1f ? 1f - fillingRate : -(fillingRate - 1f);
                }
            }
            return;
        }

        if (CrossmodNatureContent.IsProjectileNature(projectile)) {
            CrossmodNatureProjectileHandler handler = projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
            if (projectile.owner == Main.myPlayer) {
                if (handler.AttachedNatureWeapon != null) {
                    projectile.netUpdate = true;
                    goto setWreathFillingRateFine;
                }
                if (handler.ShouldChargeWreathOnDamage) {
                    item ??= projectile.GetOwnerAsPlayer().GetSelectedItem();
                    if (!(item.IsEmpty() || !item.IsANatureWeapon())) {
                        handler.AttachedNatureWeapon = item;
                        handler._syncAttachedNatureWeapon = true;
                        projectile.netUpdate = true;
                        goto setWreathFillingRateFine;
                    }
                }
            setWreathFillingRateFine:
                if (handler.AttachedNatureWeapon != null) {
                    float fillingRate = NatureWeaponHandler.GetFillingRate(handler.AttachedNatureWeapon);
                    handler.WreathFillingFine = fillingRate <= 1f ? 1f - fillingRate : -(fillingRate - 1f);
                }
            }
        }
    }

    public sealed override void OnSpawn(IEntitySource source) {
        NatureProjectileSetItem(Projectile);

        SafeOnSpawn(source);
    }

    public static void NatureProjectilePostAI(Projectile projectile) {
        if (projectile.ModProjectile is NatureProjectile natureProjectile) {
            if (natureProjectile.ShouldChargeWreathOnDamage && natureProjectile is not FormProjectile) {
                if (natureProjectile.AttachedNatureWeapon != null) {
                    if (natureProjectile.ShouldApplyAttachedNatureWeaponCurrentDamage) {
                        projectile.damage = NatureWeaponHandler.GetNatureDamage(natureProjectile.AttachedNatureWeapon, Main.player[projectile.owner]);
                    }
                    if (natureProjectile._syncAttachedNatureWeapon) {
                        projectile.netUpdate = true;
                    }
                    natureProjectile._syncAttachedNatureWeapon = false;
                }
                else {
                    if (!natureProjectile._syncAttachedNatureWeapon) {
                        projectile.netUpdate = true;
                    }
                    natureProjectile._syncAttachedNatureWeapon = true;
                }
            }
            return;
        }

        if (CrossmodNatureContent.IsProjectileNature(projectile)) {
            CrossmodNatureProjectileHandler handler = projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
            if (handler.ShouldChargeWreathOnDamage) {
                if (handler.AttachedNatureWeapon != null) {
                    if (handler.ShouldApplyAttachedNatureWeaponCurrentDamage) {
                        projectile.damage = NatureWeaponHandler.GetNatureDamage(handler.AttachedNatureWeapon, Main.player[projectile.owner]);
                    }
                    if (handler._syncAttachedNatureWeapon) {
                        projectile.netUpdate = true;
                    }
                    handler._syncAttachedNatureWeapon = false;
                }
                else {
                    if (!handler._syncAttachedNatureWeapon) {
                        projectile.netUpdate = true;
                    }
                    handler._syncAttachedNatureWeapon = true;
                }
            }
        }
    }

    public sealed override void PostAI() {
        SafePostAI();

        NatureProjectilePostAI(Projectile);
    }

    public virtual void SafePostAI() { }

    protected virtual void SafeOnSpawn(IEntitySource source) { }

    public sealed override void SetDefaults() {
        SafeSetDefaults();

        Projectile.MakeProjectileNature();

        SafeSetDefaults2();
        SafeSetDefaults3();
    }

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafeSetDefaults2() { }
    protected virtual void SafeSetDefaults3() { }
}
