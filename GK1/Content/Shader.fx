#define NUM_LIGHTS 4
#define REFLECTOR_LIGHT 0
#define POINT_LIGHT 1
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

texture BasicTexture;

sampler BasicTextureSampler = sampler_state {
	texture = <BasicTexture>;
	MinFilter = Anisotropic; 
	MagFilter = Anisotropic;
	MipFilter = Linear; 
	AddressU = Wrap; 
	AddressV = Wrap; 
};

bool TextureEnabled = true;

float3 DiffuseColor = float3(1, 1, 1);
float3 AmbientColor = float3(0.1, 0.1, 0.1);
float3 LightDirection[NUM_LIGHTS];
float3 LightPosition[NUM_LIGHTS];
float3 LightColor[NUM_LIGHTS];
float SpecularPower = 32;
float3 SpecularColor = float3(1, 1, 1);
float3 LightAttenuation[NUM_LIGHTS];
float3 LightFalloff[NUM_LIGHTS];
float LightType[NUM_LIGHTS];

float FogStart = 2;
float FogEnd = 10;
float3 FogColor = float3(0.5, 0.5, 0.5);
float FogIntensity = 0.3f;
bool FogEnabled;

struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
	float4 WorldPosition : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);

	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.UV = input.UV;
	output.Normal = mul(input.Normal, World);
	output.ViewDirection = worldPosition - LightPosition[1];

	return output;
}

float4 AddReflectorLight(int i, VertexShaderOutput input)
{
	float3 color = DiffuseColor;

	if (TextureEnabled)
		color *= tex2D(BasicTextureSampler, input.UV);

	float3 lighting = AmbientColor;

	float3 lightDir = normalize(LightDirection[i]);
	float3 normal = normalize(input.Normal);

	lighting += saturate(dot(lightDir, normal)) * LightColor[i];

	float3 refl = reflect(lightDir, normal);
	float3 view = normalize(input.ViewDirection);

	lighting += pow(saturate(dot(refl, view)), SpecularPower) * SpecularColor;

	float3 output = saturate(lighting) * color;

	return float4(output, 1);
}

float4 AddPointLight(int i, VertexShaderOutput input)
{
	float3 diffuseColor = DiffuseColor;

	if (TextureEnabled)
		diffuseColor *= tex2D(BasicTextureSampler, input.UV).rgb;
	float3 totalLight = float3(0, 0, 0);

	totalLight += AmbientColor;

	float3 lightDir = normalize(LightPosition[i] - input.WorldPosition);
	float diffuse = saturate(dot(normalize(input.Normal), lightDir));
	float d = distance(LightPosition[i], input.WorldPosition);
	float att = 1 - pow(clamp(d / LightAttenuation[i], 0, 1),
		LightFalloff[i]);

	totalLight += diffuse * att * LightColor[i];

	return float4(diffuseColor * totalLight, 1);
}

float4 CalculateLights(VertexShaderOutput input)
{
	float4 outColor = float4(0, 0, 0, 0);
	//outColor += AddReflectorLight(0, input);
	outColor += AddReflectorLight(1, input);
	outColor += AddPointLight(2, input);
	outColor += AddPointLight(3, input);

	float dist = length(input.ViewDirection);
	float fog = clamp((dist - FogStart) / (FogEnd - FogStart), 0, 1);

	if (FogEnabled)
		return float4(lerp(outColor, FogColor, fog * FogIntensity), 1);
	else
		return outColor;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return CalculateLights(input);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}