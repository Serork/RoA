using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoA.Core.Data;

struct Vertex2D(Vector2 position, Color color, Vector3 texCoord) : IVertexType {
    private static readonly VertexDeclaration _vertexDeclaration = new([new(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                                                                        new(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                                                                        new(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)]);

    public Vector2 Position = position;
    public Color Color = color;
    public Vector3 TexCoords = texCoord;

    public override string ToString() => $"[{Position}, {Color}, {TexCoords}]";

    public VertexDeclaration VertexDeclaration => _vertexDeclaration;
}