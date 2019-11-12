#ifndef CHROMAB_DEF
#define CHORMAB_DEF

#include "Data.hlsl"

uniform extern float2 _chromaticAbberationAmount;

float4 ChromaticAbberationPS(in VSOut input) : SV_Target0
{
    float2 offset = float2(_chromaticAbberationAmount.x / _screenSize.x, _chromaticAbberationAmount.y / _screenSize.y);
    float dist = distance(input.TextureCoordinate, float2(0.5, 0.5));

    return float4
    (
        tex2D(textureSampler, input.TextureCoordinate + offset * dist).r,
        tex2D(textureSampler, input.TextureCoordinate - offset * dist).g,
        tex2D(textureSampler, input.TextureCoordinate + float2(offset.x, 0) * dist).b,
        1
    );
}

TECHNIQUE(PrimitiveChromaticAbberation, PrimitiveVS, ChromaticAbberationPS);

#endif
