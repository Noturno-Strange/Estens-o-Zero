using ExtensaoZero.Core;
using ExtensaoZero.Interaction;
using ExtensaoZero.Investigation;
using ExtensaoZero.Player;
using ExtensaoZero.Telephone;
using UnityEngine;
using Phone = ExtensaoZero.Telephone.Telephone;

namespace ExtensaoZero.Environment
{
    /// <summary>
    /// Constrói a instalação inteira em runtime: sala da central, arquivo de vozes e
    /// corredor. Geometria determinística, sem prefabs de cena frágeis. A mesma ordem de
    /// construção gera os mesmos IDs de interação em todos os peers.
    /// </summary>
    public static class WorldBuilder
    {
        private const float WallHeight = 3.4f;
        private const float CorridorHeight = 3.0f;

        public static WorldContext Build(GameManager manager)
        {
            InteractionRegistry.Clear();
            var context = new WorldContext
            {
                Seed = 1337,
                AmbientZone = "sala_central",
                PlayerSpawn = new Vector3(0f, 0.12f, -4.2f),
                PlayerSpawnRotation = Quaternion.identity
            };

            var root = new GameObject("World");
            context.Root = root;

            BuildCentralRoom(root.transform);
            BuildArchive(root.transform);
            BuildCorridor(root.transform, context);
            BuildSwitchboard(root.transform, context);
            BuildDesk(root.transform, context);
            BuildArchiveProps(root.transform, context);
            BuildCorridorProps(root.transform, context);
            BuildPlayer(root.transform, context);
            LightingRig.Build(root.transform, context);
            Debug.Log($"[EZ] Mundo construído: {context.SwitchboardSockets.Count} soquetes, " +
                      $"{context.CeilingLights.Count} luzes, player em {context.PlayerSpawn}.");
            return context;
        }

        private static void BuildCentralRoom(Transform root)
        {
            var floor = PropFactory.CreateBox(root, "Piso_Central", new Vector3(0f, -0.06f, 0f),
                new Vector3(14f, 0.12f, 12f), MaterialLibrary.Floor);
            floor.isStatic = true;

            var ceiling = PropFactory.CreateBox(root, "Teto_Central", new Vector3(0f, WallHeight + 0.06f, 0f),
                new Vector3(14f, 0.12f, 12f), MaterialLibrary.Ceiling);
            ceiling.isStatic = true;

            Wall(root, "Parede_Sul", new Vector3(0f, WallHeight * 0.5f, -6f), new Vector3(14f, WallHeight, 0.2f));
            Wall(root, "Parede_Leste", new Vector3(7f, WallHeight * 0.5f, 0f), new Vector3(0.2f, WallHeight, 12f));
            Wall(root, "Parede_Norte_Esq", new Vector3(-4.1f, WallHeight * 0.5f, 6f), new Vector3(5.8f, WallHeight, 0.2f));
            Wall(root, "Parede_Norte_Dir", new Vector3(4.1f, WallHeight * 0.5f, 6f), new Vector3(5.8f, WallHeight, 0.2f));
            Wall(root, "Parede_Oeste_Norte", new Vector3(-7f, WallHeight * 0.5f, 3.5f), new Vector3(0.2f, WallHeight, 5f));
            Wall(root, "Parede_Oeste_Sul", new Vector3(-7f, WallHeight * 0.5f, -3.5f), new Vector3(0.2f, WallHeight, 5f));

            // Pequenos fechamentos entre o vão do corredor e a parede norte.
            Wall(root, "Parede_Norte_Fecho_Esq", new Vector3(-1.4f, WallHeight * 0.5f, 6f), new Vector3(0.4f, WallHeight, 0.2f));
            Wall(root, "Parede_Norte_Fecho_Dir", new Vector3(1.4f, WallHeight * 0.5f, 6f), new Vector3(0.4f, WallHeight, 0.2f));

            // Rodapés e vigas dão leitura de escala sem custo relevante.
            PropFactory.CreateBox(root, "Rodape_Sul", new Vector3(0f, 0.12f, -5.86f), new Vector3(14f, 0.24f, 0.08f), MaterialLibrary.MetalDark);
            PropFactory.CreateBox(root, "Viga_Central", new Vector3(0f, 3.1f, 0f), new Vector3(14f, 0.18f, 0.22f), MaterialLibrary.MetalDark);
            PropFactory.CreateBox(root, "Viga_Central_2", new Vector3(0f, 3.1f, -3f), new Vector3(14f, 0.18f, 0.22f), MaterialLibrary.MetalDark);
        }

