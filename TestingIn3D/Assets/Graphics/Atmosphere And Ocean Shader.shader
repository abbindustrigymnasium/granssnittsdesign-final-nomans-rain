Shader "Planet/Atmosphere And Ocean"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
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

				sampler2D _MainTex;
				sampler2D _BakedOpticalDepth;
				sampler2D _CameraDepthTexture;

				static const int maxNumSpheres = 10;
				// vars
				float4 dirToSun[maxNumSpheres];
				float4 planetCentre[maxNumSpheres];
				float atmosphereRadius[maxNumSpheres];

				float numInScatteringPoints[maxNumSpheres];
				float numOpticalDepthPoints[maxNumSpheres];
				float4 scatteringCoefficients[maxNumSpheres];
				float scatteringStrength[maxNumSpheres];
				float densityFalloff[maxNumSpheres];

				float4 oceanDeep[maxNumSpheres];
				float4 oceanShallow[maxNumSpheres];
				float oceanRadius[maxNumSpheres];
				float depthMultiplier[maxNumSpheres];
				float alphaMultiplier[maxNumSpheres];
				float smoothness[maxNumSpheres];
				float4 specularCol[maxNumSpheres];
				float selfGlow[maxNumSpheres];

                float waveSpeed[maxNumSpheres];
				sampler2D waveNormalA;
				sampler2D waveNormalB;
                float waveNormalScale[maxNumSpheres];
                float waveStrength[maxNumSpheres];

				int numPlanetsToRender;

				// Returns vector (dstToSphere, dstThroughSphere)
				// If ray origin is inside sphere, dstToSphere = 0
				// If ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
				float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir) {
					float3 offset = rayOrigin - sphereCentre;
					float a = 1; // Set to dot(rayDir, rayDir) if rayDir might not be normalized
					float b = 2 * dot(offset, rayDir);
					float c = dot(offset, offset) - sphereRadius * sphereRadius;
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

				float densityAtPoint(float3 densitySamplePoint, int sphereIndex)
				{
					float heightAboveSurface = length(densitySamplePoint - planetCentre[sphereIndex].xyz) - oceanRadius[sphereIndex];
					float height01 = heightAboveSurface / (atmosphereRadius[sphereIndex] - oceanRadius[sphereIndex]);
					float localDensity = exp(-height01 * densityFalloff[sphereIndex]) * (1 - height01);
					return localDensity;
				}

				float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength, int sphereIndex)
				{
					float3 densitySamplePoint = rayOrigin;
					float stepSize = rayLength / (numOpticalDepthPoints[sphereIndex] - 1);
					float opticalDepth = 0;

					for (int i = 0; i < numOpticalDepthPoints[sphereIndex]; i++)
					{
						float localDensity = densityAtPoint(densitySamplePoint, sphereIndex);
						opticalDepth += localDensity * stepSize;
						densitySamplePoint += rayDir * stepSize;
					}
					return opticalDepth;
				}

				float3 calculateLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 calculatedScatteringCoefficients, float3 originalCol, int sphereIndex) {
					float3 inScatterPoint = rayOrigin;
					float stepSize = rayLength / (numInScatteringPoints[sphereIndex] - 1);
					float3 inScatteredLight = 0;
					float viewRayOpticalDepth = 0;

					for (int i = 0; i < numInScatteringPoints[sphereIndex]; i++)
					{
						float sunRayLength = raySphere(planetCentre[sphereIndex].xyz, atmosphereRadius[sphereIndex], inScatterPoint, dirToSun[sphereIndex]).y;
						float sunRayOpticalDepth = opticalDepth(inScatterPoint, dirToSun[sphereIndex], sunRayLength, sphereIndex);
						viewRayOpticalDepth = opticalDepth(inScatterPoint, -rayDir, stepSize * i, sphereIndex);
						float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * calculatedScatteringCoefficients);
						float localDensity = densityAtPoint(inScatterPoint, sphereIndex);

						inScatteredLight += localDensity * transmittance * calculatedScatteringCoefficients * stepSize;
						inScatterPoint += rayDir * stepSize;
					}
					float originalColTransmittance = exp(-viewRayOpticalDepth);
					return originalCol * originalColTransmittance + inScatteredLight;
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

				float4 frag(output o) : SV_Target
				{
					fixed4 originalCol = tex2D(_MainTex, o.uv);
					float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, o.uv);
					float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(o.viewVector);

					float3 rayOrigin = _WorldSpaceCameraPos;
					float3 rayDir = normalize(o.viewVector);

					float3 clipPlanePos = rayOrigin + o.viewVector * _ProjectionParams.y;

					for (int sphereIndex = 0; sphereIndex < numPlanetsToRender; sphereIndex ++)
					{
						float2 dstToOcean = raySphere(planetCentre[sphereIndex].xyz, oceanRadius[sphereIndex], rayOrigin, rayDir);
						float dstToSurface = min(sceneDepth, dstToOcean);

						float2 hitInfo = raySphere(planetCentre[sphereIndex].xyz, atmosphereRadius[sphereIndex], rayOrigin, rayDir);
						float dstToAtmosphere = hitInfo.x;
						float dstThroughAtmosphere = min(hitInfo.y, dstToSurface - dstToAtmosphere);

						float3 calculatedScatteringCoefficients;

						calculatedScatteringCoefficients.x = pow(400 / scatteringCoefficients[sphereIndex].x, 4) * scatteringStrength[sphereIndex];
						calculatedScatteringCoefficients.y = pow(400 / scatteringCoefficients[sphereIndex].y, 4) * scatteringStrength[sphereIndex];
						calculatedScatteringCoefficients.z = pow(400 / scatteringCoefficients[sphereIndex].z, 4) * scatteringStrength[sphereIndex];

						float dstToOceanMin = dstToOcean.x;
						float dstThroughOcean = dstToOcean.y;
						float3 rayOceanIntersectPos = rayOrigin + rayDir * dstToOceanMin - planetCentre[sphereIndex].xyz;

						float oceanViewDepth = min(dstThroughOcean, sceneDepth - dstToOceanMin);

						if (oceanViewDepth > 0) {
							float dstAboveWater = length(clipPlanePos - planetCentre[sphereIndex].xyz) - oceanRadius[sphereIndex];

							float opticalDepth01 = 1 - exp(-oceanViewDepth * depthMultiplier[sphereIndex]);
							float alpha = 1 - exp(-oceanViewDepth * alphaMultiplier[sphereIndex]);

							float3 oceanNormal = normalize(rayOrigin + rayDir * dstToOceanMin - planetCentre[sphereIndex].xyz);
							float3 oceanSphereNormal = normalize(rayOceanIntersectPos);

							float2 waveOffsetA;
							float2 waveOffsetB;
							float3 waveNormal;
							float diffuseLighting;
							float specularAngle;
							float specularExponent;
							float specularHighlight;

							if (waveSpeed[sphereIndex] != 0)
							{
								waveOffsetA = float2(_Time.x * waveSpeed[sphereIndex], _Time.x * waveSpeed[sphereIndex] * 0.8);
								waveOffsetB = float2(_Time.x * waveSpeed[sphereIndex] * - 0.8, _Time.x * waveSpeed[sphereIndex] * -0.3);
								waveNormal = triplanarNormal(rayOceanIntersectPos, oceanNormal, waveNormalScale[sphereIndex], waveOffsetA, waveNormalA);
								waveNormal = triplanarNormal(rayOceanIntersectPos, waveNormal, waveNormalScale[sphereIndex], waveOffsetB, waveNormalB);
								waveNormal = normalize(lerp(oceanSphereNormal, waveNormal, waveStrength[sphereIndex]));
								
								diffuseLighting = saturate(dot(oceanSphereNormal, dirToSun[sphereIndex].xyz));
								specularAngle = acos(dot(normalize(dirToSun[sphereIndex].xyz - rayDir), waveNormal));
								specularExponent = specularAngle / (1 - smoothness[sphereIndex]);
								specularHighlight = exp(-specularExponent * specularExponent);
							}
							else
							{
								specularAngle = acos(dot(normalize(dirToSun[sphereIndex].xyz - rayDir), oceanNormal));
								specularExponent = specularAngle / (1 - smoothness[sphereIndex]);
								specularHighlight = exp(-specularExponent * specularExponent);							

								diffuseLighting = (selfGlow[sphereIndex]) ? 1 : saturate(dot(oceanSphereNormal, dirToSun[sphereIndex].xyz));
							}

							float4 oceanCol = lerp(oceanShallow[sphereIndex], oceanDeep[sphereIndex], opticalDepth01) * diffuseLighting + (specularHighlight * (dstAboveWater > 0) * specularCol[sphereIndex]);
							originalCol = lerp(originalCol, oceanCol, alpha);
						}

						if (dstThroughAtmosphere > 0)
						{
							float3 pointInAtmosphere = rayOrigin + rayDir * dstToAtmosphere;
							float3 light = calculateLight(pointInAtmosphere, rayDir, dstThroughAtmosphere, calculatedScatteringCoefficients, originalCol, sphereIndex);
							originalCol = float4 (light, 0);
						}
					}

					return originalCol;
				}
				ENDCG
			}
		}
}
