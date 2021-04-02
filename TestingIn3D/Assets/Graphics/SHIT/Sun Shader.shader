Shader "Custom/Sun Shader"
{
    Properties
    {
		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_EmissionMultiplier("Emission Multiplier", float) = 0

		// noise
		_Frequency ("Frequency", float) = 0.5
		_SunNoiseSpeed ("Sun Noise Speed", float) = 0.5
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows vertex:vert
			#pragma target 3.5

			#include "UnityCG.cginc"
			#include "/Assets/Resources/FastNoiseLite.cginc"

			float _EmissionMultiplier;
			float3 _EmissionColor;
			float _Glossiness;

			// noise
			float _Frequency;
			float _SunNoiseSpeed;
 
            struct input
            {
                float4 pos : POSITION;
            };

			struct output
			{
				float4 pos : SV_POSITION;
			};

			output vert(input i)
            {
                output o;
                o.pos = UnityObjectToClipPos(i.pos);
                return o;
            }

			/*
			void vert (inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input, o);
				o.normal = v.normal;
				o.vertPos = v.vertex;

				// noise on this shit
				fnl_state noise = fnlCreateState();
				noise.frequency = _Frequency;
				float noiseValue = (1 + fnlGetNoise3D(noise, o.pos.x, o.pos.y + _Time[0] * _SunNoiseSpeed, o.pos.z)) * 0.5;
				//float noiseValue = 1;
			
				o.col = _EmissionColor * noiseValue * _EmissionMultiplier;
			}
			*/
			void surf (output o)
			{
				// noise on this shit
				fnl_state noise = fnlCreateState();
				noise.frequency = _Frequency;
				float noiseValue = (1 + fnlGetNoise3D(noise, o.pos.x, o.pos.y + _Time[0] * _SunNoiseSpeed, o.pos.z)) * 0.5;
				//float noiseValue = 1;
			
				o.col = _EmissionColor * noiseValue * _EmissionMultiplier;

				o.Emission = o.col;
			}
			ENDCG
		}
    }
}