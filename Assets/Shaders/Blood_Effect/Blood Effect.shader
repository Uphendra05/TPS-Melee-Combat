
Shader "Universal Render Pipeline/Particles/Blood Effect"
{
    Properties
    {
        [Header(Color Controls)]
        [HDR] _BaseColor ("Base Color Mult", Color) = (1,1,1,1)
        _LightStr ("Lighting Strength", Float) = 0.85
        _AlphaMin ("Alpha Clip Min", Range(-0.01, 1.01)) = 0.1
        _AlphaSoft ("Alpha Clip Softness", Range(0,1)) = 0.022
        _EdgeDarken ("Edge Darkening", Float) = 1.0
        _ProcMask ("Procedural Mask Strength", Float) = 1.0

        [Header(Mask Controls)]
        _MainTex ("Mask Texture", 2D) = "white" {}
        _MaskStr ("Mask Strength", Float) = 0.7
        _Columns ("Flipbook Columns", Float) = 1
        _Rows ("Flipbook Rows", Float) = 1
        _ChannelMask ("Channel Mask", Vector) = (1,0,0,0)
        [Toggle] _FlipU ("Flip U Randomly", Float) = 0
        [Toggle] _FlipV ("Flip V Randomly", Float) = 0

        [Header(Noise Controls)]
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseAlphaStr ("Noise Strength", Float) = 0.8
        _NoiseColorStr ("Noise Color Strength", Float) = 0
        _ChannelMask2 ("Noise Channel Mask", Vector) = (1,0,0,0)
        _Randomize ("Randomize Noise", Float) = 1

        [Header(UV Warp Controls)]
        _WarpTex ("Warp Texture", 2D) = "gray" {}
        _WarpStr ("Warp Strength", Float) = 1

        [Header(Vertex Physics)]
        _FallOffset ("Gravity Offset", Range(-1,0)) = -1
        _FallRandomness ("Gravity Randomness", Float) = 0.25

        [Header(Specular Reflection)]
        _SpecularColor ("Specular Color / Metalness", Color) = (1,1,1,0.5)
        _ReflectionTex ("Reflection Texture", 2D) = "gray" {}
        _ReflectionSat ("Reflection Saturation", Range(0,1)) = 1
        _Normal ("Normal Map", 2D) = "bump" {}
        _FlattenNormal ("Flatten Normal", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "FORWARD"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM

            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma multi_compile SPECULAR_REFLECTION_ON SPECULAR_REFLECTION_OFF

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

            // ----------------------------------------------------
            // TEXTURES
            // ----------------------------------------------------

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            TEXTURE2D(_WarpTex);
            SAMPLER(sampler_WarpTex);

            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);

            #ifdef SPECULAR_REFLECTION_ON
                TEXTURE2D(_Normal);
                SAMPLER(sampler_Normal);
            #endif

            // ----------------------------------------------------
            // MATERIAL PROPERTIES
            // ----------------------------------------------------

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseColor;
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float4 _WarpTex_ST;
                float4 _ReflectionTex_ST;

                float _LightStr;
                float _AlphaMin;
                float _AlphaSoft;
                float _EdgeDarken;
                float _ProcMask;

                float _MaskStr;
                float _Columns;
                float _Rows;

                float4 _ChannelMask;
                float4 _ChannelMask2;

                float _FlipU;
                float _FlipV;

                float _NoiseAlphaStr;
                float _NoiseColorStr;
                float _Randomize;

                float _WarpStr;

                float _FallOffset;
                float _FallRandomness;

                float4 _SpecularColor;
                float _ReflectionSat;
                float _FlattenNormal;

            CBUFFER_END

            // ----------------------------------------------------
            // VERTEX INPUT
            // ----------------------------------------------------

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;

                // Z = Random
                // W = Lifetime
                float4 texcoord0 : TEXCOORD0;

                // X = Pan Offset
                // Y = UV Warp Strength
                // Z = Gravity
                float3 texcoord1 : TEXCOORD1;

                float4 color : COLOR;

                #ifdef SPECULAR_REFLECTION_ON
                    float4 tangentOS : TANGENT;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // ----------------------------------------------------
            // VERTEX OUTPUT
            // ----------------------------------------------------

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float4 uv : TEXCOORD0;

                float3 viewDirWS : TEXCOORD1;

                float4 vertLight : TEXCOORD2;

                float3 customData : TEXCOORD3;

                float fogFactor : TEXCOORD4;

                float4 color : COLOR;

                #ifdef SPECULAR_REFLECTION_ON
                    float3 tangentWS : TEXCOORD5;
                    float3 bitangentWS : TEXCOORD6;
                    float3 normalWS : TEXCOORD7;
                #else
                    float3 normalWS : TEXCOORD5;
                #endif

                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ----------------------------------------------------
            // VERTEX SHADER
            // ----------------------------------------------------

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Particle lifetime and gravity

                float lifetime = v.texcoord0.w;

                lifetime =
                    lifetime * lifetime +
                    (_FallOffset +
                    ((v.texcoord0.z - 0.5) * _FallRandomness))
                    * lifetime;

                float4 fallPos =
                    lifetime * float4(0, v.texcoord1.z, 0, 0);

                #if defined(SHADER_API_GLES3)
                    fallPos *= -1.0;
                #endif

                // Preserve original vertex displacement behavior

                float3 worldPos =
                    TransformObjectToWorld(v.positionOS.xyz)
                    + fallPos.xyz;

                float4 positionCS =
                    TransformObjectToHClip(v.positionOS.xyz);

                positionCS += fallPos;

                o.positionCS = positionCS;

                // Random UV flipping

                float2 UVflip = round(
                    frac(
                        float2(
                            v.texcoord0.z * 13,
                            v.texcoord0.z * 8
                        )
                    )
                );

                UVflip = UVflip * 2 - 1;

                UVflip = lerp(
                    1,
                    UVflip,
                    float2(_FlipU, _FlipV)
                );

                // UV coordinates

                o.uv.xy = TRANSFORM_TEX(
                    v.texcoord0.xy * UVflip,
                    _MainTex
                );

                o.uv.zw =
                    o.uv.xy * float2(_Columns, _Rows)
                    + v.texcoord0.z * float2(3, 8) * _Randomize;

                // Particle color

                o.color = v.color;

                o.color.a *= o.color.a;
                o.color.a += _AlphaMin;

                // Custom particle data

                o.customData = float3(
                    v.texcoord1.xy,
                    v.texcoord0.z
                );

                // World normal

                o.normalWS =
                    TransformObjectToWorldNormal(v.normalOS);

                // Specular reflection data

                #ifdef SPECULAR_REFLECTION_ON

                    float3 tangentWS =
                        TransformObjectToWorldDir(v.tangentOS.xyz);

                    tangentWS = normalize(tangentWS);

                    float3 normalWS = normalize(o.normalWS);

                    float3 bitangentWS =
                        cross(normalWS, tangentWS) * v.tangentOS.w;

                    o.tangentWS = tangentWS;
                    o.bitangentWS = normalize(bitangentWS);

                    o.viewDirWS = normalize(
                        GetWorldSpaceViewDir(worldPos)
                    );

                #endif

                // Spherical harmonics / ambient lighting

                float3 shade = SampleSH(o.normalWS);

                shade = max(
                    shade,
                    (unity_AmbientSky +
                     unity_AmbientGround +
                     unity_AmbientEquator) * 0.15
                );

                o.vertLight.xyz = lerp(
                    1,
                    shade,
                    _LightStr
                );

                // Fog

                o.fogFactor = ComputeFogFactor(o.positionCS.z);

                return o;
            }

            // ----------------------------------------------------
            // FRAGMENT SHADER
            // ----------------------------------------------------

            half4 frag(Varyings i) : SV_Target
            {
                // ------------------------------------------------
                // UV WARP
                // ------------------------------------------------

                float2 warpUV =
                    i.uv.zw * _WarpTex_ST.xy
                    + _WarpTex_ST.zw * (i.customData.x + 1)
                    + float2(5, 8) * i.customData.z;

                half4 uvWarp = SAMPLE_TEXTURE2D(
                    _WarpTex,
                    sampler_WarpTex,
                    warpUV
                );

                float2 warp = uvWarp.xy * 2 - 1;

                warp *= _WarpStr * i.customData.y;

                // ------------------------------------------------
                // MASK
                // ------------------------------------------------

                float2 maskUV =
                    i.uv.xy * _MainTex_ST.xy + warp;

                half4 mask = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    maskUV
                );

                mask = saturate(
                    lerp(1, mask, _MaskStr)
                );

                // ------------------------------------------------
                // PROCEDURAL EDGE MASK
                // ------------------------------------------------

                half2 tempUV =
                    frac(i.uv.xy * half2(_Columns, _Rows)) - 0.5;

                tempUV *= tempUV * 4;

                half edgeMask =
                    saturate(tempUV.x + tempUV.y);

                edgeMask *= edgeMask;

                edgeMask = 1 - edgeMask;

                edgeMask = lerp(
                    1.0,
                    edgeMask,
                    _ProcMask
                );

                mask *= edgeMask;

                // ------------------------------------------------
                // BASE COLOR
                // ------------------------------------------------

                half4 col = max(0.001, i.color);

                col.a = saturate(
                    dot(mask, _ChannelMask)
                );

                // ------------------------------------------------
                // NOISE
                // ------------------------------------------------

                float2 noiseUV =
                    i.uv.zw * _NoiseTex_ST.xy
                    + _NoiseTex_ST.zw * i.customData.x
                    + warp;

                half4 noise4 = SAMPLE_TEXTURE2D(
                    _NoiseTex,
                    sampler_NoiseTex,
                    noiseUV
                );

                half noise = dot(
                    noise4,
                    _ChannelMask2
                );

                noise = saturate(
                    lerp(1, noise, _NoiseAlphaStr)
                );

                // ------------------------------------------------
                // ALPHA CLIPPING
                // ------------------------------------------------

                col.a *= noise;

                half preClipAlpha = col.a;

                half clippedAlpha = saturate(
                    (preClipAlpha * i.color.a - _AlphaMin)
                    / max(_AlphaSoft, 0.00001)
                );

                col.a = clippedAlpha;

                // ------------------------------------------------
                // LIGHTING
                // ------------------------------------------------

                float3 baseLighting = i.vertLight.xyz;

                // ------------------------------------------------
                // SPECULAR REFLECTIONS
                // ------------------------------------------------

                #ifdef SPECULAR_REFLECTION_ON

                    // Normal map

                    half3 normalTex = UnpackNormal(
                        SAMPLE_TEXTURE2D(
                            _Normal,
                            sampler_Normal,
                            noiseUV
                        )
                    );

                    // Flatten normals near alpha edges

                    normalTex.z =
                        _FlattenNormal *
                        (
                            saturate(
                                (preClipAlpha * i.color.a - _AlphaMin)
                                / (_AlphaSoft + 0.2)
                            ) - 0.1
                        ) * 1.2;

                    normalTex = normalize(normalTex);

                    // Tangent to world transformation

                    normalTex =
                        normalize(
                            i.tangentWS * normalTex.x +
                            i.bitangentWS * normalTex.y +
                            i.normalWS * normalTex.z
                        );

                    // Combine normals

                    float3 combinedNormals =
                        normalize(i.normalWS + normalTex);

                    // Reflection vector

                    float3 reflectionVector = reflect(
                        -i.viewDirWS,
                        combinedNormals
                    );

                    reflectionVector.x = atan2(
                        reflectionVector.x,
                        reflectionVector.z
                    ) * 0.31831;

                    reflectionVector *= 0.5;

                    // Reflection UVs

                    float2 reflectionUVs =
                        reflectionVector.xy *
                        _ReflectionTex_ST.xy;

                    reflectionUVs +=
                        _ReflectionTex_ST.zw *
                        (_Time.y + i.customData.z);

                    // Sample reflection

                    half3 reflectionTex = SAMPLE_TEXTURE2D(
                        _ReflectionTex,
                        sampler_ReflectionTex,
                        reflectionUVs
                    ).rgb;

                    // Reflection saturation

                    half desatReflection =
                        dot(
                            reflectionTex,
                            half3(1,1,1)
                        ) * 0.333;

                    half3 spec = lerp(
                        desatReflection,
                        reflectionTex,
                        _ReflectionSat
                    );

                    half3 spec0 = spec;

                    half3 spec1 =
                        spec0 * spec0 * spec0 * spec0;

                    spec = clamp(
                        lerp(
                            spec0,
                            spec1,
                            _SpecularColor.w * preClipAlpha
                        ),
                        0,
                        10
                    );

                    // Fresnel

                    half fresnel =
                        1 - dot(
                            i.viewDirWS,
                            combinedNormals
                        ) * _SpecularColor.w;

                    spec *= clamp(fresnel, 0.2, 1);

                #endif

                // ------------------------------------------------
                // EDGE CALCULATION
                // ------------------------------------------------

                half edge =
                    1 - saturate(preClipAlpha * clippedAlpha);

                edge *= edge;

                edge = 1 - edge;

                edge += lerp(
                    0,
                    noise - 0.5,
                    _NoiseColorStr
                );

                // ------------------------------------------------
                // EDGE DARKENING
                // ------------------------------------------------

                edge = saturate(
                    lerp(
                        0.71,
                        edge * edge,
                        _EdgeDarken
                    )
                );

                // ------------------------------------------------
                // EDGE ALPHA
                // ------------------------------------------------

                col.a *= saturate(
                    lerp(
                        1.25,
                        _BaseColor.a,
                        edge
                    )
                );

                #ifdef SPECULAR_REFLECTION_OFF
                    edge *= 2;
                #endif

                // ------------------------------------------------
                // COLOR DARKENING
                // ------------------------------------------------

                col.rgb *= lerp(
                    min(
                        col.rgb * col.rgb * col.rgb * 0.3,
                        1.0
                    ),
                    0.71,
                    edge
                );

                // ------------------------------------------------
                // BASE COLOR AND LIGHTING
                // ------------------------------------------------

                col.rgb *= max(
                    0,
                    baseLighting * _BaseColor.rgb
                );

                // ------------------------------------------------
                // SPECULAR COMBINATION
                // ------------------------------------------------

                #ifdef SPECULAR_REFLECTION_ON

                    col.rgb +=
                        baseLighting *
                        spec *
                        _SpecularColor.rgb;

                #endif

                // ------------------------------------------------
                // URP FOG
                // ------------------------------------------------

                col.rgb = MixFog(
                    col.rgb,
                    i.fogFactor
                );

                return col;
            }

            ENDHLSL
        }
    }

    // Keep your original custom material inspector if available.
    CustomEditor "SpecularToggleEditor"
}
