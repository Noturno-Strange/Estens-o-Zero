using System.Collections;
using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Environment;
using UnityEngine;

namespace ExtensaoZero.Anomaly
{
    /// <summary>
    /// Executa o tipo escolhido pelo diretor. Efeitos curtos, reversíveis e legíveis.
    /// Nenhum efeito move o jogador ou causa dano.
    /// </summary>
    public sealed class TensionEventRunner : MonoBehaviour
    {
        private GameManager m_Manager;
        private WorldContext m_World;

        public static TensionEventRunner Create(Transform parent, GameManager manager, WorldContext world)
        {
            var go = new GameObject("TensionEventRunner");
            if (parent != null) go.transform.SetParent(parent, false);
            var runner = go.AddComponent<TensionEventRunner>();
            runner.m_Manager = manager;
            runner.m_World = world;
            return runner;
        }

        public void Run(TensionEventDefinition definition)
        {
            if (definition == null) return;
            switch (definition.kind)
            {
                case TensionEventKind.LightFlicker:
                    StartCoroutine(Flicker(definition.intensity));
                    break;
                case TensionEventKind.PhoneRingOnce:
                    PlayRingOnce();
                    break;
                case TensionEventKind.DistantSteps:
                    StartCoroutine(DistantSteps(definition.intensity));
                    break;
                case TensionEventKind.Whisper:
                    PlayScare(ProceduralAudio.Whisper(), definition.intensity);
                    break;
                case TensionEventKind.LightSequence:
                    StartCoroutine(LightSequence(definition.intensity));
                    break;
                case TensionEventKind.PhonesRingTogether:
                    StartCoroutine(PhonesTogether());
                    break;
                case TensionEventKind.SubscriberManifestation:
                    m_Manager?.Subscriber?.Manifest();
                    break;
                case TensionEventKind.RecordingAlteration:
                    PlayScare(ProceduralAudio.VoicePlaceholder(91), definition.intensity);
                    break;
            }
        }

        private void PlayRingOnce()
        {
            if (m_World == null || m_World.OfficePhone == null) return;
            m_Manager?.Audio?.PlayWorldOneShot(
                ProceduralAudio.Ring(), m_World.OfficePhone.transform.position, 0.4f, AudioCategory.Scare);
        }

        private void PlayScare(AudioClip clip, float intensity)
        {
            m_Manager?.Audio?.PlayScare(clip, Mathf.Lerp(0.35f, 0.9f, intensity));
        }

        private IEnumerator Flicker(float intensity)
        {
            if (m_World == null || m_World.CeilingLights.Count == 0) yield break;
            var light = m_World.CeilingLights[Random.Range(0, m_World.CeilingLights.Count)];
            if (light == null) yield break;
            var original = light.intensity;
            var flashes = Mathf.RoundToInt(Mathf.Lerp(2f, 5f, intensity));
            for (var i = 0; i < flashes; i++)
            {
                light.intensity = original * 0.1f;
                yield return new WaitForSeconds(Random.Range(0.04f, 0.12f));
                light.intensity = original;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.18f));
            }

            light.intensity = original;
        }

        private IEnumerator DistantSteps(float intensity)
        {
            if (m_World == null || m_World.CorridorAnchor == null) yield break;
            var position = m_World.CorridorAnchor.position;
            var count = Mathf.RoundToInt(Mathf.Lerp(3f, 6f, intensity));
            for (var i = 0; i < count; i++)
            {
                m_Manager?.Audio?.PlayWorldOneShot(
                    ProceduralAudio.Footstep(), position + Vector3.forward * (i * 0.7f), 0.3f, AudioCategory.Scare);
                yield return new WaitForSeconds(0.42f);
            }
        }

        private IEnumerator LightSequence(float intensity)
        {
            if (m_World == null || m_World.CeilingLights.Count == 0) yield break;
            var states = new float[m_World.CeilingLights.Count];
            for (var i = 0; i < m_World.CeilingLights.Count; i++)
            {
                var light = m_World.CeilingLights[i];
                if (light == null) continue;
                states[i] = light.intensity;
                light.intensity = 0.05f;
                yield return new WaitForSeconds(Mathf.Lerp(0.16f, 0.06f, intensity));
            }

            yield return new WaitForSeconds(1.8f);

            for (var i = 0; i < m_World.CeilingLights.Count; i++)
            {
                var light = m_World.CeilingLights[i];
                if (light != null) light.intensity = states[i];
            }
        }

        private IEnumerator PhonesTogether()
        {
            if (m_World == null) yield break;
            for (var i = 0; i < m_World.SwitchboardSockets.Count; i++)
            {
                var socket = m_World.SwitchboardSockets[i];
                if (socket == null) continue;
                m_Manager?.Audio?.PlayWorldOneShot(
                    ProceduralAudio.Ring(), socket.transform.position, 0.22f, AudioCategory.Scare);
                yield return new WaitForSeconds(0.08f);
            }
        }
    }
}
