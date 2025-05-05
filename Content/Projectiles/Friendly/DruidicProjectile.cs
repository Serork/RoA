using RoA.Common.Druid;
using RoA.Content.Items;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;
using System.Data.SqlTypes;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Projectiles.Friendly;

abstract class FormProjectile : DruidicProjectile {
    protected override void SafeSetDefaults3() {
        ShouldChargeWreath = false;
    }

    //protected sealed override void SafeOnSpawn(IEntitySource source) {
    //    SafeOnSpawn2(source);

    //    ShouldIncreaseWreathPoints = false;
    //}

    //protected virtual void SafeOnSpawn2(IEntitySource source) { }
}

sealed class CrossmodNatureProjectileHandler : GlobalProjectile {
    internal float _wreathFillingFine;
    internal bool _syncAttachedItem;

    public Item AttachedItem { get; internal set; } = null;

    public bool ShouldChargeWreath { get; internal set; } = true;
    public bool ShouldApplyAttachedItemDamage { get; internal set; } = true;

    public float WreathFillingFine {
        get => _wreathFillingFine;
        internal set {
            _wreathFillingFine = Math.Clamp(value, -1f, 1f);
        }
    }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => CrossmodNatureContent.IsProjectileNature(entity);

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
        DruidicProjectile.NatureProjectileSendExtraAI(projectile, binaryWriter);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
        DruidicProjectile.NatureProjectileReceiveExtraAI(projectile, binaryReader);
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source) {
        DruidicProjectile.NatureProjectileSetItem(projectile);
    }

    public override void PostAI(Projectile projectile) {
        DruidicProjectile.NatureProjectilePostAI(projectile);
    }
}

abstract class DruidicProjectile : ModProjectile {
    private float _wreathFillingFine;
    private bool _syncAttachedItem;

    public Item AttachedItem { get; private set; } = null;

    public bool ShouldChargeWreath { get; protected set; } = true;
    public bool ShouldApplyAttachedItemDamage { get; protected set; } = true;

    public float WreathFillingFine {
        get => _wreathFillingFine;
        protected set {
            _wreathFillingFine = Math.Clamp(value, -1f, 1f);
        }
    }

    public static void NatureProjectileSetValues(Projectile projectile, bool shouldChargeWreath = true, bool shouldApplyAttachedItemDamage = true, float wreathFillingFine = 0f) {
        if (projectile.ModProjectile is DruidicProjectile natureProjectile) {
            natureProjectile.ShouldChargeWreath = shouldChargeWreath;
            natureProjectile.ShouldApplyAttachedItemDamage = shouldApplyAttachedItemDamage;
            natureProjectile.WreathFillingFine = wreathFillingFine;

            return;
        }

        try {
            CrossmodNatureProjectileHandler handler = projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
            handler.ShouldChargeWreath = shouldChargeWreath;
            handler.ShouldApplyAttachedItemDamage = shouldApplyAttachedItemDamage;
            handler.WreathFillingFine = wreathFillingFine;
        }
        catch (Exception exception) {
            throw new Exception(exception.Message);
        }
    }

    public static void NatureProjectileSendExtraAI(Projectile projectile, BinaryWriter writer) {
        if (projectile.ModProjectile is DruidicProjectile natureProjectile) {
            if (natureProjectile.ShouldChargeWreath) {
                if (natureProjectile is not FormProjectile) {
                    writer.Write(natureProjectile._syncAttachedItem);
                    if (natureProjectile._syncAttachedItem) {
                        writer.Write(natureProjectile.WreathFillingFine);
                        ItemIO.Send(natureProjectile.AttachedItem, writer, true);
                    }
                }
            }
            return;
        }

        try {
            CrossmodNatureProjectileHandler handler = projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
            if (handler.ShouldChargeWreath) {
                writer.Write(handler._syncAttachedItem);
                if (handler._syncAttachedItem) {
                    writer.Write(handler.WreathFillingFine);
                    ItemIO.Send(handler.AttachedItem, writer, true);
                }
            }
        }
        catch (Exception exception) {
            throw new Exception(exception.Message);
        }
    }

