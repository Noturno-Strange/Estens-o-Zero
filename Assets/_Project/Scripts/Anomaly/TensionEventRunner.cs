using System.Collections;
using System.Collections.Generic;
using ExtensaoZero.Audio;
using ExtensaoZero.Core;
using ExtensaoZero.Environment;
using ExtensaoZero.UI;
using UnityEngine;

namespace ExtensaoZero.Anomaly
{
    /// <summary>
    /// Executa o evento escolhido pelo diretor, sempre perto do jogador local (setor atual).
    /// Efeitos curtos, reversíveis e legíveis; nenhum move o jogador ou causa dano.
    /// "Reduzir flashes" e "Reduzir intensidade" geram versões atenuadas.
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

        private bool ReduceFlashes => m_Manager != null && m_Manager.Settings != null && m_Manager.Settings.reduceFlashes;
        private bool ReduceScare => m_Manager != null && m_Manager.Settings != null && m_Manager.Settings.reduceScareIntensity;

        private Vector3 PlayerPosition
        {
            get
            {
                var controller = m_Manager != null ? m_Manager.LocalPlayerController : null;
                return controller != null ? controller.transform.position : Vector3.zero;
            }
        }

        public void Run(TensionEventDefinition definition)
        {
            if (definition == null || !isActiveAndEnabled) return;
            var intensity = ReduceScare ? definition.intensity * 0.5f : definition.intensity;
            switch (definition.kind)
            {
                case TensionEventKind.LightFlicker:
                    StartCoroutine(Flicker(intensity));
                    break;
                case TensionEventKind.PhoneRingOnce:
                    RingNearestPhone(0.4f);
                    break;
                case TensionEventKind.DistantSteps:
                    StartCoroutine(DistantSteps(intensity));
                    break;
                case TensionEventKind.Whisper:
                    PlayScare(ProceduralAudio.Whisper(), intensity, BehindPlayer(3.5f));
                    break;
                case TensionEventKind.LightSequence:
                    StartCoroutine(LightSequence(intensity));
                    break;
                case TensionEventKind.PhonesRingTogether:
                    StartCoroutine(PhonesTogether(intensity));
                    break;
                case TensionEventKind.SubscriberManifestation:
                    m_Manager?.Subscriber?.ManifestNearPlayer();
                    break;
                case TensionEventKind.RecordingAlteration:
                    PlayScare(ProceduralAudio.VoicePlaceholder(91), intensity, BehindPlayer(4f));
                    break;
                case TensionEventKind.KnocksDivergent:
                    StartCoroutine(Knocks(intensity));
                    break;
                case TensionEventKind.PhoneDisconnectedRing:
                    RingNearestPhone(0.55f);
                    break;
                case TensionEventKind.StepsMirror:
                    StartCoroutine(StepsMirror(intensity));
                    break;
                case TensionEventKind.LedDisconnected:
                    StartCoroutine(LedFlash());
                    break;
                case TensionEventKind.PrivateCall:
                    PrivateCall();
                    break;
            }

            if (!string.IsNullOrEmpty(definition.subtitle))
            {
                var color = definition.category == TensionCategory.Threat ? UITheme.Red
                    : definition.category == TensionCategory.Contradiction ? UITheme.Amber
                    : UITheme.Dim;
                m_Manager?.UI?.ShowEventSubtitle(definition.subtitle, color, 5f);
            }
        }

        private List<Light> LocalLights()
        {
            var sector = m_Manager != null && m_Manager.Sectors != null ? m_Manager.Sectors.Current : null;
            if (sector != null && sector.Lights.Count > 0) return sector.Lights;
            return m_World != null ? m_World.CeilingLights : new List<Light>();
        }

        private Vector3 BehindPlayer(float distance)
        {
            var controller = m_Manager != null ? m_Manager.LocalPlayerController : null;
            if (controller == null) return Vector3.zero;
            return controller.transform.position - controller.transform.forward * distance + Vector3.up * 1.4f;
        }

        private void PlayScare(AudioClip clip, float intensity, Vector3 position)
        {
            var volume = Mathf.Lerp(0.35f, 0.9f, intensity);
            if (position != Vector3.zero) m_Manager?.Audio?.PlayWorldOneShot(clip, position, volume, AudioCategory.Scare);
            else m_Manager?.Audio?.PlayScare(clip, volume);
        }

        private void RingNearestPhone(float volume)
        {
            if (m_World == null) return;
            Telephone.Telephone nearest = null;
            var best = float.MaxValue;
            foreach (var phone in m_World.Phones.Values)
            {
                if (phone == null || phone.State != Telephone.TelephoneState.Idle) continue;
                var distance = (phone.transform.position - PlayerPosition).sqrMagnitude;
                if (distance < best)
                {
                    best = distance;
                    nearest = phone;
                }
            }

            nearest?.RingOnce(ReduceScare ? volume * 0.6f : volume);
        }

