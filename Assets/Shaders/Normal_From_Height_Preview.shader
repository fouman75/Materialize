Shader "Custom/Normal_From_Height_Preview" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_HeightTex ("Base (RGB)", 2D) = "white" {}
		_BlurTex0 ("Base (RGB)", 2D) = "white" {}
		_BlurTex1 ("Base (RGB)", 2D) = "white" {}
		_BlurTex2 ("Base (RGB)", 2D) = "white" {}
		_BlurTex3 ("Base (RGB)", 2D) = "white" {}
		_BlurTex4 ("Base (RGB)", 2D) = "white" {}
		_BlurTex5 ("Base (RGB)", 2D) = "white" {}
		_BlurTex6 ("Base (RGB)", 2D) = "white" {}
		
		//_SlopeBias ("Slope Bias", Float) = 0.0
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
			
			sampler2D _HeightTex;
			sampler2D _HeightBlurTex;
			
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

			float _FinalContrast;
			
			float _SlopeBias;
			float _ShapeRecognition;
			float _LightRotation;
			
			float _Angularity;
			float _AngularIntensity;
			
			int _FlipNormalY;

			float _Slider;

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
				half4 heightTex = tex2D(_HeightTex, UV);

				half4 mainTex = half4( tex2Dlod(_BlurTex0, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur0Weight;
				half4 blurTex1 = half4( tex2Dlod(_BlurTex1, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur1Weight;
				half4 blurTex2 = half4( tex2Dlod(_BlurTex2, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur2Weight;
				half4 blurTex3 = half4( tex2Dlod(_BlurTex3, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur3Weight;
				half4 blurTex4 = half4( tex2Dlod(_BlurTex4, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur4Weight;
				half4 blurTex5 = half4( tex2Dlod(_BlurTex5, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur5Weight;
				half4 blurTex6 = half4( tex2Dlod(_BlurTex6, float4( UV, 0, 0 ) ).xyz, 1.0 ) * _Blur6Weight;
				
				mainTex = mainTex + blurTex1 + blurTex2 + blurTex3 + blurTex4 + blurTex5 + blurTex6;
				
				mainTex *= 1.0 / mainTex.w;
				
				mainTex.xyz = normalize( mainTex.xyz * 2.0 - 1.0 );
				
				float3 angularDir = normalize( float3( normalize( float3( mainTex.xy, 0.01 ) ).xy * _AngularIntensity, max( 1.0 - _AngularIntensity, 0.01 ) ) );
				mainTex.xyz = lerp( mainTex.xyz, angularDir, _Angularity );
				
				mainTex.xy = mainTex.xy * max( _FinalContrast, 0.01 );
				mainTex.z = pow( saturate( mainTex.z ), max( _FinalContrast, 0.01 ) );
				
				mainTex.xyz = normalize( mainTex.xyz ) * 0.5 + 0.5;

                float3 finalColor = lerp( mainTex.xyz, heightTex.xxx, smoothstep( _Slider - 0.01, _Slider + 0.01, UV.x ) );
				return float4( finalColor, 1 );
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
