using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FortyWorks.FGAImporter
{
    public class FGAParser
    {
        private readonly string _fgaContent;
        private readonly string _outputPath;
        private const int HeaderLength = 10;

        public FGAParser(string fgaContent, string outputPath)
        {
            _fgaContent = fgaContent;
            _outputPath = outputPath;
        }

        private float[] ParseFgaContent()
        {
            // Remove CR or LF from content. Then parse all values as float.
            return _fgaContent
                .Replace("\r", "")
                .Replace("\n", "")
                .Split(',').Select(TextToFloat)
                .ToArray();
        }

        private float TextToFloat(string x)
        {
            float value;
            float.TryParse(x, out value);
            return value;
        }

        private Texture3D CreateEmptyTexture(int sizeX, int sizeY, int sizeZ)
        {
            return new Texture3D(
                sizeX,
                sizeY,
                sizeZ,
                TextureFormat.RGBAFloat,
                false
            )
            {
                wrapMode = TextureWrapMode.Clamp
            };
        }

        public void Parse()
        {
            var allValues = ParseFgaContent();
            // The first 3 values in fga is Vector Density in XYZ.
            var density = new Vector3(allValues[0], allValues[1], allValues[2]);
            // The next 6 values in fga is BB Size min XYZ, BB Size max XYZ.
            var bbMinSize = new Vector3(allValues[3], allValues[4], allValues[5]);
            var bbMaxSize = new Vector3(allValues[6], allValues[7], allValues[8]);
            var mainVectors = allValues.Skip(9).ToArray();
            
            var vectorField = CreateEmptyTexture((int) density.x, (int) density.y, (int) density.z);
            var contentLength = allValues.Length - 9;
            var resultLength = Mathf.RoundToInt(contentLength / 3f);
            
            // The remap vector field to texture.
            var colors = Enumerable.Range(0, resultLength).Select(i =>
            {
                var v = new Vector3(
                    mainVectors[i * 3 + 0],
                    mainVectors[i * 3 + 1],
                    mainVectors[i * 3 + 2]
                ).normalized;
                return new Color(v.x, v.y, v.z, 1f);
            }).ToArray();
            
            vectorField.SetPixels(colors);
            vectorField.Apply(false);
            
            // Save as asset.
            AssetDatabase.CreateAsset(vectorField, _outputPath);
        }
    }
}