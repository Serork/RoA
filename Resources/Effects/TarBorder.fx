sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);

float3 colors[3];
float noiseEase;
float2 noiseFromPos;
float2 noiseToPos;
float2 pixel;

float4 MainPS(float2 coords : TEXCOORD0) : COLOR0
{
    float visible = max(tex2D(uImage0, coords + float2(pixel.x, 0)).a,
                    max(tex2D(uImage0, coords + float2(-pixel.x, 0)).a,
                    max(tex2D(uImage0, coords + float2(0, pixel.y)).a, tex2D(uImage0, coords + float2(0, -pixel.y)).a)));

    float2 pfrom = float2((1 + coords.x + noiseFromPos.x) % 1, (1 + coords.y + noiseFromPos.y) % 1);
    float2 pto = float2((1 + coords.x + noiseToPos.x) % 1, (1 + coords.y + noiseToPos.y) % 1);

    float from = tex2D(uImage1, pfrom).r;
    float to = tex2D(uImage2, pto).r;
    float ease = (from + (to - from) * noiseEase);
    
    return float4(colors[floor(ease * 3)], 1) * visible;
}

technique Technique1
{
    pass MainPS
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}