// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/Sun With Noise"
{
    Properties
    {
		_SunColor1("Sun Color1", Color) = (0,0,0,0)
		_SunColor2("Sun Color2", Color) = (0,0,0,0)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.5
		_EmissionMultiplier("Emission Multiplier", float) = 0

		// noise
		_Frequency ("Frequency", float) = 0.5
		_SunNoiseSpeed ("Sun Noise Speed", float) = 0.5
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
			float4 _SunColor1;
			float4 _SunColor2;
			float _Glossiness;
			float _Metallic;

			// noise
			float _Frequency;
			float _SunNoiseSpeed;

            struct output
            {
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            output vert(appdata_base v)
            {
                output o;
				o.worldPos = mul (unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
            }

            fixed3 frag (output o) : SV_Target
            {
				// noise on this shit
				fnl_state noise = fnlCreateState();
				noise.frequency = _Frequency;
				fixed4 col = lerp(_SunColor1, _SunColor2, (1 + fnlGetNoise3D(noise, o.worldPos.x+_Time[0]*_SunNoiseSpeed, o.worldPos.y+_Time[0]*_SunNoiseSpeed, o.worldPos.z+_Time[0]*_SunNoiseSpeed)*0.5));
				return col;
            }
            ENDCG
        }
    }
}
