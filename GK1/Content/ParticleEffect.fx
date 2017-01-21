float4x4 View;
float4x4 Projection;
texture ParticleTexture;
sampler2D texSampler = sampler_state {
	texture = <ParticleTexture>;
};
float Time;
float Lifespan;
float2 Size;
float3 Wind;
float3 Up;
float3 Side;
float FadeInTime;

float4 ClipPlane;
bool ClipPlaneEnabled = false;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Direction : TEXCOORD1;
	float Speed : TEXCOORD2;
	float StartTime : TEXCOORD3;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Position1 : POSITION1;
	float2 UV : TEXCOORD0;
	float2 RelativeTime : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position1 = input.Position;
	float3 position = float3((input.Position.x / 3) * (input.Position.x / 3), (input.Position.y / 3) *(input.Position.y / 3), (input.Position.x / 3) * (input.Position.x / 3) + (input.Position.y / 3) *(input.Position.y / 3));
	float2 offset = Size * float2((input.UV.x - 0.5f) * 2.0f, -(input.UV.y - 0.5f) * 2.0f);
	position += offset.x * Side + offset.y * Up + float3(-10, 10, 0);
	float relativeTime = (Time - input.StartTime);
	output.RelativeTime = relativeTime;
	position += (input.Direction * input.Speed + Wind) * relativeTime;
	output.Position = mul(float4(position, 1), mul(View, Projection));
	output.UV = input.UV;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if (ClipPlaneEnabled)
		clip(dot(input.Position1, ClipPlane.xyz) + ClipPlane.w);
	clip(input.RelativeTime);
	float4 color = tex2D(texSampler, input.UV);
	float d = clamp(1.0f - pow((input.RelativeTime / Lifespan), 10), 0, 1);

	d *= clamp((input.RelativeTime / FadeInTime), 0, 1);
	return float4(color * d);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}