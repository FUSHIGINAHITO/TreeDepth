Shader "Unlit/Rect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius",float) = 0
        _Width ("Width",float) = 0
        _Smooth ("Smooth",float) = 0
        _Size ("Size",float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;
            float _Size;
            float _Width;
            float _Smooth;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            float mix (float a, float b)
            {
                //return a * a + b * (1 - a);
                return 0.5 * ((a - b) * (a - b) + a + b);
            }

            fixed4 frag (v2f i) : SV_Target
            {  
            	//坐标等价到左下角
                float2 p = abs(step(0.5,i.uv) - i.uv);






                float r = 0.5 - _Size + _Radius;
                float cond1 = smoothstep(0.5 - _Size - _Smooth, 0.5 - _Size, p.x) * smoothstep(0.5 - _Size - _Smooth, 0.5 - _Size, p.y);
                //float cond2 = max(smoothstep(r - _Smooth, r, p.x), smoothstep(r - _Smooth, r, p.y));
                float cond2 = mix(smoothstep(r - _Smooth, r, p.x), smoothstep(r - _Smooth, r, p.y));
                float d = length(float2(p.x - r, p.y - r));
                //float cond3 = smoothstep(d - _Smooth, d, _Radius);
                float cond3 = smoothstep(d - _Smooth, d, _Radius);
                //fixed4 col =  tex2D(_MainTex, i.uv) * (cond1 * max(cond2, cond3));
                fixed4 col =  tex2D(_MainTex, i.uv) * (cond1 * mix(cond2, cond3));
                //fixed4 col2 =  tex2D(_MainTex, i.uv) * (step(_Radius - _Width,p.x) ||step( _Radius - _Width  ,p.y*_Ratio) || step(length(float2(p.x-_Radius + _Width,p.y*_Ratio-_Radius + _Width)),_Radius - _Width));
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

            ENDCG
        }
    }
}