// NIGHT43 builder U1 (design.n43.screens43): the backdrop blur for the shop and the results (G24: "Blur or otherwise visually separate the gameplay
// background"). A dual-filter Kawase blur (the down and up passes of Marius Bjorge's "Bandwidth-efficient rendering", SIGGRAPH 2015; the same
// filter as Bluecadet UI Blur, ui-art-web B8), run ONCE on a captured frame by Graphics.Blit (Game/UiBlur43.cs): nothing runs per frame.
// Pass 0 down (centre x4 + four half-texel diagonals, / 8), pass 1 up (four edge taps + four diagonal taps x2, / 12).
// In a Resources folder so Shader.Find has it in a Player without the Always Included list.
Shader "SKYGRIND/UiBlur43"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _Offset ("Offset (texels)", Float) = 1.0
    }
    SubShader
    {
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        float _Offset;

        float4 Down(v2f_img i) : SV_Target
        {
            float2 h = _MainTex_TexelSize.xy * 0.5 * _Offset;
            float4 s = tex2D(_MainTex, i.uv) * 4.0;
            s += tex2D(_MainTex, i.uv - h);
            s += tex2D(_MainTex, i.uv + h);
            s += tex2D(_MainTex, i.uv + float2(h.x, -h.y));
            s += tex2D(_MainTex, i.uv - float2(h.x, -h.y));
            return s / 8.0;
        }

        float4 Up(v2f_img i) : SV_Target
        {
            float2 h = _MainTex_TexelSize.xy * 0.5 * _Offset;
            float4 s = tex2D(_MainTex, i.uv + float2(-h.x * 2.0, 0.0));
            s += tex2D(_MainTex, i.uv + float2(-h.x, h.y)) * 2.0;
            s += tex2D(_MainTex, i.uv + float2(0.0, h.y * 2.0));
            s += tex2D(_MainTex, i.uv + float2(h.x, h.y)) * 2.0;
            s += tex2D(_MainTex, i.uv + float2(h.x * 2.0, 0.0));
            s += tex2D(_MainTex, i.uv + float2(h.x, -h.y)) * 2.0;
            s += tex2D(_MainTex, i.uv + float2(0.0, -h.y * 2.0));
            s += tex2D(_MainTex, i.uv + float2(-h.x, -h.y)) * 2.0;
            return s / 12.0;
        }
        ENDCG

        Pass
        {
            Name "Down"
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Down
            ENDCG
        }
        Pass
        {
            Name "Up"
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Up
            ENDCG
        }
    }
    Fallback Off
}
