#ifndef BLUR_DEF
#define BLUR_DEF

#include "Data.hlsl"

static const float _offset[] = { 0.0, 1.3846153846, 3.2307692308 };
static const float _weight[] = { 0.2270270270, 0.3162162162, 0.0702702703 };

float4 Blur(in VSOut input) : SV_Target0
{

    float4 color = tex2D(textureSampler, input.TextureCoordinate.xy) * _weight[0];
    if (_vertical)
    {
        for (int i = 0; i < 3; i++)
        {
            color += tex2D(textureSampler, input.TextureCoordinate.xy + float2(0.0, _offset[i] / _screenSize.y)) * _weight[i];
            color += tex2D(textureSampler, input.TextureCoordinate.xy - float2(0.0, _offset[i] / _screenSize.y)) * _weight[i];
        }
    }
    else
    {
        for (int i = 0; i < 3; i++)
        {
            color += tex2D(textureSampler, input.TextureCoordinate.xy + float2(_offset[i] / _screenSize.x, 0.0)) * _weight[i];
            color += tex2D(textureSampler, input.TextureCoordinate.xy - float2(_offset[i] / _screenSize.x, 0.0)) * _weight[i];
        }
    }

    return color;
}

TECHNIQUE(PrimitiveBlur, PrimitiveVS, Blur);

#endif