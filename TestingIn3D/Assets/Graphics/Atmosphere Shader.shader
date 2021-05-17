Shader "Planet/Atmosphere"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}

		scatteringCoefficients("Scattering Coefficients", vector) = (0, 0, 0, 0)
		scatteringStrength("Scattering Strength", float) = 1

		planetCentre("Planet Centre", vector) = (0,0,0)
		atmosphereRadius("Atmosphere Radius", float) = 1
		oceanRadius("Ocean Radius", float) = 1

		numInScatteringPoints("Num In Scattering Points", int) = 1
		numOpticalDepthPoints("Num Optical Depth Points", int) = 1
		//ditherStrength("Dither Strength", float) = 1
		//ditherScale("Dither Scale", float) = 1
		densityFalloff("Density Falloff", float) = 1
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

			float2 squareUV(float2 uv) {
				float width = _ScreenParams.x;
				float height =_ScreenParams.y;
				//float minDim = min(width, height);
				float scale = 1000;
				float x = uv.x * width;
				float y = uv.y * height;
				return float2 (x/scale, y/scale);
			}

			sampler2D _BlueNoise;
			sampler2D _MainTex;
			sampler2D _BakedOpticalDepth;
			sampler2D _CameraDepthTexture;

			float3 dirToSun;

			float3 planetCentre;
			float atmosphereRadius;
			float oceanRadius;

			// Paramaters
			int numInScatteringPoints;
			int numOpticalDepthPoints;
			float intensity;
			float3 scatteringCoefficients;
			float scatteringStrength;
			float ditherScale;
			float densityFalloff;

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

			float densityAtPoint(float3 densitySamplePoint)
			{
				float heightAboveSurface = length(densitySamplePoint - planetCentre) - oceanRadius;
				float height01 = heightAboveSurface / (atmosphereRadius - oceanRadius);
				float localDensity = exp(-height01 * densityFalloff) * (1 - height01);
				return localDensity;
			}

			float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength)
			{
				float3 densitySamplePoint = rayOrigin;
				float stepSize = rayLength / (numOpticalDepthPoints - 1);
				float opticalDepth = 0;

				for (int i = 0; i < numOpticalDepthPoints; i++)
				{
					float localDensity = densityAtPoint(densitySamplePoint);
					opticalDepth += localDensity * stepSize;
					densitySamplePoint += rayDir * stepSize;
				}
				return opticalDepth;
			}
			
			float3 calculateLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 calculatedScatteringCoefficients, float3 originalCol) {
				float3 inScatterPoint = rayOrigin;
				float stepSize = rayLength / (numInScatteringPoints - 1);
				float3 inScatteredLight = 0;
				float viewRayOpticalDepth = 0;

				for (int i = 0; i < numInScatteringPoints; i++)
				{
					float sunRayLength = raySphere(planetCentre, atmosphereRadius, inScatterPoint, dirToSun).y;
					float sunRayOpticalDepth = opticalDepth(inScatterPoint, dirToSun, sunRayLength);
					viewRayOpticalDepth = opticalDepth(inScatterPoint, -rayDir, stepSize * i);
					float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * calculatedScatteringCoefficients);
					float localDensity = densityAtPoint(inScatterPoint);

					inScatteredLight += localDensity * transmittance * calculatedScatteringCoefficients * stepSize;
					inScatterPoint += rayDir * stepSize;
				}
				float originalColTransmittance = exp(-viewRayOpticalDepth);
				return originalCol * originalColTransmittance + inScatteredLight;
			}

			float4 frag (output o) : SV_Target
			{
				fixed4 originalCol = tex2D(_MainTex, o.uv);
				float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, o.uv);
				float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(o.viewVector);

				float3 rayOrigin = _WorldSpaceCameraPos;
				float3 rayDir = normalize(o.viewVector);
				dirToSun = _WorldSpaceLightPos0.xyz;

				float2 dstToOcean = raySphere(planetCentre, oceanRadius, rayOrigin, rayDir);
				float dstToSurface = min(sceneDepth, dstToOcean);

				float2 hitInfo = raySphere(planetCentre, atmosphereRadius, rayOrigin, rayDir);
				float dstToAtmosphere = hitInfo.x;
				float dstThroughAtmosphere = min(hitInfo.y, dstToSurface - dstToAtmosphere);

				float3 calculatedScatteringCoefficients;

				calculatedScatteringCoefficients.x = pow(400 / scatteringCoefficients.x, 4) * scatteringStrength;
				calculatedScatteringCoefficients.y = pow(400 / scatteringCoefficients.y, 4) * scatteringStrength;
				calculatedScatteringCoefficients.z = pow(400 / scatteringCoefficients.z, 4) * scatteringStrength;

				if (dstThroughAtmosphere > 0)
				{
					float3 pointInAtmosphere = rayOrigin + rayDir * dstToAtmosphere;
					float3 light = calculateLight(pointInAtmosphere, rayDir, dstThroughAtmosphere, calculatedScatteringCoefficients, originalCol);
					return float4 (light, 0);
				}
				return originalCol;
			}
            ENDCG
        }
    }
}
