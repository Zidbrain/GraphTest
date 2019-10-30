#ifndef LIGHTING_DEF
#define LIGHTING_DEF

#include "Data.h"

float4 CalculateColorLighting(in VSOut input) : SV_Target0
{
    float3 position = tex2D(positionBuffer, input.TextureCoordinate).rgb;
    float _length = length(_lightPosition - position);
    float shadowValue = tex2D(shadowMap, input.TextureCoordinate).r;
    float4 color = tex2D(textureSampler, input.TextureCoordinate);

    if (_length >= _radius)
        return color * shadowValue;

    float3 normal = tex2D(normalBuffer, input.TextureCoordinate).rgb;

    float3 lightDirection = normalize(_lightPosition - position);
    float lightIntensity = _diffuseIntensity * dot(normal, lightDirection);

    float3 diffuseColor = float3(0, 0, 0);
    if (lightIntensity >= 0)
    {
        diffuseColor = CreateFloat3(lightIntensity);
        _length = pow(_length, _diffuseIntensity);
        _radius = pow(_radius, _diffuseIntensity);
    }
    else
        return color * shadowValue;

    float4 originalColor = tex2D(renderTargetSampler, input.TextureCoordinate);
    float4 depthMask = tex2D(depthBuffer, input.TextureCoordinate);
    
    float3 specularColor = float3(0, 0, 0);
    if (depthMask.g == 1)
    {
        float3 r = normalize(2 * dot(-lightDirection, normal) * normal + lightDirection);
        specularColor = max(pow(dot(r, _viewVector), 250), 0) * originalColor.rgb;
    }

    return float4(color.rgb + lerp(CreateFloat3(0),
        originalColor.rgb * (diffuseColor + specularColor) * (1.0 - _length / _radius), CreateFloat3(shadowValue)), 1.0);
}

float4 ApplyGamma(in VSOut input) : SV_Target0
{
    return Gamma(tex2D(textureSampler, input.TextureCoordinate));
}

float4 SoftShadows(in VSOut input) : SV_Target0
{
    float4 color = (float4) 0;

    float4 pos = tex2D(positionBuffer, input.TextureCoordinate);
    float4 lightViewPosition = mul(pos, _lightMatrix);
    float4 tex = tex2D(depthBuffer, input.TextureCoordinate);

    float2 projText = float2(lightViewPosition.x / lightViewPosition.w / 2.0 + 0.5, -lightViewPosition.y / lightViewPosition.w / 2.0 + 0.5);
    if ((saturate(projText.x) == projText.x) && (saturate(projText.y) == projText.y) && lightViewPosition.z > 0)
    {
        float lightDepth = 1.0 / lightViewPosition.w;
        float depthValue = tex2D(shadowMap, projText).r;
        if (lightDepth < depthValue)
            color = (tex - CreateFloat4(1.0 / _amountOfLights, 0.0));
        else
            color = tex;
    }
    else
        color = tex;

    return color;
}

float4 ApplyAmbient(in VSOut input) : SV_Target0
{
    return tex2D(textureSampler, input.TextureCoordinate) * CreateFloat4(_ambientColor, 1);
}

TECHNIQUE(PrimitiveApplyLighting, PrimitiveVS, CalculateColorLighting);
TECHNIQUE(PrimitiveGamma, PrimitiveVS, ApplyGamma);
TECHNIQUE(PrimitiveApplyAmbient, PrimitiveVS, ApplyAmbient);
TECHNIQUE(PrimitiveSoftShadows, PrimitiveVS, SoftShadows);

TECHNIQUE(MeshSoftShadows, MeshVS, SoftShadows);

#endif