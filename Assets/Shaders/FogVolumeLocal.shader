Shader "Custom/FogVolumeLocal"
{
    // Mimics HDRP's Local Volumetric Fog: uniform density through the whole box,
    // soft blend at the faces, scrolling 3D noise mask. The sim texture only carves the wake.
    Properties {
        _DensityTex ("Carve Mask (sim)", 2D) = "white" {}
        _FogColor ("Fog Color (lit)", Color) = (0.82, 0.88, 0.95, 1.0)
        _ShadowColor ("Fog Color (shadowed / low)", Color) = (0.45, 0.52, 0.62, 1.0)

        [Header(Volume)]
        _Density ("Density per World Unit", Range(0, 2)) = 0.25
        _BlendDistance ("Blend Distance (world units)", Range(0.01, 10)) = 2.0
        [Toggle] _FadeBottom ("Fade Bottom Face (off = fog sits on ground)", Float) = 0
        _HeightFade ("Extra Fade Toward Top", Range(0, 1)) = 0.3

        [Header(Noise Mask)]
        _NoiseWorldSize ("Noise Size (world units)", Range(0.5, 40)) = 6.0
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.75
        _NoiseContrast ("Noise Contrast", Range(0.5, 4)) = 1.6
        _NoiseScroll ("Noise Scroll (world units / sec)", Vector) = (0.4, 0.08, 0.25, 0)

        [Header(Wake Carving)]
        _SimInfluence ("Sim Influence", Range(0, 1)) = 1.0
        _CarveHeight ("Carve Height (world units)", Range(0.1, 10)) = 2.5

        [Header(Lighting)]
        _Anisotropy ("Forward Scattering (g)", Range(0, 0.95)) = 0.4
        _ScatterStrength ("Light Scatter Strength", Range(0, 4)) = 1.0
    }
    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Front

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_DensityTex);
            SAMPLER(sampler_DensityTex);
            float4 _FogColor, _ShadowColor;
            float _Density, _BlendDistance, _FadeBottom, _HeightFade;
            float _NoiseWorldSize, _NoiseStrength, _NoiseContrast;
            float4 _NoiseScroll;
            float _SimInfluence, _CarveHeight;
            float _Anisotropy, _ScatterStrength;

            struct Attributes { float4 positionOS : POSITION; };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 cameraOS : TEXCOORD1;
                float3 scaleWS : TEXCOORD2;
            };

            Varyings vert(Attributes input) {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                output.cameraOS = TransformWorldToObject(_WorldSpaceCameraPos);
                float4x4 m = GetObjectToWorldMatrix();
                output.scaleWS = float3(length(m._m00_m10_m20), length(m._m01_m11_m21), length(m._m02_m12_m22));
                return output;
            }

            float2 BoxIntersect(float3 ro, float3 rd) {
                float3 t0 = (-0.5 - ro) / rd;
                float3 t1 = (0.5 - ro) / rd;
                float3 tmin = min(t0, t1), tmax = max(t0, t1);
                return float2(max(max(tmin.x, tmin.y), tmin.z), min(min(tmax.x, tmax.y), tmax.z));
            }

            float Hash13(float3 p) {
                p = frac(p * 0.1031);
                p += dot(p, p.zyx + 31.32);
                return frac((p.x + p.y) * p.z);
            }

            float Noise3(float3 x) {
                float3 i = floor(x), f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                float n000 = Hash13(i),                   n100 = Hash13(i + float3(1, 0, 0));
                float n010 = Hash13(i + float3(0, 1, 0)), n110 = Hash13(i + float3(1, 1, 0));
                float n001 = Hash13(i + float3(0, 0, 1)), n101 = Hash13(i + float3(1, 0, 1));
                float n011 = Hash13(i + float3(0, 1, 1)), n111 = Hash13(i + float3(1, 1, 1));
                return lerp(lerp(lerp(n000, n100, f.x), lerp(n010, n110, f.x), f.y),
                            lerp(lerp(n001, n101, f.x), lerp(n011, n111, f.x), f.y), f.z);
            }

            float Fbm(float3 p) {
                return Noise3(p) * 0.55 + Noise3(p * 2.1 + 11.3) * 0.30 + Noise3(p * 4.3 + 27.9) * 0.15;
            }

            float HG(float cosTheta, float g) {
                float g2 = g * g;
                return (1.0 - g2) / (4.0 * PI * pow(max(1.0 + g2 - 2.0 * g * cosTheta, 1e-4), 1.5));
            }

            half4 frag(Varyings input) : SV_Target {
                float3 ro = input.cameraOS;
                float3 rd = normalize(input.positionOS - ro);
                float2 t = BoxIntersect(ro, rd);
                t.x = max(t.x, 0.0);

                // Stop at opaque geometry so fog never draws through walls or characters
                float2 screenUV = input.positionCS.xy / _ScaledScreenParams.xy;
                float rawDepth = SampleSceneDepth(screenUV);
                float3 sceneOS = TransformWorldToObject(ComputeWorldSpacePosition(screenUV, rawDepth, UNITY_MATRIX_I_VP));
                t.y = min(t.y, dot(sceneOS - ro, rd));
                if (t.y <= t.x) return half4(0, 0, 0, 0);

                const int stepCount = 40;
                float stepSize = (t.y - t.x) / stepCount;
                float stepWS = stepSize * length(rd * input.scaleWS);

                float jitter = frac(52.9829189 * frac(dot(input.positionCS.xy, float2(0.06711056, 0.00583715))));
                float3 p = ro + rd * (t.x + stepSize * jitter);
                float3 dp = rd * stepSize;

                float3 rdWS = normalize(TransformObjectToWorldDir(rd));
                float phase = HG(dot(rdWS, normalize(_MainLightPosition.xyz)), _Anisotropy) * PI;
                float3 lightCol = _MainLightColor.rgb * _ScatterStrength * (0.25 + phase);

                float3 scroll = _NoiseScroll.xyz * _Time.y;
                float invNoise = 1.0 / _NoiseWorldSize;
                float blend = max(_BlendDistance, 1e-3);

                float3 color = 0.0;
                float transmittance = 1.0;

                [loop]
                for (int i = 0; i < stepCount; i++) {
                    float3 pw = p * input.scaleWS;     // world-scaled position inside the box
                    float h = p.y + 0.5;               // 0 floor .. 1 top
                    float hw = h * input.scaleWS.y;    // world height above the volume floor

                    // HDRP-style blend distance: soft fade on every face of the box
                    float3 distToFace = (0.5 - abs(p)) * input.scaleWS;
                    float dSide = min(distToFace.x, distToFace.z);
                    float dTop = distToFace.y;
                    float dBottom = _FadeBottom > 0.5 ? hw : 1e5;
                    float fade = saturate(min(min(dSide, dTop), dBottom) / blend);
                    fade = fade * fade * (3.0 - 2.0 * fade);

                    // Scrolling 3D noise mask
                    float n = Fbm(pw * invNoise + scroll);
                    n = saturate((n - 0.5) * _NoiseContrast + 0.5);
                    float noiseMask = lerp(1.0, n * 2.0, _NoiseStrength);

                    // Wake carving: a hole through the fog column that heals above head height
                    float sim = SAMPLE_TEXTURE2D_LOD(_DensityTex, sampler_DensityTex, p.xz + 0.5, 0).r;
                    float carve = saturate(sim * 2.0);
                    carve = lerp(carve, 1.0, saturate((hw - _CarveHeight) / _CarveHeight));
                    float simMask = lerp(1.0, carve, _SimInfluence);

                    float heightFade = lerp(1.0, 1.0 - h, _HeightFade);

                    float d = _Density * fade * max(noiseMask, 0.0) * simMask * heightFade;
                    float a = 1.0 - exp(-d * stepWS);

                    float3 base = lerp(_ShadowColor.rgb, _FogColor.rgb, saturate(h * 0.6 + n * 0.4));
                    float3 col = base * (0.6 + lightCol);
                    color += col * (a * transmittance);
                    transmittance *= 1.0 - a;
                    if (transmittance < 0.01) break;
                    p += dp;
                }

                return half4(color, 1.0 - transmittance);
            }
            ENDHLSL
        }
    }
}
