using RasteryzatorInator.MathLibrary;

namespace RasteryzatorInator;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello There");

        const int width = 750;
        const int height = 480;

        Buffer buffer = new Buffer();
        buffer.SetSize(width, height);
        buffer.ClearColor(25, 25, 25);
        buffer.ClearDepth(1.0f);

        VertexProcessor vertexProcessor = new VertexProcessor();

        // kamera
        Vector3 eyePosition = new Vector3(0, 0, 5);
        Vector3 focusPoint = Vector3.Zero;
        Vector3 upVector = Vector3.Up;
        vertexProcessor.SetLookAt(eyePosition, focusPoint, upVector);


        Rasterizer rasterizer = new Rasterizer(buffer, vertexProcessor, eyePosition);


        var directionalLight = new LightDirectional(
                direction: new Vector3(0.5f, -1.0f, -0.7f).Normalized(),
                ambient: new RawColor(20, 20, 20),
                diffuse: new RawColor(200, 160, 140),
                specular: new RawColor(20, 20, 20),
                shininess: 32f
            );

        //var pointLight = new LightPoint(
        //    position: new Vector3(0f, 1f, 3f),
        //    ambient: new RawColor(10, 10, 10),
        //    diffuse: new RawColor(220, 180, 150),
        //    specular: new RawColor(255, 255, 255),
        //    shininess: 32f
        //)
        //{
        //    ConstantAttenuation = 1.0f,
        //    LinearAttenuation = 0.00f,
        //    QuadraticAttenuation = 0.00f
        //};

        //pointLight.SetSpotlight(new Vector3(-0.2f, -0.5f, -1), 30);


        rasterizer.Lights.Add(directionalLight);
        //rasterizer.Lights.Add(pointLight);

        float fovYDegrees = 60.0f;
        float aspectRatio = (float)width / height;
        float nearPlane = 0.1f;
        float farPlane = 100.0f;
        vertexProcessor.SetPerspective(fovYDegrees, aspectRatio, nearPlane, farPlane);


        // Tekstury ---------------------------------------
        string outputDirectory = Directory.GetCurrentDirectory();

        string textureFilePath1 = Path.Combine(outputDirectory, "texture1.png");
        string textureFilePath2 = Path.Combine(outputDirectory, "textureBricks.png");
        string textureFilePath3 = Path.Combine(outputDirectory, "textureGrass.png");


        Texture woodPlankTexture = null;
        Texture bricksTexture = null;
        Texture grassTexture = null;

        try
        {
            woodPlankTexture = new Texture(textureFilePath1);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Nie znaleziono pliku tekstury '{textureFilePath1}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BŁĄD podczas ładowania tekstury '{textureFilePath1}': {ex.Message}");
        }

        try
        {
            bricksTexture = new Texture(textureFilePath2);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Nie znaleziono pliku tekstury '{textureFilePath1}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BŁĄD podczas ładowania tekstury '{textureFilePath1}': {ex.Message}");
        }

        try
        {
            grassTexture = new Texture(textureFilePath3);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Nie znaleziono pliku tekstury '{textureFilePath1}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BŁĄD podczas ładowania tekstury '{textureFilePath1}': {ex.Message}");
        }

        //vertexProcessor.ResetObjectTransform();
        //vertexProcessor.Rotate(Vector3.UnitX, -45);
        //vertexProcessor.Translate(new Vector3(-1.9f, 0, 0));
        //vertexProcessor.Scale(new Vector3(0.5f, 0.5f, 0.5f));
        //rasterizer.DrawTorus(2, 1, 32, 16, ShadingMode.Flat, new RawColor(0, 255, 255), true, woodPlankTexture);

        vertexProcessor.ResetObjectTransform();
        vertexProcessor.Rotate(Vector3.UnitX, -40);
        vertexProcessor.Translate(new Vector3(2.6f, -1, 0));
        rasterizer.DrawCylinder(16, 2, 2, ShadingMode.Phong, new RawColor(255, 255, 0), true, bricksTexture);

        vertexProcessor.ResetObjectTransform();
        vertexProcessor.Rotate(Vector3.UnitZ, 60);
        vertexProcessor.Translate(new Vector3(0, -1, 0));
        rasterizer.DrawCylinder(16, 2, 2, ShadingMode.Phong, new RawColor(255, 255, 0), true, woodPlankTexture);

        vertexProcessor.ResetObjectTransform();
        vertexProcessor.Rotate(Vector3.UnitX, -20);
        vertexProcessor.Translate(new Vector3(-2.6f, -1, 0));
        rasterizer.DrawCone(16, 2, ShadingMode.Phong, new RawColor(255, 255, 0), false, bricksTexture);


        buffer.SaveTGA("output_rasterized.tga");
    }
}
