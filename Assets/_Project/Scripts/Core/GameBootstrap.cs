using UnityEngine;

namespace ExtensaoZero.Core
{
    /// <summary>
    /// Ponto de entrada. A cena Bootstrap é vazia; todo o mundo é construído aqui.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (GameManager.Instance != null) return;

            Debug.Log("[EZ] Bootstrap: criando GameManager e mundo procedural.");
            var root = new GameObject("EXTENSAO ZERO");
            Object.DontDestroyOnLoad(root);
            var manager = root.AddComponent<GameManager>();
            manager.Initialize();
        }
    }
}
