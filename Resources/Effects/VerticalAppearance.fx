sampler uImage0 : register(s0);

float progress;
float size;
float size2;
float min;
float max;
float4 drawColor;

float4 MainPS(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    color *= drawColor;
    float y = 1.0 - coords.y;
    float visibility = smoothstep(progress - size, progress + size, y);    
    color *= clamp(visibility, 0.0, 1.0);
    color.rgb *= clamp(smoothstep(progress, progress + size2, y), min, max);
    return color;
}

technique Technique1
{
    pass MainPS
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}