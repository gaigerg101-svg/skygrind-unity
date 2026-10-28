// Atmosphere motes, after skygrind/src/fx/motes.js (POLISH V4; NIGHT12 U3-25.2): drifting dust and a few blinking data
// flecks in the air around you. One static mesh (Motes.cs: one quad per mote, its corner and seed in TEXCOORD0) drawn in
// one call; every mote lives in a box that wraps around the camera in the vertex shader (no CPU work a frame), drifts
// on a slow wind, fades in near the camera and out at the box's edge. Dust is warm and soft, the flecks pale violet
// squares that blink. Additive like the web's (three.js AdditiveBlending: SrcAlpha, One; the colour premultiplied by a).
// The web's gl_PointSize (pixels on its 1080 px canvas) becomes a camera-facing quad of the same pixel size, scaled to
// the render target's height (_RefHeight: UnityTuning.MoteRefHeight). The numbers in the shader are motes.js's own
// GLSL literals, kept line for line; the tuned ones come from config.motes (Motes.cs). The clock (_MoteTime) is the
// view's: the world's time in play (hitstop and pause freeze it), frame time on TITLE.
Shader "SKYGRIND/Motes"
{
    Properties
    {
        _Box ("Box (m)", Vector) = (60, 26, 60, 0)
        _Wind ("Wind (m/s)", Vector) = (0.25, 0.04, -0.12, 0)
        _Dust ("Dust", Color) = (1, 0.77, 0.6, 1)
        _Fleck ("Fleck", Color) = (0.81, 0.76, 1, 1)
        _Size ("Size", Float) = 1.6
        _Opacity ("Opacity", Float) = 0.55
        _MoteTime ("Time (s)", Float) = 0
        _RefHeight ("Reference height (px)", Float) = 1080
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Motes"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Box;
            float4 _Wind;
            float4 _Dust;
            float4 _Fleck;
            float _Size;
            float _Opacity;
            float _MoteTime;
            float _RefHeight;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION; // the mote's home in the box (Unity space; drawn with the identity matrix)
                float4 uv : TEXCOORD0; // xy the quad corner (-1..1), z the mote's seed (0..1)
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 corner : TEXCOORD0; // gl_PointCoord - 0.5
                float3 data : TEXCOORD1; // vFade, vFleck, vBlink
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float aSeed = v.uv.z;
                float uTime = _MoteTime;
                float3 p = v.positionOS.xyz + _Wind.xyz * uTime * (0.6 + aSeed);
                p.y += sin(uTime * 0.4 + aSeed * 40.0) * 0.4;
                // Wrap into a box centred on the camera (GLSL mod: x - y floor(x / y)).
                float3 cam = _WorldSpaceCameraPos.xyz;
                float3 q = p - cam + _Box.xyz * 0.5;
                p = cam + (q - _Box.xyz * floor(q / _Box.xyz)) - _Box.xyz * 0.5;
                float3 mv = TransformWorldToView(p);
                float d = length(mv);
                float vFade = smoothstep(0.8, 3.0, d) * (1.0 - smoothstep(_Box.x * 0.3, _Box.x * 0.5, d));
                float vFleck = step(0.9, aSeed);
                float vBlink = vFleck > 0.5 ? step(0.4, frac(uTime * (0.3 + aSeed) + aSeed * 13.0)) : 1.0;
                float pointSize = _Size * (vFleck > 0.5 ? 1.4 : 1.0) * (0.6 + aSeed) * 30.0 / max(d, 0.1);
                pointSize *= _ScreenParams.y / _RefHeight;
                float4 clip = TransformWViewToHClip(mv);
                // gl_PointSize pixels wide: each corner sits half of it from the centre, in NDC (2 / screen) x w in clip.
                clip.xy += v.uv.xy * (pointSize / _ScreenParams.xy) * clip.w;
                o.positionCS = clip;
                o.corner = v.uv.xy * 0.5;
                o.data = float3(vFade, vFleck, vBlink);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 c = i.corner;
                float soft = 1.0 - smoothstep(0.1, 0.5, length(c));
                float square = step(max(abs(c.x), abs(c.y)), 0.32);
                float shape = lerp(soft, square, i.data.y);
                float a = shape * i.data.x * i.data.z * _Opacity;
                return half4(lerp(_Dust.rgb, _Fleck.rgb, i.data.y) * a, a);
            }
            ENDHLSL
        }
    }
}
