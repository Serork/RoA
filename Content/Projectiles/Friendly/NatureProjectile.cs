﻿using RoA.Common.Druid;
using RoA.Core;
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
        ShouldIncreaseWreathPoints = false;
    }

    //protected sealed override void SafeOnSpawn(IEntitySource source) {
    //    SafeOnSpawn2(source);

    //    ShouldIncreaseWreathPoints = false;
    //}

    //protected virtual void SafeOnSpawn2(IEntitySource source) { }
}

abstract class NatureProjectile : ModProjectile {
    private float _wreathPointsFine;
    private bool _syncItem;

    internal Item Item { get; private set; } = null;

    public bool ShouldIncreaseWreathPoints { get; protected set; } = true;
    public bool ShouldApplyItemDamage { get; protected set; } = true;

    public float WreathPointsFine {
        get => _wreathPointsFine;
        protected set {
            _wreathPointsFine = Math.Clamp(value, -1f, 1f);
        }
    }

    public sealed override void SendExtraAI(BinaryWriter writer) {
        if (ShouldIncreaseWreathPoints) {
            if (this is not FormProjectile) {
                writer.Write(_syncItem);
                if (_syncItem) {
                    writer.Write(WreathPointsFine);
                    ItemIO.Send(Item, writer, true);
                }
            }
        }

        SafeSendExtraAI(writer);
    }

    protected virtual void SafeSendExtraAI(BinaryWriter writer) { }

    public sealed override void ReceiveExtraAI(BinaryReader reader) {
        if (ShouldIncreaseWreathPoints) {
            if (this is not FormProjectile) {
                _syncItem = reader.ReadBoolean();
                if (_syncItem) {
                    WreathPointsFine = reader.ReadSingle();
                    Item = ItemIO.Receive(reader, true);
                }
            }
        }

        SafeReceiveExtraAI(reader);
    }

    protected virtual void SafeReceiveExtraAI(BinaryReader reader) { }

    private void SetItem(Item item) {
        if (!ShouldIncreaseWreathPoints) {
            return;
        }
        if (item.IsEmpty() || !item.IsADruidicWeapon()) {
            return;
        }
        Item = item;
        float fillingRate = NatureWeaponHandler.GetFillingRate(Item);
        WreathPointsFine = fillingRate <= 1f ? 1f - fillingRate : -(fillingRate - 1f);
        _syncItem = true;
        Projectile.netUpdate = true;
    }

    public sealed override void OnSpawn(IEntitySource source) {
        if (this is not FormProjectile && Projectile.owner == Main.myPlayer) {
            SetItem(Projectile.GetOwnerAsPlayer().GetSelectedItem());
        }
        SafeOnSpawn(source);
    }

    public sealed override void PostAI() {
        SafePostAI();

        if (ShouldIncreaseWreathPoints && this is not FormProjectile) {
            if (Item != null) {
                if (ShouldApplyItemDamage) {
                    Projectile.damage = NatureWeaponHandler.GetNatureDamage(Item, Main.player[Projectile.owner]);
                }
                _syncItem = false;
                Projectile.netUpdate = true;
            }
            else {
                _syncItem = true;
            }
        }
    }

    public virtual void SafePostAI() { }

    protected virtual void SafeOnSpawn(IEntitySource source) { }

    public sealed override void SetDefaults() {
        SafeSetDefaults();

        if (Projectile.IsDamageable() && Projectile.friendly) {
            Projectile.SetDefaultToDruidicProjectile();
        }

        SafeSetDefaults2();
        SafeSetDefaults3();
    }

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafeSetDefaults2() { }
    protected virtual void SafeSetDefaults3() { }
}
