#ifndef TOON_DEF
#define TOON_DEF

#include "Data.h"

VSOut MeshToonVS0(in Mesh input)
{
    VSOut output;

    output.Position = mul(input.Position, _matrix);
    output.Position.xy += 0.05 * float2(cos(_time * 10), sin(_time * 10));

    output.Color = _color;

    return output;
}

VSOut MeshToonVS1(in Mesh input)
{
    VSOut output;

    output.Position = mul(input.Position, _matrix);
    output.Position.xy += 0.05 * float2(cos(_time * 10 + 1.0 / 3.0 * 6.28), sin(_time * 10 + 1.0 / 3.0 * 6.28));

    output.Color = _color;

    return output;
}

VSOut MeshToonVS2(in Mesh input)
{
    VSOut output;

    output.Position = mul(input.Position, _matrix);
    output.Position.xy += 0.05 * float2(cos(_time * 10 + 2.0 / 3.0 * 6.28), sin(_time * 10 + 2.0 / 3.0 * 6.28));

    output.Color = _color;

    return output;
}

VSOut PrimitiveToonVS0(in Primitive input)
{
    VSOut output;

    output.Position = mul(input.Position, _matrix);
    output.Position.xy += 0.05 * float2(cos(_time * 10), sin(_time * 10));

    output.Color = input.Color;

 
    return output;
}

VSOut PrimitiveToonVS1(in Primitive input)
{
    VSOut output;

    output.Position = mul(input.Position, _matrix);
    output.Position.xy += 0.05 * float2(cos(_time * 10 + 1.0 / 3.0 * 6.28), sin(_time * 10 + 1.0 / 3.0 * 6.28));

    output.Color = input.Color;

    return output;
}

VSOut PrimitiveToonVS2(in Primitive input)
{
    VSOut output;

    output.Position = mul(input.Position, _matrix);
    output.Position.xy += 0.05 * float2(cos(_time * 10 + 2.0 / 3.0 * 6.28), sin(_time * 10 + 2.0 / 3.0 * 6.28));

    output.Color = input.Color;

    return output;
}

float4 ToonPS0(in VSOut input) : SV_Target0
{
    return float4(input.Color.r, 0, 0, 1);
}

float4 ToonPS1(in VSOut input) : SV_Target0
{
    return float4(0, input.Color.g, 0, 1);
}

float4 ToonPS2(in VSOut input) : SV_Target0
{
    return float4(0, 0, input.Color.b, 1);
}

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

#endif