Shader "Planet/Sun With Noise"
{
    Properties
    {
		_SunColorUndertone("Dark Sun Color Undertone", Color) = (0,0,0,0)
		_SunColorMidtone("Dark Sun Color Overtone", Color) = (0,0,0,0)
		_SunColorOvertone("Dark Sun Color Overtone", Color) = (0,0,0,0)
		
		//_EmissionMultiplier("Emission Multiplier", float) = 0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
			#include "/Assets/Resources/FastNoiseLite.cginc"

            float _EmissionMultiplier;
			// color
			float4 _SunColorUndertone;
			float4 _SunColorMidtone;
			float4 _SunColorOvertone;

            struct output
            {
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

			float remap01(float v) {
				return saturate(0.5+v);
			}


            output vert(appdata_base v)
            {
                output o;
				o.worldPos = mul (unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);

				/*
				float _SunNoiseSpeed;
				fnl_state noise = fnlCreateState();

				_SunNoiseSpeed = 25;
				noise.fractal_type = 1;
				noise.octaves = 5;
				noise.lacunarity = 2.25;
				noise.gain = 1;
				noise.weighted_strength = 0;
				noise.frequency = 0.15;

				noise.domain_warp_amp = 3;

				float x = o.worldPos.x+_Time[0]*_SunNoiseSpeed;
				float y = o.worldPos.y+_Time[0]*_SunNoiseSpeed;
				float z = o.worldPos.z+_Time[0]*_SunNoiseSpeed;
				fnlDomainWarp3D(noise, x, y, z);
				float noiseLight = remap01(fnlGetNoise3D(noise, x, y, z));

				o.vertex += normalize(o.vertex)*noiseLight*10;
				*/

				return o;
            }

            fixed4 frag (output o) : SV_Target
            {
				float _SunNoiseSpeed;
				fnl_state noise = fnlCreateState();
				noise.rotation_type_3d = 2;

				// noise for the dark spots on sun
				_SunNoiseSpeed = 10;
				noise.fractal_type = 1;
				noise.octaves = 5;
				noise.lacunarity = 3;
				noise.gain = 1;
				noise.weighted_strength = 0;
				noise.frequency = 0.15;
				float noiseDark = remap01(fnlGetNoise3D(noise, o.worldPos.x+_Time[0]*_SunNoiseSpeed, o.worldPos.y+_Time[0]*_SunNoiseSpeed, o.worldPos.z+_Time[0]*_SunNoiseSpeed));
				float4 colDark = lerp(_SunColorUndertone, _SunColorMidtone, noiseDark);

				// noise for the light spots on sun
				_SunNoiseSpeed = 25;
				noise.fractal_type = 1;
				noise.octaves = 5;
				noise.lacunarity = 2.25;
				noise.gain = 1;
				noise.weighted_strength = 0;
				noise.frequency = 0.15;

				noise.domain_warp_amp = 3;

				float x = o.worldPos.x+_Time[0]*_SunNoiseSpeed;
				float y = o.worldPos.y+_Time[0]*_SunNoiseSpeed;
				float z = o.worldPos.z+_Time[0]*_SunNoiseSpeed;
				fnlDomainWarp3D(noise, x, y, z);
				float noiseLight = remap01(fnlGetNoise3D(noise, x, y, z));
				float4 colLight = lerp(_SunColorMidtone, _SunColorOvertone, noiseLight);

				// noise to use as a mask between light and dark
				_SunNoiseSpeed = 50;
				noise.fractal_type = 1;
				noise.octaves = 5;
				noise.lacunarity = 2.5;
				noise.gain = 0.25;
				noise.weighted_strength = 0;
				noise.frequency = 0.05;
				fixed4 finalCol = lerp(colDark, colLight, remap01(fnlGetNoise3D(noise, o.worldPos.x+_Time[0]*_SunNoiseSpeed, o.worldPos.y+_Time[0]*_SunNoiseSpeed, o.worldPos.z+_Time[0]*_SunNoiseSpeed)));

				return finalCol;
            }
            ENDCG
        }
    }
}
