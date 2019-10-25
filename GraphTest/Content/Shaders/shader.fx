#include "Data.h"
#include "Toon.h"
#include "Hot.h"
#include "Blur.h"
#include "Lighting.h"

VSOut MeshVS(in Mesh input)
{
    VSOut output;

    output.Position = mul(input.Position, _matrix);
    output.TextureCoordinate = input.TextureCoordinate;
    output.Color = _color;
    output.WorldPosition = mul(input.Position, _modelTransform).xyz;
    output.Normal = input.Normal.rgb;

    return output;
}

VSOut PrimitiveVS(in Primitive input)
{
    VSOut output;

    output.Position = mul(input.Position, _matrix);
    output.TextureCoordinate = input.TextureCoordinate;
    output.Color = input.Color;
    output.WorldPosition = mul(input.Position, _modelTransform).xyz;
    output.Normal = input.Normal.rgb;

    return output;
}

Target PS(in VSOut input)
{
    Target outp = (Target) 0;

    CHECKDEPTH;

    float4 color = input.Color;
    if (_textureenabled)
        color *= tex2D(textureSampler, input.TextureCoordinate);

    outp.Color = color;
    if (_writeOnlyColor)
        return outp;

    outp.Position = CreateFloat4(input.WorldPosition, 1.0);
    outp.Normal = CreateFloat4(input.Normal, 1);
    outp.Depth = float4(input.Position.z / input.Position.w, 0, 0, 1.0);

    if (_specular)
        outp.Depth.g = 1.0;

    return outp;
}


TECHNIQUE(MeshStandart, MeshVS, PS);
TECHNIQUE(MeshWriteDepth, MeshVS, WallPS0);
TECHNIQUE(MeshSoftShadows, MeshVS, SoftShadows);

TECHNIQUE(PrimitiveStandart, PrimitiveVS, PS);
TECHNIQUE(PrimitiveWriteDepth, PrimitiveVS, WallPS0);
TECHNIQUE(PrimitiveSoftShadows, PrimitiveVS, SoftShadows);
TECHNIQUE(PrimitiveHot, PrimitiveVS, HotPS);
TECHNIQUE(PrimitiveBlur, PrimitiveVS, Blur);

TECHNIQUE(PrimitiveApplyLighting, PrimitiveVS, CalculateColorLighting);
TECHNIQUE(PrimitiveGamma, PrimitiveVS, ApplyGamma);
TECHNIQUE(PrimitiveApplyAmbient, PrimitiveVS, ApplyAmbient);


technique PrimitiveToon
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PrimitiveToonVS0();
        PixelShader = compile PS_SHADERMODEL ToonPS0();
    }
    pass P1
    {
        VertexShader = compile VS_SHADERMODEL PrimitiveToonVS1();
        PixelShader = compile PS_SHADERMODEL ToonPS1();
    }
    pass P2
    {
        VertexShader = compile VS_SHADERMODEL PrimitiveToonVS2();
        PixelShader = compile PS_SHADERMODEL ToonPS2();
    }
};

technique MeshToon
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MeshToonVS0();
        PixelShader = compile PS_SHADERMODEL ToonPS0();
    }
    pass P1
    {
        VertexShader = compile VS_SHADERMODEL MeshToonVS1();
        PixelShader = compile PS_SHADERMODEL ToonPS1();
    }
    pass P2
    {
        VertexShader = compile VS_SHADERMODEL MeshToonVS2();
        PixelShader = compile PS_SHADERMODEL ToonPS2();
    }
};