// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "test/MyShader"
{
	Properties
	{
			_MainTex("Cracks", 2D) = "black" {}
			_CracksVisibility("Cracks impact", Range(0.0, 1.0)) = 0.5
				_CracksTiling("Cracks tiling", Range(0.0, 1.0)) = 0.5
			_CracksNormals("Cracks Normals", 2D) = "black" {}
			_BumpCracks("Cracks Normals Impact", Range(0.0, 1.0)) = 0.5
			_SkinNormals("Skin Normals", 2D) = "black" {}
			_BumpSkin("Skin Normals Impact", Range(0.0, 1.0)) = 0.5
	    _Color("Color base", Color) = (1,0.2,0.3,1)
				_Glossiness("Glossiness", Range(0.0, 1.0)) = 0.5
				_Specular("Specular", Range(0.0, 1.0)) = 0.5
			_ColorFatigue("Color fatigue", Color) = (0,0.2,1,1)
			_FatigueColImpact("Fatigue color impact", Range(0.0, 1.0)) = 0.5
				
			
	}
		SubShader
	{
			/*Tags { "RenderType" = "Opaque" }
			LOD 100*/

				Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
				LOD 100

				ZWrite Off
				Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					//#pragma geometry geom

					#include "UnityCG.cginc"

					struct appdata
					{
							float4 vertex : POSITION;
							float3 normal : NORMAL;
							float2 uv : TEXCOORD0;
							float4 color : COLOR;
					};

					struct v2f
					{
							float4 vertex : SV_POSITION;
							float3 normal : NORMAL;
							float2 uv : TEXCOORD0;
							float3 worldPosition : TEXCOORD1;
							float4 color : COLOR;
					};

					sampler2D _MainTex;
					sampler2D _CracksNormals;
					sampler2D _SkinNormals;
					float4 _MainTex_ST;
					float _Smooth;
					float _FatigueColImpact;
					float4 _Color;
					float4 _ColorFatigue;
					float _BumpCracks;
					float _BumpSkin;
					float _Glossiness;
					float _Specular;
					float _CracksVisibility;
					float _CracksTiling;

					//float rand(float n) { return frac(sin(n) * 43758.5453123); }
					float rand(float2 n) {
						return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
					}

					/*float noise(float2 n) {
						const float2 d = float2(0.0, 1.0);
						float2 b = floor(n), f = smoothstep(float2(0.0,0.0), float2(1.0,1.0), frac(n));
						return lerp(lerp(rand(b), rand(b + d.yx), f.x), lerp(rand(b + d.xy), rand(b + d.yy), f.x), f.y);
					}*/

					float noiseMe(float2 p) {
						float2 ip = floor(p);
						float2 u = frac(p);
						u = u * u*(3.0 - 2.0*u);

						float res = lerp(
							lerp(rand(ip), rand(ip + float2(1.0, 0.0)), u.x),
							lerp(rand(ip + float2(0.0, 1.0)), rand(ip + float2(1.0, 1.0)), u.x), u.y);
						return res;
					}

					float noise2(float2 co)
					{
						return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
					}

					v2f vert(appdata v)
					{
							v2f o;
							o.vertex = UnityObjectToClipPos(v.vertex);
							o.uv = TRANSFORM_TEX(v.uv, _MainTex);
							o.normal = v.normal;
							o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
							o.color = v.color;
							return o;
					}

					[maxvertexcount(3)]
					void geom(triangle v2f input[3], inout TriangleStream<v2f> OutputStream)
					{
							v2f test = (v2f)0;
							float3 normal = normalize(cross(input[1].worldPosition.xyz - input[0].worldPosition.xyz, input[2].worldPosition.xyz - input[0].worldPosition.xyz));
							for (int i = 0; i < 3; i++) {
								test.normal = lerp(normal, input[i].normal, _Smooth);
									
									test.vertex = input[i].vertex;
									test.uv = input[i].uv;
									test.color = input[i].color;
									OutputStream.Append(test);
							}		
					}

					fixed4 frag(v2f i) : SV_Target
					{
						//Params recu dans la couleur
					  float distance = saturate(i.color.r);
						float fatigue = saturate(i.color.b);

						//Base color
						fixed4 col = _Color;

						//Ajout de fatigue 
						col = saturate(lerp	(col,_ColorFatigue, fatigue * _FatigueColImpact));

						//Assombrissement sur la distance
						float impactDistance = pow(distance, 3);
						//col = saturate((1 - impactDistance) * col);
						float alpha = (1 - impactDistance);

						//Normals
						fixed4 cracksNormals = tex2D(_CracksNormals, i.uv);
						fixed4 skinNormals = tex2D(_SkinNormals, i.uv);

						//Cracks
						fixed4 colCrack = tex2D(_MainTex, i.uv*10* _CracksTiling);
						float4 crack = pow(fatigue, 1.5) * saturate(1 - (colCrack*pow(smoothstep(0, 1, colCrack.g), 15))) * _CracksVisibility * 10;
								
						//Normale
						float3 normal = i.normal*-1;
						normal += _BumpSkin * skinNormals;
						normal += _BumpCracks * cracksNormals;
						normal = normalize(normal);

						//Lights
						float3 lightDir1 = normalize(float3(1, 1, 0.2));
						float3 lightDir2 = normalize(float3(-1, 0, 0.2));
						float3 dirView = normalize(float3(0, 0, 1));

						//Diffuse
						float ndotl = max(0,dot(normal, lightDir1));
						ndotl += max(0, dot(normal, lightDir2));

						//Base light
						float base = 0.5 + noiseMe(i.uv * 3)/10 + noiseMe(i.uv * 10)/20;

						//Specular
						float3 halfVec = normalize(dirView + lightDir1);
						float spec1 = max(0, dot(normal, halfVec));
						halfVec = normalize(dirView + lightDir2);
						float spec2 = max(0, dot(normal, halfVec));
						float spec = max(spec1, spec2);
						float4 specColor = float4(1, 1, 1, 0);
						spec = pow(spec, 100 * _Glossiness)*5*_Specular  ;

						col = base * col + ndotl * col + specColor * spec;

						alpha -= crack;

						//return float4(i.uv*20, 0, 1);

						//return float4(noiseMe(i.uv*5), 0, 0, 1);

						return fixed4(col.rgb , alpha);
						
						//return saturate((col * max(0.0, 0.5+ndotl + pow(ndotl,10))) - crack);
						//return 1-(i.color.r* fixed4(1, 1, 1, 1));
				}
				ENDCG
		}
	}
}