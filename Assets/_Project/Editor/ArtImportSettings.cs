using UnityEditor;

namespace ExtensaoZero.EditorTools
{
    /// <summary>
    /// Configura a importação dos assets CC0 de Resources/Art (CREDITS.md): normal maps como
    /// NormalMap, mapas ARM (AO/rugosidade/metal) lineares, teto de 1024 px e mipmaps
    /// (PERFORMANCE_TARGETS §2: texturas de 512–1024 px, compressão desktop).
    /// </summary>
    public sealed class ArtImportSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/Resources/Art/")) return;
            var importer = (TextureImporter)assetImporter;
            var lower = assetPath.ToLowerInvariant();
            importer.maxTextureSize = 1024;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            if (lower.Contains("_nor_gl") || lower.Contains("_nor_dx"))
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else if (lower.Contains("_arm_") || lower.Contains("_rough") || lower.Contains("_ao_") || lower.Contains("_metal"))
            {
                importer.sRGBTexture = false;
            }
        }
    }
}
