#ifndef HOT_DEF
#define HOT_DEF

#include "Data.hlsl"

float2 Hot(float2 input, float distortionFactor)
{
    float2 coordinate = input;
    coordinate -= _time * _riseFactor;
	
    float2 distortionPositionOffset = (tex2D(hotSampler, coordinate).rg - 0.5f) * 2 * distortionFactor * input.y;
    
    return input + distortionPositionOffset;
}

Target HotPS(in VSOut input)
{
    Target outp = (Target) 0;

    float2 newCoord = Hot(input.TextureCoordinate.xy, _distortionFactor);
    float2 newCoord2 = (newCoord - input.TextureCoordinate.xy) * (input.Position.z / input.Position.w) + input.Position.xy / _screenSize;

    outp.Color = Combine(tex2D(textureSampler, newCoord) * input.Color, tex2D(renderTargetSampler, newCoord2));
    outp.Position = CreateFloat4(input.WorldPosition, 1.0);
    outp.Normal = CreateFloat4(input.Normal, 1);
    outp.Depth = float2(input.Position.z / input.Position.w, 0);

    return outp;
}

float4 WallPS0(in VSOut input) : SV_Target0
{
    return CreateFloat4(input.Position.z / input.Position.w, 1);
}

TECHNIQUE(PrimitiveHot, PrimitiveVS, HotPS);

#endif