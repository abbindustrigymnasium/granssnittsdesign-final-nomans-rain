Shader "Hidden/PostProcessing"
{
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			sampler2D _CameraDepthTexture;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 depth : TEXCOORD0;
            };

            v2f vert (appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_DEPTH(o.depth);
                return o;
            }

            half4 frag(v2f i) : SV_Target {
//                float depth = tex2D(_CameraDepthTexture, i.uv).r;
				return 1;//depth;
            }
            ENDCG
        }
    }
}