        private IEnumerator Flicker(float intensity)
        {
            var lights = LocalLights();
            if (lights.Count == 0) yield break;
            var light = lights[Random.Range(0, lights.Count)];
            if (light == null) yield break;
            var original = light.intensity;
            var flashes = ReduceFlashes ? 1 : Mathf.RoundToInt(Mathf.Lerp(2f, 5f, intensity));
            var dip = ReduceFlashes ? 0.55f : 0.1f;
            for (var i = 0; i < flashes; i++)
            {
                light.intensity = original * dip;
                yield return new WaitForSeconds(ReduceFlashes ? 0.35f : Random.Range(0.04f, 0.12f));
                light.intensity = original;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.18f));
            }

            light.intensity = original;
        }

        private IEnumerator DistantSteps(float intensity)
        {
            var origin = PlayerPosition + Vector3.forward * 7f;
            var count = Mathf.RoundToInt(Mathf.Lerp(3f, 6f, intensity));
            for (var i = 0; i < count; i++)
            {
                m_Manager?.Audio?.PlayWorldOneShot(ProceduralAudio.Footstep(), origin + Vector3.right * (i * 0.7f), 0.3f, AudioCategory.Scare);
                yield return new WaitForSeconds(0.42f);
            }
        }

        private IEnumerator StepsMirror(float intensity)
        {
            var controller = m_Manager != null ? m_Manager.LocalPlayerController : null;
            if (controller == null) yield break;
            var end = Time.time + Mathf.Lerp(5f, 9f, intensity);
            var last = controller.transform.position;
            var extraStep = false;
            while (Time.time < end)
            {
                yield return new WaitForSeconds(0.45f);
                var now = controller.transform.position;
                var moving = (now - last).sqrMagnitude > 0.02f;
                last = now;
                if (moving)
                {
                    extraStep = true;
                    m_Manager?.Audio?.PlayWorldOneShot(ProceduralAudio.Footstep(), BehindPlayer(5f), 0.28f, AudioCategory.Scare);
                }
                else if (extraStep)
                {
                    // Um passo a mais depois que o jogador parou.
                    extraStep = false;
                    m_Manager?.Audio?.PlayWorldOneShot(ProceduralAudio.Footstep(), BehindPlayer(4f), 0.32f, AudioCategory.Scare);
                }
            }
        }

        /// <summary>Três batidas para uns, quatro para outros (índice do jogador).</summary>
        private IEnumerator Knocks(float intensity)
        {
            var index = m_Manager != null ? m_Manager.LocalPlayerIndex : 0;
            var count = index % 2 == 0 ? 3 : 4;
            m_Manager?.UI?.ShowEventSubtitle(count == 3 ? "Três batidas atrás da parede." : "Quatro batidas atrás da parede.", UITheme.Amber, 4f);
            var position = BehindPlayer(2.5f);
            for (var i = 0; i < count; i++)
            {
                m_Manager?.Audio?.PlayWorldOneShot(ProceduralAudio.Impact(), position, Mathf.Lerp(0.3f, 0.6f, intensity), AudioCategory.Scare);
                yield return new WaitForSeconds(0.7f);
            }
        }

        private IEnumerator LightSequence(float intensity)
        {
            var lights = LocalLights();
            if (lights.Count == 0) yield break;
            var states = new float[lights.Count];
            for (var i = 0; i < lights.Count; i++)
            {
                var light = lights[i];
                if (light == null) continue;
                states[i] = light.intensity;
                light.intensity = ReduceFlashes ? states[i] * 0.4f : 0.05f;
                m_Manager?.Audio?.PlayWorldOneShot(ProceduralAudio.RelayClick(), light.transform.position, 0.4f, AudioCategory.Scare);
                yield return new WaitForSeconds(ReduceFlashes ? 0.4f : Mathf.Lerp(0.16f, 0.06f, intensity));
            }

            yield return new WaitForSeconds(1.8f);

            for (var i = 0; i < lights.Count; i++)
            {
                var light = lights[i];
                if (light != null) light.intensity = states[i];
            }
        }

        private IEnumerator PhonesTogether(float intensity)
        {
            if (m_World == null) yield break;
            foreach (var phone in m_World.Phones.Values)
            {
                if (phone == null) continue;
                phone.RingOnce(Mathf.Lerp(0.15f, 0.3f, intensity));
                yield return new WaitForSeconds(0.08f);
            }
        }

        private IEnumerator LedFlash()
        {
            var board = m_World != null ? m_World.Switchboard : null;
            if (board == null || board.Sockets.Count == 0) yield break;
            var socket = board.Sockets[Random.Range(0, board.Sockets.Count)];
            if (socket == null || socket.Connected || socket.Compromised) yield break;
            socket.SetSelected(true);
            yield return new WaitForSeconds(2.4f);
            if (socket != null) socket.SetSelected(false);
        }

        /// <summary>Chamada particular: só um jogador ouve (em rede, o de índice determinado pelo host).</summary>
        private int m_PrivateCalls;

        private void PrivateCall()
        {
            // Todos os peers executam a mesma sequência de eventos: o contador é igual em todos.
            var index = m_Manager != null ? m_Manager.LocalPlayerIndex : 0;
            var players = m_Manager != null ? m_Manager.PlayerCount : 1;
            var chosen = Mathf.Abs((m_Manager != null ? m_Manager.Seed : 0) + m_PrivateCalls++) % Mathf.Max(1, players);
            if (index != chosen) return;
            m_Manager?.Audio?.PlayScare(ProceduralAudio.Ring(), ReduceScare ? 0.3f : 0.55f);
            m_Manager?.UI?.ShowEventSubtitle("Seu receptor toca uma vez. Só você ouviu. A tela marca: EXTENSÃO 0.", UITheme.Red, 5f);
        }
    }
}
