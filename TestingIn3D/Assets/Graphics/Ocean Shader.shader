Shader "Planet/Ocean"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		/*
		_OceanRadius ("Ocean Radius", float) = 1
		_OceanDeep ("Ocean Deep", Color) = (1,1,1,1)
		_OceanShallow ("Ocean Shallow", Color) = (1,1,1,1)
		_DepthMultiplier ("Depth Multiplier", float) = 15
		_AlphaMultiplier ("Alpha Multiplier", float) = 60
		_Smoothness ("Smoothness", Range(0,1)) = 1
		
		_OceanGlow("Ocean Glow", float) = 1

		_Frequency("Frequency", float) = 2
		_WaveSpeed("Wave Speed", float) = 2000*/
    }
    SubShader
    {
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

			static const int maxNumSpheres = 10;
			float4 _OceanDeep[maxNumSpheres];
            float4 _OceanShallow[maxNumSpheres];
			float _OceanRadius[maxNumSpheres];
			float _DepthMultiplier[maxNumSpheres];
			float _AlphaMultiplier[maxNumSpheres];
			float4 _DirToSun[maxNumSpheres];

			float _Smoothness[maxNumSpheres];

			//float3 _OceanCentre;
			float4 _OceanCentre[maxNumSpheres];
			int _NumOceans;

			//float3 _SunCentre;

			//float _OceanGlow;

			/*float _Frequency;
			float _WaveSpeed;*/

			/*
			float _WaveSpeed;
			sampler2D _WaveNormalA;
			float _WaveNormalScale;
			sampler2D _WaveNormalB;
			float _WaveStrength;
			*/
			float4 _SpecularCol[maxNumSpheres];
			

			float _SelfGlow[maxNumSpheres];

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

			float3 blend_rnm(float3 n1, float3 n2)
			{
				n1.z += 1;
				n2.xy = -n2.xy;

				return n1 * dot(n1, n2) / n1.z - n2;
			}

			float3 triplanarNormal(float3 vertPos, float3 normal, float3 scale, float2 offset, sampler2D normalMap) {
				float3 absNormal = abs(normal);

				// Calculate triplanar blend
				float3 blendWeight = saturate(pow(normal, 4));
				// Divide blend weight by the sum of its components. This will make x + y + z = 1
				blendWeight /= dot(blendWeight, 1);

				// Calculate triplanar coordinates
				float2 uvX = vertPos.zy * scale + offset;
				float2 uvY = vertPos.xz * scale + offset;
				float2 uvZ = vertPos.xy * scale + offset;

				// Sample tangent space normal maps
				// UnpackNormal puts values in range [-1, 1] (and accounts for DXT5nm compression)
				float3 tangentNormalX = UnpackNormal(tex2D(normalMap, uvX));
				float3 tangentNormalY = UnpackNormal(tex2D(normalMap, uvY));
				float3 tangentNormalZ = UnpackNormal(tex2D(normalMap, uvZ));

				// Swizzle normals to match tangent space and apply reoriented normal mapping blend
				tangentNormalX = blend_rnm(half3(normal.zy, absNormal.x), tangentNormalX);
				tangentNormalY = blend_rnm(half3(normal.xz, absNormal.y), tangentNormalY);
				tangentNormalZ = blend_rnm(half3(normal.xy, absNormal.z), tangentNormalZ);

				// Apply input normal sign to tangent space Z
				float3 axisSign = sign(normal);
				tangentNormalX.z *= axisSign.x;
				tangentNormalY.z *= axisSign.y;
				tangentNormalZ.z *= axisSign.z;

				// Swizzle tangent normals to match input normal and blend together
				float3 outputNormal = normalize(
					tangentNormalX.zyx * blendWeight.x +
					tangentNormalY.xzy * blendWeight.y +
					tangentNormalZ.xyz * blendWeight.z
				);

				return outputNormal;
			}

            fixed4 frag (output o) : SV_Target
            {
				fixed4 originalCol = tex2D(_MainTex, o.uv);

				float3 rayPos = _WorldSpaceCameraPos;
				float viewLength = length(o.viewVector);
				float3 rayDir = o.viewVector / viewLength;

				float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, o.uv);
				float sceneDepth = LinearEyeDepth(nonlin_depth) * viewLength;

				//float3 dirToSun = _WorldSpaceLightPos0.xyz;
				float3 clipPlanePos = rayPos + o.viewVector * _ProjectionParams.y;

				for (int sphereIndex = 0; sphereIndex < _NumOceans; sphereIndex ++) {

					float2 hitInfo = raySphere(_OceanCentre[sphereIndex].xyz, _OceanRadius[sphereIndex], rayPos, rayDir);
					float dstToOcean = hitInfo.x;
					float dstThroughOcean = hitInfo.y;
					float3 rayOceanIntersectPos = rayPos + rayDir * dstToOcean - _OceanCentre[sphereIndex].xyz;

					// dst that view ray travels through ocean (before hitting terrain / exiting ocean)
					float oceanViewDepth = min(dstThroughOcean, sceneDepth - dstToOcean);

					if (oceanViewDepth > 0){
						float dstAboveWater = length(clipPlanePos - _OceanCentre[sphereIndex].xyz) - _OceanRadius[sphereIndex];

						float opticalDepth01 = 1 - exp(-oceanViewDepth * _DepthMultiplier[sphereIndex]);
						float alpha = 1 - exp(-oceanViewDepth * _AlphaMultiplier[sphereIndex]);

						float3 oceanNormal = normalize(rayPos + rayDir * dstToOcean - _OceanCentre[sphereIndex].xyz);
						float3 oceanSphereNormal = normalize(rayOceanIntersectPos);

						/*
						float2 waveOffsetA = float2(_Time.x * _WaveSpeed, _Time.x * _WaveSpeed * 0.8);
						float2 waveOffsetB = float2(_Time.x * _WaveSpeed * - 0.8, _Time.x * _WaveSpeed * -0.3);
						float3 waveNormal = triplanarNormal(rayOceanIntersectPos, oceanSphereNormal, _WaveNormalScale, waveOffsetA, _WaveNormalA);
						waveNormal = triplanarNormal(rayOceanIntersectPos, waveNormal, _WaveNormalScale, waveOffsetB, _WaveNormalB);
						waveNormal = normalize(lerp(oceanSphereNormal, waveNormal, _WaveStrength));
					
						float diffuseLighting = saturate(dot(oceanSphereNormal, dirToSun));
						float specularAngle = acos(dot(normalize(dirToSun - rayDir), waveNormal));
						float specularExponent = specularAngle / (1 - _Smoothness);
						float specularHighlight = exp(-specularExponent * specularExponent);
					*/
				
						float specularAngle = acos(dot(normalize(_DirToSun[sphereIndex].xyz - rayDir), oceanNormal));
					
						// noise on this shit
						//fnl_state noise = fnlCreateState();
						//noise.frequency = _Frequency;
						//specularAngle *= (1 + fnlGetNoise3D(noise, o.pos.x, o.pos.y+_Time[0]*_WaveSpeed/(length(rayPos - _OceanCentre)), o.pos.z)*0.5);
					
						float specularExponent = specularAngle / (1-_Smoothness[sphereIndex]);
						float specularHighlight = exp(-specularExponent * specularExponent);

						float diffuseLighting = (_SelfGlow[sphereIndex]) ? 1 : saturate(dot(oceanSphereNormal, _DirToSun[sphereIndex].xyz));
				
						float4 oceanCol = lerp(_OceanShallow[sphereIndex], _OceanDeep[sphereIndex], opticalDepth01) * diffuseLighting + (specularHighlight * (dstAboveWater > 0) * _SpecularCol[sphereIndex]);
						return lerp(originalCol, oceanCol, alpha);
					}
				}
				return originalCol;
            }
            ENDCG
        }
    }
}
