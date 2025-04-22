using RasteryzatorInator.MathLibrary;

namespace RasteryzatorInator
{
    internal abstract class Light
    {
        // Właściwości "materiału" narzucone przez to światło
        public RawColor AmbientColor { get; set; } = new RawColor(25, 25, 25); // Jak powierzchnie reagują na ambient tego światła
        public RawColor DiffuseColor { get; set; } = RawColor.Gray(255);      // Jak powierzchnie reagują na diffuse tego światła
        public RawColor SpecularColor { get; set; } = RawColor.Gray(255);     // Jak powierzchnie reagują na specular tego światła
        public float Shininess { get; set; } = 32.0f;                   // Połyskliwość narzucona przez to światło

        // Można dodać ogólną intensywność, jeśli kolory powyżej to tylko odcienie,
        // ale dla uproszczenia załóżmy, że kolory już zawierają intensywność.
        // public float Intensity { get; set; } = 1.0f;

        public bool IsEnabled { get; set; } = true;

        // Konstruktor może przyjmować te wartości
        protected Light(RawColor? ambient = null, RawColor? diffuse = null, RawColor? specular = null, float shininess = 32.0f)
        {
            AmbientColor = ambient ?? new RawColor(25, 25, 25);
            DiffuseColor = diffuse ?? RawColor.Gray(255);
            SpecularColor = specular ?? RawColor.Gray(255);
            Shininess = shininess;
        }

        /// <summary>
        /// Oblicza kolor wierzchołka pod wpływem TEGO światła, używając
        /// właściwości "materiału" zdefiniowanych w tym świetle.
        /// </summary>
        /// <param name="worldPosition">Pozycja punktu w przestrzeni świata.</param>
        /// <param name="worldNormal">Normalna powierzchni w przestrzeni świata.</param>
        /// <param name="viewDirection">Znormalizowany kierunek od punktu do kamery.</param>
        /// <param name="vertexBaseColor">Bazowy kolor (albedo) wierzchołka.</param>
        /// <returns>Kolor wynikający z interakcji tego światła z punktem.</returns>
        public abstract RawColor Calculate(Vector3 worldPosition, Vector3 worldNormal, Vector3 viewDirection, RawColor vertexBaseColor);

        // Metody pomocnicze (MultiplyColors, AddColors, ScaleColor - bez zmian jak w poprzedniej odpowiedzi)
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
            factor = Math.Max(0f, factor); // Upewnijmy się, że factor nie jest ujemny
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
            Direction = (direction).Normalized();
        }

        public override RawColor Calculate(Vector3 worldPosition, Vector3 worldNormal, Vector3 viewDirection, RawColor vertexBaseColor)
        {
            Vector3 lightDir = -Direction; // Kierunek DO źródła światła

            // Komponent Ambient (używa AmbientColor z tego światła)
            // Nie jest modulowany przez vertexBaseColor
            RawColor ambientComp = AmbientColor;

            // Komponent Diffuse (używa DiffuseColor z tego światła, modulowany przez vertexBaseColor)
            float diffFactor = MathF.Max(0f, Vector3.Dot(worldNormal, lightDir));
            RawColor diffuseModulated = MultiplyColors(DiffuseColor, vertexBaseColor); // Modulacja przez bazowy kolor wierzchołka
            RawColor diffuseComp = ScaleColor(diffuseModulated, diffFactor);

            // Komponent Specular (używa SpecularColor i Shininess z tego światła)
            // Nie jest modulowany przez vertexBaseColor
            Vector3 halfwayDir = (lightDir + viewDirection).Normalized();
            float specFactorBase = MathF.Max(0f, Vector3.Dot(worldNormal, halfwayDir));
            // Zapobiegaj odblaskom od spodu
            if (diffFactor <= 0) specFactorBase = 0;

            float specFactor = MathF.Pow(specFactorBase, Shininess);
            RawColor specularComp = ScaleColor(SpecularColor, specFactor);

            // Suma komponentów
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

        public void DisableSpotlight()
        {
            SpotCutoffCos = -1.0f;
        }


        public override RawColor Calculate(Vector3 worldPosition, Vector3 worldNormal, Vector3 viewDirection, RawColor vertexBaseColor)
        {
            Vector3 lightVector = Position - worldPosition;
            float distance = lightVector.Length();
            if (distance < 0.0001f) distance = 0.0001f;
            Vector3 lightDir = lightVector / distance;

            // 1. Tłumienie
            float attenuation = 1.0f / (ConstantAttenuation + LinearAttenuation * distance + QuadraticAttenuation * distance * distance);
            attenuation = Math.Clamp(attenuation, 0f, 1f);

            // 2. Spotlight
            float spotFactor = 1.0f;
            if (SpotCutoffCos > -1.0f)
            {
                float angleCos = Vector3.Dot(SpotDirection, -lightDir);
                if (angleCos < SpotCutoffCos)
                {
                    return RawColor.Gray(0); // Poza stożkiem
                }
                spotFactor = MathF.Pow(MathF.Max(0f, angleCos), SpotExponent);
            }

            // 3. Obliczenia komponentów z uwzględnieniem tłumienia i spotlight

            // Ambient (używa AmbientColor z tego światła, nie tłumiony, nie wpływa spotlight, nie modulowany)
            RawColor ambientComp = AmbientColor; // Zauważ: Światło punktowe dodające ambient jest dziwne, ale zgodne z życzeniem

            // Diffuse (używa DiffuseColor, modulowany przez vertexBaseColor, uwzględnia tłumienie i spotlight)
            float diffFactor = MathF.Max(0f, Vector3.Dot(worldNormal, lightDir));
            RawColor diffuseModulated = MultiplyColors(DiffuseColor, vertexBaseColor);
            RawColor diffuseComp = ScaleColor(diffuseModulated, diffFactor * attenuation * spotFactor);

            // Specular (używa SpecularColor i Shininess, uwzględnia tłumienie i spotlight)
            Vector3 halfwayDir = (lightDir + viewDirection).Normalized();
            float specFactorBase = MathF.Max(0f, Vector3.Dot(worldNormal, halfwayDir));
            // Zapobiegaj odblaskom od spodu
            if (diffFactor <= 0) specFactorBase = 0;

            float specFactor = MathF.Pow(specFactorBase, Shininess);
            RawColor specularComp = ScaleColor(SpecularColor, specFactor * attenuation * spotFactor);

            // Suma komponentów
            return AddColors(AddColors(ambientComp, diffuseComp), specularComp);
        }
    }
}
