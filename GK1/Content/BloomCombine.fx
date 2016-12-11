texture BloomTexture;
sampler BloomSampler  : register(s3)
{
	Texture = (BloomTexture);
	Filter = Linear;
	AddressU = clamp;
	AddressV = clamp;
};

texture BaseTexture;
sampler BaseSampler  : register(s4)
{
	Texture = (BaseTexture);
	Filter = Linear;
	AddressU = clamp;
	AddressV = clamp;
};

float BloomThreshold;

float BloomIntensity;
float BaseIntensity;

float BloomSaturation;
float BaseSaturation;

#define SAMPLE_COUNT 15
float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];


float4 AdjustSaturation(float4 color, float saturation)
{
	float grey = dot((float3)color, float3(0.3, 0.59, 0.11));
	return lerp(grey, color, saturation);
}



float4 PixelShaderBloomCombineFunction(float2 texCoord : TEXCOORD0) : COLOR0
{

	float4 bloom = tex2D(BloomSampler, texCoord);
	float4 base = tex2D(BaseSampler, texCoord);
	bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
	base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
	base *= (1 - saturate(bloom));
	return base + bloom;
}


technique BloomCombine
{
	pass Pass2
	{
		PixelShader = compile ps_5_0 PixelShaderBloomCombineFunction();
	}
}
