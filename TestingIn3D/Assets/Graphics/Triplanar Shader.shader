Shader "Unlit/Triplanar"
{
    Properties
    {
		_Texture ("Texture", 2D) = "gray" {}

		_NormalMapNoiseBreakof ("Normal Map Noise Breakof", 2D) = "white" {}
		_NormalMapA ("Normal MapA", 2D) = "white" {}
		_NormalMapB ("Normal MapB", 2D) = "white" {}

		_ScaleTexture ("Scale Texture", float) = 1
		_ScaleNormalMapA ("Scale Normal Map A", float) = 1
		_ScaleNormalMapB ("Scale Normal Map B", float) = 1
		_ScaleNoise ("Scale Noise", float) = 1

		_NormalMapStrength ("Normal Map Strength", Range(0, 1)) = 1
		_BlendSharpness ("Blend Sharpness", float) = 1
    }
    SubShader
    {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.5

		#include "UnityCG.cginc"

        struct Input
        {
            float3 normal;
            float3 vertPos;
            float4 pos;
        };

		sampler2D _Texture;

		sampler2D _NormalMapNoiseBreakof;
		sampler2D _NormalMapA;
		sampler2D _NormalMapB;

		float _ScaleTexture;
		float _ScaleNormalMapA;
		float _ScaleNormalMapB;
		float _ScaleNoise;

		float _NormalMapStrength;
		float _BlendSharpness;

        void vert (inout appdata_full v, out Input o)
        {
			UNITY_INITIALIZE_OUTPUT(Input, o);
/*            o.objNormal : TEXCOORD0;
            o.coords : TEXCOORD1;
            float4 pos : SV_POSITION;*/

			o.normal = v.normal;
			o.vertPos = v.vertex;
//			o.pos = v.pos;

/*            o.pos = UnityObjectToClipPos(pos);
            o.coords = pos.xyz;
            o.objNormal = normal;
			// o.uv = uv;
            return o;*/
        }

		float3 triplanarNormal(float3 position, float3 surfaceNormal, sampler2D normalMap, float scale){
			float3 tnormalX = UnpackNormal(tex2D(normalMap, position.zy * scale));
			float3 tnormalY = UnpackNormal(tex2D(normalMap, position.xz * scale));
			float3 tnormalZ = UnpackNormal(tex2D(normalMap, position.xy * scale));

			tnormalX = float3(tnormalX.xy + surfaceNormal.zy, tnormalX.z * surfaceNormal.x);
			tnormalY = float3(tnormalY.xy + surfaceNormal.xz, tnormalY.z * surfaceNormal.y);
			tnormalZ = float3(tnormalZ.xy + surfaceNormal.xy, tnormalZ.z * surfaceNormal.z);

			float3 weight = pow(abs(surfaceNormal), _BlendSharpness);
			weight /= dot(weight, 1);

			return normalize(tnormalX.zyx * weight.x + tnormalY.xzy * weight.y + tnormalZ.xyz * weight.z);
		}

/*		float4 frag(v2f i) : SV_Target{
			float2 uvX = i.coords.zy * _ScaleTexture;
			float2 uvY = i.coords.xz * _ScaleTexture;
			float2 uvZ = i.coords.xy * _ScaleTexture;

			float4 colX = tex2D(_Texture, uvX);
			float4 colY = tex2D(_Texture, uvY);
			float4 colZ = tex2D(_Texture, uvZ);

			float3 blendWeight = pow(abs(i.objNormal), _BlendSharpness);
			blendWeight /= dot(blendWeight, 1);

			float4 col = colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;

			float3 lightingNormal = triplanarNormal(i.coords, i.objNormal, _NormalMapA, _ScaleNormalMapA); // beroende på noise, sampla normalmap pixel
			float lightShading = saturate(dot(lightingNormal, _WorldSpaceLightPos0.xyz));
			return col * lightShading; // 0 eller 1, 
		}*/

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float2 uvX = IN.vertPos.zy * _ScaleTexture;
			float2 uvY = IN.vertPos.xz * _ScaleTexture;
			float2 uvZ = IN.vertPos.xy * _ScaleTexture;

			float4 colX = tex2D(_Texture, uvX);
			float4 colY = tex2D(_Texture, uvY);
			float4 colZ = tex2D(_Texture, uvZ);

			float3 blendWeight = pow(abs(IN.normal), _BlendSharpness);
			blendWeight /= dot(blendWeight, 1);

			float4 col = colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;

			float3 lightingNormal = triplanarNormal(IN.vertPos, IN.normal, _NormalMapA, _ScaleNormalMapA); // beroende på noise, sampla normalmap pixel
			float lightShading = saturate(dot(lightingNormal, _WorldSpaceLightPos0.xyz));
//			c = col * lightShading; // 0 eller 1, 
//			fixed4 c = tex2D(_Texture, IN.uv_MainTex);
			
			o.Albedo = col.rgb * lightShading;//c.rgb;
		}
		ENDCG
    }
	FallBack "Diffuse"
}
