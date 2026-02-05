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

float uSine = 0.5f;

float NormalSin(float time)
{
    return 1 - (sin(time) + 1) / 2;
}

float2 FrameFix(float2 coords)
{
    float frameSizeX = uSourceRect.z / uImageSize0.x;
    float x = coords.x % frameSizeX;
    float frameSizeY = uLegacyArmorSourceRect.w / uImageSize0.y;
    float y = coords.y % frameSizeY;
    return float2(x * 1 / frameSizeX, y * 1 / frameSizeY);
}

float3 Screen(float3 base, float3 screen)
{
    return float3(1, 1, 1) - 2 * (float3(1, 1, 1) - base) * (float3(1, 1, 1) - screen);
}

float4 Recolor(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 origColor = tex2D(uImage0, coords);
    float time = uTime * 10;
    float2 textureCoords = FrameFix(coords);   
    float opacity = 1; 
    float screenSine = 0.8 + 0.2 * sin(uTime * 0.6 + uSaturation * 0.6 + 0.6);
    float3 screenedColor = Screen(origColor.rgb, uColor) * sampleColor.a * origColor.a;
    origColor.rgb = lerp(origColor.rgb, screenedColor, (0.5 - 0.5 * textureCoords.y) * screenSine * uOpacity);
    sampleColor.rgb = lerp(sampleColor.rgb, screenedColor, 0.2 * uOpacity);
    origColor = lerp(origColor, tex2D(uImage0, float2((coords.x + sin(uTime * 15 + textureCoords.y * 50) * (2 / uImageSize0.x)) % 1, coords.y)), uSine * uOpacity);
    opacity = max(0.25, opacity);
    
    return origColor * sampleColor * opacity;
}

technique Technique1
{
    pass FlameTintPass
    {
        PixelShader = compile ps_2_0 Recolor();
    }
}