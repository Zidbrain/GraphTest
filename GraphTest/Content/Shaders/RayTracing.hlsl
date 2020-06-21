#ifndef RAY_TRACING
#define RAY_TRACING

#include "Data.hlsl"
#include "Shader.hlsl"

uniform extern float3 _normal;
uniform extern float3 _points[3];

bool IsInsideTriangle(in float3 value)
{
    float3 x = value - _points[0];
    float3 a = _points[1] - _points[0];
    float3 b = _points[2] - _points[0];
    float det = a.x * b.y - b.x * a.y;
    //float newX = (x.x * b.y - x.y * b.x) / det;
    //float newY = (a.x * x.y - a.y * x.x) / det;
    //float3x3 arcT = float3x3(b.y, -b.x, 0,
    //                        -a.x, a.x, 0,
    //                         a.y * b.z - a.z * b.y, -a.x * b.z + a.z * b.x, a.x * b.y - b.x * a.y) / det;
    float3x3 arcT = float3x3(a.x, b.x, 0,
                             a.y, b.y, 0,
                             a.z, b.z, 0);
    float3 newBasis = mul(arcT, x);
    

    return (newBasis.x >= 0 && newBasis.x <= 1 && newBasis.y >= 0 && newBasis.y <= 1 && newBasis.x + newBasis.y <= 1);
}

float4 RayTracingPS(in float4 input : SV_Position) : SV_Target
{
    float2 texCoord = input.xy / _screenSize;

    float4 originalColor = tex2D(renderTargetSampler, texCoord);
    float4 color = tex2D(textureSampler, texCoord);
    float4 pos = tex2D(positionBuffer, texCoord);
    if (pos.a == 0)
        return float4(originalColor.r, originalColor.g, originalColor.b, 1);

    float3 vecStart = pos.rgb;
    float3 vecEnd = _lightPosition;
    float3 vec = vecEnd - vecStart;

    float vecLength = length(vec);
    if (vecLength >= 12.0)
        return float4(originalColor.r * 0.75, originalColor.g * 0.75, originalColor.b * 0.75, 1);

    float div = dot(vec, _normal);

    if (div != 0)
    {
        float d = dot(_points[0] - vecStart, _normal) / div;
        float3 intersection = d * vec + vecStart;

        if (d > 0 && d < 1 && IsInsideTriangle(intersection))
            return float4(originalColor.r * 0.25, originalColor.g * 0.25, originalColor.b * 0.25, 1);

    }
    return color;
}

float4 RayTracingVS(in float4 position : POSITION0) : SV_Position
{
    return position;
}

TECHNIQUE(PrimitiveRayTracing, RayTracingVS, RayTracingPS);

#endif