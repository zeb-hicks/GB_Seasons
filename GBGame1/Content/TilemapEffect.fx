#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state {
	Texture = <SpriteTexture>;
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Wrap;
	AddressV = Wrap;
};
Texture2D FadeLut;
sampler2D FadeLutSampler = sampler_state {
	Texture = <FadeLut>;
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

float FadeAmount;
float2 FadePos;

struct VertexShaderOutput {
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input, float2 vPos : VPOS) : COLOR {
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;

	float2 lutuv = (vPos - FadePos) / float2(160.0, 144.0);

	float fade = clamp(1.0 - tex2D(FadeLutSampler, lutuv).r, 0.0, 1.0);
	float alpha = 1.0;

	//if (fade < FadeAmount) alpha = 0.0;
	alpha = 1.0 - (1.0 - fade) * FadeAmount;

	return float4(color.rgb, color.a * alpha);
}

technique SpriteDrawing {
	pass P0 {
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};