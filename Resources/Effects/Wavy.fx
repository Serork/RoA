sampler uImage0 : register(s0);

uniform float waveFactor = 0.0;
uniform float strengthX = 1.0;
uniform float strengthY = 1.0;
uniform float timeFactor = 1.0;
uniform float yFactor = 0.5;
uniform float4 drawColor = (1.0, 1.0, 1.0, 1.0);

float2 GetDisplacement(float2 texCoords)
{
    float2 position = texCoords;
    position.x += strengthX * sin(strengthY * position.y + timeFactor * waveFactor) * (1.0 - pow(position.y, yFactor));

    return position;
}

struct VertexShaderInput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

float4 MainPS(VertexShaderInput input) : COLOR0
{
    float4 color = tex2D(uImage0, GetDisplacement(input.TextureCoordinates));
    color *= drawColor;
    return color;
}

technique Wavy
{
    pass MainPS
    {
        PixelShader = compile ps_2_0 MainPS();
    }
};