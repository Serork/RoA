// wrath of gods

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

float splitIntensity;
float2 impactPoint;

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
    float2 textureCoords = FrameFix(coords);
    
    // Split colors into their composite RGB values.
    float splitDistance = splitIntensity * 0.048 / (distance(textureCoords, impactPoint) + 1);
    float4 result = origColor;
    result.r = tex2D(uImage0, textureCoords + float2(-0.707, -0.707) * splitDistance).r;
    result.g = tex2D(uImage0, textureCoords + float2(0.707, -0.707) * splitDistance).g;
    result.b = tex2D(uImage0, textureCoords + float2(0, 1) * splitDistance).b;
    
    return lerp(origColor.rgba, result.rgba, uSaturation) * sampleColor;
}

technique Technique1
{
    pass ChromaticAbberationPass
    {
        PixelShader = compile ps_2_0 Recolor();
    }
}