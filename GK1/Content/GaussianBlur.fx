
texture BaseTexture;
sampler BaseSampler  : register(s4)
{
	Texture = (BaseTexture);
	Filter = Linear;
	AddressU = clamp;
	AddressV = clamp;
};

#define SAMPLE_COUNT 15
float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];


float4 PixelShaderGaussianBlurFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 c = 0;
	for (int i = 0; i < SAMPLE_COUNT; i++)
	{
		c += tex2D(BaseSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
	}
	return c;
}





technique GaussianBlur
{
	pass Pass1
	{
		PixelShader = compile ps_5_0 PixelShaderGaussianBlurFunction();
	}
}