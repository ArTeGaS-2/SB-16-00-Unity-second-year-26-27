Shader "Constructor/Stylized Flow Water"
{
    Properties
    {
        [MainColor] _BaseColor("Колір води", Color) = (.07,.32,.39,.64)
        _LightColor("Світлі грані", Color) = (.13,.46,.49,1)
        _FoamColor("Брижі й край", Color) = (.52,.77,.72,1)
        _FlowSpeed("Швидкість течії", Range(0,4)) = 1
        _WorldUV("Озерні брижі у світових координатах", Float) = 0
        _EdgeMode("Край: 0 немає, 1 два, 2 берег, 3 вхід", Float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "UniversalMaterialType"="Unlit" }
        Pass
        {
            Name "FlowWater"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor, _LightColor, _FoamColor;
            float _FlowSpeed, _WorldUV, _EdgeMode;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; half fog:TEXCOORD3; UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO };
            Varyings vert(Attributes input)
            {
                Varyings o=(Varyings)0; UNITY_SETUP_INSTANCE_ID(input); UNITY_TRANSFER_INSTANCE_ID(input,o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                VertexPositionInputs p=GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS=p.positionCS;o.positionWS=p.positionWS;o.normalWS=TransformObjectToWorldNormal(input.normalOS);o.uv=input.uv;o.fog=ComputeFogFactor(p.positionCS.z);return o;
            }
            float hash21(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            half4 frag(Varyings input):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                // Closed source geometry, but internal tile walls must not show through the surface.
                clip(input.normalWS.y-.5);
                float2 uv=lerp(input.uv,input.positionWS.xz*.5,saturate(_WorldUV));
                float2 moving=float2(uv.x*4,uv.y*6-_Time.y*_FlowSpeed);
                float2 cell=floor(moving),f=frac(moving);cell.y=cell.y-floor(cell.y/6)*6;float facet=step(1,f.x+f.y);
                float2 worldPattern=input.positionWS.xz*float2(2,3);
                float2 worldCell=floor(worldPattern),worldFraction=frac(worldPattern);
                float shade=hash21(worldCell+step(1,worldFraction.x+worldFraction.y)*float2(19,31));
                half3 color=lerp(_BaseColor.rgb,_LightColor.rgb,.18+shade*.27);
                float streak=step(.83,hash21(cell+4))*smoothstep(.21,.15,abs(f.x-.5))*smoothstep(.12,.065,abs(f.y-.5));
                float edge=0;
                if(_EdgeMode>.5&&_EdgeMode<1.5)edge=max(1-smoothstep(0,.065,input.uv.x),smoothstep(.935,1,input.uv.x));
                else if(_EdgeMode>1.5)edge=smoothstep(.94,1,input.uv.x);
                if(_EdgeMode>2.5){float opening=smoothstep(.47,.52,input.uv.y)*(1-smoothstep(1.48,1.53,input.uv.y));edge*=1-opening;}
                float top=saturate(input.normalWS.y*4);float foam=saturate((edge*(.65+.2*sin(uv.y*15-_Time.y*_FlowSpeed))+streak*.24)*top);
                // Sparse lake ripples: one event per five-metre cell every 7-12 seconds.
                float ripple=0;
                if(_WorldUV>.5){
                    float2 tile=floor(input.positionWS.xz/5);
                    for(int rx=-1;rx<=1;rx++)for(int rz=-1;rz<=1;rz++){
                        float2 id=tile+float2(rx,rz);float h=hash21(id+71);
                        float period=7+h*5;float cycle=floor((_Time.y+h*period)/period);
                        float age=frac((_Time.y+h*period)/period)*period;
                        float2 center=(id+.2+.6*float2(hash21(id+cycle+23),hash21(id-cycle+57)))*5;
                        float d=length(input.positionWS.xz-center);float ring=1-smoothstep(.018,.055,abs(d-age*.32));
                        ripple+=ring*smoothstep(0,.3,age)*(1-smoothstep(1.5,2.7,age));
                    }
                }
                foam=saturate(foam+ripple*.42*top);
                color=lerp(color,_FoamColor.rgb,foam);
                Light main=GetMainLight();color*=.78+.22*saturate(dot(normalize(input.normalWS),main.direction));
                color=MixFog(color,input.fog);return half4(color,lerp(_BaseColor.a,.88,foam));
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}

