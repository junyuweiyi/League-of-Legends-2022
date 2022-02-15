Shader "SLG/RainDrop" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BumpAmt("Distortion", range(0,500)) = 20
		_BumpMap("NormalMap", 2D) = "bump" {}
		[Toggle(USEDIFFUSE)] _UseDiffuse("Use Diffuse", Float) = 0
	}

	HLSLINCLUDE
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
	ENDHLSL

	SubShader {
		Pass {
			Tags { "LightMode" = "Always" "RenderPipeline" = "UniversalPipeline"}
			ZTest Always
			Cull Off
			ZWrite Off
			Fog{ Mode Off }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_local __ USEDIFFUSE
			//#include "UnityCG.cginc"

			float _BumpAmt;
			float4 _BumpMap_ST;
			TEXTURE2D(_BumpMap);
			TEXTURE2D(_MainTex);
			float4 _MainTex_TexelSize;
			SAMPLER(sampler_BumpMap);
			SAMPLER(sampler_MainTex);

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord: TEXCOORD0;
#ifdef USEDIFFUSE
				float3 normalOS     : NORMAL;
				float4 tangentOS    : TANGENT;
#endif
			};

			struct v2f {
				float4 vertex : POSITION;
				float4 uvgrab : TEXCOORD0;
				float2 uvbump : TEXCOORD1;
#ifdef USEDIFFUSE
				float4 tangentWS        : TANGENT;
				float4 normalWS         : NORMAL;
				float4 BtangentWS       : TEXCOORD3;
#endif
			};

			v2f vert(appdata_t v) 
			{
				v2f o;
				o.vertex = TransformObjectToHClip(v.vertex);
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
				o.uvgrab.zw = o.vertex.zw;
				o.uvbump = TRANSFORM_TEX(v.texcoord, _BumpMap);

#ifdef USEDIFFUSE
				o.normalWS.xyz = normalize(TransformObjectToWorldNormal(v.normalOS));
				o.tangentWS.xyz = normalize(TransformObjectToWorld(v.tangentOS));
				o.BtangentWS.xyz = cross(o.normalWS.xyz, o.tangentWS.xyz) * v.tangentOS.w * unity_WorldTransformParams.w;
				//这里乘一个unity_WorldTransformParams.w是为判断是否使用了奇数相反的缩放

				float3 positionWS = TransformObjectToWorld(v.vertex);
				o.tangentWS.w = positionWS.x;
				o.BtangentWS.w = positionWS.y;
				o.normalWS.w = positionWS.z;
#endif

				return o;
			}
			
			float4 frag(v2f i) : COLOR
			{
				real4 nortex = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uvbump);
#ifdef USEDIFFUSE
				Light mylight = GetMainLight();
				float3 WSpos = float3(i.tangentWS.w, i.BtangentWS.w, i.normalWS.w);
				float3x3 T2W = { i.tangentWS.xyz,i.BtangentWS.xyz,i.normalWS.xyz };
				float3 normalTS = UnpackNormalScale(nortex, 2);
				normalTS.z = pow((1 - pow(normalTS.x, 2) - pow(normalTS.y, 2)), 0.5);//规范化法线
				float3 norWS = mul(normalTS, T2W);//注意这里是右乘T2W的，等同于左乘T2W的逆
				float halflambot = dot(norWS, (0, 0, 1))*0.5+0.5;//计算半兰伯特
#endif

				real4 diff = 1;
				float2 offset = (nortex.xy - 0.504) * _BumpAmt * _MainTex_TexelSize.xy;
				i.uvgrab.xy = offset + i.uvgrab.xy;
				diff.rgb = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvgrab.xy);
#ifdef USEDIFFUSE
				diff = diff * halflambot;
#endif
				return diff;
			}

			ENDHLSL
		}
	}
}
