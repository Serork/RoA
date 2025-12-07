using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Players;
using RoA.Common.ScreenTargets;
using RoA.Content.Items.Equipables.Armor.Nature.Hardmode;
using RoA.Content.Items.Equipables.Vanity;
using RoA.Content.Tiles.Decorations;
using RoA.Core.Utility.Vanilla;

using System.Reflection;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common;

// starlight river

class ReflectionTarget : IPostSetupContent {
    //Drawing Player to Target. Should be safe. Excuse me if im duplicating something that alr exists :p
    public const string simpleReflectionShaderPath = "StarlightRiver:SimpleReflection";

    private MethodInfo NpcsOverTilesDrawMethod;
    private MethodInfo PlayerBehindNPCsDrawMethod;
    private MethodInfo NpcsBehindTilesDrawMethod;
    private MethodInfo PlayerAfterProjDrawMethod;
    private MethodInfo ProjectileDrawMethod;
    private MethodInfo drawCachedProjsMethod;
    private MethodInfo drawCachedNPCsMethod;
    private MethodInfo dustDrawMethod;
    private MethodInfo goreDrawMethod;

    public static ScreenTarget Target;
    private static ScreenTarget reflectionNormalMapTarget;

    /// <summary>
    /// lets other components know that targets on this component are being rendered so they cannot try to use them.
    /// </summary>
    public static bool canUseTarget = false;

    /// <summary>
    /// determines whether or not to draw the entities to the reflection RT, set to true if either the wall reflections are active OR the homogenized version is in use
    /// </summary>
    public static bool isDrawReflectablesThisFrame = false;

    /// <summary>
    /// determines whether or not to apply the shader and actually draw the wall reflections to the screen, only set to true if the wall reflections are in use this frame
    /// </summary>
    public static bool applyWallReflectionsThisFrame = false;

    void IPostSetupContent.PostSetupContent() {
        if (Main.dedServ)
            return;

        //Since this renders alot of other things, we need to render it seperately to prevent deadlocks with other render targets
        Target = new(null, () => true, 1.2f);
        reflectionNormalMapTarget = new(DrawTargets, () => true, 1.15f);

        NpcsOverTilesDrawMethod = typeof(Main).GetMethod("DoDraw_DrawNPCsOverTiles", BindingFlags.NonPublic | BindingFlags.Instance);
        NpcsBehindTilesDrawMethod = typeof(Main).GetMethod("DoDraw_DrawNPCsBehindTiles", BindingFlags.NonPublic | BindingFlags.Instance);
        PlayerBehindNPCsDrawMethod = typeof(Main).GetMethod("DrawPlayers_BehindNPCs", BindingFlags.NonPublic | BindingFlags.Instance);
        PlayerAfterProjDrawMethod = typeof(Main).GetMethod("DrawPlayers_AfterProjectiles", BindingFlags.NonPublic | BindingFlags.Instance);
        ProjectileDrawMethod = typeof(Main).GetMethod("DrawProjectiles", BindingFlags.NonPublic | BindingFlags.Instance);
        drawCachedProjsMethod = typeof(Main).GetMethod("DrawCachedProjs", BindingFlags.NonPublic | BindingFlags.Instance);
        drawCachedNPCsMethod = typeof(Main).GetMethod("DrawCachedNPCs", BindingFlags.NonPublic | BindingFlags.Instance);
        dustDrawMethod = typeof(Main).GetMethod("DrawDust", BindingFlags.NonPublic | BindingFlags.Instance);
        goreDrawMethod = typeof(Main).GetMethod("DrawGore", BindingFlags.NonPublic | BindingFlags.Instance);

        //On_Main.DoDraw_WallsAndBlacks += DrawWallReflectionLayer;
        On_Main.DoDraw_Tiles_NonSolid += On_Main_DoDraw_Tiles_NonSolid;
        On_Main.CheckMonoliths += SpecialDraww;

        DrawWallReflectionNormalMapEvent += drawGlassWallReflectionNormalMap;

        GameShaders.Misc[simpleReflectionShaderPath] = new MiscShaderData(ShaderLoader.SimpleReflection, "TileReflectionPass");

        On_Main.DrawPlayers_BehindNPCs += On_Main_DrawPlayers_BehindNPCs;
        On_Main.DrawPlayers_AfterProjectiles += On_Main_DrawPlayers_AfterProjectiles;
    }

    private record struct OutfitSnapshot(int Head, int Body, int Legs,
        int handon, int handoff, int back, int front, int shoe, int waist, int shield, int neck,
        int face, int balloon, int backpack, int tail, int faceHead, int faceFlower, int balloonFront, int beard);

