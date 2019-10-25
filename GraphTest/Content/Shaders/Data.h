#ifndef DATA_DEF
#define DATA_DEF

#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

#define TECHNIQUE(techniqueName, vertexShaderName, pixelShaderName) technique techniqueName {pass P0 { VertexShader = compile VS_SHADERMODEL vertexShaderName(); PixelShader = compile PS_SHADERMODEL pixelShaderName();}}
#define TECHNIQUE_PARAMETERS(techiqueName, vertexShaderName, pixelShaderName, additionalParametes) technique techniqueName{pass P0 {additionalParameters VertexShader = compile VS_SHADERMODEL vertexShaderName(); PixelShader = compile PS_SHADERMODEL pixelShaderName();}}

#define WRITEDEPTH outp.Depth = CreateFloat4(input.Position.z / input.Position.w, 1.0)

#define CHECKDEPTH if (_checkDepth) if (input.Position.z / input.Position.w >= tex2D(depthBuffer, input.Position.xy / _screenSize).r) return outp

// All standart shaders in primitive and mesh drawing must have theese macros

uniform extern matrix _matrix;
uniform extern texture _texture;
uniform extern float _time;
uniform extern bool _textureenabled;
uniform extern float4 _color;
uniform extern float _radius;
uniform extern texture _hotTexture;
uniform extern float _riseFactor;
uniform extern float _distortionFactor;
uniform extern texture2D _renderTarget;
uniform extern texture2D _depthBuffer;
uniform extern bool _checkDepth;
uniform extern matrix _modelTransform;
uniform extern float3 _lightPosition;
uniform extern float _diffuseIntensity;
uniform extern float3 _ambientColor;
uniform extern float3 _viewVector;
uniform extern texture _shadowMap;
uniform extern matrix _lightMatrix;
uniform extern bool _vertical;
uniform extern bool _writeOnlyColor;
uniform extern texture _normalBuffer;
uniform extern texture _positionBuffer;
uniform extern int _amountOfLights;
uniform extern bool _specular;

static const float2 _screenSize = float2(1920, 1080);

float3 CreateFloat3(float value)
{
    return float3(value, value, value);
}

float4 CreateFloat4(float3 value, float valuew)
{
    return float4(value.x, value.y, value.z, valuew);
}

float4 CreateFloat4(float value, float valuew)
{
    return CreateFloat4(CreateFloat3(value), valuew);
}

float4 Gamma(float4 color)
{
    return CreateFloat4(pow(color.rgb, 1 / 2.2), color.a);
}

sampler2D textureSampler = sampler_state
{
    Texture = <_texture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
    Filter = ANISOTROPIC;
    MaxAnisotropy = 16;
};

sampler2D hotSampler = sampler_state
{
    Texture = <_hotTexture>;
    AddressU = WRAP;
    AddressV = WRAP;
    AddressW = WRAP;
};

sampler2D renderTargetSampler = sampler_state
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
    Texture = <_renderTarget>;
};

sampler2D depthBuffer = sampler_state
{
    Texture = <_depthBuffer>;
    AddressU = BORDER;
    AddressV = BORDER;
    AddressW = BORDER;
    BorderColor = 0x000000;
};

sampler2D normalBuffer = sampler_state
{
    Texture = <_normalBuffer>;
    AddressU = BORDER;
    AddressV = BORDER;
    AddressW = BORDER;
    BorderColor = 0x000000;
};

sampler2D positionBuffer = sampler_state
{
    Texture = <_positionBuffer>;
    AddressU = BORDER;
    AddressV = BORDER;
    AddressW = BORDER;
    BorderColor = 0x000000;
};

sampler2D shadowMap = sampler_state
{
    Texture = <_shadowMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
};

sampler2D maskBuffer = sampler_state
{
    Texture = <_shadowMap>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    AddressW = CLAMP;
};

struct Mesh
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct Primitive
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VSOut
{
    float4 Position : SV_Position0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float3 Normal : NORMAL0;
};

struct Target
{
    float4 Color : SV_Target0;
    float4 Position : SV_Target1;
    float4 Normal : SV_Target2;
    float4 Depth : SV_Target3;
};

float4 Combine(float4 a, float4 b)
{
    float c = a.a + b.a * (1 - a.a);
    return float4((a.r * a.a) + (b.r * b.a * (1.0 - a.a)) / c,
    (a.g * a.a) + (b.g * b.a * (1.0 - a.a)) / c,
    (a.b * a.a) + (b.b * b.a * (1.0 - a.a)) / c,
    a.a + (b.a * (1.0 - a.a)));
}

#endif