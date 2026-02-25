sampler uImage0 : register(s0);

float waveTime;
float waveAmplitude;
float waveFrequency;
float waveSpeed;
float bendDirection;
float bendStrength;
float baseStability;
float tipWiggle;

float4 MainPS(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float tipFactor = texCoord.y;
    
    float wave = sin(texCoord.y * waveFrequency * 10 + waveTime * waveSpeed) * waveAmplitude;
    
    float bend = tipFactor * bendDirection * bendStrength * 0.5;
    
    float stability = 1.0 - min(1.0, max(0, tipFactor - baseStability) / max(0.001, 1.0 - baseStability));
    
    float jitter = 0;
    if (tipFactor > 0.7)
    {
        jitter = sin(texCoord.y * 50 + waveTime * 15) * waveAmplitude * tipWiggle * 0.3;
    }
    
    float totalOffset = wave * stability + bend + jitter;
    
    float2 distortedCoord = texCoord;
    distortedCoord.x += totalOffset;
    distortedCoord.x = frac(distortedCoord.x);
    
    float4 texColor = tex2D(uImage0, distortedCoord);
    texColor *= color;
    
    return texColor;
}

technique FilamentThread
{
    pass WormholeTentacle
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}