// Used to blend overlapping projectors by fading out regions that overlap.
Shader "HEVS/ProjectorBlend"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BlendTex ("Texture", 2D) = "white" {}
		_BlendMode("Blend Mode", int) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		// No culling or depth
		Cull Off

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
				float2 uv1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _BlendTex;
			float4 _MainTex_ST;
			float4 _BlendTex_ST;
			int _BlendMode;
			
			v2f vert (appdata v)
			{
				v2f o;
				v.uv1 = v.vertex.xyz;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv1 = TRANSFORM_TEX(v.uv1, _BlendTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 blend = tex2D(_BlendTex, i.uv1);

				// apply the blend
				// Note: an older method used an inverted alpha channel. This will
				// be phased out in a future release.
				if (_BlendMode == 0)
					col.rgb = col.rgb * blend.r;
				else if (_BlendMode == 1)
					col = col * (1.0 - blend.a);

				return col;
			}
			ENDCG
		}
	}
}
