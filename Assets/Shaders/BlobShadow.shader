// Contact shadows, after skygrind/src/fx/blobShadows.js (POLISH V4; NIGHT12 U3-25.2): a soft dark blob on the floor
// under every enemy, so bodies sit on the floor instead of hovering over it. One instanced draw (BlobShadows.cs:
// Graphics.RenderMeshInstanced of a flat 1 m quad in the XZ plane) with a radial falloff, normal blending, no depth
// write, pulled toward the camera like the web's polygonOffset(-3, -3). Each blob's alpha (1 grounded,
// blobShadows.airAlpha airborne) rides in its instance matrix's y scale: the quad is flat in y, so that scale moves no
// vertex. The numbers in the shader are blobShadows.js's own GLSL literals; colour and opacity come from config.
Shader "SKYGRIND/BlobShadow"
{
    Properties
    {
        _Color ("Colour", Color) = (0.04, 0.03, 0.08, 1)
        _Opacity ("Opacity", Float) = 0.55
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-50" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Pass
        {
            Name "BlobShadow"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Offset -3, -3
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Opacity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float alpha : TEXCOORD1;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                float4x4 m = GetObjectToWorldMatrix();
                o.alpha = length(float3(m._m01, m._m11, m._m21)); // aAlpha: the instance's y scale
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float r = length(i.uv - 0.5) * 2.0;
                float a = (1.0 - smoothstep(0.2, 1.0, r)) * _Opacity * i.alpha;
                return half4(_Color.rgb, a);
            }
            ENDHLSL
        }
    }
}
