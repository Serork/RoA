sampler uImage0 : register(s0); 
sampler uTransformImage : register(s1);
float uTime; 
float uOpacity;

float4 Recolor(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    if (!any(color))
    {
        return color * uOpacity;
    }
    float2 barCoord = float2((coords.x * 0.1 - uTime * 0.5) % 1.0, (coords.y * 0.1 + uTime * 0.5) % 1.0);
    if (barCoord.x < 0)
    {
        barCoord.x = 1 + barCoord.x;
    }
	
    float4 result = tex2D(uTransformImage, barCoord) * 0.5 + color;
    return result * sampleColor * uOpacity;
}

technique Technique1
{
    pass TerraDyePass
    {
        PixelShader = compile ps_2_0 Recolor();
    }
}