sampler uImage0 : register(s0);

float waveFrequency = 0.0;
float wavePhase = 0.0;
float waveAmplitude = 0.0;

float2 GetDisplacement(float2 texCoords)
{
    float2 position = texCoords;
    float waveOffset = sin(position.x * waveFrequency * 6.28318530717958647693 + wavePhase) * waveAmplitude;
    position.y += waveOffset;

    return position;
}

float4 MainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, GetDisplacement(coords));
    float4 result = color.rgba * sampleColor;
    return result;
}

technique FilamentThread
{
    pass MainPS
    {
        PixelShader = compile ps_2_0 MainPS();
    }
};