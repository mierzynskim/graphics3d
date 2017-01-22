float2 PixelSize;
float time;
float dt;

texture LiquidTexture;
sampler2D LiquidTextureSampler : register(s0)
= sampler_state {
    Texture = (LiquidTexture);
};

texture Perlin;
sampler2D PerlinSampler : register(s1)
= sampler_state {
    Texture = (Perlin);
    AddressU = WRAP;
    AddressV = WRAP;
};


float4 PixelShaderLiquidFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 h = float4(0.0f, 0.0f, 0.0f, 0.0f);

    h.x = tex2D(LiquidTextureSampler, texCoord).r;
    h.y = tex2D(LiquidTextureSampler, texCoord + float2(-PixelSize.x, -PixelSize.y)).r; // LU
    h.z = tex2D(LiquidTextureSampler, texCoord + float2(-PixelSize.x, 0)).r; // RU
    h.w = tex2D(LiquidTextureSampler, texCoord + float2(0, -PixelSize.y)).r; // U

    float d = tex2D(PerlinSampler, texCoord + float2(8.0*time,time)).r * 2.0 - 1.0;
    float e = tex2D(PerlinSampler, texCoord).r;
    float r;
    float a = 100.0*dt;
    float w = 0.4;
    if (d < 0.0)
    {
        d = -d;
        r = a * e * ((1.0 - d) * h.w + d * h.y) + (1.0) * h.x - w * a * e;
    }
    else
        r = a * e * ((1.0 - d) * h.w + d * h.z) + (1.0) * h.x - w * a * e;

    return float4(r, r, r, 1.0f);
}

technique LiquidTechnique
{
    pass Pass1
    {
        PixelShader = compile ps_5_0 PixelShaderLiquidFunction();
    }
}