#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

#define TECHNIQUE(techniqueName, vertexShaderName, pixelShaderName) technique techniqueName {pass P0 {VertexShader = compile VS_SHADERMODEL vertexShaderName(); PixelShader = compile PS_SHADERMODEL pixelShaderName();}}

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
uniform extern bool _specular;
uniform extern texture _shadowMap;
uniform extern matrix _lightMatrix;
uniform extern bool _vertical;
uniform extern bool _writeOnlyColor;
uniform extern texture _normalBuffer;
uniform extern texture _positionBuffer;

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

float4 CalculateColorLighting(in VSOut input) : SV_Target0
{
    float shadowValue = tex2D(shadowMap, input.TextureCoordinate).r;

    float3 normal = tex2D(normalBuffer, input.TextureCoordinate).rgb;
    float3 position = tex2D(positionBuffer, input.TextureCoordinate).rgb;
    float3 color = tex2D(textureSampler, input.TextureCoordinate).rgb;
    float3 originalColor = tex2D(renderTargetSampler, input.TextureCoordinate).rgb;

    float3 lightDirection = normalize(_lightPosition - position);
    float lightIntensity = _diffuseIntensity * dot(normal, lightDirection);
    
    float3 specularColor = float3(0, 0, 0);
    if (_specular)
    {
        float3 r = normalize(2 * dot(-lightDirection, normal) * normal + lightDirection);
        specularColor = max(pow(dot(r, _viewVector), 50), 0) * color;
    }

    float3 diffuseColor = CreateFloat3(lightIntensity);

    return float4(color + lerp(CreateFloat3(0),
        originalColor * (saturate(diffuseColor) + saturate(specularColor)) * _radius * _radius / pow(length(_lightPosition - position), 2), CreateFloat3(shadowValue)), 1.0);
}

float4 ApplyGamma(in VSOut input) : SV_Target0
{
    return Gamma(tex2D(textureSampler, input.TextureCoordinate));
}

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
    output.WorldPosition = input.Position.xyz;
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
    outp.Depth = CreateFloat4(input.Position.z / input.Position.w, 1.0);

    return outp;
}

float4 Combine(float4 a, float4 b)
{
    float c = a.a + b.a * (1 - a.a);
    return float4((a.r * a.a) + (b.r * b.a * (1.0 - a.a)) / c,
    (a.g * a.a) + (b.g * b.a * (1.0 - a.a)) / c,
    (a.b * a.a) + (b.b * b.a * (1.0 - a.a)) / c,
    a.a + (b.a * (1.0 - a.a)));
}

float2 Hot(float2 input, float distortionFactor)
{
    float2 distortionMapCoordinate = input;
    distortionMapCoordinate.y -= _time * _riseFactor;
	
    float4 distortionMapValue = tex2D(hotSampler, distortionMapCoordinate);
    float2 distortionPositionOffset = (float2) distortionMapValue - 0.5f;
    distortionPositionOffset *= 2.0;
	
    distortionPositionOffset *= distortionFactor;
	
    distortionPositionOffset *= input.y;
    
    return (float2) input + distortionPositionOffset;
}

Target HotPS(in VSOut input)
{
    Target outp = (Target) 0;

    float2 newCoord = Hot(input.TextureCoordinate.xy, _distortionFactor);
    float2 newCoord2 = (newCoord - input.TextureCoordinate.xy) * (input.Position.z / input.Position.w) + input.Position.xy / _screenSize;

    outp.Color = Combine(tex2D(textureSampler, newCoord) * input.Color, tex2D(renderTargetSampler, newCoord2));
    outp.Position = CreateFloat4(input.WorldPosition, 1.0);
    outp.Normal = CreateFloat4(input.Normal, 1);
    outp.Depth = CreateFloat4(input.Position.z / input.Position.w, 1.0);

    return outp;
}

float4 WallPS0(in VSOut input) : SV_Target0
{
    return CreateFloat4(input.Position.z / input.Position.w, 1);
}

float4 SoftShadows(in VSOut input) : SV_Target0
{
    float4 color = (float4) 0;

    float4 lightViewPosition = mul(CreateFloat4(input.WorldPosition, 1.0), _lightMatrix);

    float2 projText = float2(lightViewPosition.x / lightViewPosition.w / 2.0 + 0.5, -lightViewPosition.y / lightViewPosition.w / 2.0 + 0.5);
    if ((saturate(projText.x) == projText.x) && (saturate(projText.y) == projText.y))
    {
        float lightDepth = 1.0 / lightViewPosition.w;
        float depthValue = tex2D(shadowMap, projText).r;
        if (lightDepth < depthValue)
            color = float4(0, 0, 0, 1);
        else
            color = tex2D(depthBuffer, input.Position.xy / _screenSize);
    }
    else
        color = tex2D(depthBuffer, input.Position.xy / _screenSize);

    return color;
}

static const float _offset[] = { 0.0, 1.3846153846, 3.2307692308 };
static const float _weight[] = { 0.2270270270, 0.3162162162, 0.0702702703 };

float4 Blur(in VSOut input) : SV_Target0
{

    float4 color = tex2D(textureSampler, input.TextureCoordinate.xy) * _weight[0];
    if (_vertical)
    {
        for (int i = 0; i < 3; i++)
        {
            color += tex2D(textureSampler, input.TextureCoordinate.xy + float2(0.0, _offset[i] / 1080)) * _weight[i];
            color += tex2D(textureSampler, input.TextureCoordinate.xy - float2(0.0, _offset[i] / 1080)) * _weight[i];
        }
    }
    else
    {
        for (int i = 0; i < 3; i++)
        {
            color += tex2D(textureSampler, input.TextureCoordinate.xy + float2(_offset[i] / 1920, 0.0)) * _weight[i];
            color += tex2D(textureSampler, input.TextureCoordinate.xy - float2(_offset[i] / 1920, 0.0)) * _weight[i];
        }
    }

    return color;
}

float4 ApplyAmbient(in VSOut input) : SV_Target0
{
    return tex2D(textureSampler, input.TextureCoordinate) * CreateFloat4(_ambientColor, 1);
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