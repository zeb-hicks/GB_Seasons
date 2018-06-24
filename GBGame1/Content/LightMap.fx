#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float ScreenFlash;
float GlowAmount;
float2 GlowPos;
float2 Resolution;

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
	Texture = <SpriteTexture>;
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Wrap;
	AddressV = Wrap;
};
Texture2D ColorLUT;
sampler2D ColorLUTSampler = sampler_state {
	Texture = <ColorLUT>;
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};
Texture2D GlowLUT;
sampler2D GlowLUTSampler = sampler_state {
	Texture = <GlowLUT>;
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 sprite = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float lum = sprite.r * 4.0 / 5.0 + 1.0 / 8.0;
	lum /= 1.0 - ScreenFlash;
	float3 lut = tex2D(ColorLUTSampler, float2(lum, 0.5)).rgb;

	return float4(lut, 1.0);
	//return tex2D(ColorLUTSampler,input.TextureCoordinates) * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};