// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Camera/CameraShader"
{
    Properties
    {	
		_MainTex ("Texture", 2D) = "white" {}
/*
		_OceanDeep ("Ocean Deep", Color) = (1,1,1,1)
		_OceanShallow ("Ocean Shallow", Color) = (1,1,1,1)
*/
    }
    SubShader
    {

		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			uniform float4 _CamWorldSpace;
			uniform float4x4 _CamFrustum;
			uniform float4x4 _CamToWorld;
	/*
			float4 _OceanDeep;
			float4 _OceanShallow;

			sampler2D _CameraDepthTexture;
			float2 raySphere(float3 centre, float radius, float3 rayOrigin, float3 rayDir){
				float3 offset = rayOrigin - centre;
				const float a = 1;
				float b = 2 * dot(offset, rayDir);
				float c = dot(offset, offset) - radius * radius;

				float discriminant = b*b-4*a*c;
				if (discriminant < 0){
					float s = sqrt(discriminant);
					float dstToSphereNear = max(0, (-b-s)/(2*a));
					float dstToSphereFar = (-b+s)/(2*a);

					if (dstToSphereFar >= 0){
						return float2(dstToSphereNear, dstToSphereFar-dstToSphereNear);
					}
				}

				return float2(999999, 0);
			}
	*/
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
	//				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
	//				float3 viewVector : TEXCOORD1;
				float4 vertex : SV_POSITION; //
				float3 ray : TEXCOORD1;
	//			float depth : DEPTH; //
			};

			v2f vert (appdata v)
			{
				v2f o;
				half index = v.vertex.z;
				v.vertex.z = 0;
	//				o.pos = UnityObjectToClipPos(v.vertex);
	//				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.ray = _CamFrustum[(int)index].xyz;
				o.ray /= abs(o.ray.z);
				o.ray = mul(_CamToWorld, o.ray);
//				o.vertex = UnityObjectToClipPos(v.vertex); //
//				o.depth = -UnityObjectToClipPos(v.vertex).z * _ProjectionParams.w; //

	//				float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
	//				o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 rayDirection = normalize(i.ray.xyz);
				float3 rayOrigin = _CamWorldSpace;
				return fixed4(rayDirection, 1);
				
	//			float invert = 1 - i.depth;
	//			return fixed4(invert,invert,invert,1);
	/*
				float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float sceneDepth = LinearEyeDepth(nonLinearDepth) * length(i.pos); // viewVector???
				float3 rayPos = _WorldSpaceCameraPos;
				float3 rayDir = normalize(i.pos); // viewVector???
				float2 hitInfo = raySphere(float3(0,0,0), 0, rayPos, rayDir); // oceanCentre = (0,0,0), oceanRadius = 1?
				float dstToOcean = hitInfo.x;
				float dstThroughOcean = hitInfo.y;
				float oceanViewDepth = min(dstThroughOcean, sceneDepth - dstToOcean);

				if (oceanViewDepth > 0){
					return float4(1,1,1,1);
				}

				return col;
	*/
			}
			ENDCG
		}
	}
}