Shader "Custom/FogVolume"
{
    Properties {
        _DensityTex ("Density (sim)", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (0.85, 0.9, 1.0, 1.0)
        _ShadowColor ("Shadow Color", Color) = (0.45, 0.5, 0.62, 1.0)
        _DensityScale ("Density Scale", Range(0, 20)) = 6.0

        [Header(Height)]
        _FogHeight ("Max Fog Height (0-1 of volume Y)", Range(0.05, 1)) = 0.6
        _HeightResponse ("Height Follows Density", Range(0.5, 6)) = 2.0
        _HeightFalloff ("Vertical Falloff", Range(0.2, 4)) = 1.5

        [Header(Billowing Noise)]
        _NoiseWorldSize ("Noise Size (world units)", Range(0.5, 30)) = 6.0
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.7
        _NoiseSpeed ("Noise Wind (world xz) / Rise (y)", Vector) = (0.4, 0.15, 0.2, 0)

        [Header(Quality)]
        _EdgeFade ("Side Edge Fade", Range(0.01, 0.5)) = 0.15
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
            float _DensityScale;
            float _FogHeight, _HeightResponse, _HeightFalloff;
            float _NoiseWorldSize, _NoiseStrength;
            float4 _NoiseSpeed;
            float _EdgeFade;

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

            // Slab intersection against the unit cube in object space; returns (tNear, tFar)
            float2 BoxIntersect(float3 ro, float3 rd) {
                float3 t0 = (-0.5 - ro) / rd;
                float3 t1 = (0.5 - ro) / rd;
                float3 tmin = min(t0, t1), tmax = max(t0, t1);
                return float2(max(max(tmin.x, tmin.y), tmin.z), min(min(tmax.x, tmax.y), tmax.z));
            }

            // --- 3D value noise -------------------------------------------------
            float Hash13(float3 p) {
                p = frac(p * 0.1031);
                p += dot(p, p.zyx + 31.32);
                return frac((p.x + p.y) * p.z);
            }

            float Noise3(float3 x) {
                float3 i = floor(x), f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                float n000 = Hash13(i),                  n100 = Hash13(i + float3(1, 0, 0));
                float n010 = Hash13(i + float3(0, 1, 0)), n110 = Hash13(i + float3(1, 1, 0));
                float n001 = Hash13(i + float3(0, 0, 1)), n101 = Hash13(i + float3(1, 0, 1));
                float n011 = Hash13(i + float3(0, 1, 1)), n111 = Hash13(i + float3(1, 1, 1));
                return lerp(lerp(lerp(n000, n100, f.x), lerp(n010, n110, f.x), f.y),
                            lerp(lerp(n001, n101, f.x), lerp(n011, n111, f.x), f.y), f.z);
            }

            float Fbm(float3 p) {
                return Noise3(p) * 0.65 + Noise3(p * 2.3 + 17.0) * 0.35;
            }

            half4 frag(Varyings input) : SV_Target {
                float3 ro = input.cameraOS;
                float3 rd = normalize(input.positionOS - ro);
                float2 t = BoxIntersect(ro, rd);
                t.x = max(t.x, 0.0); // camera may be inside the volume

                // Stop the march at opaque geometry so fog doesn't draw through walls/props
                float2 screenUV = input.positionCS.xy / _ScaledScreenParams.xy;
                float rawDepth = SampleSceneDepth(screenUV);
                float3 sceneWS = ComputeWorldSpacePosition(screenUV, rawDepth, UNITY_MATRIX_I_VP);
                float3 sceneOS = TransformWorldToObject(sceneWS);
                float tScene = dot(sceneOS - ro, rd);
                t.y = min(t.y, tScene);
                if (t.y <= t.x) return half4(0, 0, 0, 0);

                const int stepCount = 32;
                float stepSize = (t.y - t.x) / stepCount;

                // Per-pixel jitter hides banding from the limited step count
                float jitter = frac(52.9829189 * frac(dot(input.positionCS.xy, float2(0.06711056, 0.00583715))));
                float3 p = ro + rd * (t.x + stepSize * jitter);
                float3 dp = rd * stepSize;

                float3 noiseDrift = float3(_NoiseSpeed.x, _NoiseSpeed.z, _NoiseSpeed.y) * _Time.y;
                float invNoiseSize = 1.0 / _NoiseWorldSize;

                float3 color = 0.0;
                float transmittance = 1.0;

                [loop]
                for (int i = 0; i < stepCount; i++) {
                    float h = p.y + 0.5; // 0 at the floor of the volume, 1 at the top

                    // 2D simulation density: this is where the player / obstacles carve holes
                    float sim = SAMPLE_TEXTURE2D_LOD(_DensityTex, sampler_DensityTex, p.xz + 0.5, 0).r;

                    // Fog column height follows simulated density, so carved areas also lose height
                    float top = _FogHeight * saturate(sim * _HeightResponse);
                    float heightMask = saturate(1.0 - h / max(top, 1e-3));
                    heightMask = pow(heightMask, _HeightFalloff);

                    // Billowing 3D noise in world units, drifting with wind and slowly rising
                    float3 np = (p * input.scaleWS) * invNoiseSize + noiseDrift;
                    float n = Fbm(np);
                    float shape = lerp(1.0, n * 2.0, _NoiseStrength);

                    // Soften the sides of the volume so the box walls never show
                    float2 e = abs(p.xz);
                    float edge = smoothstep(0.5, 0.5 - _EdgeFade, max(e.x, e.y));

                    float d = sim * heightMask * max(shape, 0.0) * edge * _DensityScale;
                    float a = 1.0 - exp(-d * stepSize); // Beer-Lambert per step

                    // Darker at the bottom, brighter toward the fog top
                    float3 col = lerp(_ShadowColor.rgb, _FogColor.rgb, saturate(h * 0.8 + 0.2 + (n - 0.5) * 0.3));
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
