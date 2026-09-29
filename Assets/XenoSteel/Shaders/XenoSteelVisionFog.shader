Shader "XenoSteel/WorldVisionFog"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0, 0, 0, 1)
        _FogDensity ("Fog Density", Range(0, 2)) = 1
        _FogMask ("Fog Mask", 2D) = "black" {}
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            sampler2D _FogMask;

            float4 _FogColor;
            float _FogDensity;

            float4 _GridMin;
            float4 _GridMax;

            v2f vert(appdata v)
            {
                v2f o;

                float4 worldPos =
                    mul(
                        unity_ObjectToWorld,
                        v.vertex);

                o.worldPos = worldPos.xyz;
                o.vertex =
                    UnityObjectToClipPos(v.vertex);

                return o;
            }

            float GetFogMask(float3 worldPos)
            {
                float width =
                    max(
                        _GridMax.x -
                        _GridMin.x,
                        0.001);

                float height =
                    max(
                        _GridMax.y -
                        _GridMin.y,
                        0.001);

                float2 uv;

                uv.x =
                    (worldPos.x -
                     _GridMin.x) /
                    width;

                uv.y =
                    (worldPos.z -
                     _GridMin.y) /
                    height;

                uv = saturate(uv);

                return tex2D(
                    _FogMask,
                    uv).r;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 cameraPos =
                    _WorldSpaceCameraPos;

                float3 ray =
                    normalize(
                        i.worldPos -
                        cameraPos);

                float distanceToVolume =
                    distance(
                        cameraPos,
                        i.worldPos);

                float stepSize = 0.5;

                int steps =
                    min(
                        64,
                        (int)(
                            distanceToVolume /
                            stepSize));

                float fogAmount = 0.0;

                float3 samplePos =
                    cameraPos;

                for (int step = 0;
                     step < steps;
                     step++)
                {
                    samplePos +=
                        ray * stepSize;

                    float mask =
                        GetFogMask(samplePos);

                    fogAmount +=
                        mask *
                        _FogDensity *
                        0.035;

                    fogAmount =
                        saturate(fogAmount);
                }

                return fixed4(
                    _FogColor.rgb,
                    fogAmount);
            }

            ENDCG
        }
    }
}