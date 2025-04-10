using System;
using RasteryzatorInator;

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
        // local to world position
        public Matrix4 ObjectToWorld { get; set; }

        // world to camera view
        public Matrix4 WorldToView { get; set; }

        // camera view to clip sapce (perspective matrix)
        public Matrix4 ViewToProjection { get; set; }



        public Matrix4 ModelViewMatrix => WorldToView * ObjectToWorld;

        public Matrix4 ModelViewProjectionMatrix => ViewToProjection * WorldToView * ObjectToWorld;

        public VertexProcessor()
        {
            ObjectToWorld = Matrix4.Identity();
            WorldToView = Matrix4.Identity();
            ViewToProjection = Matrix4.Identity();
        }

        public void SetPerspective(float fovyDegrees, float aspectRatio, float nearPlane, float farPlane)
        {
            ViewToProjection = Matrix4.CreatePerspective(fovyDegrees, aspectRatio, nearPlane, farPlane);
        }

        public void SetLookAt(Vector3 eye, Vector3 center, Vector3 up)
        {
            WorldToView = Matrix4.LookAt(eye, center, up);
        }

        public void ApplyTranslation(Vector3 v)
        {
            ObjectToWorld = Matrix4.Translate(ObjectToWorld, v);
        }

        public void ApplyRotation(float angleDegrees, Vector3 axis)
        {
            float angleRadians = angleDegrees * (MathF.PI / 180.0f);
            ObjectToWorld = Matrix4.Rotate(ObjectToWorld, axis, angleRadians);
        }

        public void ApplyScale(Vector3 v)
        {
            ObjectToWorld = Matrix4.Scale(ObjectToWorld, v);
        }


        public void MultByTranslation(Vector3 v)
        {
            // Wymaga logiki tworzenia macierzy translacji (np. z Matrix4.CreateTranslation)
            Matrix4 translationMatrix = new Matrix4( // Symulacja CreateTranslation
                new Vector4 (1,0,0,0),
                new Vector4 (0,1,0,0),
                new Vector4 (0,0,1,0),
                new Vector4(v.X, v.Y, v.Z, 1));
            ObjectToWorld = translationMatrix * ObjectToWorld;
        }

        public void MultByRotation(float angleDegrees, Vector3 axis)
        {
            // Wymaga logiki tworzenia macierzy rotacji (np. z Matrix4.CreateRotation)
            float angleRadians = angleDegrees * (MathF.PI / 180.0f);
            try { axis.Normalize(); } catch (DivideByZeroException) { return; }
            float x = axis.X, y = axis.Y, z = axis.Z;
            float c = MathF.Cos(angleRadians); float s = MathF.Sin(angleRadians); float t = 1.0f - c;
            Matrix4 rotationMatrix = new Matrix4( // Symulacja CreateRotation
                new Vector4(t * x * x + c, t * x * y + s * z, t * x * z - s * y, 0),
                new Vector4(t * x * y - s * z, t * y * y + c, t * y * z + s * x, 0),
                new Vector4(t * x * z + s * y, t * y * z - s * x, t * z * z + c, 0),
                new Vector4(0,0,0,1));
            ObjectToWorld = rotationMatrix * ObjectToWorld; // M_new * M_old
        }



        public void MultByScale(Vector3 v)
        {
            // Wymaga logiki tworzenia macierzy skali (np. z Matrix4.CreateScale)
            Matrix4 scaleMatrix = new Matrix4( // Symulacja CreateScale
                new Vector4(v.X, 0, 0, 0),
                new Vector4(0, v.Y, 0, 0),
                new Vector4(0, 0, v.Z, 0),
                new Vector4(0, 0, 0, 1));
            ObjectToWorld = scaleMatrix * ObjectToWorld; // M_new * M_old
        }


        public Vector4 TransformPositionToClipSpace(Vector3 objectPosition) => ModelViewProjectionMatrix * objectPosition;
        public Vector3 TransformNormalToViewSpace(Vector3 objectNormal)
        {
            Matrix4 modelView = ModelViewMatrix;
            Vector4 normal4 = new Vector4(objectNormal, 0.0f);
            Vector4 viewNormal4 = modelView * normal4;
            Vector3 viewNormal = new Vector3(viewNormal4.X, viewNormal4.Y, viewNormal4.Z);
            try { viewNormal.Normalize(); } catch (DivideByZeroException) { }
            return viewNormal;
        }
    }
}