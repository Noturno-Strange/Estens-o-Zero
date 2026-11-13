using System.Collections.Generic;
using UnityEngine;

namespace ExtensaoZero.Environment
{
    /// <summary>Superfícies texturizadas do kit modular (ART_DIRECTION §4).</summary>
    public enum Surface
    {
        WallUpper,
        WallLower,
        WallPeeling,
        WallConcrete,
        WallTunnel,
        WallFacade,
        WallBurned,
        FloorLinoleum,
        FloorTiles,
        FloorWood,
        FloorConcrete,
        FloorMetal,
        FloorTunnel,
        Ceiling,
        CeilingTunnel,
        MetalPanel,
        Rust,
        Corrugated,
        Ground,
        Asphalt,
        DoorWood,
        DoorMetal,
        Trim
    }

    /// <summary>
    /// Materiais PBR e modelos CC0 (Poly Haven, ver CREDITS.md) carregados de Resources/Art.
    /// Tudo tem substituto procedural: se uma textura ou modelo não importar, o mundo continua
    /// jogável com a cor da paleta e uma caixa do tamanho certo.
    /// </summary>
    public static class ArtLibrary
    {
        private struct SurfaceSpec
        {
            public string Texture;
            public Color Tint;
            public float Tile;
            public float Smoothness;
            public float Metallic;
        }

        private static readonly Dictionary<Surface, SurfaceSpec> s_Specs = new Dictionary<Surface, SurfaceSpec>
        {
            { Surface.WallUpper, Spec("painted_plaster_wall", 0.42f, 0.60f, 0.54f, 2.5f, 0.12f) },
            { Surface.WallLower, Spec("painted_plaster_wall", 0.17f, 0.32f, 0.28f, 2.0f, 0.2f) },
            { Surface.WallPeeling, Spec("peeling_painted_wall", 0.62f, 0.68f, 0.64f, 2.2f, 0.1f) },
            { Surface.WallConcrete, Spec("concrete_block_wall", 0.9f, 0.9f, 0.88f, 2.4f, 0.08f) },
            { Surface.WallTunnel, Spec("concrete_wall_008", 0.48f, 0.55f, 0.6f, 3f, 0.1f) },
            { Surface.WallFacade, Spec("concrete_block_wall", 0.8f, 0.82f, 0.8f, 2.6f, 0.05f) },
            { Surface.WallBurned, Spec("concrete_wall_008", 0.22f, 0.2f, 0.19f, 2.5f, 0.05f) },
            { Surface.FloorLinoleum, Spec("old_linoleum_flooring_01", 0.48f, 0.44f, 0.4f, 2f, 0.32f) },
            { Surface.FloorTiles, Spec("worn_tile_floor", 1f, 1f, 1f, 2f, 0.25f) },
            { Surface.FloorWood, Spec("dark_wooden_planks", 0.85f, 0.8f, 0.75f, 2.5f, 0.18f) },
            { Surface.FloorConcrete, Spec("concrete_floor_worn_001", 1f, 1f, 1f, 3f, 0.15f) },
            { Surface.FloorMetal, Spec("metal_plate_02", 1f, 1f, 1f, 2f, 0.35f, 0.6f) },
            { Surface.FloorTunnel, Spec("concrete_floor_worn_001", 0.6f, 0.66f, 0.72f, 3f, 0.3f) },
            { Surface.Ceiling, Spec("plaster_grey_04", 0.42f, 0.44f, 0.42f, 3f, 0.05f) },
            { Surface.CeilingTunnel, Spec("concrete_wall_008", 0.3f, 0.33f, 0.36f, 3f, 0.05f) },
            { Surface.MetalPanel, Spec("metal_plate_02", 0.75f, 0.8f, 0.78f, 1.5f, 0.4f, 0.65f) },
            { Surface.Rust, Spec("rusty_metal_02", 0.65f, 0.6f, 0.55f, 1.5f, 0.3f, 0.5f) },
            { Surface.Corrugated, Spec("corrugated_iron_02", 0.8f, 0.82f, 0.8f, 2f, 0.3f, 0.6f) },
            { Surface.Ground, Spec("dry_ground_01", 0.55f, 0.56f, 0.5f, 4f, 0.05f) },
            { Surface.Asphalt, Spec("asphalt_02", 0.7f, 0.7f, 0.7f, 3f, 0.2f) },
            { Surface.DoorWood, Spec("dark_wooden_planks", 0.7f, 0.62f, 0.55f, 1.2f, 0.2f) },
            { Surface.DoorMetal, Spec("rusty_metal_02", 0.42f, 0.5f, 0.46f, 1.4f, 0.35f, 0.5f) },
            { Surface.Trim, Spec("metal_plate_02", 0.35f, 0.38f, 0.36f, 1f, 0.3f, 0.5f) }
        };

