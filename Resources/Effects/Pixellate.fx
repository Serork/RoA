sampler uImage0 : register(s0);

uniform float pixelDensity = 16.0;
uniform float2 bufferSize = float2(320, 180); 
uniform float2 screenSize = float2(1280, 720);

float2 pixelize(float2 uv, float2 bufferDim, float2 screenDim)
{
    float2 screenPos = uv * screenDim;
    
    float2 lowResPos = screenPos / bufferDim;
    
    float2 pixelPos = floor(lowResPos);
    
    float2 finalLowResPos = pixelPos + 0.5;
    
    return finalLowResPos * bufferDim / screenDim;
}

float2 pixelizeSimple(float2 uv, float density)
{
    return (floor(uv * density) + 0.5) / density;
}

float4 MainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelizedUV = pixelize(coords, bufferSize, screenSize);
    pixelizedUV = pixelizeSimple(pixelizedUV, pixelDensity);
    
    float4 texColor = tex2D(uImage0, pixelizedUV);
    
    texColor *= sampleColor;
    
    return texColor;
}

technique FilamentThread
{
    pass Pixellate
    {
        PixelShader = compile ps_2_0 MainPS();
    }
};