// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PolyToots/ASE_OceanDisplace"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin]_SeaColor("Sea Color", Color) = (0,0.3254902,0.4039216,1)
		_Displace_3D("Displace_3D", 3D) = "white" {}
		_Normals_3D("Normals_3D", 3D) = "white" {}
		_Distort("Distort", Range( 0 , 0.2)) = 0.08510161
		_NormalPower("Normal Power", Range( 0 , 1)) = 1
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.9
		_Depth("Depth", Range( 0 , 1)) = 1
		_Tiling("Tiling", Float) = 1
		_WaveSpeed("Wave Speed", Float) = 0.4
		_EdgePower("Edge Power", Range( 0 , 1)) = 0.3
		_WaveHeight("Wave Height", Range( 0 , 1)) = 1
		_CausticsScale("Caustics Scale", Float) = 5
		[HDR]_CausticsColour("Caustics Colour", Color) = (2,2,2,0)
		_WaveMask("WaveMask", 2D) = "white" {}
		[ASEEnd]_MaskStrength("Mask Strength", Range( 0 , 1)) = 1

		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 16
		_TessMin( "Tess Min Distance", Float ) = 10
		_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		Cull Off
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 3.5

		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			Offset 0,0
			ColorMask RGBA
			

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define TESSELLATION_ON 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_DISTANCE_TESSELLATION
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 80200
			#define REQUIRE_DEPTH_TEXTURE 1
			#define REQUIRE_OPAQUE_TEXTURE 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_SCREEN_POSITION


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SeaColor;
			float4 _CausticsColour;
			float _Tiling;
			float _WaveSpeed;
			float _WaveHeight;
			float _MaskStrength;
			float _NormalPower;
			float _CausticsScale;
			float _Distort;
			float _Depth;
			float _EdgePower;
			float _Smoothness;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			TEXTURE3D(_Displace_3D);
			SAMPLER(sampler_Displace_3D);
			TEXTURE2D(_WaveMask);
			SAMPLER(sampler_WaveMask);
			TEXTURE3D(_Normals_3D);
			SAMPLER(sampler_Normals_3D);
			uniform float4 _CameraDepthTexture_TexelSize;


					float2 voronoihash108( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi108( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0;
						for ( int j = -1; j <= 1; j++ )
						{
							for ( int i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash108( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.707 * sqrt(dot( r, r ));
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						 		}
						 	}
						}
						return (F2 + F1) * 0.5;
					}
			
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDir72_g107( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float mulTime74 = _TimeParameters.x * _WaveSpeed;
				float3 appendResult75 = (float3(( ase_worldPos.x * _Tiling ) , ( ase_worldPos.z * _Tiling ) , mulTime74));
				float3 _3duvs127 = appendResult75;
				float4 tex3DNode61 = SAMPLE_TEXTURE3D_LOD( _Displace_3D, sampler_Displace_3D, _3duvs127, 0.0 );
				float2 appendResult280 = (float2(v.vertex.xyz.x , v.vertex.xyz.z));
				float _cheekySmoother338 = saturate( ( SAMPLE_TEXTURE2D_LOD( _WaveMask, sampler_WaveMask, (appendResult280*0.043 + 0.5), 0.0 ).r + (1.0 + (_MaskStrength - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) ) );
				float4 vertexOffset133 = ( tex3DNode61 * float4( ( ( float3(0,1,0) * (0.0 + (_WaveHeight - 0.0) * (0.5 - 0.0) / (1.0 - 0.0)) ) * _cheekySmoother338 ) , 0.0 ) );
				
				float3 objectToViewPos = TransformWorldToView(TransformObjectToWorld(v.vertex.xyz));
				float eyeDepth = -objectToViewPos.z;
				o.ase_texcoord7.x = eyeDepth;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.yzw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = vertexOffset133.rgb;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float mulTime74 = _TimeParameters.x * _WaveSpeed;
				float3 appendResult75 = (float3(( WorldPosition.x * _Tiling ) , ( WorldPosition.z * _Tiling ) , mulTime74));
				float3 _3duvs127 = appendResult75;
				float4 tex3DNode61 = SAMPLE_TEXTURE3D( _Displace_3D, sampler_Displace_3D, _3duvs127 );
				float4 _displace305 = tex3DNode61;
				float4 lerpResult307 = lerp( _SeaColor , ( _SeaColor + 0.1 ) , _displace305);
				float4 Albedo137 = lerpResult307;
				
				float lerpResult392 = lerp( 0.0 , _NormalPower , WorldNormal.y);
				float3 unpack66 = UnpackNormalScale( SAMPLE_TEXTURE3D( _Normals_3D, sampler_Normals_3D, _3duvs127 ), lerpResult392 );
				unpack66.z = lerp( 1, unpack66.z, saturate(lerpResult392) );
				float3 normals123 = unpack66;
				
				float _Time130 = mulTime74;
				float time108 = ( _Time130 * 8.0 );
				float eyeDepth = IN.ase_texcoord7.x;
				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float eyeDepth28_g102 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float2 temp_output_20_0_g102 = ( (normals123).xy * ( _Distort / max( eyeDepth , 0.1 ) ) * saturate( ( eyeDepth28_g102 - eyeDepth ) ) );
				float eyeDepth2_g102 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ( float4( temp_output_20_0_g102, 0.0 , 0.0 ) + ase_screenPosNorm ).xy ),_ZBufferParams);
				float2 temp_output_32_0_g102 = (( float4( ( temp_output_20_0_g102 * saturate( ( eyeDepth2_g102 - eyeDepth ) ) ), 0.0 , 0.0 ) + ase_screenPosNorm )).xy;
				float2 temp_output_150_38 = temp_output_32_0_g102;
				float2 _refractUV168 = temp_output_150_38;
				float2 temp_output_73_0_g107 = _refractUV168;
				float2 UV22_g108 = float4( temp_output_73_0_g107, 0.0 , 0.0 ).xy;
				float2 localUnStereo22_g108 = UnStereo( UV22_g108 );
				float2 break64_g107 = localUnStereo22_g108;
				float clampDepth69_g107 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( float4( temp_output_73_0_g107, 0.0 , 0.0 ).xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g107 = ( 1.0 - clampDepth69_g107 );
				#else
				float staticSwitch38_g107 = clampDepth69_g107;
				#endif
				float3 appendResult39_g107 = (float3(break64_g107.x , break64_g107.y , staticSwitch38_g107));
				float4 appendResult42_g107 = (float4((appendResult39_g107*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g107 = mul( unity_CameraInvProjection, appendResult42_g107 );
				float3 In72_g107 = ( (temp_output_43_0_g107).xyz / (temp_output_43_0_g107).w );
				float3 localInvertDepthDir72_g107 = InvertDepthDir72_g107( In72_g107 );
				float4 appendResult49_g107 = (float4(localInvertDepthDir72_g107 , 1.0));
				float2 coords108 = (mul( unity_CameraToWorld, appendResult49_g107 )).xz * _CausticsScale;
				float2 id108 = 0;
				float2 uv108 = 0;
				float voroi108 = voronoi108( coords108, time108, id108, uv108, 0 );
				float smoothstepResult273 = smoothstep( 0.38 , 1.25 , voroi108);
				float eyeDepth238 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( float4( _refractUV168, 0.0 , 0.0 ).xy ),_ZBufferParams);
				float temp_output_239_0 = ( eyeDepth238 - eyeDepth );
				float _depthMask259 = ( 1.0 - saturate( ( temp_output_239_0 + (-1.0 + (_Depth - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) ) ) );
				float4 fetchOpaqueVal149 = float4( SHADERGRAPH_SAMPLE_SCENE_COLOR( temp_output_150_38 ), 1.0 );
				float4 _distortion189 = fetchOpaqueVal149;
				float _edgeFoam295 = ( 1.0 - saturate( ( temp_output_239_0 * 12.0 ) ) );
				float4 emission121 = ( ( ( ( saturate( smoothstepResult273 ) * _CausticsColour ) * _depthMask259 ) + saturate( ( _distortion189 * _depthMask259 ) ) ) + ( _edgeFoam295 * _EdgePower ) );
				
				float3 Albedo = Albedo137.rgb;
				float3 Normal = normals123;
				float3 Emission = emission121.rgb;
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = _Smoothness;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				half4 color = UniversalFragmentPBR(
					inputData, 
					Albedo, 
					Metallic, 
					Specular, 
					Smoothness, 
					Occlusion, 
					Emission, 
					Alpha);

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _TransmissionShadow;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif

				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );

					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, WorldNormal ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif
				
				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define TESSELLATION_ON 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_DISTANCE_TESSELLATION
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 80200

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SeaColor;
			float4 _CausticsColour;
			float _Tiling;
			float _WaveSpeed;
			float _WaveHeight;
			float _MaskStrength;
			float _NormalPower;
			float _CausticsScale;
			float _Distort;
			float _Depth;
			float _EdgePower;
			float _Smoothness;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			TEXTURE3D(_Displace_3D);
			SAMPLER(sampler_Displace_3D);
			TEXTURE2D(_WaveMask);
			SAMPLER(sampler_WaveMask);


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float mulTime74 = _TimeParameters.x * _WaveSpeed;
				float3 appendResult75 = (float3(( ase_worldPos.x * _Tiling ) , ( ase_worldPos.z * _Tiling ) , mulTime74));
				float3 _3duvs127 = appendResult75;
				float4 tex3DNode61 = SAMPLE_TEXTURE3D_LOD( _Displace_3D, sampler_Displace_3D, _3duvs127, 0.0 );
				float2 appendResult280 = (float2(v.vertex.xyz.x , v.vertex.xyz.z));
				float _cheekySmoother338 = saturate( ( SAMPLE_TEXTURE2D_LOD( _WaveMask, sampler_WaveMask, (appendResult280*0.043 + 0.5), 0.0 ).r + (1.0 + (_MaskStrength - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) ) );
				float4 vertexOffset133 = ( tex3DNode61 * float4( ( ( float3(0,1,0) * (0.0 + (_WaveHeight - 0.0) * (0.5 - 0.0) / (1.0 - 0.0)) ) * _cheekySmoother338 ) , 0.0 ) );
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = vertexOffset133.rgb;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define TESSELLATION_ON 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_DISTANCE_TESSELLATION
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 80200
			#define REQUIRE_DEPTH_TEXTURE 1
			#define REQUIRE_OPAQUE_TEXTURE 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SeaColor;
			float4 _CausticsColour;
			float _Tiling;
			float _WaveSpeed;
			float _WaveHeight;
			float _MaskStrength;
			float _NormalPower;
			float _CausticsScale;
			float _Distort;
			float _Depth;
			float _EdgePower;
			float _Smoothness;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			TEXTURE3D(_Displace_3D);
			SAMPLER(sampler_Displace_3D);
			TEXTURE2D(_WaveMask);
			SAMPLER(sampler_WaveMask);
			TEXTURE3D(_Normals_3D);
			SAMPLER(sampler_Normals_3D);
			uniform float4 _CameraDepthTexture_TexelSize;


					float2 voronoihash108( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi108( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0;
						for ( int j = -1; j <= 1; j++ )
						{
							for ( int i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash108( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.707 * sqrt(dot( r, r ));
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						 		}
						 	}
						}
						return (F2 + F1) * 0.5;
					}
			
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDir72_g107( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float mulTime74 = _TimeParameters.x * _WaveSpeed;
				float3 appendResult75 = (float3(( ase_worldPos.x * _Tiling ) , ( ase_worldPos.z * _Tiling ) , mulTime74));
				float3 _3duvs127 = appendResult75;
				float4 tex3DNode61 = SAMPLE_TEXTURE3D_LOD( _Displace_3D, sampler_Displace_3D, _3duvs127, 0.0 );
				float2 appendResult280 = (float2(v.vertex.xyz.x , v.vertex.xyz.z));
				float _cheekySmoother338 = saturate( ( SAMPLE_TEXTURE2D_LOD( _WaveMask, sampler_WaveMask, (appendResult280*0.043 + 0.5), 0.0 ).r + (1.0 + (_MaskStrength - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) ) );
				float4 vertexOffset133 = ( tex3DNode61 * float4( ( ( float3(0,1,0) * (0.0 + (_WaveHeight - 0.0) * (0.5 - 0.0) / (1.0 - 0.0)) ) * _cheekySmoother338 ) , 0.0 ) );
				
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord2.xyz = ase_worldNormal;
				float3 objectToViewPos = TransformWorldToView(TransformObjectToWorld(v.vertex.xyz));
				float eyeDepth = -objectToViewPos.z;
				o.ase_texcoord2.w = eyeDepth;
				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord3 = screenPos;
				
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = vertexOffset133.rgb;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float mulTime74 = _TimeParameters.x * _WaveSpeed;
				float3 appendResult75 = (float3(( WorldPosition.x * _Tiling ) , ( WorldPosition.z * _Tiling ) , mulTime74));
				float3 _3duvs127 = appendResult75;
				float4 tex3DNode61 = SAMPLE_TEXTURE3D( _Displace_3D, sampler_Displace_3D, _3duvs127 );
				float4 _displace305 = tex3DNode61;
				float4 lerpResult307 = lerp( _SeaColor , ( _SeaColor + 0.1 ) , _displace305);
				float4 Albedo137 = lerpResult307;
				
				float _Time130 = mulTime74;
				float time108 = ( _Time130 * 8.0 );
				float3 ase_worldNormal = IN.ase_texcoord2.xyz;
				float lerpResult392 = lerp( 0.0 , _NormalPower , ase_worldNormal.y);
				float3 unpack66 = UnpackNormalScale( SAMPLE_TEXTURE3D( _Normals_3D, sampler_Normals_3D, _3duvs127 ), lerpResult392 );
				unpack66.z = lerp( 1, unpack66.z, saturate(lerpResult392) );
				float3 normals123 = unpack66;
				float eyeDepth = IN.ase_texcoord2.w;
				float4 screenPos = IN.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float eyeDepth28_g102 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float2 temp_output_20_0_g102 = ( (normals123).xy * ( _Distort / max( eyeDepth , 0.1 ) ) * saturate( ( eyeDepth28_g102 - eyeDepth ) ) );
				float eyeDepth2_g102 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ( float4( temp_output_20_0_g102, 0.0 , 0.0 ) + ase_screenPosNorm ).xy ),_ZBufferParams);
				float2 temp_output_32_0_g102 = (( float4( ( temp_output_20_0_g102 * saturate( ( eyeDepth2_g102 - eyeDepth ) ) ), 0.0 , 0.0 ) + ase_screenPosNorm )).xy;
				float2 temp_output_150_38 = temp_output_32_0_g102;
				float2 _refractUV168 = temp_output_150_38;
				float2 temp_output_73_0_g107 = _refractUV168;
				float2 UV22_g108 = float4( temp_output_73_0_g107, 0.0 , 0.0 ).xy;
				float2 localUnStereo22_g108 = UnStereo( UV22_g108 );
				float2 break64_g107 = localUnStereo22_g108;
				float clampDepth69_g107 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( float4( temp_output_73_0_g107, 0.0 , 0.0 ).xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g107 = ( 1.0 - clampDepth69_g107 );
				#else
				float staticSwitch38_g107 = clampDepth69_g107;
				#endif
				float3 appendResult39_g107 = (float3(break64_g107.x , break64_g107.y , staticSwitch38_g107));
				float4 appendResult42_g107 = (float4((appendResult39_g107*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g107 = mul( unity_CameraInvProjection, appendResult42_g107 );
				float3 In72_g107 = ( (temp_output_43_0_g107).xyz / (temp_output_43_0_g107).w );
				float3 localInvertDepthDir72_g107 = InvertDepthDir72_g107( In72_g107 );
				float4 appendResult49_g107 = (float4(localInvertDepthDir72_g107 , 1.0));
				float2 coords108 = (mul( unity_CameraToWorld, appendResult49_g107 )).xz * _CausticsScale;
				float2 id108 = 0;
				float2 uv108 = 0;
				float voroi108 = voronoi108( coords108, time108, id108, uv108, 0 );
				float smoothstepResult273 = smoothstep( 0.38 , 1.25 , voroi108);
				float eyeDepth238 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( float4( _refractUV168, 0.0 , 0.0 ).xy ),_ZBufferParams);
				float temp_output_239_0 = ( eyeDepth238 - eyeDepth );
				float _depthMask259 = ( 1.0 - saturate( ( temp_output_239_0 + (-1.0 + (_Depth - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) ) ) );
				float4 fetchOpaqueVal149 = float4( SHADERGRAPH_SAMPLE_SCENE_COLOR( temp_output_150_38 ), 1.0 );
				float4 _distortion189 = fetchOpaqueVal149;
				float _edgeFoam295 = ( 1.0 - saturate( ( temp_output_239_0 * 12.0 ) ) );
				float4 emission121 = ( ( ( ( saturate( smoothstepResult273 ) * _CausticsColour ) * _depthMask259 ) + saturate( ( _distortion189 * _depthMask259 ) ) ) + ( _edgeFoam295 * _EdgePower ) );
				
				
				float3 Albedo = Albedo137.rgb;
				float3 Emission = emission121.rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = Albedo;
				metaInput.Emission = Emission;
				
				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			Offset 0,0
			ColorMask RGBA

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define TESSELLATION_ON 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_DISTANCE_TESSELLATION
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 80200

			#pragma enable_d3d11_debug_symbols
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SeaColor;
			float4 _CausticsColour;
			float _Tiling;
			float _WaveSpeed;
			float _WaveHeight;
			float _MaskStrength;
			float _NormalPower;
			float _CausticsScale;
			float _Distort;
			float _Depth;
			float _EdgePower;
			float _Smoothness;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			TEXTURE3D(_Displace_3D);
			SAMPLER(sampler_Displace_3D);
			TEXTURE2D(_WaveMask);
			SAMPLER(sampler_WaveMask);


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float mulTime74 = _TimeParameters.x * _WaveSpeed;
				float3 appendResult75 = (float3(( ase_worldPos.x * _Tiling ) , ( ase_worldPos.z * _Tiling ) , mulTime74));
				float3 _3duvs127 = appendResult75;
				float4 tex3DNode61 = SAMPLE_TEXTURE3D_LOD( _Displace_3D, sampler_Displace_3D, _3duvs127, 0.0 );
				float2 appendResult280 = (float2(v.vertex.xyz.x , v.vertex.xyz.z));
				float _cheekySmoother338 = saturate( ( SAMPLE_TEXTURE2D_LOD( _WaveMask, sampler_WaveMask, (appendResult280*0.043 + 0.5), 0.0 ).r + (1.0 + (_MaskStrength - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) ) );
				float4 vertexOffset133 = ( tex3DNode61 * float4( ( ( float3(0,1,0) * (0.0 + (_WaveHeight - 0.0) * (0.5 - 0.0) / (1.0 - 0.0)) ) * _cheekySmoother338 ) , 0.0 ) );
				
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = vertexOffset133.rgb;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float mulTime74 = _TimeParameters.x * _WaveSpeed;
				float3 appendResult75 = (float3(( WorldPosition.x * _Tiling ) , ( WorldPosition.z * _Tiling ) , mulTime74));
				float3 _3duvs127 = appendResult75;
				float4 tex3DNode61 = SAMPLE_TEXTURE3D( _Displace_3D, sampler_Displace_3D, _3duvs127 );
				float4 _displace305 = tex3DNode61;
				float4 lerpResult307 = lerp( _SeaColor , ( _SeaColor + 0.1 ) , _displace305);
				float4 Albedo137 = lerpResult307;
				
				
				float3 Albedo = Albedo137.rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				half4 color = half4( Albedo, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}
		
	}
	/*ase_lod*/
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18501
381;64;1006;411;2618.011;2671.945;2.000913;True;False
Node;AmplifyShaderEditor.CommentaryNode;291;-4211.086,-173.5038;Inherit;False;1766.387;542.8701;Comment;11;336;288;333;289;280;290;282;279;337;338;340;Cheeky Wave Smoother;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;279;-4161.085,-123.5037;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;77;-3726.676,-1729.457;Inherit;False;1083.396;512.6696;Comment;9;127;75;73;74;72;70;71;130;343;3d_TextureUVs;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;70;-3645.677,-1484.815;Inherit;False;Property;_Tiling;Tiling;7;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;343;-3661.034,-1682.624;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;71;-3421.677,-1344.457;Inherit;False;Property;_WaveSpeed;Wave Speed;8;0;Create;True;0;0;False;0;False;0.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;282;-4004.524,53.77597;Inherit;False;Constant;_ProjectionScale;ProjectionScale;12;0;Create;True;0;0;False;0;False;0.043;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;290;-4004.484,140.7239;Inherit;False;Constant;_ProjectionOffset;ProjectionOffset;13;0;Create;True;0;0;False;0;False;0.5;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;280;-3959.979,-89.38065;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-3289.677,-1507.457;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-3283.677,-1675.457;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;74;-3245.677,-1340.457;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;289;-3690.443,9.53789;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;333;-3616.818,194.698;Inherit;False;Property;_MaskStrength;Mask Strength;14;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;75;-3055.677,-1603.457;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;340;-3318.271,189.4939;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;288;-3456.293,-20.373;Inherit;True;Property;_WaveMask;WaveMask;13;0;Create;True;0;0;False;0;False;-1;9e4afcd6d6fda81418ccf68f6cded108;9e4afcd6d6fda81418ccf68f6cded108;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;127;-2888.676,-1606.82;Inherit;False;_3duvs;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;336;-3082.524,1.871921;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;135;-1920.05,-855.0711;Inherit;False;1515.38;700.3642;Comment;11;129;133;93;142;61;79;141;277;305;335;339;Displace;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-1888.74,-358.4025;Inherit;False;Property;_WaveHeight;Wave Height;10;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;337;-2907.362,6.73198;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;129;-1848.038,-770.4926;Inherit;False;127;_3duvs;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;141;-1607.331,-524.4096;Inherit;False;Constant;_Vector0;Vector 0;9;0;Create;True;0;0;False;0;False;0,1,0;0,1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TFHCRemapNode;335;-1611.758,-352.7395;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;61;-1631.354,-799.071;Inherit;True;Property;_Displace_3D;Displace_3D;1;0;Create;True;0;0;False;0;False;-1;ede26a347e1c6634d9d6c87e30361481;ede26a347e1c6634d9d6c87e30361481;True;0;False;white;LockedToTexture3D;False;Object;-1;Auto;Texture3D;8;0;SAMPLER3D;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;139;-3020.286,-852.8118;Inherit;False;929.8838;385.0968;Comment;6;306;307;310;67;312;137;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;338;-2716.411,5.78193;Inherit;False;_cheekySmoother;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;142;-1416.988,-437.5934;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;339;-1265.586,-281.1995;Inherit;False;338;_cheekySmoother;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;305;-1275.095,-655.9173;Inherit;False;_displace;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;67;-2992.201,-805.8253;Inherit;False;Property;_SeaColor;Sea Color;0;0;Create;True;0;0;False;0;False;0,0.3254902,0.4039216,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;312;-2969.724,-623.6198;Inherit;False;Constant;_Float4;Float 4;14;0;Create;True;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;310;-2731.65,-717.2911;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.142978;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;306;-2709.911,-593.3343;Inherit;False;305;_displace;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;277;-992.5174,-591.9963;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-826.8288,-800.2112;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;307;-2454.896,-794.9055;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;133;-616.6078,-803.1741;Inherit;False;vertexOffset;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;395;-2075.532,-1652.859;Inherit;False;677.3872;403.3722;Comment;3;392;391;106;SideFix;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;140;-1241.929,-1728.598;Inherit;False;1011.688;304.9631;Comment;3;123;66;331;Normals;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;151;-4662.679,-1003.585;Inherit;False;1302.432;449.3139;Comment;6;189;148;149;168;152;150;Refraction + UVs;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;396;156.1215,-2456.116;Inherit;False;1767.147;515.429;Comment;12;263;304;247;301;248;264;262;266;303;265;302;121;Depth_Merged;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;137;-2279.269,-797.3871;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;60;-1968.951,-2483.267;Inherit;False;1919.79;438.3458;Comment;11;273;145;117;200;108;17;131;144;143;341;399;Caustics;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;244;-3951.183,-2593.947;Inherit;False;1724.984;573.2555;Comment;15;334;259;296;253;297;238;255;299;298;239;295;235;251;240;256;DepthMasks;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;247;424.3591,-2351.701;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;266;911.4647,-2207.191;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;138;-120.0694,-315.9911;Inherit;False;137;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;302;1144.767,-2167.331;Inherit;False;295;_edgeFoam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;148;-4618.741,-772.4777;Inherit;False;Property;_Distort;Distort;3;0;Create;True;0;0;False;0;False;0.08510161;0;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-227.4595,45.258;Inherit;False;Property;_Smoothness;Smoothness;5;0;Create;True;0;0;False;0;False;0.9;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;66;-865.4077,-1672.597;Inherit;True;Property;_Normals_3D;Normals_3D;2;0;Create;True;0;0;False;0;False;-1;15730400ed428da42a95ec30431ce5d4;15730400ed428da42a95ec30431ce5d4;True;0;False;white;LockedToTexture3D;True;Object;-1;Auto;Texture3D;8;0;SAMPLER3D;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;132;-123.8674,-200.7202;Inherit;False;123;normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;190;-126.5638,-86.2793;Inherit;False;121;emission;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;334;-3419.126,-2248.128;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;255;-3217.707,-2265.362;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;130;-2966.677,-1346.457;Inherit;False;_Time;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;235;-3910.736,-2540.544;Inherit;False;168;_refractUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;189;-3574.268,-937.0414;Inherit;False;_distortion;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenDepthNode;238;-3712.688,-2539.492;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;264;741.9996,-2200.24;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;263;520.2737,-2229.927;Inherit;False;189;_distortion;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;297;-3218.662,-2408.425;Inherit;False;Constant;_Float3;Float 3;14;0;Create;True;0;0;False;0;False;12;12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;117;-552.4708,-2353.657;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;123;-490.7915,-1649.108;Inherit;False;normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SurfaceDepthNode;240;-3760.161,-2429.487;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;239;-3450.438,-2484.329;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;265;518.7541,-2103.694;Inherit;False;259;_depthMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;108;-1016.423,-2436.766;Inherit;True;0;1;1;3;1;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;5;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;121;1703.329,-2361.079;Inherit;False;emission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;144;-1280.088,-2230.279;Inherit;False;Property;_CausticsScale;Caustics Scale;11;0;Create;True;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;251;-3730.196,-2250.465;Inherit;False;Property;_Depth;Depth;6;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;149;-3868.942,-934.9869;Inherit;False;Global;_GrabScreen0;Grab Screen 0;2;0;Create;True;0;0;False;0;False;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;134;-143.3189,179.8605;Inherit;False;133;vertexOffset;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;150;-4315.004,-928.4248;Inherit;False;Sai_DepthMaskedRefraction;-1;;102;a63c900487a193f41a51cf8b7e7f977f;2,40,0,103,0;2;35;FLOAT3;0,0,0;False;37;FLOAT;0.02;False;1;FLOAT2;38
Node;AmplifyShaderEditor.GetLocalVarNode;331;-1091.778,-1670.662;Inherit;False;127;_3duvs;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;296;-3043.4,-2485.103;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;273;-789.8936,-2422.108;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.38;False;2;FLOAT;1.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;253;-2909.6,-2265.313;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;299;-2689.092,-2479.141;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;304;1053.222,-2066.687;Inherit;False;Property;_EdgePower;Edge Power;9;0;Create;True;0;0;False;0;False;0.3;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;399;-1698.962,-2427.968;Inherit;False;Reconstruct World Position From Depth 2;-1;;107;4f04fb07708c6c44ba16506cff07981e;0;1;73;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;295;-2490.498,-2511.697;Inherit;False;_edgeFoam;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;152;-4610.243,-946.8492;Inherit;False;123;normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;168;-3888.979,-719.2836;Inherit;False;_refractUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;298;-2866.194,-2479.848;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;256;-3069.247,-2263.274;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;-1275.677,-2342.984;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;262;1067.203,-2373.361;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;145;-376.1655,-2283.904;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;303;1348.221,-2138.687;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;301;1496.638,-2373.642;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;131;-1463.07,-2344.943;Inherit;False;130;_Time;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;391;-1888.542,-1440.287;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;259;-2495.558,-2273.792;Inherit;False;_depthMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;17;-1291.887,-2437.194;Inherit;False;True;False;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;392;-1663.547,-1513.171;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;106;-1950.614,-1599.652;Inherit;False;Property;_NormalPower;Normal Power;4;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;341;-737.241,-2252.204;Inherit;False;Property;_CausticsColour;Caustics Colour;12;1;[HDR];Create;True;0;0;False;0;False;2,2,2,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;248;202.7264,-2235.463;Inherit;False;259;_depthMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;200;-1907.575,-2434.325;Inherit;False;168;_refractUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;5;False;-1;10;False;-1;1;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;True;1;False;-1;True;3;False;-1;True;False;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;163.1833,-101.2293;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;PolyToots/ASE_OceanDisplace;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;17;False;False;False;False;False;False;False;False;True;0;False;-1;True;2;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;3;0;True;1;5;False;-1;10;False;-1;1;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;False;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;36;Workflow;1;Surface;1;  Refraction Model;0;  Blend;0;Two Sided;0;Fragment Normal Space,InvertActionOnDeselection;0;Transmission;0;  Transmission Shadow;0.5,False,-1;Translucency;0;  Translucency Strength;1,False,-1;  Normal Distortion;0.5,False,-1;  Scattering;2,False,-1;  Direct;0.9,False,-1;  Ambient;0.1,False,-1;  Shadow;0.5,False,-1;Cast Shadows;0;  Use Shadow Threshold;0;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;_FinalColorxAlpha;0;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;DOTS Instancing;0;Tessellation;1;  Phong;0;  Strength;0.5,False,-1;  Type;1;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Vertex Position,InvertActionOnDeselection;1;0;6;False;True;False;True;True;True;False;;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;4;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;6.61174,179.2111;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;False;False;False;False;0;False;-1;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;True;0
WireConnection;280;0;279;1
WireConnection;280;1;279;3
WireConnection;73;0;343;3
WireConnection;73;1;70;0
WireConnection;72;0;343;1
WireConnection;72;1;70;0
WireConnection;74;0;71;0
WireConnection;289;0;280;0
WireConnection;289;1;282;0
WireConnection;289;2;290;0
WireConnection;75;0;72;0
WireConnection;75;1;73;0
WireConnection;75;2;74;0
WireConnection;340;0;333;0
WireConnection;288;1;289;0
WireConnection;127;0;75;0
WireConnection;336;0;288;1
WireConnection;336;1;340;0
WireConnection;337;0;336;0
WireConnection;335;0;79;0
WireConnection;61;1;129;0
WireConnection;338;0;337;0
WireConnection;142;0;141;0
WireConnection;142;1;335;0
WireConnection;305;0;61;0
WireConnection;310;0;67;0
WireConnection;310;1;312;0
WireConnection;277;0;142;0
WireConnection;277;1;339;0
WireConnection;93;0;61;0
WireConnection;93;1;277;0
WireConnection;307;0;67;0
WireConnection;307;1;310;0
WireConnection;307;2;306;0
WireConnection;133;0;93;0
WireConnection;137;0;307;0
WireConnection;247;0;145;0
WireConnection;247;1;248;0
WireConnection;266;0;264;0
WireConnection;66;1;331;0
WireConnection;66;5;392;0
WireConnection;334;0;251;0
WireConnection;255;0;239;0
WireConnection;255;1;334;0
WireConnection;130;0;74;0
WireConnection;189;0;149;0
WireConnection;238;0;235;0
WireConnection;264;0;263;0
WireConnection;264;1;265;0
WireConnection;117;0;273;0
WireConnection;123;0;66;0
WireConnection;239;0;238;0
WireConnection;239;1;240;0
WireConnection;108;0;17;0
WireConnection;108;1;143;0
WireConnection;108;2;144;0
WireConnection;121;0;301;0
WireConnection;149;0;150;38
WireConnection;150;35;152;0
WireConnection;150;37;148;0
WireConnection;296;0;239;0
WireConnection;296;1;297;0
WireConnection;273;0;108;0
WireConnection;253;0;256;0
WireConnection;299;0;298;0
WireConnection;399;73;200;0
WireConnection;295;0;299;0
WireConnection;168;0;150;38
WireConnection;298;0;296;0
WireConnection;256;0;255;0
WireConnection;143;0;131;0
WireConnection;262;0;247;0
WireConnection;262;1;266;0
WireConnection;145;0;117;0
WireConnection;145;1;341;0
WireConnection;303;0;302;0
WireConnection;303;1;304;0
WireConnection;301;0;262;0
WireConnection;301;1;303;0
WireConnection;259;0;253;0
WireConnection;17;0;399;0
WireConnection;392;1;106;0
WireConnection;392;2;391;2
WireConnection;1;0;138;0
WireConnection;1;1;132;0
WireConnection;1;2;190;0
WireConnection;1;4;68;0
WireConnection;1;8;134;0
ASEEND*/
//CHKSM=8BC8C293EDA1C9BA9CB7D6D9C371976D841043A5