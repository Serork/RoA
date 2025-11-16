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
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float NormalSin(float time)
{
    return (sin(time) + 1) / 2;
}

float2 FrameFix(float2 coords)
{
    float frameSizeX = uSourceRect.z / uImageSize0.x;
    float x = coords.x % frameSizeX;
    float frameSizeY = uLegacyArmorSourceRect.w / uImageSize0.y;
    float y = coords.y % frameSizeY;
    return float2(x * 1 / frameSizeX, y * 1 / frameSizeY);
}

float4 Recolor(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 origColor = tex2D(uImage0, coords);
    float time = uTime * 10;
    float2 textureCoords = FrameFix(coords);
    float r = NormalSin(origColor.r * 5 + time + textureCoords.y);
    origColor.r = lerp(origColor.r, r, 0.5 * origColor.a);
    float g = NormalSin(origColor.g * 5 + time + textureCoords.x);
    origColor.g = lerp(origColor.g, g, 0.5 * origColor.a);
    return origColor * sampleColor;
}

technique Technique1
{
    pass GodDescentPass
    {
        PixelShader = compile ps_2_0 Recolor();
    }
}