#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D LightingRender;
sampler2D LightingRenderSampler = sampler_state
{
	Texture = <LightingRender>;
}; 

Texture2D LightingStencilRender;
sampler2D LightingStencilRenderSampler = sampler_state
{
	Texture = <LightingStencilRender>;
};

float4 CurrentLighting;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 white = float4(0.1f, 1.0f, 1.0f, 1.0f);
	float4 clear = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 color = tex2D(LightingRenderSampler, input.TextureCoordinates);
	float4 colorStencil = tex2D(LightingStencilRenderSampler, input.TextureCoordinates);
	if (colorStencil.r == CurrentLighting.r)
	{
		return color.rgba = colorStencil.rgba;
	}
	else
	{
		return color;
	}
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};