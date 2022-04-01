// Applies a blacklevel filter to aid in blending black areas
// of overlapping projectors.
Shader "HEVS/Blacklevel" 
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlackTex ("Black (R)", 2D) = "white" {}
		_BlackLevel ("Black Level", Range(0.0,1.0)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
		
			#include "UnityCG.cginc"
		
			uniform sampler2D _MainTex;
			uniform sampler2D _BlackTex;
			uniform float _BlackLevel;
		
			fixed4 frag(v2f_img i) : SV_Target {

				float3 rgb =  tex2D(_MainTex,i.uv).rgb;

				float blackThreshold = tex2D(_BlackTex, i.uv).r;

				if (blackThreshold < 0.5) 
					rgb = clamp(rgb, _BlackLevel, 1.0);

				return fixed4(rgb, 1);
			}
			ENDCG
		}
	}
}
