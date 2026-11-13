using System;
using System.Collections;
using System.IO;
using ExtensaoZero.AI;
using ExtensaoZero.Core;
using ExtensaoZero.Interaction;
using ExtensaoZero.Player;
using ExtensaoZero.Telephone;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace ExtensaoZero.Tests
{
    /// <summary>
    /// Teste de fumaça da fatia jogável em PlayMode. Executa o fluxo real da missão
    /// usando os mesmos métodos chamados pelos interactables, sem injetar entrada
    /// artificial no jogo. Também tenta salvar capturas de tela para inspeção visual.
    /// </summary>
    public sealed class SoloMissionPlayModeTests
    {
        private const string ShotDirectory = "/tmp/opencode/shots";

        private bool m_HadSave;
        private byte[] m_SaveBackup;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Directory.CreateDirectory(ShotDirectory);
            var savePath = SaveSystem.SavePath;
            m_HadSave = File.Exists(savePath);
            m_SaveBackup = m_HadSave ? File.ReadAllBytes(savePath) : null;

            yield return WaitFor(
                () => GameManager.Instance != null && GameManager.Instance.State == GameState.MainMenu,
                90f,
                "inicialização do mundo");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var savePath = SaveSystem.SavePath;
            try
            {
                if (m_HadSave && m_SaveBackup != null)
                {
                    File.WriteAllBytes(savePath, m_SaveBackup);
                }
                else if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[EZ-TEST] Não foi possível restaurar o save: {e.Message}");
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SoloMission_CanBeCompletedEndToEnd()
        {
            var manager = GameManager.Instance;
            Assert.NotNull(manager, "GameManager não inicializou.");
            var world = manager.World;
            Assert.NotNull(world, "WorldContext não foi construído.");
            Assert.NotNull(world.OfficePhone, "Telefone da central ausente.");
            Assert.NotNull(world.Switchboard, "Central telefônica ausente.");
            Assert.AreEqual(22, world.SwitchboardSockets.Count, "A central deve ter 22 soquetes (200–219, 0 e 9).");
            Assert.NotNull(world.Switchboard.GetSocket(214), "Soquete 214 ausente.");
            Assert.NotNull(world.Switchboard.GetSocket(0), "Soquete 0 ausente.");
            Assert.NotNull(world.Switchboard.GetSocket(9), "Soquete 9 ausente.");

            // Estado local limpo para que as asserções de evidência sejam determinísticas.
            manager.Notebook.Model.Clear();
            manager.Save.evidenceIds.Clear();
            manager.Save.missionCompleted = false;

            yield return Capture("01-menu");

            manager.StartSolo();
            Assert.AreEqual(GameState.Playing, manager.State, "Iniciar sozinho deve entrar em Playing.");
            Assert.IsTrue(world.Player.activeSelf, "O jogador deve ficar ativo no modo individual.");
            yield return Capture("02-inicio");

            yield return WaitFor(
                () => world.OfficePhone.State == TelephoneState.Ringing,
                8f,
                "primeira chamada tocar");
            Assert.AreEqual(0, manager.Mission.StepIndex, "A missão começa no passo 0.");

            world.OfficePhone.Answer();
            Assert.AreEqual(1, manager.Mission.StepIndex, "Atender a primeira chamada deve avançar para o passo 1.");
            Assert.IsTrue(manager.CallSession.IsActive, "A sessão de chamada deve estar ativa.");
            Assert.GreaterOrEqual(manager.Notebook.Count, 4, "Atender a chamada deve registrar as pistas das quatro camadas.");

            var equipment = world.Player.GetComponent<PlayerEquipment>();
            var interactor = world.Player.GetComponent<PlayerInteractor>();
            Assert.NotNull(equipment, "PlayerEquipment ausente no jogador.");
            Assert.NotNull(interactor, "PlayerInteractor ausente no jogador.");

            var layerBefore = manager.CallSession.CurrentLayer;
            equipment.CycleLayer();
            Assert.AreNotEqual(layerBefore, manager.CallSession.CurrentLayer, "O sintonizador deve alternar a camada ativa.");
            Assert.AreEqual(manager.CallSession.CurrentLayer, equipment.Tuner.CurrentLayer, "O HUD do sintonizador deve acompanhar a sessão.");

            world.OfficePhone.HangUp();
            yield return null;
            Assert.AreEqual(2, manager.Mission.StepIndex, "Sem o cabo, a missão deve pedir o cabo 214.");

            manager.Pause();
            Assert.AreEqual(GameState.Paused, manager.State, "Esc deve pausar.");
            manager.Resume();
            Assert.AreEqual(GameState.Playing, manager.State, "Continuar deve retomar o jogo.");

            manager.OpenNotebook();
            Assert.AreEqual(GameState.Notebook, manager.State, "Tab deve abrir o caderno.");
            yield return Capture("03-caderno");
            manager.CloseNotebook();
            Assert.AreEqual(GameState.Playing, manager.State, "Fechar o caderno deve retomar o jogo.");

            var pickup = UnityEngine.Object.FindFirstObjectByType<CablePickup>();
            Assert.NotNull(pickup, "Cabo 214 não encontrado no arquivo de vozes.");
            pickup.Interact(interactor);
            Assert.IsTrue(equipment.HasCable, "O jogador deve carregar o cabo após recolhê-lo.");
            Assert.AreEqual(3, manager.Mission.StepIndex, "Recolher o cabo deve avançar para o passo 3.");

            var board = world.Switchboard;

            // Conexão errada com a linha 9: contaminação alta e Linha Ocupada recuperável.
            Assert.IsTrue(board.Connect(214, 9, equipment), "A central deve aceitar fisicamente a conexão 214→9.");
            Assert.GreaterOrEqual(manager.Contamination.Value, 55f, "A linha 9 deve elevar a contaminação acima do limiar.");
            Assert.IsTrue(manager.Contamination.IsLineBusy, "A contaminação deve iniciar a Linha Ocupada.");
            Assert.AreEqual(3, manager.Mission.StepIndex, "Uma conexão errada não deve avançar a missão.");

            var tunerBefore = equipment.Tuner.CurrentLayer;
            equipment.CycleLayer();
            Assert.AreEqual(tunerBefore, equipment.Tuner.CurrentLayer, "O sintonizador deve ficar bloqueado durante a Linha Ocupada.");
            yield return Capture("04-linha-ocupada");

            manager.Contamination.TryRecoverByFrequency();
            Assert.IsFalse(manager.Contamination.IsLineBusy, "A frequência de retorno deve encerrar a Linha Ocupada.");
            Assert.Less(manager.Contamination.Value, 55f, "A recuperação deve reduzir a contaminação.");

            // Conexão correta 214→0 e segunda chamada.
            Assert.IsTrue(board.Connect(214, 0, equipment), "A central deve aceitar a conexão 214→0.");
            Assert.AreEqual(4, manager.Mission.StepIndex, "A conexão correta deve avançar para o passo 4.");

            yield return WaitFor(
                () => world.OfficePhone.State == TelephoneState.Ringing,
                10f,
                "segunda chamada tocar");
            world.OfficePhone.Answer();
            Assert.AreEqual(5, manager.Mission.StepIndex, "Atender a segunda chamada deve avançar para o passo 5.");
            world.OfficePhone.HangUp();
            yield return null;

            // Gatilho físico do corredor: teleporta o CharacterController para dentro do volume.
            var corridorTrigger = UnityEngine.Object.FindFirstObjectByType<CorridorTrigger>();
            Assert.NotNull(corridorTrigger, "Gatilho do corredor não encontrado.");
            var controller = manager.LocalPlayerController;
            Assert.NotNull(controller, "Controlador local ausente.");
            controller.Teleport(new Vector3(0f, 0.12f, 6.8f), Quaternion.identity);
            controller.Character.Move(new Vector3(0f, 0f, 0.15f));

            yield return WaitFor(
                () => manager.Mission.StepIndex >= 6,
                8f,
                "manifestação da Assinante no corredor");
            Assert.IsTrue(manager.Notebook.Contains("silhouette"), "A manifestação deve registrar a silhueta no caderno.");
            Assert.AreEqual(SubscriberState.Manifesting, manager.Subscriber.State, "A Assinante deve estar manifestando.");
            yield return Capture("05-corredor");

            // Isolar a linha conclui a missão.
            var lever = UnityEngine.Object.FindFirstObjectByType<IsolatorLever>();
            Assert.NotNull(lever, "Alavanca de isolamento não encontrada.");
            lever.Interact(interactor);
            Assert.IsTrue(manager.Mission.Completed, "Puxar a alavanca deve concluir a missão.");
            Assert.AreEqual(7, manager.Mission.StepIndex, "A missão deve terminar no passo 7.");
            Assert.IsFalse(string.IsNullOrEmpty(manager.Mission.Summary), "A missão deve produzir um resumo final.");

            yield return WaitFor(
                () => manager.State == GameState.MissionComplete,
                8f,
                "tela de conclusão");
            Assert.IsTrue(manager.Save.missionCompleted, "O save deve registrar a missão concluída.");
            Assert.IsTrue(File.Exists(SaveSystem.SavePath), "O save local deve existir no disco.");
            Assert.IsTrue(SaveSystem.Load().missionCompleted, "O save recarregado deve manter a conclusão.");

            yield return Capture("06-conclusao");
            Debug.Log("[EZ-TEST] Fluxo individual completo validado em PlayMode.");
        }

        private static IEnumerator WaitFor(Func<bool> condition, float timeoutSeconds, string what)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    Assert.Fail($"Tempo esgotado ({timeoutSeconds:0}s) esperando por: {what}.");
                }

                yield return null;
            }
        }

        /// <summary>
        /// Renderiza a câmera principal para uma RenderTexture e grava PNG. Funciona em
        /// batchmode, onde ScreenCapture.CaptureScreenshot não tem backbuffer de janela.
        /// Captura apenas o mundo 3D; a UI overlay é validada pelas asserções de estado.
        /// </summary>
        private static IEnumerator Capture(string fileName)
        {
            yield return null;

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Debug.Log($"[EZ-TEST] Sem dispositivo gráfico; captura '{fileName}' ignorada.");
                yield break;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogWarning($"[EZ-TEST] Câmera principal ausente; captura '{fileName}' ignorada.");
                yield break;
            }

            Debug.Log($"[EZ-TEST] Captura '{fileName}': câmera em {camera.transform.position}, " +
                      $"fov={camera.fieldOfView:0.#}, aspect={camera.aspect:0.###}, " +
                      $"tela={Screen.width}x{Screen.height}, alvo={camera.targetTexture != null}.");

            const int width = 1280;
            const int height = 720;
            var path = Path.Combine(ShotDirectory, fileName + ".png");
            var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var previousTarget = camera.targetTexture;
            var previousAspect = camera.aspect;
            var previousActive = RenderTexture.active;

            try
            {
                camera.targetTexture = target;
                // Em batchmode a tela é 640x480; força o aspecto do alvo para não esticar a imagem.
                camera.aspect = (float)width / height;
                camera.Render();
                RenderTexture.active = target;

                var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.Destroy(texture);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                camera.aspect = previousAspect;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(target);
            }

            if (File.Exists(path))
            {
                Debug.Log($"[EZ-TEST] Captura salva: {path} ({new FileInfo(path).Length} bytes).");
            }
            else
            {
                Debug.LogWarning($"[EZ-TEST] Falha ao salvar captura: {path}.");
            }

            yield return null;
        }
    }
}
