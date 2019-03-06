Shader "Custom/Edge_From_Normal_Preview" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurTex0 ("Base (RGB)", 2D) = "white" {}
		_BlurTex1 ("Base (RGB)", 2D) = "white" {}
		_BlurTex2 ("Base (RGB)", 2D) = "white" {}
		_BlurTex3 ("Base (RGB)", 2D) = "white" {}
		_BlurTex4 ("Base (RGB)", 2D) = "white" {}
		_BlurTex5 ("Base (RGB)", 2D) = "white" {}
		_BlurTex6 ("Base (RGB)", 2D) = "white" {}
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
			
			sampler2D _BlurTex0;
			sampler2D _BlurTex1;
			sampler2D _BlurTex2;
			sampler2D _BlurTex3;
			sampler2D _BlurTex4;
			sampler2D _BlurTex5;
			sampler2D _BlurTex6;
			
			float _BlurContrast;
		
			float _Blur0Weight;
			float _Blur0Contrast;

			float _Blur1Weight;
			float _Blur1Contrast;

			float _Blur2Weight;
			float _Blur2Contrast;

			float _Blur3Weight;
			float _Blur3Contrast;

			float _Blur4Weight;
			float _Blur4Contrast;

			float _Blur5Weight;
			float _Blur5Contrast;

			float _Blur6Weight;
			float _Blur6Contrast;
			
			float _FinalBias;

			float _FinalContrast;
			
			float _Pinch;
			
			float _Pillow;
			
			float _EdgeAmount;
			
			float _CreviceAmount;
			

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

				half4 mainTex = tex2Dlod(_MainTex, float4( UV, 0, 0 ) );
				half4 blurTex1 = tex2Dlod(_BlurTex1, float4( UV, 0, 0 ) );
				half4 blurTex2 = tex2Dlod(_BlurTex2, float4( UV, 0, 0 ) );
				half4 blurTex3 = tex2Dlod(_BlurTex3, float4( UV, 0, 0 ) );
				half4 blurTex4 = tex2Dlod(_BlurTex4, float4( UV, 0, 0 ) );
				half4 blurTex5 = tex2Dlod(_BlurTex5, float4( UV, 0, 0 ) );
				half4 blurTex6 = tex2Dlod(_BlurTex6, float4( UV, 0, 0 ) );
				
				mainTex.w = 1.0;
				blurTex1.w = 1.0;
				blurTex2.w = 1.0;
				blurTex3.w = 1.0;
				blurTex4.w = 1.0;
				blurTex5.w = 1.0;
				blurTex6.w = 1.0;
				
				//Put these on slider?
				
				mainTex *= _Blur0Weight;
				blurTex1 *= _Blur1Weight;
				blurTex2 *= _Blur2Weight;
				blurTex3 *= _Blur3Weight;
				blurTex4 *= _Blur4Weight;
				blurTex5 *= _Blur5Weight;
				blurTex6 *= _Blur6Weight;
				
				mainTex = mainTex + blurTex1 + blurTex2 + blurTex3 + blurTex4 + blurTex5 + blurTex6;
				
				mainTex *= 1.0 / mainTex.w;
				
				if( mainTex.x > 0.5 ){
					mainTex.x = max( mainTex.x * 2.0 - 1.0, 0.0 );
					mainTex.x = pow( mainTex.x, _Pinch );
					mainTex.x *= _EdgeAmount;
					mainTex.x = mainTex.x * 0.5 + 0.5;
				}else{
					mainTex.x = max( -( mainTex.x * 2.0 - 1.0 ), 0.0 );
					mainTex.x = pow( mainTex.x, _Pinch );
					mainTex.x *= _CreviceAmount;
					mainTex.x = -mainTex.x * 0.5 + 0.5;
				}
				
				mainTex.x = ( ( mainTex.x - 0.5 ) * _FinalContrast ) + 0.5;
				
				mainTex.x = pow( mainTex.x, _Pillow );
				
				mainTex.x = saturate( mainTex.x + _FinalBias );

				return float4( mainTex.xxx, 1 );
			}

			ENDCG
		} 
	} 
	FallBack "Diffuse"
}