        private static readonly Dictionary<Surface, Material> s_Materials = new Dictionary<Surface, Material>();
        private static readonly Dictionary<Material, float> s_Tiles = new Dictionary<Material, float>();
        private static readonly Dictionary<string, GameObject> s_Models = new Dictionary<string, GameObject>();
        private static readonly HashSet<string> s_MissingModels = new HashSet<string>();

        /// <summary>Tamanhos aproximados (m) dos modelos, para o substituto procedural.</summary>
        private static readonly Dictionary<string, Vector3> s_ModelSizes = new Dictionary<string, Vector3>
        {
            { "metal_office_desk", new Vector3(2f, 0.79f, 0.95f) },
            { "painted_wooden_chair_01", new Vector3(0.43f, 0.96f, 0.54f) },
            { "drawer_cabinet", new Vector3(1.14f, 1.88f, 0.49f) },
            { "wooden_bookshelf_worn", new Vector3(1.37f, 2.06f, 0.58f) },
            { "worn_metal_rack", new Vector3(0.92f, 1.9f, 0.6f) },
            { "cardboard_box_01", new Vector3(0.39f, 0.34f, 0.52f) },
            { "wooden_crate_01", new Vector3(0.83f, 0.35f, 0.41f) },
            { "old_military_crate", new Vector3(1.82f, 0.3f, 0.98f) },
            { "korean_fire_extinguisher_01", new Vector3(0.28f, 0.66f, 0.37f) },
            { "power_box_01", new Vector3(0.46f, 0.5f, 0.4f) },
            { "utility_box_01", new Vector3(0.52f, 1.12f, 0.43f) },
            { "portable_generator", new Vector3(0.82f, 0.58f, 0.56f) },
            { "vintage_radio_transceiver", new Vector3(0.62f, 0.43f, 0.42f) },
            { "desk_lamp_arm_01", new Vector3(0.2f, 0.89f, 0.61f) },
            { "old_bed_frame", new Vector3(0.9f, 1.2f, 2f) },
            { "old_gas_mask", new Vector3(0.21f, 0.4f, 0.31f) },
            { "portable_cassette_player", new Vector3(0.17f, 0.13f, 0.04f) },
            { "vintage_telephone_wall_clock", new Vector3(0.34f, 0.5f, 0.19f) },
            { "barrel_03", new Vector3(0.63f, 0.93f, 0.64f) },
            { "metal_jerrycan", new Vector3(0.35f, 0.46f, 0.17f) },
            { "street_lamp_01", new Vector3(0.7f, 3.87f, 0.39f) },
            { "korean_public_payphone_01", new Vector3(0.4f, 0.74f, 0.3f) },
            { "rollershutter_door", new Vector3(3.08f, 2.4f, 0.3f) },
            { "metal_tool_chest", new Vector3(0.69f, 0.65f, 0.41f) },
            { "industrial_caged_sconce", new Vector3(0.3f, 0.4f, 0.3f) },
            { "hanging_industrial_lamp", new Vector3(0.55f, 1.36f, 0.55f) }
        };

