Shader "Blit/Blit_MaskMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MetallicTex ("Texture", 2D) = "white" {}
        _SmoothnessTex ("Texture", 2D) = "white" {}
        _AoTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MetallicTex;
            sampler2D _SmoothnessTex;
            sampler2D _AoTex;
            float4 _MainTex_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 metallic = tex2D(_MetallicTex, i.uv);
                fixed4 smoothness = tex2D(_SmoothnessTex, i.uv);
                fixed4 ao = tex2D(_AoTex, i.uv);

                return fixed4(metallic.r, ao.g, 0, smoothness.a);
            }
            ENDCG
        }
    }
}