    private static OutfitSnapshot _outfitSnapshot;

    private void On_Main_DrawPlayers_AfterProjectiles(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
        ChangePlayerOutfit();
        orig(self);
        ResetPlayerOutfit();
    }

    private void On_Main_DrawPlayers_BehindNPCs(On_Main.orig_DrawPlayers_BehindNPCs orig, Main self) {
        ChangePlayerOutfit();
        orig(self);
        ResetPlayerOutfit();
    }

    private void ChangePlayerOutfit() {
        // TODO: test multiplayer and add MORE
        Player player = Main.LocalPlayer;
        _outfitSnapshot = new OutfitSnapshot(player.head, player.body, player.legs,
            player.handon, player.handoff, player.back, player.front, player.shoe, player.waist, player.shield, player.neck,
            player.face, player.balloon, player.backpack, player.tail, player.faceHead, player.faceFlower, player.balloonFront, player.beard);
        
        if (isDrawReflectablesThisFrame) {
            int tylerHead = EquipLoader.GetEquipSlot(RoA.Instance, nameof(SoapSellersShades), EquipType.Head),
                tylerBody = EquipLoader.GetEquipSlot(RoA.Instance, nameof(SoapSellersJacket), EquipType.Body),
                tylerLegs = EquipLoader.GetEquipSlot(RoA.Instance, nameof(SoapSellersJeans), EquipType.Legs);
            if (player.head == tylerHead) {
                player.head = ArmorIDs.Head.FamiliarWig;
            }
            if (player.body == tylerBody) {
                player.body = ArmorIDs.Body.FamiliarShirt;
            }
            if (player.legs == tylerLegs) {
                player.legs = ArmorIDs.Legs.FamiliarPants;
            }
            if (player.head == ItemID.FamiliarWig) {
                player.head = tylerHead;
            }
            if (player.body == ItemID.FamiliarShirt) {
                player.body = tylerBody;
            }
            if (player.legs == ItemID.FamiliarPants) {
                player.legs = tylerLegs;
            }

            int druidHallowedHelmet1 = EquipLoader.GetEquipSlot(RoA.Instance, nameof(HallowedVisor), EquipType.Head),
                druidHallowedHelmet2 = EquipLoader.GetEquipSlot(RoA.Instance, nameof(AncientHallowedVisor), EquipType.Head);
            if (player.head == druidHallowedHelmet1 || player.head == druidHallowedHelmet2) {
                player.head = -1;
                player.GetCommon().DrawJokeVisor = true;
            }

            //player.ResetVisibleAccessories();
        }
    }

    private void ResetPlayerOutfit() {
        // from ResetVisibleAccessories

        Player player = Main.LocalPlayer;

        if (player.head == -1 && player.GetCommon().DrawJokeVisor) {
            player.GetCommon().DrawJokeVisor = false;
        }

        player.head = _outfitSnapshot.Head;
        player.body = _outfitSnapshot.Body;
        player.legs = _outfitSnapshot.Legs;

        player.handon = _outfitSnapshot.handon;
        player.handoff = _outfitSnapshot.handoff;
        player.back = _outfitSnapshot.back;
        player.front = _outfitSnapshot.front;
        player.shoe = _outfitSnapshot.shoe;
        player.waist = _outfitSnapshot.waist;
        player.shield = _outfitSnapshot.shield;
        player.neck = _outfitSnapshot.neck;
        player.face = _outfitSnapshot.face;
        player.balloon = _outfitSnapshot.balloon;
        player.backpack = _outfitSnapshot.backpack;
        player.tail = _outfitSnapshot.tail;
        player.faceHead = _outfitSnapshot.faceHead;
        player.faceFlower = _outfitSnapshot.faceFlower;
        player.balloonFront = _outfitSnapshot.balloonFront;
        player.beard = _outfitSnapshot.beard;
    }

    void ILoadable.Unload() {
        //On_Main.DoDraw_WallsAndBlacks -= DrawWallReflectionLayer;
        On_Main.DoDraw_Tiles_NonSolid -= On_Main_DoDraw_Tiles_NonSolid;

        DrawWallReflectionNormalMapEvent -= drawGlassWallReflectionNormalMap;
    }

    private void SpecialDraww(On_Main.orig_CheckMonoliths orig) {
        orig();

        if (Main.gameMenu || Main.dedServ)
            return;

        GraphicsDevice GD = Main.graphics.GraphicsDevice;
        GD.SetRenderTarget(Target.RenderTarget);

        if (isDrawReflectablesThisFrame) {
            DrawReflectableEntities(Main.spriteBatch);
            isDrawReflectablesThisFrame = false;
        }

        GD.SetRenderTarget(null);
    }

