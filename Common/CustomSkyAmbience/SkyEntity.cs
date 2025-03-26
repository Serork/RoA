using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Ambience;

namespace RoA.Common.CustomSkyAmbience;

abstract class SkyEntity {
    public Vector2 Position;
    public Asset<Texture2D> Texture;
    public SpriteFrame Frame;
    public float Depth;
    public SpriteEffects Effects;
    public bool IsActive = true;
    public float Rotation;

    public Rectangle SourceRectangle => Frame.GetSourceRectangle(Texture.Value);

    protected void NextFrame() {
        Frame.CurrentRow = (byte)((Frame.CurrentRow + 1) % Frame.RowCount);
    }

    public abstract Color GetColor(Color backgroundColor);

    public abstract void Update(int frameCount);

    protected void SetPositionInWorldBasedOnScreenSpace(Vector2 actualWorldSpace) {
        Vector2 vector = actualWorldSpace - Main.Camera.Center;
        Vector2 position = Main.Camera.Center + vector * (Depth / 3f);
        Position = position;
    }

    public abstract Vector2 GetDrawPosition();

    public virtual void Draw(SpriteBatch spriteBatch, float depthScale, float minDepth, float maxDepth) {
        CommonDraw(spriteBatch, depthScale, minDepth, maxDepth);
    }

    public void CommonDraw(SpriteBatch spriteBatch, float depthScale, float minDepth, float maxDepth) {
        if (!(Depth <= minDepth) && !(Depth > maxDepth)) {
            Vector2 drawPositionByDepth = GetDrawPositionByDepth();
            Color color = GetColor(Main.ColorOfTheSkies) * Main.atmo;
            Vector2 origin = SourceRectangle.Size() / 2f;
            float scale = depthScale / Depth;
            spriteBatch.Draw(Texture.Value, drawPositionByDepth - Main.Camera.UnscaledPosition, SourceRectangle, color, Rotation, origin, scale, Effects, 0f);
        }
    }

    internal Vector2 GetDrawPositionByDepth() => (GetDrawPosition() - Main.Camera.Center) * new Vector2(1f / Depth, 0.9f / Depth) + Main.Camera.Center;

    internal float Helper_GetOpacityWithAccountingForOceanWaterLine() {
        Vector2 vector = GetDrawPositionByDepth() - Main.Camera.UnscaledPosition;
        int num = SourceRectangle.Height / 2;
        float t = vector.Y + num;
        float yScreenPosition = AmbientSkyDrawCache.Instance.OceanLineInfo.YScreenPosition;
        float lerpValue = Utils.GetLerpValue(yScreenPosition - 10f, yScreenPosition - 2f, t, clamped: true);
        lerpValue *= AmbientSkyDrawCache.Instance.OceanLineInfo.OceanOpacity;
        return 1f - lerpValue;
    }
}
