Shader "Custom/FogVolumeMist"
{
    Properties {
        _DensityTex ("Density (sim)", 2D) = "white" {}
        _FogColor ("Fog Color (lit)", Color) = (0.7, 0.85, 0.95, 1.0)
        _ShadowColor ("Shadow Color (low / unlit)", Color) = (0.25, 0.4, 0.5, 1.0)
        _DensityScale ("Density per World Unit", Range(0, 2)) = 0.15

        [Header(Layer Shape)]
        _FogHeight ("Layer Thickness (0-1 of volume Y)", Range(0.02, 1)) = 0.35
        _HeightResponse ("Height Follows Sim Density", Range(0.5, 6)) = 2.0
        _HeightFalloff ("Exponential Falloff", Range(0.5, 8)) = 3.0

        [Header(Wispy Noise)]
        _NoiseWorldSize ("Noise Size (world units)", Range(1, 60)) = 14.0
        _VerticalSquash ("Vertical Squash (sheets)", Range(1, 12)) = 5.0
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.85
        _WindA ("Layer A drift (xz) , rise (w)", Vector) = (0.5, 0.0, 0.2, 0.05)
        _WindB ("Layer B drift (xz) , rise (w)", Vector) = (-0.3, 0.0, 0.15, 0.03)

        [Header(Lighting)]
        _Anisotropy ("Forward Scattering (g)", Range(0, 0.95)) = 0.55
        _ScatterStrength ("Light Scatter Strength", Range(0, 4)) = 1.2
        _EdgeFade ("Side Edge Fade", Range(0.01, 0.5)) = 0.2
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
            float _DensityScale, _FogHeight, _HeightResponse, _HeightFalloff;
            float _NoiseWorldSize, _VerticalSquash, _NoiseStrength;
            float4 _WindA, _WindB;
            float _Anisotropy, _ScatterStrength, _EdgeFade;

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

            float2 BoxIntersect(float3 ro, float3 rd, float3 bmin, float3 bmax) {
                float3 t0 = (bmin - ro) / rd;
                float3 t1 = (bmax - ro) / rd;
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

            // Henyey-Greenstein phase: bright when looking toward the light through the fog
            float HG(float cosTheta, float g) {
                float g2 = g * g;
                return (1.0 - g2) / (4.0 * PI * pow(max(1.0 + g2 - 2.0 * g * cosTheta, 1e-4), 1.5));
            }

            half4 frag(Varyings input) : SV_Target {
                float3 ro = input.cameraOS;
                float3 rd = normalize(input.positionOS - ro);

                // March only the thin fog band, not the whole box, so steps aren't wasted on empty air
                float3 bmin = float3(-0.5, -0.5, -0.5);
                float3 bmax = float3(0.5, -0.5 + _FogHeight, 0.5);
                float2 t = BoxIntersect(ro, rd, bmin, bmax);
                t.x = max(t.x, 0.0);

                // Stop at opaque geometry (the water surface, props, the player)
                float2 screenUV = input.positionCS.xy / _ScaledScreenParams.xy;
                float rawDepth = SampleSceneDepth(screenUV);
                float3 sceneOS = TransformWorldToObject(ComputeWorldSpacePosition(screenUV, rawDepth, UNITY_MATRIX_I_VP));
                t.y = min(t.y, dot(sceneOS - ro, rd));
                if (t.y <= t.x) return half4(0, 0, 0, 0);

                const int stepCount = 32;
                float stepSize = (t.y - t.x) / stepCount;
                float stepWS = stepSize * length(rd * input.scaleWS); // world-space step length for Beer-Lambert

                float jitter = frac(52.9829189 * frac(dot(input.positionCS.xy, float2(0.06711056, 0.00583715))));
                float3 p = ro + rd * (t.x + stepSize * jitter);
                float3 dp = rd * stepSize;

                // Light scattering toward the main light
                float3 rdWS = normalize(TransformObjectToWorldDir(rd));
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float phase = HG(dot(rdWS, lightDir), _Anisotropy) * 4.0 * PI * 0.25; // roughly 0.25..several
                float3 lightCol = _MainLightColor.rgb * _ScatterStrength * phase;

                float3 driftA = float3(_WindA.x, _WindA.w, _WindA.z) * _Time.y;
                float3 driftB = float3(_WindB.x, _WindB.w, _WindB.z) * _Time.y;
                float3 noiseScale = float3(1.0, _VerticalSquash, 1.0) / _NoiseWorldSize;

                float3 color = 0.0;
                float transmittance = 1.0;

                [loop]
                for (int i = 0; i < stepCount; i++) {
                    float h = p.y + 0.5;

                    float sim = SAMPLE_TEXTURE2D_LOD(_DensityTex, sampler_DensityTex, p.xz + 0.5, 0).r;

                    // Layer thickness follows the sim, so cleared areas lose both density and height
                    float top = _FogHeight * saturate(sim * _HeightResponse);
                    float hn = h / max(top, 1e-3);                      // 0 at floor, 1 at layer top
                    float heightMask = exp(-hn * _HeightFalloff) * (1.0 - smoothstep(0.6, 1.0, hn));

                    // Two horizontally stretched noise layers drifting differently -> wispy sheets
                    float3 pw = p * input.scaleWS;
                    float nA = Noise3(pw * noiseScale + driftA);
                    float nB = Noise3(pw * noiseScale * 2.1 + driftB + 31.7);
                    float n = nA * 0.65 + nB * 0.35;
                    float shape = max(lerp(1.0, n * 2.0, _NoiseStrength), 0.0);

                    float2 e = abs(p.xz);
                    float edge = smoothstep(0.5, 0.5 - _EdgeFade, max(e.x, e.y));

                    float d = sim * heightMask * shape * edge * _DensityScale;
                    float a = 1.0 - exp(-d * stepWS);

                    // Low fog is darker / more tinted, upper fog catches the light
                    float3 base = lerp(_ShadowColor.rgb, _FogColor.rgb, saturate(hn * 0.7 + 0.3));
                    float3 col = base + lightCol * saturate(0.4 + n);
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