    /// <summary>
    /// Use this event for anything that wants to add reflections to the layer right after walls are drawn, 
    /// elements higher up in the draw chain or with custom offsets will have to use the version of the reflection shader with homogenized coordinates (look at vitricbossAltar as an example)
    /// </summary>
    public static event DrawWallReflectionNormalMapDelegate DrawWallReflectionNormalMapEvent;
    public delegate void DrawWallReflectionNormalMapDelegate(SpriteBatch spritebatch);

    private static void DrawTargets(SpriteBatch sb) {
        //TODO: this may benefit from adding booleans for other places in the code to check if they're going to use the RTs since we don't necessarily need these generated on every frame for some performance improvements

        //ReflectionSubConfig reflectionConfig = ModContent.GetInstance<GraphicsConfig>().ReflectionConfig;

        //if (reflectionConfig.isReflectingAnything()) 
        {
            GraphicsDevice GD = Main.graphics.GraphicsDevice;
            canUseTarget = false;

            GD.Clear(Color.Transparent);
            Main.GameViewMatrix.Zoom = new Vector2(1, 1);
            sb.End();
            sb.Begin(SpriteSortMode.Texture, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default);

            DrawWallReflectionNormalMapEvent?.Invoke(sb);
        }

        canUseTarget = true;
    }

    private void DrawReflectableEntities(SpriteBatch sb) {
        canUseTarget = false;

        Vector2 storedZoom = Main.GameViewMatrix.Zoom;
        Main.GameViewMatrix.Zoom = new Vector2(1, 1);
        SpriteEffects storedSpriteEffects = Main.GameViewMatrix.Effects;
        Main.GameViewMatrix.Effects = SpriteEffects.None;

        //ReflectionSubConfig reflectionConfig = ModContent.GetInstance<GraphicsConfig>().ReflectionConfig;

        Main.graphics.GraphicsDevice.Clear(Color.Transparent);

        //if (reflectionConfig.NpcReflectionsOn)
            NpcsBehindTilesDrawMethod?.Invoke(Main.instance, null);

        //if (reflectionConfig.PlayerReflectionsOn)
            PlayerBehindNPCsDrawMethod?.Invoke(Main.instance, null);

        //if (reflectionConfig.NpcReflectionsOn)
            NpcsOverTilesDrawMethod?.Invoke(Main.instance, null);

        //if (reflectionConfig.ProjReflectionsOn) {
        //    drawCachedProjsMethod?.Invoke(Main.instance, new object[] { Main.instance.DrawCacheProjsBehindProjectiles, true });
        //    ProjectileDrawMethod?.Invoke(Main.instance, null);
        //}

        //if (reflectionConfig.PlayerReflectionsOn)
            PlayerAfterProjDrawMethod?.Invoke(Main.instance, new object[] { });

        //if (reflectionConfig.ProjReflectionsOn)
        //    drawCachedProjsMethod?.Invoke(Main.instance, new object[] { Main.instance.DrawCacheProjsOverPlayers, true });

        //if (reflectionConfig.NpcReflectionsOn) 
        {
            drawCachedNPCsMethod?.Invoke(Main.instance, new object[] { Main.instance.DrawCacheNPCsOverPlayers, false });

            //if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<Biomes.PermafrostTempleBiome>())) {
            //    Main.spriteBatch.Begin();
            //    DrawUnderCathedralWater.DrawWater();
            //    Main.spriteBatch.End();
            //}
        }

        //if (reflectionConfig.DustReflectionsOn) {
        //    sb.Begin(SpriteSortMode.Deferred, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
        //    try {
        //        //tml does this try catch for some reason, maybe gores are bugged in this version, v2022.3.35.3, possible TODO: remove the try catch if tml removes theirs
        //        goreDrawMethod?.Invoke(Main.instance, null);
        //    }
        //    catch (Exception e2) {
        //        TimeLogger.DrawException(e2);
        //    }

        //    sb.End();

        //    dustDrawMethod?.Invoke(Main.instance, null);
        //}

        Overlays.Scene.Draw(sb, RenderLayers.Entities, true);

        Main.GameViewMatrix.Zoom = storedZoom;
        Main.GameViewMatrix.Effects = storedSpriteEffects;

        canUseTarget = true;
    }

    /// <summary>
    /// draw background reflections immediately after wall tiles are drawn 
    /// </summary>
    /// <param name="orig"></param>
    ///
    //public void DrawWallReflectionLayer(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self) {
    //    orig(self);

    //    if (applyWallReflectionsThisFrame) {
    //        Main.spriteBatch.End();
    //        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

