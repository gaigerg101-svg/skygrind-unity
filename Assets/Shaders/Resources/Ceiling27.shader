// NIGHT27 builder V (roof27): the ceiling's solid colour. SKYGRIND/Particle's colour sits in the instancing buffer outside UnityPerMaterial, and the first Player reel
// (n27-2) showed no ceiling drawn with it on a MeshRenderer; this one keeps _Color in the SRP-batcher constant buffer. Opaque, unlit, both faces, fogged like URP Unlit.
// A slow darkening toward the far side of the view keeps a flat colour from reading as a hole; no textures.
Shader "SKYGRIND/Ceiling27"
{
    Properties
    {
        _Color ("Color", Color) = (0.1, 0.06, 0.28, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "Ceiling27"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off
            ZWrite On
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            CBUFFER_END

            // NIGHT51 ceil51 (UnityTuning.N51.cs, Night51.Apply): a coffer grid from MeshKit's face coordinates (uv3): beams between panels sized
            // to divide the face evenly, lighter; the panels' middles darker. Off (-skygrindOff51: _Stone51C.w 0) or no face data: the flat colour.
            float4 _Stone51C;
            float4 _Ceil51; // x panel target (m), y beam width (m), z beam light (x), w recess dark (x)
            // NIGHT74 crown74 (Game/Crown74.cs; Tools/night/n74-crown.py is its meter): THE CROWN's ceiling lifted with a cool fill. Globals the C# side sets (zero = off: the block below is skipped).
            float4 _Crown74;  // rgb the fill in display (sRGB) values 0-1 (a = the weight 0-1, faded in and out), added to the fogged ceiling colour in display space
            float4 _Crown74B; // x display luma under which a ceiling pixel takes the fill whole, y display luma from which it takes none (smoothstep between)

            struct Attributes { float4 positionOS : POSITION; float4 face : TEXCOORD3; };
            struct Varyings { float4 positionCS : SV_POSITION; float fogFactor : TEXCOORD0; float4 face : TEXCOORD1; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.fogFactor = ComputeFogFactor(p.positionCS.z);
                o.face = v.face;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 c = _Color.rgb;
                if (_Stone51C.w > 0.5 && i.face.z > 0.01 && i.face.w > 0.01 && _Ceil51.x > 0)
                {
                    float2 count = max(1, round(i.face.zw / _Ceil51.x));
                    float2 cell = i.face.zw / count;
                    float2 fr = frac(i.face.xy / cell);
                    float2 d = min(fr, 1 - fr) * cell; // m to the nearest beam line (the face's edges are beams too)
                    float e = min(d.x, d.y);
                    float aa = max(max(fwidth(i.face.x), fwidth(i.face.y)), 1e-4);
                    float beam = 1 - smoothstep(_Ceil51.y - aa, _Ceil51.y + aa, e);
                    float recess = smoothstep(_Ceil51.y, _Ceil51.y + 0.6, e);
                    c *= lerp(lerp(1, _Ceil51.w, recess), _Ceil51.z, beam);
                }
                half3 outc = MixFog(c, i.fogFactor);
                if (_Crown74.a > 0.0)
                {
                    // NIGHT74 crown74: the fogged ceiling over the arena's far side is near black (gg-proxy-close #9: THE CROWN's ceiling 900-1300 x 150-450). The fill is added in display
                    // space (the sRGB curves, inline as Crt29.shader's), strongest on the darkest pixels (w = 1 under _Crown74B.x, 0 from _Crown74B.y), so the coffer grid keeps its shape
                    // and a lit beam is left alone. A cool fill: red never over green (no danger hue), blue leading.
                    float3 dsp = max((float3)outc, 0.0);
                    dsp = lerp(1.055 * pow(dsp, 1.0 / 2.4) - 0.055, dsp * 12.92, step(dsp, 0.0031308));
                    float lum = dot(dsp, float3(0.299, 0.587, 0.114));
                    float wc = 1.0 - smoothstep(_Crown74B.x, _Crown74B.y, lum);
                    dsp = saturate(dsp + _Crown74.rgb * (_Crown74.a * wc));
                    outc = (half3)lerp(pow((dsp + 0.055) / 1.055, 2.4), dsp / 12.92, step(dsp, 0.04045));
                }
                return half4(outc, 1);
            }
            ENDHLSL
        }
    }
}
