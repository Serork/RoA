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

float2 FrameFix(float2 coords)
{
    float pixelSize = 2;
    float frameSizeX = uSourceRect.z / uImageSize0.x;
    float x = floor(coords.x * uImageSize0.x / pixelSize) / uImageSize0.x * pixelSize % frameSizeX;
    float frameSizeY = uSourceRect.w / uImageSize0.y;
    float y = floor(coords.y * uImageSize0.y / pixelSize) / uImageSize0.y * pixelSize % frameSizeY;
    return float2(x / frameSizeX, y / frameSizeY);
}

float4 Recolor(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float2 textureCoords = FrameFix(coords);
    color = lerp(color, tex2D(uImage0, float2((coords.x + sin(uTime * 10 + textureCoords.y * 30 * uSaturation) * (2 / uImageSize0.x)) % 1, coords.y)), 0.5f);
    color.r *= uColor.r;
    color.g *= uColor.g;
    color.b *= uColor.b;
    return color * sampleColor;
}

technique Technique1
{
    pass WreathDyePass
    {
        PixelShader = compile ps_2_0 Recolor();
    }
}