        private static SurfaceSpec Spec(string texture, float r, float g, float b, float tile, float smoothness, float metallic = 0f)
        {
            return new SurfaceSpec { Texture = texture, Tint = new Color(r, g, b), Tile = tile, Smoothness = smoothness, Metallic = metallic };
        }

        public static Material Get(Surface surface)
        {
            if (s_Materials.TryGetValue(surface, out var existing) && existing != null) return existing;
            var spec = s_Specs[surface];
            var material = MaterialLibrary.Get("surface_" + surface, spec.Tint, spec.Smoothness, spec.Metallic);
            var albedo = Resources.Load<Texture2D>($"Art/Textures/{spec.Texture}/{spec.Texture}_diff_1k");
            var normal = Resources.Load<Texture2D>($"Art/Textures/{spec.Texture}/{spec.Texture}_nor_gl_1k");
            if (albedo != null)
            {
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", albedo);
                if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", albedo);
                material.SetTextureScale("_BaseMap", Vector2.one);
            }
            else
            {
                Debug.LogWarning($"[EZ] Textura ausente: {spec.Texture}. Usando cor da paleta.");
            }

            if (normal != null && material.HasProperty("_BumpMap"))
            {
                material.SetTexture("_BumpMap", normal);
                material.EnableKeyword("_NORMALMAP");
                if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", 1f);
            }

            s_Materials[surface] = material;
            s_Tiles[material] = spec.Tile;
            return material;
        }

        /// <summary>Metros por repetição de textura (1 m para materiais sem textura).</summary>
        public static float TileOf(Material material)
        {
            return material != null && s_Tiles.TryGetValue(material, out var tile) ? tile : 1f;
        }

        /// <summary>
        /// Instancia um modelo CC0. Frente do modelo = +Z local. Se o modelo não estiver
        /// disponível, cria uma caixa do mesmo tamanho com a cor dada.
        /// </summary>
        public static GameObject Prop(Transform parent, string model, Vector3 position, float yaw, bool collider = true,
            float scale = 1f, Color? fallbackColor = null)
        {
            var prefab = LoadModel(model);
            GameObject go;
            if (prefab != null)
            {
                go = Object.Instantiate(prefab, parent, false);
                go.name = model;
                go.transform.localPosition = position;
                go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                go.transform.localScale = Vector3.one * scale;
                foreach (var existing in go.GetComponentsInChildren<Collider>()) Object.Destroy(existing);
                if (collider) FitBoxCollider(go);
            }
            else
            {
                var size = s_ModelSizes.TryGetValue(model, out var s) ? s * scale : Vector3.one * 0.5f;
                var material = MaterialLibrary.Get("fallback_" + model, fallbackColor ?? new Color(0.2f, 0.21f, 0.2f), 0.2f);
                go = PropFactory.CreateBox(parent, model, position + Vector3.up * size.y * 0.5f, size, material, collider);
                go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            }

            return go;
        }

        private static GameObject LoadModel(string model)
        {
            if (string.IsNullOrEmpty(model) || s_MissingModels.Contains(model)) return null;
            if (s_Models.TryGetValue(model, out var cached) && cached != null) return cached;
            var prefab = Resources.Load<GameObject>($"Art/Models/{model}/{model}_1k");
            if (prefab == null)
            {
                s_MissingModels.Add(model);
                Debug.LogWarning($"[EZ] Modelo ausente ou não importado: {model}. Usando substituto procedural.");
                return null;
            }

            s_Models[model] = prefab;
            return prefab;
        }

        /// <summary>Um BoxCollider no pai cobrindo todos os renderers (barato e estável).</summary>
        private static void FitBoxCollider(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            var box = go.AddComponent<BoxCollider>();
            var inverse = go.transform.worldToLocalMatrix;
            var center = inverse.MultiplyPoint3x4(bounds.center);
            var size = inverse.MultiplyVector(bounds.size);
            box.center = center;
            box.size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        }
    }
}
