Shader "Hidden/Ocean Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		_OceanRadius ("Ocean Radius", float) = 1
		_OceanDeep ("Ocean Deep", Color) = (1,1,1,1)
		_OceanShallow ("Ocean Shallow", Color) = (1,1,1,1)
		_DepthMultiplier ("Depth Multiplier", float) = 15
		_AlphaMultiplier ("Alpha Multiplier", float) = 60
		_Smoothness ("Smoothness", Range(0,1)) = 1
		
		_Frequency("Frequency", float) = 2
		_WaveSpeed("Wave Speed", float) = 2000
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
			#include "/Assets/Resources/FastNoiseLite.cginc"

            uniform sampler2D _MainTex;
			uniform sampler2D _CameraDepthTexture;
			uniform fixed _DepthLevel;
			uniform float4 _MainTex_TexelSize;

			float4 _OceanDeep;
            float4 _OceanShallow;
			float _OceanRadius;
			float _DepthMultiplier;
			float _AlphaMultiplier;

			float _Smoothness;

			float3 _OceanCentre;

			float _Frequency;
			float _WaveSpeed;

            struct input
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct output
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
				float3 viewVector : TEXCOORD1;
            };

            output vert(input i)
            {
                output o;
                o.pos = UnityObjectToClipPos(i.pos);
                o.uv = i.uv; 
				float3 viewVector = mul(unity_CameraInvProjection, float4(i.uv * 2 - 1, 0, -1));
				o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return o;
            }

			// Returns vector (dstToSphere, dstThroughSphere)
			// If ray origin is inside sphere, dstToSphere = 0
			// If ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
			float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir) {
				float3 offset = rayOrigin - sphereCentre;
				float a = 1; // Set to dot(rayDir, rayDir) if rayDir might not be normalized
				float b = 2 * dot(offset, rayDir);
				float c = dot (offset, offset) - sphereRadius * sphereRadius;
				float d = b * b - 4 * a * c; // Discriminant from quadratic formula

				// Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
				if (d > 0) {
					float s = sqrt(d);
					float dstToSphereNear = max(0, (-b - s) / (2 * a));
					float dstToSphereFar = (-b + s) / (2 * a);

					// Ignore intersections that occur behind the ray
					if (dstToSphereFar >= 0) {
						return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
					}
				}
				// Ray did not intersect sphere
				return float2(3.402823466e+38, 0);
			}

            fixed4 frag (output o) : SV_Target
            {
				fixed4 originalCol = tex2D(_MainTex, o.uv);

				float3 rayPos = _WorldSpaceCameraPos;
				float viewLength = length(o.viewVector);
				float3 rayDir = o.viewVector / viewLength;

				float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, o.uv);
				float sceneDepth = LinearEyeDepth(nonlin_depth) * viewLength;

				float2 hitInfo = raySphere(_OceanCentre, _OceanRadius, rayPos, rayDir);
				float dstToOcean = hitInfo.x;
				float dstThroughOcean = hitInfo.y;
				float3 rayOceanIntersectPos = rayPos + rayDir * dstToOcean - _OceanCentre;

				// dst that view ray travels through ocean (before hitting terrain / exiting ocean)
				float oceanViewDepth = min(dstThroughOcean, sceneDepth - dstToOcean);

				if (oceanViewDepth > 0){
//					float oneMinusDot = 1.0 - dot(_WorldSpaceLightPos0.xyz, _OceanCentre);
//					float dirToSun = pow(oneMinusDot, 5.0);

					//float3 dirToSun = -normalize(_OceanCentre);//FOR POINT LIGHT;
					float3 dirToSun = _WorldSpaceLightPos0.xyz; //FOR DIRECTIONAL LIGHT

					float opticalDepth01 = 1 - exp(-oceanViewDepth * _DepthMultiplier);
					float alpha = 1 - exp(-oceanViewDepth * _AlphaMultiplier);

					float3 oceanNormal = normalize(rayPos + rayDir * dstToOcean - _OceanCentre);

					float specularAngle = acos(dot(normalize(dirToSun - rayDir), oceanNormal));

/*					// noise on this shit
					fnl_state noise = fnlCreateState();
					noise.frequency = _Frequency;
					specularAngle *= (1 + fnlGetNoise3D(noise, o.pos.x, o.pos.y+_Time[0]*_WaveSpeed/(length(rayPos - _OceanCentre)), o.pos.z)*0.5);
*/
					float specularExponent = specularAngle / (1-_Smoothness);
					float specularHighlight = exp(-specularExponent * specularExponent);

					float diffuseLightning = saturate(dot(oceanNormal, dirToSun));

					float4 oceanCol = lerp(_OceanShallow, _OceanDeep, opticalDepth01) * diffuseLightning + specularHighlight;
					return lerp(originalCol, oceanCol, alpha);
				}
				return originalCol;
				
//				fixed4 originalCol = tex2D(_MainTex, i.uv);

/*				float3 rayPos = _WorldSpaceCameraPos;
				float viewLength = length(i.viewVector);
				float3 rayDir = i.viewVector / viewLength;*/
/*
				float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float sceneDepth = LinearEyeDepth(nonlin_depth) * length(i.viewVector);
				return sceneDepth/10;*/
            }
            ENDCG
        }
    }
}
