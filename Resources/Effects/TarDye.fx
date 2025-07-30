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

texture rgbNoise;
sampler2D rgbNoiseTex = sampler_state
{
    texture = <rgbNoise>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};
float3 liquidColor1;
float3 liquidColor2;

float1 Wave(float1 x, float1 y)
{
    return (float)sin(((double)((cos(x + uTime) + sin(y + uTime))) - uTime / 2.0) * 6.2831854820251465);
}

float4 Recolor(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 frameUv = (coords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw;
    float4 baseColor = tex2D(uImage0, coords);
    float2 rgbNoiseUv = frameUv * (uSourceRect.zw / uImageSize1) * 0.5;
    rgbNoiseUv = floor(rgbNoiseUv * uImageSize1) / uImageSize1;
    rgbNoiseUv += float2(0, uTime * 0.05);
    float3 rgbNoise = tex2D(rgbNoiseTex, rgbNoiseUv).rgb;
    float4 color = tex2D(uImage0, coords);
    float shimmerWave = Wave(frameUv.x, frameUv.y);
    float3 brightColor = liquidColor1;
    float3 darkColor = liquidColor2;
    float3 output = lerp(brightColor, darkColor, shimmerWave) * baseColor.a * sampleColor.a;
    float3 returnColor = color.rgb * output.rgb;
    float1 glow = step(0.97, rgbNoise.b) * uSecondaryColor * output;
    returnColor += glow;
    returnColor = lerp(baseColor.rgb, returnColor, min(uOpacity, 1));
    float opacity = baseColor.a * sampleColor.a;
    return float4(returnColor * sampleColor.rgb * opacity, opacity);
}

technique Technique1
{
    pass TarDyePass
    {
        PixelShader = compile ps_2_0 Recolor();
    }
}