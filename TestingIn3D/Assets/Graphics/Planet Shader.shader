Shader "Planet/PlanetShader"
{
    Properties
    {
        _MainTex ("texture", 2D) = "white" {}

		// shore
		_DampShoreColor ("Damp Shore Color", Color) = (1,1,1,1)
		_DryShoreColor ("Dry Shore Color", Color) = (1,1,1,1)
		_OceanRadius ("Ocean Radius", Float) = 5.0
		_DampShoreHeightAboveWater ("Damp Shore Height Above Water", Float) = 0.5
		_DryShoreHeightAboveWater ("Dry Shore Height Above Water", Float) = 0.5
		_ShoreBlend ("Blending Of Shore", Float) = 0.5
		_OceanBlend ("Blending Underneath Ocean", Float) = 0.5

		// test
		_TestColor ("Test Color", Color) = (1,1,1,1)
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
			float2 uv_MainTex;
			float3 worldPos;
			float4 terrainData;
			float3 vertPos;
			float3 normal;
			float4 tangent;
        };

        sampler2D _MainTex;

		// shore
		fixed4 _DampShoreColor;
		fixed4 _DryShoreColor;
		fixed _OceanRadius;
		fixed _DampShoreHeightAboveWater;
		fixed _DryShoreHeightAboveWater;
		fixed _ShoreBlend;
		fixed _OceanBlend;

		// Test
		fixed4 _TestColor;
        
		// börja blenda, hur långt den ska blenda, vart den är just nu
		float Blend(float startHeight, float blendDst, float height) {
			 return smoothstep(startHeight - blendDst / 2, startHeight + blendDst / 2, height);
		}

		float remap01(float v, float minOld, float maxOld)
		{
			return saturate((v-minOld) / (maxOld-minOld));
		}

		void vert (inout appdata_full v, out Input o)
        {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertPos = v.vertex;
			o.normal = v.normal;
			o.terrainData = v.texcoord;
			o.tangent = v.tangent;
        }

		void surf (Input IN, inout SurfaceOutputStandard o)
        {
			// Beräkna steepness: 0 = flat, 1 = steep
			float3 sphereNormal = normalize(IN.vertPos);
			float steepness = 1 - dot(sphereNormal, IN.normal);

			// Beräkna heights
			float terrainHeight = length(IN.vertPos);
			float dampShoreHeight = _OceanRadius + _DampShoreHeightAboveWater;
			float dryShoreHeight = dampShoreHeight + _DryShoreHeightAboveWater;

			// shore colours
			float4 shoreColour;
			shoreColour = lerp(_DampShoreColor, _DryShoreColor, Blend(dampShoreHeight, _OceanBlend, terrainHeight));
			shoreColour = lerp(shoreColour, _TestColor, Blend(dryShoreHeight, _ShoreBlend, terrainHeight));

			float4 terrainColour = shoreColour;

			// ändra materialets färger
			o.Albedo = terrainColour;
//			o.Smoothness = dot(o.Albedo, 1) / 3 * _Smoothness;
//			o.Metallic = _Metallic;
        }
        ENDCG
    }
	FallBack "Diffuse"
}