        private static void BuildArchive(Transform root)
        {
            var floor = PropFactory.CreateBox(root, "Piso_Arquivo", new Vector3(-11f, -0.06f, 0f),
                new Vector3(8f, 0.12f, 6f), MaterialLibrary.Floor);
            floor.isStatic = true;

            var ceiling = PropFactory.CreateBox(root, "Teto_Arquivo", new Vector3(-11f, 3.06f, 0f),
                new Vector3(8f, 0.12f, 6f), MaterialLibrary.Ceiling);
            ceiling.isStatic = true;

            Wall(root, "Arquivo_Sul", new Vector3(-11f, 1.5f, -3f), new Vector3(8f, 3f, 0.2f));
            Wall(root, "Arquivo_Norte", new Vector3(-11f, 1.5f, 3f), new Vector3(8f, 3f, 0.2f));
            Wall(root, "Arquivo_Oeste", new Vector3(-15f, 1.5f, 0f), new Vector3(0.2f, 3f, 6f));

            // Porta do arquivo, com dobradiça visual simples.
            var door = PropFactory.CreateBox(root, "Porta_Arquivo", new Vector3(-7f, 1.05f, 0f),
                new Vector3(0.14f, 2.1f, 1.8f), MaterialLibrary.WoodDark);
            var doorComponent = door.AddComponent<DoorInteractable>();
            doorComponent.openAngle = 92f;
            PropFactory.CreateSphere(root, "Puxador", new Vector3(-7f, 1.05f, -0.82f), 0.09f, MaterialLibrary.Metal);
        }

