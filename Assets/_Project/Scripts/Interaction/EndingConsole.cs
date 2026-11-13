using ExtensaoZero.Campaign;
using ExtensaoZero.Core;
using ExtensaoZero.Player;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Interaction
{
    /// <summary>
    /// Uma das três conclusões do Capítulo 5. Mostra se está disponível; se não estiver,
    /// mostra o requisito ao jogador que interagiu.
    /// </summary>
    public sealed class EndingConsole : InteractableBase
    {
        public EndingKind ending = EndingKind.Isolate;
        public string objectLabel = "Console";
        public bool Armed { get; private set; }

        public void Arm(bool armed)
        {
            Armed = armed;
        }

        private bool Available
        {
            get
            {
                var save = GameManager.Instance != null ? GameManager.Instance.Save : null;
                return save != null && EndingRules.IsAvailable(ending, save.flags, save.isolatedCircuits);
            }
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (!Armed) return objectLabel;
            return Available ? $"{EndingRules.Title(ending)} — {objectLabel}" : $"{EndingRules.Title(ending)} (indisponível)";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            var manager = GameManager.Instance;
            if (manager == null) return;
            if (!Armed)
            {
                TellActor(interactor, objectLabel.ToUpperInvariant(), "Ainda não. A linha precisa estar aberta para isso ter efeito.", UITheme.Dim, 4f);
                return;
            }

            // A disponibilidade é decidida no host, que é dono do progresso da campanha.
            if (manager.IsHost && !Available)
            {
                manager.Announce(EndingRules.Title(ending), "Requisito: " + EndingRules.Requirement(ending), UITheme.Dim, 6f);
                return;
            }

            manager.RaiseEvent("ending:" + (int)ending);
        }
    }
}
