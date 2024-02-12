#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

bool Tint;
matrix WorldViewProjection;
matrix World;
matrix View;
matrix Projection;
texture Texture;

sampler BasicTextureSampler = sampler_state
{
    texture = <Texture>;
    MinFilter = Anisotropic; // Minification Filter
    MagFilter = Linear;      // Magnification Filter
    MipFilter = Linear;      // Mip-mapping
    AddressU = Wrap;         // Address Mode for U Coordinates
    AddressV = Wrap;         // Address Mode for V Coordinates
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Position = mul(input.Position, WorldViewProjection);
    output.UV = input.UV;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 output = tex2D(BasicTextureSampler, input.UV);
    if (Tint)
        output.r = 1;
    return float4(output.rgb, 1);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};