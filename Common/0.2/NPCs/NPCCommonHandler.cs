﻿using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class NPCCommonHandler : GlobalNPC {
    private static float MAXFALLSPEEDMODIFIERFORFALL => 0.5f;

    private bool _fell;
    private float _fellTimer;

    public bool Fell { get; private set; }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => !entity.noGravity;

    public delegate void ModifyHitByProjectileDelegate(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers);
    public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;
    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
        ModifyHitByProjectileEvent?.Invoke(npc, projectile, ref modifiers);
    }

    public override void PostAI(NPC npc) {
        TouchGround(npc);
    }

    private void TouchGround(NPC npc) {
        if (npc.velocity.Y >= npc.maxFallSpeed * MAXFALLSPEEDMODIFIERFORFALL && !_fell) {
            _fell = true;
        }
        if (_fell && npc.velocity.Y == 0f && !Fell) {
            Fell = true;
            _fell = false;
            _fellTimer = 10f;
            return;
        }
        if (MathF.Abs(npc.velocity.Y) > 1f) {
            Fell = false;
        }
        if (_fellTimer > 0f) {
            _fellTimer--;
        }
        else if (!_fell) {
            Fell = false;
        }
    }
}
