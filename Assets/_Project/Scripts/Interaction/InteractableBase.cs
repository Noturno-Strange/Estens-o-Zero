using ExtensaoZero.Core;
using ExtensaoZero.Player;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField] protected string prompt = "Interagir";
        [SerializeField] protected bool interactionEnabled = true;

        public int InteractionId { get; set; } = -1;
        public Transform InteractionTransform => transform;

        protected virtual void Awake()
        {
            InteractionRegistry.Register(this);
        }

        protected virtual void OnDestroy()
        {
            InteractionRegistry.Unregister(this);
        }

        public virtual string GetPrompt(PlayerInteractor interactor)
        {
            return prompt;
        }

        public virtual bool CanInteract(PlayerInteractor interactor)
        {
            return interactionEnabled && isActiveAndEnabled;
        }

        public abstract void Interact(PlayerInteractor interactor);

        public void SetInteractionEnabled(bool enabled)
        {
            interactionEnabled = enabled;
        }

        /// <summary>
        /// Interações são executadas em todos os peers; mensagens pessoais (dicas, falhas)
        /// só aparecem para quem interagiu.
        /// </summary>
        protected static bool IsLocal(PlayerInteractor interactor)
        {
            return interactor == null || interactor.IsLocal;
        }

        protected static void TellActor(PlayerInteractor interactor, string title, string body, Color color, float duration = 4f)
        {
            if (!IsLocal(interactor)) return;
            GameManager.Instance?.UI?.ShowSubtitle(title, body, color, duration);
        }

        protected static Color Dim => UITheme.Dim;
    }
}
