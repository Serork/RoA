sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float Radius;
float FadeDistance;

float smoothProgress(float progress) {
    progress = lerp(-3.14/2, 3.14/2, progress);
    progress = sin(progress);
    progress = (progress/2) + .5;
    return progress;
}

float4 MainPS(float2 coords : TEXCOORD0) : COLOR0
{
    float INTENSITY = 15.0;
    float4 BORDERCOLOR = float4(uColor.r, uColor.g, uColor.b, 0.0);

    float4 color = tex2D(uImage0, coords);
    float vig = INTENSITY * coords.x * coords.y * (1.0 - coords.x) * (1.0 - coords.y);
    vig = pow(vig, uIntensity * 0.25);
    color = lerp(BORDERCOLOR, color * vig, vig);
    return color;
}

technique Technique1
{
    pass MainPS
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}