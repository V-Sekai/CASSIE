Shader "Procedural 3D Grid Shader"
{
    Subshader
    {

        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        ZWrite Off
        ZTest LEqual
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref 1
            Comp always
        }

        Pass
        {
            Cull Off
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex VSMain
            #pragma geometry geo
            #pragma fragment PSMain
            #pragma target 5.0

            #define PI 3.1415
            #define N_VERTEX_CIRCLE 20

            uint _DisplayGridLines;

            float4x4 _CanvasToWorldMatrix;
            float3 _GridAnchorPos;
            float _DistanceBetweenPoints;
            float _PointRadius;
            uint _PointsPerDim;

            fixed4 _Color;

            float _NearFade;
            float _FarFade;
            float _FadeZone;
            float3 _FocusPoint;
            float _BaseOpacity;

            struct geometryOutput
            {
                float4 pos : SV_POSITION;
                float opacity : COLOR0;
            };

            float ComputeOpacity(float3 worldSpacePos, float baseOpacity)
            {
                // Compare world space input position with focus point world space position
                float focus_point_distance = distance(worldSpacePos, _FocusPoint);
                float opacity = baseOpacity;
                if (focus_point_distance > _FarFade)
                {
                    opacity = clamp(1.0 + (_FarFade - focus_point_distance) / _FadeZone, 0.0, 1.0) * baseOpacity;
                }

                float depth = UnityObjectToViewPos(worldSpacePos).z * -1.0;
                if (depth < _NearFade)
                {
                    opacity = clamp(-_FadeZone + depth / _NearFade, 0.0, 1.0) * baseOpacity;
                }

                return opacity;
            }

            float4 VSMain(uint id:SV_VertexID, out float3 p : TEXCOORD0) : SV_POSITION
            {
                uint xIdx = id % _PointsPerDim;
                uint yIdx = (id / _PointsPerDim) % _PointsPerDim;
                uint zIdx = id / (_PointsPerDim * _PointsPerDim);

                float3 d = float3(xIdx * _DistanceBetweenPoints, yIdx * _DistanceBetweenPoints, zIdx * _DistanceBetweenPoints);
                p = _GridAnchorPos + d;
                return mul(_CanvasToWorldMatrix , float4(p, 1.0));
            }

            float4 ScreenScale(float4 vec)
            {
                vec.x = vec.x / _ScreenParams.x;
                vec.y = vec.y / _ScreenParams.y;
                return vec;
            }

            void AddLine(float3 A, float3 B, float lineWidth, float lineOpacity, inout TriangleStream<geometryOutput> triStream)
            {
                float4 AClip = UnityObjectToClipPos(A);
                float4 BClip = UnityObjectToClipPos(B);
                float2 DirClip = normalize(BClip.xy / BClip.w - AClip.xy / AClip.w);
                float4 OrthDirClip = float4(DirClip.y, -DirClip.x, 0.0, 0.0);
                OrthDirClip = ScreenScale(OrthDirClip);
                if (distance(AClip, BClip) > 0.01)
                {
                    geometryOutput o;

                    float oA = ComputeOpacity(A, lineOpacity);
                    o.pos = AClip - lineWidth * 0.5 * OrthDirClip;
                    o.opacity = oA;
                    triStream.Append(o);

                    o.pos = AClip + lineWidth * 0.5 * OrthDirClip;
                    o.opacity = oA;
                    triStream.Append(o);

                    float oB = ComputeOpacity(B, lineOpacity);
                    o.pos = BClip - lineWidth * 0.5 * OrthDirClip;
                    o.opacity = oB;
                    triStream.Append(o);

                    o.pos = BClip + lineWidth * 0.5 * OrthDirClip;
                    o.opacity = oB;
                    triStream.Append(o);
                }

                triStream.RestartStrip();
            }

            [maxvertexcount((N_VERTEX_CIRCLE + 1) * 2 + 4 * 3)]
            void geo(point float4 IN[1] : SV_POSITION, inout TriangleStream<geometryOutput> triStream)
            {
                float3 pos = IN[0].xyz;
                // Compare world space input position with focus point world space position
                float focus_point_distance = distance(pos, _FocusPoint);
                float opacity = ComputeOpacity(pos, _BaseOpacity);

                if (opacity < 0.1)
                    return; // Don't even bother drawing it

                // Draw circle in clip space, centered at input position
                float4 centerClipPos = UnityObjectToClipPos(pos);
                centerClipPos.z = centerClipPos.z - 0.001; // prevent z fighting between grid points and stroke
                geometryOutput o;

                float angle = 2.0 * PI / N_VERTEX_CIRCLE;

                for (int i = 0; i <= N_VERTEX_CIRCLE; i++)
                {
                    float4 onCirclePos = centerClipPos + ScreenScale(float4(
                        _PointRadius * cos(i * angle),
                        _PointRadius * sin(i * angle),
                        0.0, 0.0));

                    o.pos = onCirclePos;
                    o.opacity = opacity;
                    triStream.Append(o);

                    o.pos = centerClipPos;
                    o.opacity = opacity;
                    triStream.Append(o);
                }

                if (_DisplayGridLines == 0)
                    return;

                triStream.RestartStrip();

                // Draw 3D cross centered around vertex
                float lineWidth = _PointRadius * 0.3;
                float lineOpacity = _BaseOpacity * 0.2;

                float dl = _DistanceBetweenPoints * 0.5;

                float3 xDir = mul(_CanvasToWorldMatrix, float3(1.0, 0.0, 0.0));
                float3 xA = pos + dl * xDir;
                float3 xB = pos - dl * xDir;

                float3 yDir = mul(_CanvasToWorldMatrix, float3(0.0, 1.0, 0.0));
                float3 yA = pos + dl * yDir;
                float3 yB = pos - dl * yDir;

                float3 zA = pos + mul(_CanvasToWorldMatrix, float3(0.0, 0.0, dl));
                float3 zB = pos + mul(_CanvasToWorldMatrix, float3(0.0, 0.0, -dl));

                // Draw 3 quads aligned to viewing plane

                // x direction
                AddLine(xA, xB, lineWidth, lineOpacity, triStream);

                // y direction
                AddLine(yA, yB, lineWidth, lineOpacity, triStream);

                // z direction
                AddLine(zA, zB, lineWidth, lineOpacity, triStream);
            }

            float4 PSMain(geometryOutput i) : SV_Target
            {
                float4 c = _Color;
                c.a = i.opacity;
                return c;
            }
            ENDCG
        }

    }
}