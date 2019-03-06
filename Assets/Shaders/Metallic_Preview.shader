Shader "Custom/Metallic_Preview" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurTex ("Base (RGB)", 2D) = "white" {}
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
			
			float4 _MetalColor;
			
			float2 _SampleUV;
			
			float _HueWeight;
			float _SatWeight;
			float _LumWeight;
			
			float _MaskLow;
			float _MaskHigh;
			
			sampler2D _BlurTex;
			
			sampler2D _OverlayBlurTex;
			float _BlurOverlay;

			float _FinalContrast;
			float _FinalBias;
			
			float _Slider;
			
			#include "Photoshop.cginc"

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

				half3 mainTex = tex2Dlod( _MainTex, float4( UV, 0, 0 ) ).xyz;
				half3 blurTex = tex2Dlod( _BlurTex, float4( UV, 0, 0 ) ).xyz;
				half3 overlayBlurTex = tex2Dlod( _OverlayBlurTex, float4( UV, 0, 0 ) ).xyz;
				
				half3 overlay = ( mainTex - overlayBlurTex );
				half overlayGrey = overlay.x * 0.3 + overlay.y * 0.5 + overlay.z * 0.2;
				
				half3 metalSample = tex2Dlod( _BlurTex, float4( _SampleUV, 0, 0 ) ).xyz;
				
				half3 mainTexHSL = RGBToHSL( mainTex.xyz );
				half3 blurTexHSL = RGBToHSL( blurTex.xyz );
				half3 metalHSL = RGBToHSL( _MetalColor.xyz );
				half3 metalSampleHSL = metalHSL;//RGBToHSL( metalSample.xyz );
				
				float hueDif = 1.0 - min( min( abs( blurTexHSL.x - metalSampleHSL.x ), abs( ( blurTexHSL.x + 1.0 ) - metalSampleHSL.x ) ), abs( ( blurTexHSL.x - 1.0 ) - metalSampleHSL.x ) ) * 2.0;
				float satDif = 1.0 - abs( blurTexHSL.y - metalSampleHSL.y );
				float lumDif = 1.0 - abs( blurTexHSL.z - metalSampleHSL.z );
				
				
				float finalDiff = ( hueDif * _HueWeight ) + ( satDif * _SatWeight ) + ( lumDif * _LumWeight );
				finalDiff *= 1.0 / ( _HueWeight + _SatWeight + _LumWeight );
				finalDiff = smoothstep( _MaskLow, _MaskHigh, finalDiff );
				
				finalDiff = saturate( ( finalDiff - 0.5 ) * _FinalContrast + 0.5 + _FinalBias );
				finalDiff *= clamp( ( overlayGrey * _BlurOverlay ) + 1.0, 0.0, 10.0 );
				finalDiff = saturate( finalDiff );
				
				float3 finalColor = lerp( mainTex.xyz, finalDiff.xxx, smoothstep( _Slider - 0.01, _Slider + 0.01, UV.x ) );

				return float4( finalColor, 1 );
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
