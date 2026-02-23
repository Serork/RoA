sampler uImage0 : register(s0);

float uTime = 0.0;
float waveCount1 = 6.0;
float waveCount2 = 6.0;
float waveSize1 = 0.08;
float waveSize2 = 0.01;
float waveRadius = 0.5;

float4 MainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords * 2.0 - 1.0;
    
    float angle = uv.x / (uv.y + 0.001) + uTime;
    
    float dist2 = uv.x * uv.x + uv.y * uv.y;
    
    float wave = sin(angle * waveCount1) * waveSize1;
    float radius2 = waveRadius + wave;
    
    float mask = saturate((radius2 + 0.1 - dist2) * 4.0);
    
    float2 duv = coords;
    duv.x += sin(uv.y * waveCount2 + uTime) * waveSize2 * mask;
    duv.y += cos(uv.x * waveCount2 - uTime) * waveSize2 * mask;
    
    float4 color = tex2D(uImage0, duv);
    color.a *= mask;
    
    return color * sampleColor;
}

technique WavyCircle
{
    pass MainPS
    {
        PixelShader = compile ps_2_0 MainPS();
    }
};