    public static void NatureProjectileReceiveExtraAI(Projectile projectile, BinaryReader reader) {
        if (projectile.ModProjectile is DruidicProjectile natureProjectile) {
            if (natureProjectile.ShouldChargeWreath) {
                if (natureProjectile is not FormProjectile) {
                    natureProjectile._syncAttachedItem = reader.ReadBoolean();
                    if (natureProjectile._syncAttachedItem) {
                        natureProjectile.WreathFillingFine = reader.ReadSingle();
                        natureProjectile.AttachedItem = ItemIO.Receive(reader, true);
                    }
                }
            }
            return;
        }

        try {
            CrossmodNatureProjectileHandler handler = projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
            if (handler.ShouldChargeWreath) {
                handler._syncAttachedItem = reader.ReadBoolean();
                if (handler._syncAttachedItem) {
                    handler.WreathFillingFine = reader.ReadSingle();
                    handler.AttachedItem = ItemIO.Receive(reader, true);
                }
            }
        }
        catch (Exception exception) {
            throw new Exception(exception.Message);
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

    public static void NatureProjectileSetItem(Projectile projectile) {
        if (projectile.ModProjectile is DruidicProjectile natureProjectile) {
            if (natureProjectile is not FormProjectile && projectile.owner == Main.myPlayer) {
                if (natureProjectile.ShouldChargeWreath) {
                    Item item = projectile.GetOwnerAsPlayer().GetSelectedItem();
                    if (!(item.IsEmpty() || !item.IsADruidicWeapon())) {
                        natureProjectile.AttachedItem = item;
                        float fillingRate = NatureWeaponHandler.GetFillingRate(natureProjectile.AttachedItem);
                        natureProjectile.WreathFillingFine = fillingRate <= 1f ? 1f - fillingRate : -(fillingRate - 1f);
                        natureProjectile._syncAttachedItem = true;
                        projectile.netUpdate = true;
                    }
                }
            }
            return;
        }

        try {
            CrossmodNatureProjectileHandler handler = projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
            if (projectile.owner == Main.myPlayer) {
                if (handler.ShouldChargeWreath) {
                    Item item = projectile.GetOwnerAsPlayer().GetSelectedItem();
                    if (!(item.IsEmpty() || !item.IsADruidicWeapon())) {
                        handler.AttachedItem = item;
                        float fillingRate = NatureWeaponHandler.GetFillingRate(handler.AttachedItem);
                        handler.WreathFillingFine = fillingRate <= 1f ? 1f - fillingRate : -(fillingRate - 1f);
                        handler._syncAttachedItem = true;
                        projectile.netUpdate = true;
                    }
                }
            }
        }
        catch (Exception exception) {
            throw new Exception(exception.Message);
        }
        return;
    }

    public sealed override void OnSpawn(IEntitySource source) {
        NatureProjectileSetItem(Projectile);

        SafeOnSpawn(source);
    }

    public static void NatureProjectilePostAI(Projectile projectile) {
        if (projectile.ModProjectile is DruidicProjectile natureProjectile) {
            if (natureProjectile.ShouldChargeWreath && natureProjectile is not FormProjectile) {
                if (natureProjectile.AttachedItem != null) {
                    if (natureProjectile.ShouldApplyAttachedItemDamage) {
                        projectile.damage = NatureWeaponHandler.GetNatureDamage(natureProjectile.AttachedItem, Main.player[projectile.owner]);
                    }
                    natureProjectile._syncAttachedItem = false;
                    projectile.netUpdate = true;
                }
                else {
                    natureProjectile._syncAttachedItem = true;
                }
            }
            return;
        }

        try {
            CrossmodNatureProjectileHandler handler = projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>();
            if (handler.ShouldChargeWreath) {
                if (handler.AttachedItem != null) {
                    if (handler.ShouldApplyAttachedItemDamage) {
                        projectile.damage = NatureWeaponHandler.GetNatureDamage(handler.AttachedItem, Main.player[projectile.owner]);
                    }
                    handler._syncAttachedItem = false;
                    projectile.netUpdate = true;
                }
                else {
                    handler._syncAttachedItem = true;
                }
            }
        }
        catch (Exception exception) {
            throw new Exception(exception.Message);
        }
        return;
    }

    public sealed override void PostAI() {
        SafePostAI();

        NatureProjectilePostAI(Projectile);
    }

    public virtual void SafePostAI() { }

    protected virtual void SafeOnSpawn(IEntitySource source) { }

    public sealed override void SetDefaults() {
        SafeSetDefaults();

        Projectile.MakeProjectileDruidicDamageable();

        SafeSetDefaults2();
        SafeSetDefaults3();
    }

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafeSetDefaults2() { }
    protected virtual void SafeSetDefaults3() { }
}