    //        DrawReflection(Main.spriteBatch, screenPos: Vector2.Zero, normalMap: reflectionNormalMapTarget.RenderTarget, 
    //            flatOffset: new Vector2(0.028f, 0.048f), offsetScale: 0.05f, tintColor: Color.White, restartSpriteBatch: false);
    //        applyWallReflectionsThisFrame = false;

    //        Main.spriteBatch.End();
    //        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    //    }
    //}

    /// <summary>
    /// helper function to set params onto the reflection shader and draw to screen when called
    /// </summary>
    public static void DrawReflection(SpriteBatch spriteBatch, Vector2 screenPos, Texture2D normalMap, Vector2 flatOffset, float offsetScale, Color tintColor, bool restartSpriteBatch = true, Rectangle? sourceRect = null) {
        //ReflectionSubConfig reflectionConfig = ModContent.GetInstance<GraphicsConfig>().ReflectionConfig;

        if (canUseTarget/* && reflectionConfig.isReflectingAnything()*/) {

            if (restartSpriteBatch) {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, default, RasterizerState.CullNone, default);
            }

            var data = new DrawData(normalMap, screenPos, sourceRect, new Color(100, 100, 100, 0));

            Vector2 normalMapPos = new Vector2(screenPos.X / Target.RenderTarget.Width, screenPos.Y / Target.RenderTarget.Height);

            if (sourceRect != null)
                normalMapPos = new Vector2((screenPos.X - sourceRect.Value.Left) / Target.RenderTarget.Width, (screenPos.Y - sourceRect.Value.Top) / Target.RenderTarget.Height);

            //need to force the registers into using the proper data
            Main.graphics.GraphicsDevice.Textures[1] = Target.RenderTarget;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            GameShaders.Misc[simpleReflectionShaderPath].Shader.Parameters["reflectionTargetSize"].SetValue(Target.RenderTarget.Size());
            GameShaders.Misc[simpleReflectionShaderPath].Shader.Parameters["flatOffset"].SetValue(flatOffset);
            GameShaders.Misc[simpleReflectionShaderPath].Shader.Parameters["offsetScale"].SetValue(offsetScale);
            GameShaders.Misc[simpleReflectionShaderPath].Shader.Parameters["normalMapPosition"].SetValue(normalMapPos);
            GameShaders.Misc[simpleReflectionShaderPath].Shader.Parameters["tintColor"].SetValue(tintColor.ToVector4());

            GameShaders.Misc[simpleReflectionShaderPath].Apply(data);

            data.Draw(spriteBatch);

            if (restartSpriteBatch) {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
    }

    private void On_Main_DoDraw_Tiles_NonSolid(On_Main.orig_DoDraw_Tiles_NonSolid orig, Main self) {
        orig(self);

        if (applyWallReflectionsThisFrame) {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            DrawReflection(Main.spriteBatch, screenPos: Vector2.Zero, normalMap: reflectionNormalMapTarget.RenderTarget,
                flatOffset: new Vector2(0.036f, 0.06f), offsetScale: 0.05f, tintColor: Color.DarkGray, restartSpriteBatch: false);
            applyWallReflectionsThisFrame = false;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    public void drawGlassWallReflectionNormalMap(SpriteBatch spriteBatch) {
        // TODO: test in mp
        foreach (Player player in Main.ActivePlayers) {
            int TileSearchSize = 50; //limit distance from Player for getting these wall tiles
            for (int i = -TileSearchSize; i < TileSearchSize; i++) {
                for (int j = -TileSearchSize; j < TileSearchSize; j++) {
                    var p = (player.Center / 16).ToPoint();
                    var pij = new Point(p.X + i, p.Y - j);

                    if (WorldGen.InWorld(pij.X, pij.Y)) {
                        Tile tile = Framing.GetTileSafely(pij);
                        ushort type = tile.TileType;

                        if (type == ModContent.TileType<DungeonWindow>()) {
                            Vector2 pos = pij.ToVector2() * 16;
                            Texture2D tex = TextureAssets.Tile[type].Value;
                            //not sure if tile.WallFrame* is the correct value
                            if (tex != null) {
                                for (int k = 0; k < 2; k++) {
                                    spriteBatch.Draw(TextureAssets.Tile[type].Value, pos - Main.screenPosition - Vector2.One * 0, new Rectangle(tile.TileFrameX, tile.TileFrameY, 18, 18),
                                        new Color(10, 10, 10, 125));
                                }
                            }
                            isDrawReflectablesThisFrame = true;
                            applyWallReflectionsThisFrame = true;
                        }
                    }
                }
            }
        }
    }
}