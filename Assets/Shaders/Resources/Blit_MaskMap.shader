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
        Tags { "RenderType"="Transparent" }
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
                
                float  metallicMean = float(metallic.r + metallic.g + metallic.b);
                metallicMean = metallicMean / 3; 
                
                float  smoothnessMean = float(smoothness.r + smoothness.g + smoothness.b);
                smoothnessMean = smoothnessMean / 3; 
                
                float  aoMean = float(ao.r + ao.g + ao.b);
                aoMean = aoMean / 3; 

                return fixed4(metallicMean, aoMean, 0, smoothnessMean);
            }
            ENDCG
        }
    }
}
