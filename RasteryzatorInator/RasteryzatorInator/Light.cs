using RasteryzatorInator.MathLibrary;

namespace RasteryzatorInator
{
    internal abstract class Light
    {
        public RawColor AmbientColor { get; set; } = new RawColor(25, 25, 25);
        public RawColor DiffuseColor { get; set; } = RawColor.Gray(255);
        public RawColor SpecularColor { get; set; } = RawColor.Gray(255);
        public float Shininess { get; set; } = 32.0f;

        public bool IsEnabled { get; set; } = true;

        protected Light(RawColor? ambient = null, RawColor? diffuse = null, RawColor? specular = null, float shininess = 32.0f)
        {
            AmbientColor = ambient ?? new RawColor(25, 25, 25);
            DiffuseColor = diffuse ?? RawColor.Gray(255);
            SpecularColor = specular ?? RawColor.Gray(255);
            Shininess = shininess;
        }

        public abstract RawColor Calculate(Vector3 worldPosition, Vector3 worldNormal, Vector3 viewDirection, RawColor vertexBaseColor);

        protected RawColor MultiplyColors(RawColor color1, RawColor color2)
        {
            float r = (color1.R / 255.0f) * (color2.R / 255.0f);
            float g = (color1.G / 255.0f) * (color2.G / 255.0f);
            float b = (color1.B / 255.0f) * (color2.B / 255.0f);
            return new RawColor(
                (byte)Math.Clamp(r * 255.0f, 0f, 255f),
                (byte)Math.Clamp(g * 255.0f, 0f, 255f),
                (byte)Math.Clamp(b * 255.0f, 0f, 255f)
            );
        }

        protected RawColor AddColors(RawColor color1, RawColor color2)
        {
            int r = color1.R + color2.R;
            int g = color1.G + color2.G;
            int b = color1.B + color2.B;
            return new RawColor(
                (byte)Math.Clamp(r, 0, 255),
                (byte)Math.Clamp(g, 0, 255),
                (byte)Math.Clamp(b, 0, 255)
            );
        }

        protected RawColor ScaleColor(RawColor color, float factor)
        {
            factor = Math.Max(0f, factor);
            float r = color.R * factor;
            float g = color.G * factor;
            float b = color.B * factor;
            return new RawColor(
                (byte)Math.Clamp(r, 0f, 255f),
                (byte)Math.Clamp(g, 0f, 255f),
                (byte)Math.Clamp(b, 0f, 255f)
            );
        }
    }

    internal class LightDirectional : Light
    {
        public Vector3 Direction { get; set; }

        public LightDirectional(Vector3 direction, RawColor? ambient = null, RawColor? diffuse = null, RawColor? specular = null, float shininess = 32.0f)
            : base(ambient, diffuse, specular, shininess)
        {
            Direction = direction.Normalized();
        }

        public override RawColor Calculate(Vector3 worldPosition, Vector3 worldNormal, Vector3 viewDirection, RawColor vertexBaseColor)
        {
            Vector3 lightDir = -Direction;

            RawColor ambientComp = AmbientColor;

            float diffFactor = MathF.Max(0f, Vector3.Dot(worldNormal, lightDir));
            RawColor diffuseModulated = MultiplyColors(DiffuseColor, vertexBaseColor);
            RawColor diffuseComp = ScaleColor(diffuseModulated, diffFactor);

            Vector3 halfwayDir = (lightDir + viewDirection).Normalized();
            float specFactorBase = MathF.Max(0f, Vector3.Dot(worldNormal, halfwayDir));
            if (diffFactor <= 0) specFactorBase = 0;

            float specFactor = MathF.Pow(specFactorBase, Shininess);
            RawColor specularComp = ScaleColor(SpecularColor, specFactor);

            return AddColors(AddColors(ambientComp, diffuseComp), specularComp);
        }
    }

    internal class LightPoint : Light
    {
        public Vector3 Position { get; set; }
        public float ConstantAttenuation { get; set; } = 1.0f;
        public float LinearAttenuation { get; set; } = 0.0f;
        public float QuadraticAttenuation { get; set; } = 0.0f;

        // Spotlight
        public Vector3 SpotDirection { get; private set; } = Vector3.Zero;
        public float SpotCutoffCos { get; private set; } = -1.0f;
        public float SpotExponent { get; private set; } = 1.0f;

        public LightPoint(Vector3 position, RawColor? ambient = null, RawColor? diffuse = null, RawColor? specular = null, float shininess = 32.0f)
            : base(ambient, diffuse, specular, shininess)
        {
            Position = position;
        }

        public void SetSpotlight(Vector3 direction, float cutoffAngleDegrees, float exponent = 1.0f)
        {
            SpotDirection = direction.Normalized();
            float cutoffRad = cutoffAngleDegrees * MathF.PI / 180.0f;
            SpotCutoffCos = MathF.Cos(cutoffRad);
            SpotExponent = MathF.Max(0f, exponent);
        }

        public override RawColor Calculate(Vector3 worldPosition, Vector3 worldNormal, Vector3 viewDirection, RawColor vertexBaseColor)
        {
            Vector3 lightVector = Position - worldPosition;
            float distance = lightVector.Length();
            if (distance < 0.0001f) distance = 0.0001f;
            Vector3 lightDir = lightVector / distance;

            float attenuation = 1.0f / (ConstantAttenuation + LinearAttenuation * distance + QuadraticAttenuation * distance * distance);
            attenuation = Math.Clamp(attenuation, 0f, 1f);

            float spotFactor = 1.0f;
            if (SpotCutoffCos > -1.0f)
            {
                float angleCos = Vector3.Dot(SpotDirection, -lightDir);
                if (angleCos < SpotCutoffCos)
                {
                    return RawColor.Gray(0);
                }
                spotFactor = MathF.Pow(MathF.Max(0f, angleCos), SpotExponent);
            }

            RawColor ambientComp = AmbientColor;

            float diffFactor = MathF.Max(0f, Vector3.Dot(worldNormal, lightDir));
            RawColor diffuseModulated = MultiplyColors(DiffuseColor, vertexBaseColor);
            RawColor diffuseComp = ScaleColor(diffuseModulated, diffFactor * attenuation * spotFactor);

            Vector3 halfwayDir = (lightDir + viewDirection).Normalized();
            float specFactorBase = MathF.Max(0f, Vector3.Dot(worldNormal, halfwayDir));
            if (diffFactor <= 0) specFactorBase = 0;

            float specFactor = MathF.Pow(specFactorBase, Shininess);
            RawColor specularComp = ScaleColor(SpecularColor, specFactor * attenuation * spotFactor);

            return AddColors(AddColors(ambientComp, diffuseComp), specularComp);
        }
    }
}
