float4x4 View;
float4x4 Projection;
float4x4 World;
texture ParticleTexture;
sampler2D texSampler = sampler_state {
	texture = <ParticleTexture>;
};
float2 Size;
float3 Up; 
float3 Side;

float4 ClipPlane;
bool ClipPlaneEnabled = false;

bool AlphaTest = true;
bool AlphaTestGreater = true;
float AlphaTestValue = 0.5f;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Position1 : POSITION1;
	float2 UV : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float3 position = input.Position;
	output.Position1 = position;

	float2 offset = float2((input.UV.x - 0.5f) * 2.0f,
		-(input.UV.y - 0.5f) * 2.0f);
	position += offset.x * Size.x * Side + offset.y * Size.y * Up;
	output.Position = mul(float4(position, 1), mul(View, Projection));
	output.UV = input.UV;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if (ClipPlaneEnabled)
		clip(dot(input.Position1, ClipPlane.xyz) + ClipPlane.w);
	float4 color = tex2D(texSampler, input.UV);
	if (AlphaTest)
		clip((color.a - AlphaTestValue) * (AlphaTestGreater ? 1 : -1));

	return color;
}
technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}