Shader "HEVS/CameraOverlay"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}

        _StereoMode("Stereo Mode", int) = 0
        _Left("Left Eye", 2D) = "white" {}
        _Right("Right Eye", 2D) = "white" {}
        _Top("Top Texture", 2D) = "white" {}
        _Bottom("Bottom Texture", 2D) = "white" {}
        _Border("Border Width", float) = 0.0

        _LeftModifier("Left Eye Modifier", Vector) = (1,1,1,1)
        _RightModifier("Right Eye Modifier", Vector) = (1,1,1,1)

        _UseWarp("Use Warp", int) = 0
        _Warp("Warp Texture", 2D) = "white" {}

        _UseBlend("Use Blend", int) = 0
        _Blend("Blend Texture", 2D) = "white" {}

        _UseBlackLevel("Use Black Level", int) = 0
        _BlackLevelMask("Black Level Mask", 2D) = "white" {}
        _BlackLevel("Black Level", Range(0.0,1.0)) = 0.0

        _AspectScale("Aspect Scale", float) = 1
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
                float4 vertex : SV_POSITION;
            };

            // main texture if not stereo or a dome
            sampler2D _MainTex;

            // dome and stereo source images
            int _StereoMode;
            sampler2D _Left;
            sampler2D _Right;
            sampler2D _Top;
            sampler2D _Bottom;
            float _Border;

            // anaglyph colour modifiers
            float4 _LeftModifier;
            float4 _RightModifier;

            // warp & blend
            int _UseWarp;
            sampler2D _WarpTex;
            int _UseBlend;
            sampler2D _BlendTex;

            // black level modifiers
            int _UseBlackLevel;
            sampler2D _BlackLevelMask;
            float _BlackLevel;

            float _AspectScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            float3 fragDome(float2 iuv)
            {
                float2 uv = (iuv + float2(-0.5, -0.5)) * 2.0;
                uv = uv / (1.0 - _Border);

                float luv = length(uv);
                if (luv > 1.0) return fixed4(0, 0, 0, 1);
                float2 angle = float2(atan2(uv.y, uv.x) + UNITY_PI * 0.5, (1.0 - luv) * UNITY_PI * 0.5);

                float x = sin(angle.x) * cos(angle.y);
                float z = -sin(angle.y);
                float y = -cos(angle.x) * cos(angle.y);
                float3 coord = float3(x, y, z);
                float a = sqrt(0.5);
                float3x3 rot = float3x3(-a, 0, -a, 0, 1, 0, -a, 0, a);
                float3 coordr = mul(coord, rot);
                x = coordr.x; y = coordr.y; z = coordr.z;

                float absX = abs(x);
                float absY = abs(y);
                float absZ = abs(z);
                float maxAxis, uc, vc;

                // POSITIVE X
                if (x > 0 && absX >= absY && absX >= absZ) {
                    maxAxis = absX;
                    uc = -z;
                    vc = y;
                    uv.x = 0.5f * (uc / maxAxis + 1.0f);
                    uv.y = 0.5f * (vc / maxAxis + 1.0f);
                    return tex2D(_Left, uv).rgb;
                }
                // POSITIVE Y
                else if (y > 0 && absY >= absX && absY >= absZ) {
                    maxAxis = absY;
                    uc = x;
                    vc = -z;
                    uv.y = 1.0f - 0.5f * (uc / maxAxis + 1.0f);
                    uv.x = 0.5f * (vc / maxAxis + 1.0f);
                    return tex2D(_Top, uv).rgb;
                }
                // NEGATIVE Y
                else if (y <= 0 && absY >= absX && absY >= absZ) {
                    maxAxis = absY;
                    uc = x;
                    vc = z;
                    uv.y = 1.0 - 0.5f * (uc / maxAxis + 1.0f);
                    uv.x = 1.0 - 0.5f * (vc / maxAxis + 1.0f);
                    return tex2D(_Bottom, uv.yx).rgb;
                }
                // NEGATIVE Z
                else if (z <= 0 && absZ >= absX && absZ >= absY) {
                    maxAxis = absZ;
                    uc = -x;
                    vc = y;
                    uv.x = 0.5f * (uc / maxAxis + 1.0f);
                    uv.y = 0.5f * (vc / maxAxis + 1.0f);
                    return tex2D(_Right, uv).rgb;
                }

                return float3(1, 0, 1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const int STEREO_MODE_MONO = 0;
                const int STEREO_MODE_SIDEBYSIDE = 1;
                const int STEREO_MODE_TOPBOTTOM = 2;
                const int STEREO_MODE_ANAGLYPH = 3;
                const int STEREO_MODE_DOME = 4;

                float3 fragOut = float3(1, 0, 1);
                float3 uv = float3(i.uv, 1);
                float3 blend = float3(1, 1, 1);
                float blackLevelMask = 1;

                // shift UV if split screen
                if (_StereoMode == STEREO_MODE_SIDEBYSIDE)
                    uv.x = uv.x % 0.5 * 2;
                else if (_StereoMode == STEREO_MODE_TOPBOTTOM)
                    uv.y = uv.y % 0.5 * 2;

                // sample black level mask
                if (_UseBlackLevel)
                    blackLevelMask = tex2D(_BlackLevelMask, uv).r;

                // sample blend
                if (_UseBlend != 0)
                    blend = tex2D(_BlendTex, i.uv).rgb;

                // sample warp
                if (_UseWarp != 0) 
                    uv = tex2D(_WarpTex, uv.xy).rgb;
                
                // clip if outside keystone
                if (uv.z < 0.5) 
                    return fixed4(0, 0, 0, 1);

                // sample dome
                if (_StereoMode == STEREO_MODE_DOME)
                {
                    uv.x = (uv.x - 0.5) * _AspectScale + 0.5;
                    fragOut = fragDome(uv.xy);
                }
                else if (_StereoMode == STEREO_MODE_MONO)
                    fragOut = tex2D(_MainTex, uv.xy).rgb;
                else
                {
                    float3 leftEye = tex2D(_Left, uv.xy).rgb;
                    float3 rightEye = tex2D(_Right, uv.xy).rgb;

                    if (_StereoMode == STEREO_MODE_ANAGLYPH)
                        fragOut = leftEye * _LeftModifier.rgb + rightEye * _RightModifier.rgb;
                    else if (_StereoMode == STEREO_MODE_SIDEBYSIDE)
                    {
                        float mod = step(i.uv.x, 0.5);
                        fragOut = leftEye * (1 - mod) + rightEye * mod;
                    }
                    else if (_StereoMode == STEREO_MODE_TOPBOTTOM)
                    {
                        float mod = step(i.uv.y, 0.5);
                        fragOut = leftEye * mod + rightEye * (1 - mod);
                    }
                }

                // combine blend
                fragOut *= blend;

                // apply black level
                if (blackLevelMask < 0.5)
                    fragOut = clamp(fragOut, _BlackLevel, 1.0);

                return fixed4(fragOut, 1);
            }
            ENDCG
        }
    }
}
