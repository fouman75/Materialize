Shader "Custom/AO_From_Normal_Preview" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200


		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _AOBlend;
			float _FinalBias;
			float _FinalContrast;

			// vertex-to-fragment interpolation data
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// vertex shader
			v2f vert (appdata_full v) {
				v2f o;
				o.pos = UnityObjectToClipPos ( v.vertex );
				o.uv = v.texcoord;
				return o;
			}

			fixed4 frag (v2f IN) : SV_Target {

				float2 UV = IN.uv;

				half2 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) ).xy;

				half AO = lerp( mainTex.x, mainTex.y, _AOBlend);
				
				AO += _FinalBias;
				AO = pow( AO, _FinalContrast );
				AO = saturate( AO );

				return float4( AO.xxx, 1 );
			}

			ENDCG
		} 

	} 
	FallBack "Diffuse"
}
