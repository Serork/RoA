sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float2 uTargetPosition;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

const float TWO_PI_OVER_3 = 6.28 / 3;

float2 FrameFix(float2 coords)
{
    float pixelSize = 2;
    float frameSizeX = uSourceRect.z / uImageSize0.x;
    float x = floor(coords.x * uImageSize0.x / pixelSize) / uImageSize0.x * pixelSize % frameSizeX;
    float frameSizeY = uSourceRect.w / uImageSize0.y;
    float y = floor(coords.y * uImageSize0.y / pixelSize) / uImageSize0.y * pixelSize % frameSizeY;
    return float2(x / frameSizeX, y / frameSizeY);
}

float normalsin(float time)
{
    return (sin(time) + 1) / 2;
}

float4 Recolor(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 origColor = tex2D(uImage0, coords);
    float time = uTime * 20;
    float2 textureCoords = FrameFix(coords);
    float aLERP = normalsin(origColor.a * 10 + time);
    origColor.a = lerp(origColor.a, aLERP, 0.5 * origColor.a * uSaturation);
    origColor.b = lerp(origColor.b, aLERP, 0.75 * origColor.a * uSaturation);
    origColor.g = lerp(origColor.g, aLERP, 0.5 * origColor.a * uSaturation);
    origColor.r = lerp(origColor.r, aLERP, 0.15 * origColor.a * uSaturation);
    return origColor * sampleColor;
}

technique Technique1
{
    pass LightCompressorPass
    {
        PixelShader = compile ps_2_0 Recolor();
    }
}