        private static void BuildCorridor(Transform root, WorldContext context)
        {
            var floor = PropFactory.CreateBox(root, "Piso_Corredor", new Vector3(0f, -0.06f, 12.5f),
                new Vector3(3.2f, 0.12f, 13f), MaterialLibrary.Floor);
            floor.isStatic = true;

            var ceiling = PropFactory.CreateBox(root, "Teto_Corredor", new Vector3(0f, CorridorHeight + 0.06f, 12.5f),
                new Vector3(3.2f, 0.12f, 13f), MaterialLibrary.Ceiling);
            ceiling.isStatic = true;

            Wall(root, "Corredor_Oeste", new Vector3(-1.6f, CorridorHeight * 0.5f, 12.5f), new Vector3(0.2f, CorridorHeight, 13f));
            Wall(root, "Corredor_Leste", new Vector3(1.6f, CorridorHeight * 0.5f, 12.5f), new Vector3(0.2f, CorridorHeight, 13f));
            Wall(root, "Corredor_Fundo", new Vector3(0f, CorridorHeight * 0.5f, 19f), new Vector3(3.2f, CorridorHeight, 0.2f));

            var door = PropFactory.CreateBox(root, "Porta_Corredor", new Vector3(0f, 1.05f, 6f),
                new Vector3(1.8f, 2.1f, 0.14f), MaterialLibrary.WoodDark);
            door.AddComponent<DoorInteractable>();

            // Canos no teto: leitura industrial barata.
            PropFactory.CreateCylinder(root, "Cano_A", new Vector3(-1.1f, 2.72f, 12.5f), new Vector3(0.08f, 6.5f, 0.08f), MaterialLibrary.MetalDark, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            PropFactory.CreateCylinder(root, "Cano_B", new Vector3(-0.92f, 2.78f, 12.5f), new Vector3(0.05f, 6.5f, 0.05f), MaterialLibrary.Metal, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var trigger = PropFactory.CreateTrigger(root, "Gatilho_Corredor", new Vector3(0f, 1.2f, 6.8f), new Vector3(2.8f, 2.4f, 0.6f));
            trigger.AddComponent<CorridorTrigger>();

            var anchor = new GameObject("Ancora_Corredor");
            anchor.transform.SetParent(root, false);
            anchor.transform.localPosition = new Vector3(0f, 0f, 10.5f);
            context.CorridorAnchor = anchor.transform;

            var subscriberAnchor = new GameObject("Ancora_Assinante");
            subscriberAnchor.transform.SetParent(root, false);
            subscriberAnchor.transform.localPosition = new Vector3(0f, 0f, 17.9f);
            subscriberAnchor.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            context.SubscriberAnchor = subscriberAnchor.transform;
        }

        private static void BuildSwitchboard(Transform root, WorldContext context)
        {
            // Âncora sem escala: os soquetes e etiquetas são filhos e não devem herdar
            // a escala não uniforme do painel visual.
            var panelAnchor = CreateAnchor(root.transform, "Central_Telefonica", new Vector3(-2.7f, 1.95f, 5.8f));
            PropFactory.CreateBox(panelAnchor, "Painel", Vector3.zero,
                new Vector3(7.4f, 2.55f, 0.26f), MaterialLibrary.MetalDark);
            var board = panelAnchor.gameObject.AddComponent<Switchboard>();
            context.Switchboard = board;

            var lineGo = new GameObject("Cabo_Conexao");
            lineGo.transform.SetParent(panelAnchor, false);
            var line = lineGo.AddComponent<LineRenderer>();
            line.material = MaterialLibrary.Cable;
            line.widthMultiplier = 0.035f;
            line.useWorldSpace = true;
            line.positionCount = 0;
            line.enabled = false;
            var visual = lineGo.AddComponent<CableVisual>();
            visual.line = line;
            board.AttachVisual(visual);

            var extensions = new int[22];
            for (var i = 0; i < 11; i++) extensions[i] = 200 + i;
            for (var i = 0; i < 9; i++) extensions[11 + i] = 211 + i;
            extensions[20] = 0;
            extensions[21] = 9;

            for (var i = 0; i < extensions.Length; i++)
            {
                var row = i / 11;
                var col = i % 11;
                var x = -3.15f + col * 0.62f;
                var y = 0.52f - row * 0.62f;
                var ext = extensions[i];

                var socketGo = PropFactory.CreateBox(panelAnchor, $"Soquete_{ext:000}",
                    new Vector3(x, y, -0.17f), new Vector3(0.2f, 0.2f, 0.12f), MaterialLibrary.Metal);
                var socket = socketGo.AddComponent<SwitchboardSocket>();
                socket.extension = ext;
                socket.board = board;
                board.RegisterSocket(socket);

                var indicator = PropFactory.CreateSphere(panelAnchor, $"Led_{ext:000}",
                    new Vector3(x, y + 0.17f, -0.2f), 0.065f, MaterialLibrary.IndicatorIdle);
                socket.indicator = indicator.GetComponent<Renderer>();
                socket.SetConnected(false);

                PropFactory.CreateLabel(panelAnchor, ext == 0 ? "TRONCO 0" : ext == 9 ? "LINHA 9" : ext.ToString("000"),
                    new Vector3(x, y - 0.2f, -0.2f), Quaternion.identity, 0.035f,
                    ext == 9 ? new Color(0.9f, 0.45f, 0.35f) : ext == 0 ? new Color(0.95f, 0.78f, 0.4f) : new Color(0.72f, 0.7f, 0.62f));

                context.SwitchboardSockets.Add(socket);
            }

            PropFactory.CreateLabel(root, "TRONCO 0 — LINHA MORTA", new Vector3(-6.0f, 2.95f, 5.62f),
                Quaternion.identity, 0.055f, new Color(0.9f, 0.78f, 0.45f));
            PropFactory.CreateLabel(root, "CENTRAL 214", new Vector3(-2.7f, 3.15f, 5.62f),
                Quaternion.identity, 0.075f, new Color(0.82f, 0.78f, 0.62f));
            PropFactory.CreateLabel(root, "NÃO CONECTE O 9", new Vector3(1.35f, 2.95f, 5.62f),
                Quaternion.identity, 0.045f, new Color(0.82f, 0.36f, 0.28f));

            // Placa examinável: pista redundante.
            var plate = PropFactory.CreateBox(root, "Placa_Tronco_Zero", new Vector3(-5.98f, 1.15f, 5.66f),
                new Vector3(0.62f, 0.22f, 0.04f), MaterialLibrary.Paper);
            var examine = plate.AddComponent<ExamineInteractable>();
            examine.title = "PLACA DO TRONCO 0";
            examine.body = "TRONCO 0 — LINHA MORTA. Fora da rede ativa. Use apenas para reconexões de emergência.";
            examine.evidenceId = "plate_trunk0";
            examine.category = EvidenceCategory.Document;

            var leverAnchor = CreateAnchor(root.transform, "Alavanca_Isolamento", new Vector3(-6.45f, 1.45f, 5.66f));
            PropFactory.CreateBox(leverAnchor, "Base", Vector3.zero,
                new Vector3(0.32f, 0.62f, 0.18f), MaterialLibrary.MetalDark);
            var handle = PropFactory.CreateBox(leverAnchor, "Manopla", new Vector3(0f, 0.05f, -0.14f),
                new Vector3(0.08f, 0.5f, 0.08f), MaterialLibrary.Emergency);
            var lever = leverAnchor.gameObject.AddComponent<IsolatorLever>();
            lever.handle = handle.transform;
            context.IsolatorAnchor = leverAnchor;
            PropFactory.CreateLabel(root, "ISOLAMENTO", new Vector3(-6.45f, 1.92f, 5.58f),
                Quaternion.identity, 0.04f, new Color(0.9f, 0.5f, 0.38f));
        }

        private static void BuildDesk(Transform root, WorldContext context)
        {
            PropFactory.CreateBox(root, "Mesa_Central", new Vector3(3.35f, 0.44f, 4.65f),
                new Vector3(2.3f, 0.88f, 1.25f), MaterialLibrary.Wood);
            PropFactory.CreateBox(root, "Mesa_Pe", new Vector3(3.35f, 0.04f, 4.65f),
                new Vector3(2.2f, 0.08f, 1.15f), MaterialLibrary.WoodDark);

            var phone = PropFactory.CreateBox(root, "Telefone_Central", new Vector3(3.35f, 0.96f, 4.55f),
                new Vector3(0.58f, 0.14f, 0.34f), MaterialLibrary.Metal);
            var handset = PropFactory.CreateBox(root, "Fone", new Vector3(3.35f, 1.08f, 4.55f),
                new Vector3(0.5f, 0.08f, 0.12f), MaterialLibrary.MetalDark);
            handset.transform.localRotation = Quaternion.Euler(0f, 0f, 3f);
            var telephone = phone.AddComponent<Phone>();
            telephone.extension = 0;
            telephone.isOperatorPhone = true;
            context.OfficePhone = telephone;

            PropFactory.CreateLabel(root, "CENTRAL", new Vector3(3.35f, 1.22f, 4.3f),
                Quaternion.identity, 0.045f, new Color(0.85f, 0.75f, 0.55f));

            var note = PropFactory.CreateBox(root, "Anotacao_Linha_9", new Vector3(2.35f, 0.9f, 4.35f),
                new Vector3(0.3f, 0.015f, 0.22f), MaterialLibrary.Paper);
            note.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);
            var examine = note.AddComponent<ExamineInteractable>();
            examine.title = "ANOTAÇÃO";
            examine.body = "\"Não conecte o 9.\" A tinta é recente. A letra não é de ninguém da equipe.";
            examine.evidenceId = "note_line9";
            examine.category = EvidenceCategory.Document;

            // Cadeira e armário dão silhueta à sala sem custo alto.
            PropFactory.CreateBox(root, "Cadeira_Assento", new Vector3(2.2f, 0.48f, 3.3f), new Vector3(0.55f, 0.08f, 0.55f), MaterialLibrary.WoodDark);
            PropFactory.CreateBox(root, "Cadeira_Encosto", new Vector3(2.2f, 0.9f, 3.05f), new Vector3(0.55f, 0.8f, 0.08f), MaterialLibrary.WoodDark);
            PropFactory.CreateBox(root, "Armario_Central", new Vector3(6.2f, 1.1f, 4.2f), new Vector3(1.2f, 2.2f, 0.7f), MaterialLibrary.MetalDark);
        }

        private static void BuildArchiveProps(Transform root, WorldContext context)
        {
            PropFactory.CreateBox(root, "Estante_1", new Vector3(-13.8f, 1.25f, 0.2f), new Vector3(0.8f, 2.5f, 4.4f), MaterialLibrary.WoodDark);
            for (var i = 0; i < 4; i++)
            {
                PropFactory.CreateBox(root, "Prateleira_" + i, new Vector3(-13.35f, 0.45f + i * 0.55f, 0.2f),
                    new Vector3(0.55f, 0.05f, 4.2f), MaterialLibrary.Wood);
            }

            for (var i = 0; i < 7; i++)
            {
                var x = -13.1f;
                var z = -1.6f + i * 0.55f;
                PropFactory.CreateBox(root, "Caixa_Arquivo_" + i, new Vector3(x, 0.62f + (i % 3) * 0.55f, z),
                    new Vector3(0.42f, 0.36f, 0.42f), MaterialLibrary.Paper);
            }

            var cabinetAnchor = CreateAnchor(root.transform, "Armario_Cabos", new Vector3(-9.2f, 0.72f, 2.45f));
            PropFactory.CreateBox(cabinetAnchor, "Armario", Vector3.zero,
                new Vector3(1.8f, 1.45f, 0.7f), MaterialLibrary.WoodDark);
            for (var i = 0; i < 3; i++)
            {
                PropFactory.CreateBox(cabinetAnchor, "Gaveta_" + i, new Vector3(0f, 0.4f - i * 0.38f, -0.38f),
                    new Vector3(1.55f, 0.3f, 0.04f), MaterialLibrary.Wood);
                PropFactory.CreateBox(cabinetAnchor, "Puxador_Gaveta_" + i, new Vector3(0f, 0.4f - i * 0.38f, -0.42f),
                    new Vector3(0.22f, 0.035f, 0.03f), MaterialLibrary.Metal);
            }

            var spool = PropFactory.CreateCylinder(root, "Cabo_214", new Vector3(-9.2f, 1.08f, 1.92f),
                new Vector3(0.2f, 0.2f, 0.2f), MaterialLibrary.Cable);
            spool.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var pickup = spool.AddComponent<CablePickup>();
            pickup.cableLabel = "CABO 214";
            pickup.cableLength = 4;
            context.CableSpawn = spool.transform;

            PropFactory.CreateLabel(cabinetAnchor, "CABOS / 214", new Vector3(0f, 0.5f, -0.4f),
                Quaternion.identity, 0.032f, new Color(0.85f, 0.75f, 0.55f));

            var register = PropFactory.CreateBox(root, "Registro_1987", new Vector3(-11.8f, 0.9f, -1.2f),
                new Vector3(0.42f, 0.05f, 0.3f), MaterialLibrary.Paper);
            register.transform.localRotation = Quaternion.Euler(0f, -22f, 0f);
            var examine = register.AddComponent<ExamineInteractable>();
            examine.title = "REGISTRO DE 1987";
            examine.body = "A extensão 214 foi removida do painel após o incêndio. O tronco zero permaneceu ativo. Nenhum técnico assinou a remoção.";
            examine.evidenceId = "call_214_origin";
            examine.category = EvidenceCategory.Document;

            PropFactory.CreateBox(root, "Mesa_Arquivo", new Vector3(-11.5f, 0.4f, 0.2f), new Vector3(1.8f, 0.8f, 0.9f), MaterialLibrary.Wood);
        }

        private static void BuildCorridorProps(Transform root, WorldContext context)
        {
            var station = PropFactory.CreateBox(root, "Estacao_Bateria", new Vector3(1.42f, 1.2f, 11.2f),
                new Vector3(0.3f, 0.62f, 0.42f), MaterialLibrary.Metal);
            station.AddComponent<BatteryStation>();
            PropFactory.CreateLabel(root, "ENERGIA", new Vector3(1.25f, 1.58f, 11.2f),
                Quaternion.Euler(0f, 90f, 0f), 0.035f, new Color(0.75f, 0.85f, 0.7f));

            for (var i = 0; i < 4; i++)
            {
                var z = 8f + i * 2.6f;
                PropFactory.CreateBox(root, "Caixa_Corredor_" + i, new Vector3(-1.25f, 0.28f, z),
                    new Vector3(0.45f, 0.56f, 0.5f), MaterialLibrary.WoodDark);
            }

            PropFactory.CreateBox(root, "Porta_Lateral", new Vector3(1.5f, 1.05f, 14.4f),
                new Vector3(0.12f, 2.1f, 0.95f), MaterialLibrary.MetalDark);
            PropFactory.CreateLabel(root, "ARQUIVO MORTO", new Vector3(1.42f, 2.05f, 14.4f),
                Quaternion.Euler(0f, 90f, 0f), 0.03f, new Color(0.65f, 0.62f, 0.55f));
        }

        private static void BuildPlayer(Transform root, WorldContext context)
        {
            var player = new GameObject("Player");
            player.transform.SetParent(root, false);
            player.transform.localPosition = context.PlayerSpawn;
            player.transform.localRotation = context.PlayerSpawnRotation;

            player.AddComponent<CharacterController>();
            var controller = player.AddComponent<FirstPersonController>();
            controller.fieldOfView = 72f;
            controller.mouseSensitivity = 1.4f;

            var equipment = PlayerEquipment.Attach(controller);
            var interactor = player.AddComponent<PlayerInteractor>();
            interactor.Initialize(controller.Camera, equipment);

            context.Player = player;
            context.PlayerController = controller;
            context.SetPlayerActive(true);
        }

        /// <summary>
        /// Contêiner sem escala para agrupar filhos de um prop. Um cubo escalado como pai
        /// distorceria posições, tamanhos e textos dos filhos.
        /// </summary>
        private static Transform CreateAnchor(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go.transform;
        }

        private static void Wall(Transform root, string name, Vector3 position, Vector3 size)
        {
            var wall = PropFactory.CreateBox(root, name, position, size, MaterialLibrary.Wall);
            wall.isStatic = true;
        }
    }
}
