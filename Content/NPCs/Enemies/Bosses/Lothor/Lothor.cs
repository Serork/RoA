using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

[AutoloadBossHead]
sealed partial class Lothor : ModNPC {
    public sealed override void SetStaticDefaults() => Main.npcFrameCount[Type] = 1;

    public override void SetDefaults() {
        NPC.damage = 46;
        NPC.lifeMax = 6000;
        NPC.defense = 8;

        NPC.Size = Vector2.One * 72f;

        NPC.aiStyle = AIType = -1;

        NPC.npcSlots = 10f;

        NPC.boss = true;

        NPC.HitSound = new SoundStyle(ResourceManager.NPCSounds + "LothorHit") { Volume = 0.8f };

        NPC.value = Item.buyPrice(gold: 5);

        if (!Main.dedServ) {
            Music = MusicLoader.GetMusicSlot(ResourceManager.Music + "Lothor");
        }
    }

    public override void Unload() => UnloadAnimations();

    partial void UnloadAnimations();
}
