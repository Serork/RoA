using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Content.NPCs.Enemies.Backwoods.Hardmode;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed partial class NPCCommon : GlobalNPC {
    private static float MAXFALLSPEEDMODIFIERFORFALL => 0.75f;

    private bool _fell;
    private float _fellTimer;

    public bool Fell { get; private set; }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => true;

    public delegate void ModifyHitByProjectileDelegate(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers);
    public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;
    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
        ModifyHitByProjectileEvent?.Invoke(npc, projectile, ref modifiers);
    }

    public override void PostAI(NPC npc) {
        TouchGround(npc);

        NewMoneyPostAI(npc);
    }

    public partial void NewMoneyPostAI(NPC npc);

    private void TouchGround(NPC npc) {
        if (npc.noGravity) {
            return;
        }

        if (npc.velocity.Y >= npc.maxFallSpeed * MAXFALLSPEEDMODIFIERFORFALL && !_fell) {
            _fell = true;
        }
        if (_fell && npc.IsGrounded() && !Fell) {
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
