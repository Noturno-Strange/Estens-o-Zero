using UnityEngine;

namespace ExtensaoZero.Environment
{
    /// <summary>
    /// Helpers de geometria procedural. Cada prop é um primitivo com material compartilhado,
    /// o que mantém batches baixos e o build sem dependência de modelos externos.
    /// </summary>
    public static class PropFactory
    {
        private static Font s_Font;

        /// <summary>
        /// Caixa com malha de UV em escala de mundo (a textura repete, não estica). O tamanho fica
        /// na malha; a escala do transform é 1, então filhos não herdam distorção.
        /// </summary>
        public static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 size, Material material, bool collider = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = ModularMesh.Box(size, ArtLibrary.TileOf(material));
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            if (collider)
            {
                var box = go.AddComponent<BoxCollider>();
                box.size = size;
            }

            return go;
        }

        public static GameObject CreateCylinder(Transform parent, string name, Vector3 position, Vector3 size, Material material, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;
            ApplyMaterial(go, material);
            if (!collider)
            {
                var box = go.GetComponent<Collider>();
                if (box != null) Object.Destroy(box);
            }

            return go;
        }

        public static GameObject CreateSphere(Transform parent, string name, Vector3 position, float diameter, Material material, bool collider = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = Vector3.one * diameter;
            ApplyMaterial(go, material);
            if (!collider)
            {
                var box = go.GetComponent<Collider>();
                if (box != null) Object.Destroy(box);
            }

            return go;
        }

        public static TextMesh CreateLabel(Transform parent, string text, Vector3 position, Quaternion rotation, float size = 0.08f, Color? color = null)
        {
            var go = new GameObject("Label_" + text);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.fontSize = 64;
            mesh.characterSize = size;
            mesh.color = color ?? new Color(0.85f, 0.82f, 0.72f);
            if (s_Font == null) s_Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (s_Font != null)
            {
                mesh.font = s_Font;
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null) renderer.sharedMaterial = s_Font.material;
            }

            return mesh;
        }

        public static Light CreatePointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range, LightShadows shadows = LightShadows.None)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows;
            light.shadowStrength = 0.7f;
            light.renderMode = LightRenderMode.ForcePixel;
            return light;
        }

        public static GameObject CreateTrigger(Transform parent, string name, Vector3 position, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var collider = go.AddComponent<BoxCollider>();
            collider.size = size;
            collider.isTrigger = true;
            return go;
        }

        public static void ApplyMaterial(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null) renderer.sharedMaterial = material;
        }
    }
}
