using System.Collections.Generic;
using UnityEngine;

namespace ExtensaoZero.Environment
{
    /// <summary>
    /// Caixas com UV em escala de mundo: a textura repete a cada <c>tile</c> metros em vez de
    /// esticar em paredes grandes (problema dos cubos primitivos). Normais e tangentes corretas
    /// para normal maps. Malhas são cacheadas por tamanho e legíveis (static batching).
    /// </summary>
    public static class ModularMesh
    {
        private static readonly Dictionary<string, Mesh> s_Cache = new Dictionary<string, Mesh>();

        public static Mesh Box(Vector3 size, float tile)
        {
            tile = Mathf.Max(0.1f, tile);
            var key = $"{size.x:0.###}|{size.y:0.###}|{size.z:0.###}|{tile:0.##}";
            if (s_Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var h = size * 0.5f;
            var vertices = new List<Vector3>(24);
            var normals = new List<Vector3>(24);
            var uvs = new List<Vector2>(24);
            var triangles = new List<int>(36);

            // Cada face: origem, eixo U, eixo V (comprimentos em metros), normal.
            Face(vertices, normals, uvs, triangles, new Vector3(-h.x, -h.y, h.z), Vector3.right * size.x, Vector3.up * size.y, Vector3.forward, tile, true);
            Face(vertices, normals, uvs, triangles, new Vector3(h.x, -h.y, -h.z), Vector3.left * size.x, Vector3.up * size.y, Vector3.back, tile, true);
            Face(vertices, normals, uvs, triangles, new Vector3(h.x, -h.y, h.z), Vector3.back * size.z, Vector3.up * size.y, Vector3.right, tile, true);
            Face(vertices, normals, uvs, triangles, new Vector3(-h.x, -h.y, -h.z), Vector3.forward * size.z, Vector3.up * size.y, Vector3.left, tile, true);
            Face(vertices, normals, uvs, triangles, new Vector3(-h.x, h.y, -h.z), Vector3.right * size.x, Vector3.forward * size.z, Vector3.up, tile, true);
            Face(vertices, normals, uvs, triangles, new Vector3(-h.x, -h.y, h.z), Vector3.right * size.x, Vector3.back * size.z, Vector3.down, tile, true);

            var mesh = new Mesh { name = "EZ_Box_" + key };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            s_Cache[key] = mesh;
            return mesh;
        }

        private static void Face(List<Vector3> v, List<Vector3> n, List<Vector2> uv, List<int> t,
            Vector3 origin, Vector3 axisU, Vector3 axisV, Vector3 normal, float tile, bool worldUv)
        {
            var start = v.Count;
            v.Add(origin);
            v.Add(origin + axisU);
            v.Add(origin + axisU + axisV);
            v.Add(origin + axisV);
            for (var i = 0; i < 4; i++) n.Add(normal);
            var lu = axisU.magnitude / tile;
            var lv = axisV.magnitude / tile;
            uv.Add(new Vector2(0f, 0f));
            uv.Add(new Vector2(lu, 0f));
            uv.Add(new Vector2(lu, lv));
            uv.Add(new Vector2(0f, lv));

            // Ordem horária vista de fora (convenção da Unity).
            if (Vector3.Dot(Vector3.Cross(axisV, axisU), normal) > 0f)
            {
                t.Add(start); t.Add(start + 2); t.Add(start + 1);
                t.Add(start); t.Add(start + 3); t.Add(start + 2);
            }
            else
            {
                t.Add(start); t.Add(start + 1); t.Add(start + 2);
                t.Add(start); t.Add(start + 2); t.Add(start + 3);
            }
        }

        public static void Clear()
        {
            s_Cache.Clear();
        }
    }
}
