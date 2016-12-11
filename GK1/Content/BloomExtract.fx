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




float4 PixelShaderBloomExtractFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 c = tex2D(BaseSampler, texCoord);
	return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}




technique BloomExtract
{
	pass Pass1
	{
		PixelShader = compile ps_5_0 PixelShaderBloomExtractFunction();
	}
}