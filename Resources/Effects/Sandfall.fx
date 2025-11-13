sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;

float borderTop;
float borderBottom;

float4 MainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float2 localCoords = (coords - uSourceRect.xy) / uSourceRect.zw;
    if (localCoords.y < borderBottom)
    {
        float fade = (borderBottom - localCoords.y) / borderBottom;
        color = lerp(color, float4(0, 0, 0, 0), fade);
    }
    if (localCoords.y > (1.0 - borderTop))
    {
        float fade = (localCoords.y - (1.0 - borderTop)) / borderTop;
        color = lerp(color, float4(0, 0, 0, 0), fade);
    }
    color.rgb *= uColor;
    color.a *= uOpacity;
    return color;
}

technique Technique1
{
    pass MainPS
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}