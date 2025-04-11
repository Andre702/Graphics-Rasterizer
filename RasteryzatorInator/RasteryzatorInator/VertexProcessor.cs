using RasteryzatorInator.MathLibrary;

namespace RasteryzatorInator
{
    internal struct VertexData
    {
        public Vector3 Position;
        public RawColor Color;

        public VertexData(Vector3 position, RawColor color)
        {
            Position = position;
            Color = color;
        }
    }

    internal class VertexProcessor
    {
        public Matrix4 ObjectToWorld { get; set; } = Matrix4.Identity;
        public Matrix4 WorldToView { get; set; } = Matrix4.Identity;
        public Matrix4 ViewToProjection { get; set; } = Matrix4.Identity;


        public Matrix4 ModelViewProjectionMatrix => ViewToProjection * WorldToView * ObjectToWorld;

        public void SetPerspective(float fovYDegrees, float aspectRatio, float nearPlane, float farPlane)
        {
            float fovYRadians = fovYDegrees * (MathF.PI / 180.0f);

            if (fovYRadians <= 0 || fovYRadians >= MathF.PI) throw new ArgumentOutOfRangeException(nameof(fovYRadians));
            if (aspectRatio <= 0) throw new ArgumentOutOfRangeException(nameof(aspectRatio));
            if (nearPlane <= 0) throw new ArgumentOutOfRangeException(nameof(nearPlane));
            if (farPlane <= nearPlane) throw new ArgumentOutOfRangeException(nameof(farPlane));

            float f = 1.0f / MathF.Tan(fovYRadians / 2.0f);
            float rangeInv = 1.0f / (nearPlane - farPlane);

            ViewToProjection = new Matrix4(
                new Vector4(f / aspectRatio, 0, 0, 0),
                new Vector4(0, f, 0, 0),
                new Vector4(0, 0, (farPlane + nearPlane) * rangeInv, -1.0f),
                new Vector4(0, 0, 2.0f * farPlane * nearPlane * rangeInv, 0)
            );
        }



        public void SetLookAt(Vector3 eye, Vector3 center, Vector3 up)
        {
            Vector3 f = (eye - center).Normalized();
            up.Normalize();
            Vector3 s = Vector3.Cross(up, f);
            Vector3 u = Vector3.Cross(f, s);

            Matrix4 lookAt = new Matrix4(
                new Vector4(s.X, u.X, f.X, 0),
                new Vector4(s.Y, u.Y, f.Y, 0),
                new Vector4(s.Z, u.Z, f.Z, 0),
                new Vector4(0, 0, 0, 1)
                );

            Matrix4 m = Matrix4.Identity;
            m.Col3 = new Vector4(-eye, 1);

            WorldToView = lookAt * m;
        }

        public Vector4 TransformPositionToClipSpace(Vector3 objectPosition)
        {
            Vector4 objPos4 = new Vector4(objectPosition, 1.0f);
            return ModelViewProjectionMatrix * objPos4;
        }
    }
}