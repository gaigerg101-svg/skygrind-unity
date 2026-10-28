// The dusk sky, after skygrind/src/world/sky.js (the parts ported tonight): a three-stop gradient (config.colors
// skyHorizon -> skyMid -> skyZenith by elevation), a warm glow around the low sun (config.sky.sunElevationDeg /
// sunAzimuthDeg, warmGlow) and the sun disc (sunSize). The web's stars, bands and cloud sea are later.
Shader "SKYGRIND/Sky"
{
    Properties
    {
        _Zenith ("Zenith", Color) = (0.06, 0.04, 0.17, 1)
        _Mid ("Mid", Color) = (0.29, 0.13, 0.41, 1)
        _Horizon ("Horizon", Color) = (1, 0.48, 0.28, 1)
        _Sun ("Sun", Color) = (1, 0.84, 0.65, 1)
        _SunDir ("Sun direction", Vector) = (0, 0.12, 1, 0)
        _SunSize ("Sun size (deg)", Float) = 1.4
        _Glow ("Warm glow", Float) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Zenith, _Mid, _Horizon, _Sun, _SunDir;
            float _SunSize, _Glow;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 dir : TEXCOORD0; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.dir = v.positionOS.xyz;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float h = d.y;
                float3 c = h > 0 ? lerp(_Mid.rgb, _Zenith.rgb, saturate(pow(h, 0.6))) : _Mid.rgb;
                float horizon = exp(-abs(h) * 7);
                c = lerp(c, _Horizon.rgb, horizon);
                float3 s = normalize(_SunDir.xyz);
                float cosA = dot(d, s);
                c += _Horizon.rgb * _Glow * pow(saturate(cosA), 8) * 0.8;
                float disc = smoothstep(cos(radians(_SunSize)), cos(radians(_SunSize * 0.8)), cosA);
                c = lerp(c, _Sun.rgb * 3, disc);
                if (h < 0) c = lerp(c, _Mid.rgb * 0.55, saturate(-h * 4));
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
