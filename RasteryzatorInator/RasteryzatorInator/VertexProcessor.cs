using RasteryzatorInator.MathLibrary;

namespace RasteryzatorInator
{
    internal struct VertexData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public RawColor Color;
        public Vector2 TexCoord;

        public VertexData(Vector3 position, Vector3 normal, RawColor color, Vector2 textCoord)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TexCoord = textCoord;
        }

        public VertexData(Vector3 position, Vector3 normal, RawColor color) : this(position, normal, color, Vector2.Zero) { }

        public VertexData(Vector3 position, RawColor color) : this(position, Vector3.Zero, color, Vector2.Zero) { }
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
            ViewToProjection = Matrix4.CreatePerspectiveFieldOfView(fovYRadians, aspectRatio, nearPlane, farPlane);
        }

        public void SetLookAt(Vector3 eye, Vector3 center, Vector3 up)
        {
            WorldToView = Matrix4.LookAt(eye, center, up);
        }

        public void Translate(Vector3 v) => ObjectToWorld = ObjectToWorld * Matrix4.CreateTranslation(v);
        public void Rotate(Vector3 axis, float angleDegrees)
        {
            float angleRad = angleDegrees * (MathF.PI / 180.0f);
            ObjectToWorld = ObjectToWorld * Matrix4.CreateFromAxisAngle(axis, angleRad);
        }
        public void Scale(Vector3 v) => ObjectToWorld = ObjectToWorld * Matrix4.CreateScale(v);

        public void ResetObjectTransform() => ObjectToWorld = Matrix4.Identity;

        public Vector4 TransformPositionToClipSpace(Vector3 objectPosition)
        {
            Vector4 objPos4 = new Vector4(objectPosition, 1.0f);
            return ModelViewProjectionMatrix * objPos4;
        }
    }
}