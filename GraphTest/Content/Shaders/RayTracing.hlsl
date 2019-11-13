#ifndef RAY_TRACING
#define RAY_TRACING

#include "Data.hlsl"

uniform extern float3 _pointOnPlane;
uniform extern float3 _normal;

float2 FromWorldToScreen(float4 worldCoordinates)
{
    float4 transform = mul(worldCoordinates, _lightMatrix);
    return float2(transform.x / transform.w / 2.0 + 0.5, -transform.y / transform.w / 2.0 + 0.5);
}

float4 RayTracingPS(in VSOut input) : SV_Target
{
    float3 vecStart = tex2D(positionBuffer, input.TextureCoordinate).xyz;
    float3 vecEnd = _lightPosition;
    float3 vec = vecEnd - vecStart;
    float4 color = tex2D(renderTargetSampler, input.TextureCoordinate);

    float div = dot(vec, _normal);

    if (div != 0)
    {
        float3 intersection = dot((_pointOnPlane - vecStart), _normal) / div * vec + vecStart;
        float2 intersectionScreen = FromWorldToScreen(CreateFloat4(intersection, 1));
        float3 position = tex2D(positionBuffer, intersectionScreen).xyz;

        if (position.x == intersection.x && position.y == intersection.y && position.z == intersection.z)
            return float4(color.r * 0.5, color.g * 0.5, color.b * 0.5, 1);
    }
    return float4(color.r, color.g, color.b, 1);
}

TECHNIQUE(PrimitiveRayTracing, PrimitiveVS, RayTracingPS);
TECHNIQUE(MeshRayTracing, MeshVS, RayTracingPS);

#endif