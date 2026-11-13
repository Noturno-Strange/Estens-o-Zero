using System.Collections;
using ExtensaoZero.Campaign;
using ExtensaoZero.Core;
using ExtensaoZero.Environment;
using ExtensaoZero.Investigation;
using ExtensaoZero.Telephone;
using ExtensaoZero.UI;
using UnityEngine;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Missions
{
    /// <summary>
    /// Base das missões da campanha. Cada capítulo é uma máquina de estados pequena:
    /// <see cref="SetupWorld"/> roda em todos os peers (determinístico pela seed) e
    /// <see cref="StartMission"/>/eventos rodam só no host. Cada passo registra fatos,
    /// nunca soluções validadas.
    /// </summary>
    public abstract class MissionBase : MonoBehaviour
    {
        public static MissionBase Instance { get; private set; }

        public abstract int ChapterIndex { get; }
        public int StepIndex { get; protected set; }
        public string Summary { get; protected set; } = string.Empty;
        public bool Completed { get; protected set; }
        public EndingKind Ending { get; protected set; } = EndingKind.None;
        public int Seed { get; private set; }

        protected GameManager Manager { get; private set; }
        protected WorldContext World { get; private set; }
        protected DifficultyProfile Difficulty => Manager != null ? Manager.Difficulty : DifficultyProfile.Get(DifficultyLevel.Operation);
        protected bool Hints => Difficulty.HintsFor(Manager != null ? Manager.PlayerCount : 1);

        public void Bind(GameManager manager, WorldContext world)
        {
            Manager = manager;
            World = world;
        }

        protected virtual void Awake()
        {
            Instance = this;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Prepara o mundo do capítulo (portas, itens, variações). Roda em todos os peers.</summary>
        public void SetupWorld(int seed)
        {
            Seed = seed;
            StepIndex = 0;
            Completed = false;
            Summary = string.Empty;
            Ending = EndingKind.None;
            WorldCommands.ResetForChapter(Manager, ChapterIndex);
            OnSetupWorld(seed);
        }

        protected virtual void OnSetupWorld(int seed)
        {
        }

        /// <summary>Inicia a lógica da missão (host/solo).</summary>
        public void StartMission()
        {
            StopAllCoroutines();
            StepIndex = 0;
            Completed = false;
            OnStart();
            Debug.Log($"[EZ] Missão do capítulo {ChapterIndex} iniciada.");
        }

        protected abstract void OnStart();

        // ------------------------------------------------------------ ganchos

        public virtual void OnPhoneAnswered(Phone phone)
        {
            if (phone != null) HandleEvent("phone.answered:" + phone.extension + ":" + (phone.CurrentCall != null ? phone.CurrentCall.id : ""));
        }

        public virtual void OnCallFinished(Phone phone)
        {
            if (phone != null) HandleEvent("phone.finished:" + phone.extension);
        }

        public virtual void OnCablePicked(string label)
        {
            HandleEvent("cable.picked:" + label);
        }

        public virtual ConnectionVerdict EvaluateConnection(int a, int b)
        {
            return DefaultVerdict(a, b);
        }

        public virtual void OnConnectionResult(ConnectionVerdict verdict, int a, int b)
        {
            if (verdict == ConnectionVerdict.Contaminated) Contaminate(45f, a, b);
            HandleEvent($"switch.connect:{Mathf.Min(a, b)}:{Mathf.Max(a, b)}:{verdict}");
        }

        public virtual void OnCorridorEntered()
        {
            HandleEvent("zone:corridor");
        }

        public virtual void OnIsolatorPulled()
        {
            HandleEvent("lever:isolate");
        }

        public virtual void OnEvidence(string evidenceId)
        {
        }

        public virtual void HandleEvent(string id)
        {
        }

        public static ConnectionVerdict DefaultVerdict(int a, int b)
        {
            if (SwitchboardRules.IsContaminated(a, b)) return ConnectionVerdict.Contaminated;
            return SwitchboardRules.IsSolution(a, b) ? ConnectionVerdict.Solution : ConnectionVerdict.NoAnswer;
        }

        // ------------------------------------------------------------ utilidades

        protected void SetStep(int step, string objective)
        {
            StepIndex = step;
            Manager?.AdvanceMissionStep(step);
            Manager?.SetObjective(objective);
        }

        protected void Register(string id)
        {
            var entry = EvidenceCatalog.Find(id);
            if (entry != null) Manager?.RegisterEvidence(entry);
        }

        protected void Say(string title, string body, Color color, float duration = 6f)
        {
            Manager?.Announce(title, body, color, duration);
        }

        protected void Hint(string body)
        {
            if (Hints) Manager?.Announce("PISTA", body, UITheme.Dim, 6f);
        }

        protected void Ring(int extension, string callId, float delay)
        {
            StartCoroutine(RingAfter(extension, callId, delay));
        }

        private IEnumerator RingAfter(int extension, string callId, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (Completed) yield break;
            Manager?.RingPhone(extension, callId);
        }

        protected void Contaminate(float amount, int a, int b)
        {
            Manager?.Contamination?.Add(amount * Difficulty.ContaminationMultiplier, $"conexão contaminada {a:000}→{b:000}");
            Manager?.Tension?.ForceEvent(Anomaly.TensionCategory.Threat);
        }

        protected void Command(string command)
        {
            Manager?.Command(command);
        }

        /// <summary>Comando aplicado só neste peer (usado em SetupWorld, que já roda em todos).</summary>
        protected void Local(string command)
        {
            WorldCommands.Apply(Manager, command);
        }

        protected string LocationOf(int extension)
        {
            var phone = World != null ? World.GetPhone(extension) : null;
            return phone != null ? phone.locationLabel : $"EXTENSÃO {extension}";
        }

        protected void Complete(string summary, float delay = 2.2f)
        {
            if (Completed) return;
            Completed = true;
            Summary = summary;
            StartCoroutine(CompleteAfter(delay));
        }

        private IEnumerator CompleteAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            Manager?.CompleteMission();
        }

        protected void After(float delay, System.Action action)
        {
            StartCoroutine(AfterRoutine(delay, action));
        }

        private IEnumerator AfterRoutine(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            if (!Completed) action?.Invoke();
        }

        protected static bool Is(string id, string prefix) => id != null && id.StartsWith(prefix, System.StringComparison.Ordinal);
    }
}
