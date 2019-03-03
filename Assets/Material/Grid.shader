// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "test/MyShader"
{
	Properties
	{
			_MainTex("Texture", 2D) = "white" {}
			_Smooth("Smooth mat", Range(0.0, 1.0)) = 0.5
			_Color("Color base", Color) = (1,0.2,0.3,1)
	}
		SubShader
	{
			Tags { "RenderType" = "Opaque" }
			LOD 100

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
					float4 _MainTex_ST;
					float _Smooth;
					float4 _Color;

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
									//if(test.color.r > 2.f)
										OutputStream.Append(test);
							}		
					}

					fixed4 frag(v2f i) : SV_Target
					{
						// sample the texture
						fixed4 colCrack = tex2D(_MainTex, i.uv);

						fixed4 col =  (1 - i.color.r) * _Color;
						col += fixed4(0, i.color.b, 0,0) / 2;
						col += fixed4(i.color.r/2, 0, 0, 0) / 2;
						
						//col = fixed4(1, 1, 1, 1);
						col.a = 1;

						float3 lightDir = float3(1, 1, -0.2);
						
						float ndotl = dot(i.normal, normalize(lightDir));

						lightDir = float3(-1, 0, -0.2);
						ndotl += dot(i.normal, normalize(lightDir));
						float4 crack = pow(i.color.b,1.5) * saturate(1 - (colCrack*pow(smoothstep(0, 1, colCrack.g),15) ));
						return saturate((col * max(0.0, 0.5+ndotl + pow(ndotl,10))) - crack);
						//return 1-(i.color.r* fixed4(1, 1, 1, 1));
				}
				ENDCG
		}
	}
}