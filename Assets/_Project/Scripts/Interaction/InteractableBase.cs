using ExtensaoZero.Player;
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
    }
}
