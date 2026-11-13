using ExtensaoZero.Player;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    public interface IInteractable
    {
        int InteractionId { get; set; }
        Transform InteractionTransform { get; }
        string GetPrompt(PlayerInteractor interactor);
        bool CanInteract(PlayerInteractor interactor);
        void Interact(PlayerInteractor interactor);
    }

    /// <summary>
    /// IDs determinísticos por ordem de construção do mundo. Host e clientes constroem o mesmo
    /// mundo na mesma ordem, portanto os IDs coincidem sem precisar de sincronização extra.
    /// </summary>
    public static class InteractionRegistry
    {
        private static readonly System.Collections.Generic.Dictionary<int, IInteractable> s_Items =
            new System.Collections.Generic.Dictionary<int, IInteractable>();
        private static int s_NextId = 1;

        public static int Register(IInteractable item)
        {
            if (item == null) return -1;
            var id = s_NextId++;
            item.InteractionId = id;
            s_Items[id] = item;
            return id;
        }

        public static void Unregister(IInteractable item)
        {
            if (item == null) return;
            if (item.InteractionId > 0) s_Items.Remove(item.InteractionId);
        }

        public static IInteractable Get(int id)
        {
            return s_Items.TryGetValue(id, out var item) ? item : null;
        }

        public static void Clear()
        {
            s_Items.Clear();
            s_NextId = 1;
        }
    }
}
