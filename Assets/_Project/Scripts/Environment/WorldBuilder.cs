using ExtensaoZero.Campaign;
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
    /// Constrói a instalação inteira em runtime: recepção, sala da central, arquivo de vozes,
    /// corredor técnico, arquivo morto, acesso ao subsolo, túneis de cabos, sala de energia e
    /// central original. Geometria determinística: a mesma ordem de construção gera os mesmos
    /// IDs de interação em todos os peers. Setores são liberados por capítulo (ADR-010).
    /// </summary>
    public static class WorldBuilder
    {
        private const float WallHeight = 3.4f;
        private const float CorridorHeight = 3.0f;
        private const float TunnelFloor = -3f;
        private const float TunnelHeight = 2.8f;

        public static WorldContext Build(GameManager manager)
        {
            InteractionRegistry.Clear();
            SignalRegistry.Clear();
            var context = new WorldContext
            {
                Seed = 1337,
                AmbientZone = "sala_central",
                PlayerSpawn = new Vector3(0f, 0.12f, -12.5f),
                PlayerSpawnRotation = Quaternion.identity
            };

            var root = new GameObject("World");
            context.Root = root;
            var t = root.transform;

            // A ordem de construção define os IDs de interação: não reordenar sem atualizar a rede.
            BuildCentralRoom(t, context);
            BuildArchive(t, context);
            BuildCorridor(t, context);
            BuildSwitchboard(t, context);
            BuildDesk(t, context);
            BuildArchiveProps(t, context);
            BuildCorridorProps(t, context);
            BuildReception(t, context);
            BuildDeadArchive(t, context);
            BuildStairwell(t, context);
            BuildTunnels(t, context);
            BuildEnergyRoom(t, context);
            BuildOriginalCentral(t, context);
            BuildPlayer(t, context);
            LightingRig.Build(t, context);
            BuildSectors(context);
            BuildReverbZones(t, context);
            CombineStatic(root);
            Debug.Log($"[EZ] Mundo construído: {context.SwitchboardSockets.Count} soquetes, {context.Phones.Count} telefones, " +
                      $"{context.Doors.Count} portas, {context.CeilingLights.Count} luzes, {context.Sectors.Count} setores.");
            return context;
        }

        // ================================================================ térreo

        private static void BuildCentralRoom(Transform root, WorldContext context)
        {
            Slab(root, "Piso_Central", new Vector3(0f, -0.06f, 0f), new Vector3(14f, 0.12f, 12f), MaterialLibrary.Floor);
            Slab(root, "Teto_Central", new Vector3(0f, WallHeight + 0.06f, 0f), new Vector3(14f, 0.12f, 12f), MaterialLibrary.Ceiling);

            // Parede sul com vão para a recepção (x ∈ [-0,9; 0,9]).
            Wall(root, "Parede_Sul_Esq", new Vector3(-3.95f, WallHeight * 0.5f, -6f), new Vector3(6.1f, WallHeight, 0.2f));
            Wall(root, "Parede_Sul_Dir", new Vector3(3.95f, WallHeight * 0.5f, -6f), new Vector3(6.1f, WallHeight, 0.2f));
            Lintel(root, "Verga_Sul", new Vector3(0f, 0f, -6f), 1.8f, WallHeight, true);
            Wall(root, "Parede_Leste", new Vector3(7f, WallHeight * 0.5f, 0f), new Vector3(0.2f, WallHeight, 12f));
            Wall(root, "Parede_Norte_Esq", new Vector3(-4.1f, WallHeight * 0.5f, 6f), new Vector3(5.8f, WallHeight, 0.2f));
            Wall(root, "Parede_Norte_Dir", new Vector3(4.1f, WallHeight * 0.5f, 6f), new Vector3(5.8f, WallHeight, 0.2f));
            Wall(root, "Parede_Oeste_Norte", new Vector3(-7f, WallHeight * 0.5f, 3.5f), new Vector3(0.2f, WallHeight, 5f));
            Wall(root, "Parede_Oeste_Sul", new Vector3(-7f, WallHeight * 0.5f, -3.5f), new Vector3(0.2f, WallHeight, 5f));
            Lintel(root, "Verga_Oeste", new Vector3(-7f, 0f, 0f), 2f, WallHeight, false);

            // Batentes entre o vão do corredor e a porta (o vão tem 2,4 m; a porta, 1,8 m).
            Wall(root, "Batente_Corredor_Esq", new Vector3(-1.05f, WallHeight * 0.5f, 6f), new Vector3(0.3f, WallHeight, 0.2f));
            Wall(root, "Batente_Corredor_Dir", new Vector3(1.05f, WallHeight * 0.5f, 6f), new Vector3(0.3f, WallHeight, 0.2f));
            Lintel(root, "Verga_Norte", new Vector3(0f, 0f, 6f), 1.8f, WallHeight, true);

            PropFactory.CreateBox(root, "Rodape_Sul_Esq", new Vector3(-3.95f, 0.12f, -5.86f), new Vector3(6.1f, 0.24f, 0.08f), MaterialLibrary.MetalDark, false);
            PropFactory.CreateBox(root, "Rodape_Sul_Dir", new Vector3(3.95f, 0.12f, -5.86f), new Vector3(6.1f, 0.24f, 0.08f), MaterialLibrary.MetalDark, false);
            PropFactory.CreateBox(root, "Viga_Central", new Vector3(0f, 3.1f, 0f), new Vector3(14f, 0.18f, 0.22f), MaterialLibrary.MetalDark, false);
            PropFactory.CreateBox(root, "Viga_Central_2", new Vector3(0f, 3.1f, -3f), new Vector3(14f, 0.18f, 0.22f), MaterialLibrary.MetalDark, false);

            Door(root, context, "porta_recepcao", new Vector3(-0.9f, 0f, -6f), 1.8f, true, false, "");
            Anchor(root, context, "central_center", new Vector3(0f, 0f, 0f), 0f);
        }

        private static void BuildArchive(Transform root, WorldContext context)
        {
            Slab(root, "Piso_Arquivo", new Vector3(-11f, -0.06f, 0f), new Vector3(8f, 0.12f, 6f), MaterialLibrary.Floor);
            Slab(root, "Teto_Arquivo", new Vector3(-11f, 3.06f, 0f), new Vector3(8f, 0.12f, 6f), MaterialLibrary.Ceiling);
            Wall(root, "Arquivo_Sul", new Vector3(-11f, 1.5f, -3f), new Vector3(8f, 3f, 0.2f));
            Wall(root, "Arquivo_Norte", new Vector3(-11f, 1.5f, 3f), new Vector3(8f, 3f, 0.2f));
            Wall(root, "Arquivo_Oeste", new Vector3(-15f, 1.5f, 0f), new Vector3(0.2f, 3f, 6f));

            Door(root, context, "porta_arquivo", new Vector3(-7f, 0f, -0.9f), 1.8f, false, false, "");
        }

        private static void BuildCorridor(Transform root, WorldContext context)
        {
            Slab(root, "Piso_Corredor", new Vector3(0f, -0.06f, 12.5f), new Vector3(3.2f, 0.12f, 13f), MaterialLibrary.Floor);
            Slab(root, "Teto_Corredor", new Vector3(0f, CorridorHeight + 0.06f, 12.5f), new Vector3(3.2f, 0.12f, 13f), MaterialLibrary.Ceiling);
            Wall(root, "Corredor_Oeste", new Vector3(-1.6f, CorridorHeight * 0.5f, 12.5f), new Vector3(0.2f, CorridorHeight, 13f));

            // Parede leste com vão para o arquivo morto (z ∈ [13,8; 15,0]).
            Wall(root, "Corredor_Leste_Sul", new Vector3(1.6f, CorridorHeight * 0.5f, 9.9f), new Vector3(0.2f, CorridorHeight, 7.8f));
            Wall(root, "Corredor_Leste_Norte", new Vector3(1.6f, CorridorHeight * 0.5f, 17f), new Vector3(0.2f, CorridorHeight, 4f));
            Lintel(root, "Verga_Arquivo_Morto", new Vector3(1.6f, 0f, 14.4f), 1.2f, CorridorHeight, false);

            // Parede do fundo com vão para o subsolo (x ∈ [-0,9; 0,9]).
            Wall(root, "Corredor_Fundo_Esq", new Vector3(-1.25f, CorridorHeight * 0.5f, 19f), new Vector3(0.7f, CorridorHeight, 0.2f));
            Wall(root, "Corredor_Fundo_Dir", new Vector3(1.25f, CorridorHeight * 0.5f, 19f), new Vector3(0.7f, CorridorHeight, 0.2f));
            Lintel(root, "Verga_Subsolo", new Vector3(0f, 0f, 19f), 1.8f, CorridorHeight, true);

            // Abre para dentro do corredor: para o outro lado a folha atravessaria o painel da central.
            Door(root, context, "porta_corredor", new Vector3(-0.9f, 0f, 6f), 1.8f, true, false, "").openAngle = -95f;

            PropFactory.CreateCylinder(root, "Cano_A", new Vector3(-1.1f, 2.72f, 12.5f), new Vector3(0.08f, 6.5f, 0.08f), MaterialLibrary.MetalDark, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            PropFactory.CreateCylinder(root, "Cano_B", new Vector3(-0.92f, 2.78f, 12.5f), new Vector3(0.05f, 6.5f, 0.05f), MaterialLibrary.Metal, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var trigger = PropFactory.CreateTrigger(root, "Gatilho_Corredor", new Vector3(0f, 1.2f, 6.8f), new Vector3(2.8f, 2.4f, 0.6f));
            context.CorridorTrigger = trigger.AddComponent<CorridorTrigger>();

            context.CorridorAnchor = Anchor(root, context, "corridor_mid", new Vector3(0f, 0f, 10.5f), 0f);
            context.SubscriberAnchor = Anchor(root, context, "corridor_end", new Vector3(0f, 0f, 17.9f), 180f);
            Anchor(root, context, "spawn_corridor", new Vector3(0f, 0.12f, 15.5f), 0f);
        }

        private static void BuildSwitchboard(Transform root, WorldContext context)
        {
            // Âncora sem escala: soquetes e etiquetas não herdam a escala do painel visual.
            var panelAnchor = CreateAnchor(root, "Central_Telefonica", new Vector3(-2.7f, 1.95f, 5.8f));
            PropFactory.CreateBox(panelAnchor, "Painel", Vector3.zero, new Vector3(7.4f, 2.55f, 0.26f), MaterialLibrary.MetalDark);
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

                var socketGo = PropFactory.CreateBox(panelAnchor, $"Soquete_{ext:000}", new Vector3(x, y, -0.17f), new Vector3(0.2f, 0.2f, 0.12f), MaterialLibrary.Metal);
                var socket = socketGo.AddComponent<SwitchboardSocket>();
                socket.extension = ext;
                socket.board = board;
                board.RegisterSocket(socket);

                var indicator = PropFactory.CreateSphere(panelAnchor, $"Led_{ext:000}", new Vector3(x, y + 0.17f, -0.2f), 0.065f, MaterialLibrary.IndicatorIdle);
                socket.indicator = indicator.GetComponent<Renderer>();
                socket.SetContaminatedMark(ext == 9);
                socket.SetConnected(false);

                PropFactory.CreateLabel(panelAnchor, ext == 0 ? "TRONCO 0" : ext == 9 ? "LINHA 9" : ext.ToString("000"),
                    new Vector3(x, y - 0.2f, -0.2f), Quaternion.identity, 0.035f,
                    ext == 9 ? new Color(0.9f, 0.45f, 0.35f) : ext == 0 ? new Color(0.95f, 0.78f, 0.4f) : new Color(0.72f, 0.7f, 0.62f));

                context.SwitchboardSockets.Add(socket);
            }

            Label(root, "TRONCO 0 — LINHA MORTA", new Vector3(-6.0f, 2.95f, 5.62f), 0f, 0.055f, new Color(0.9f, 0.78f, 0.45f));
            Label(root, "CENTRAL 214", new Vector3(-2.7f, 3.15f, 5.62f), 0f, 0.075f, new Color(0.82f, 0.78f, 0.62f));
            Label(root, "NÃO CONECTE O 9", new Vector3(1.35f, 2.95f, 5.62f), 0f, 0.045f, new Color(0.82f, 0.36f, 0.28f));

            Document(root, context, "Placa_Tronco_Zero", new Vector3(-5.98f, 1.15f, 5.66f), new Vector3(0.62f, 0.22f, 0.04f), 0f,
                "PLACA DO TRONCO 0", "TRONCO 0 — LINHA MORTA. Fora da rede ativa. Use apenas para reconexões de emergência.", "plate_trunk0");

            var leverAnchor = CreateAnchor(root, "Alavanca_Isolamento", new Vector3(-6.45f, 1.45f, 5.66f));
            PropFactory.CreateBox(leverAnchor, "Base", Vector3.zero, new Vector3(0.32f, 0.62f, 0.18f), MaterialLibrary.MetalDark);
            var handle = PropFactory.CreateBox(leverAnchor, "Manopla", new Vector3(0f, 0.05f, -0.14f), new Vector3(0.08f, 0.5f, 0.08f), MaterialLibrary.Emergency);
            var lever = leverAnchor.gameObject.AddComponent<IsolatorLever>();
            lever.handle = handle.transform;
            context.IsolatorAnchor = leverAnchor;
            context.Isolator = lever;
            Label(root, "ISOLAMENTO", new Vector3(-6.45f, 1.92f, 5.58f), 0f, 0.04f, new Color(0.9f, 0.5f, 0.38f));
        }

        private static void BuildDesk(Transform root, WorldContext context)
        {
            PropFactory.CreateBox(root, "Mesa_Central", new Vector3(3.35f, 0.44f, 4.65f), new Vector3(2.3f, 0.88f, 1.25f), MaterialLibrary.Wood);
            PropFactory.CreateBox(root, "Mesa_Pe", new Vector3(3.35f, 0.04f, 4.65f), new Vector3(2.2f, 0.08f, 1.15f), MaterialLibrary.WoodDark, false);

            var phone = PropFactory.CreateBox(root, "Telefone_Central", new Vector3(3.35f, 0.96f, 4.55f), new Vector3(0.58f, 0.14f, 0.34f), MaterialLibrary.Metal);
            var handset = PropFactory.CreateBox(root, "Fone", new Vector3(3.35f, 1.08f, 4.55f), new Vector3(0.5f, 0.08f, 0.12f), MaterialLibrary.MetalDark, false);
            handset.transform.localRotation = Quaternion.Euler(0f, 0f, 3f);
            var telephone = phone.AddComponent<Phone>();
            telephone.extension = 0;
            telephone.isOperatorPhone = true;
            telephone.locationLabel = "CENTRAL";
            context.OfficePhone = telephone;
            context.Phones[0] = telephone;

            Label(root, "CENTRAL", new Vector3(3.35f, 1.22f, 4.3f), 0f, 0.045f, new Color(0.85f, 0.75f, 0.55f));

            var note = Document(root, context, "Anotacao_Linha_9", new Vector3(2.35f, 0.9f, 4.35f), new Vector3(0.3f, 0.015f, 0.22f), 18f,
                "ANOTAÇÃO", "\"Não conecte o 9.\" A tinta é recente. A letra não é de ninguém da equipe.", "note_line9");
            note.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);

            PropFactory.CreateBox(root, "Cadeira_Assento", new Vector3(2.2f, 0.48f, 3.3f), new Vector3(0.55f, 0.08f, 0.55f), MaterialLibrary.WoodDark);
            PropFactory.CreateBox(root, "Cadeira_Encosto", new Vector3(2.2f, 0.9f, 3.05f), new Vector3(0.55f, 0.8f, 0.08f), MaterialLibrary.WoodDark);
            PropFactory.CreateBox(root, "Armario_Central", new Vector3(6.2f, 1.1f, 4.2f), new Vector3(1.2f, 2.2f, 0.7f), MaterialLibrary.MetalDark);
        }

        private static void BuildArchiveProps(Transform root, WorldContext context)
        {
            PropFactory.CreateBox(root, "Estante_1", new Vector3(-13.8f, 1.25f, 0.2f), new Vector3(0.8f, 2.5f, 4.4f), MaterialLibrary.WoodDark);
            for (var i = 0; i < 4; i++)
            {
                PropFactory.CreateBox(root, "Prateleira_" + i, new Vector3(-13.35f, 0.45f + i * 0.55f, 0.2f), new Vector3(0.55f, 0.05f, 4.2f), MaterialLibrary.Wood, false);
            }

            for (var i = 0; i < 7; i++)
            {
                PropFactory.CreateBox(root, "Caixa_Arquivo_" + i, new Vector3(-13.1f, 0.62f + (i % 3) * 0.55f, -1.6f + i * 0.55f),
                    new Vector3(0.42f, 0.36f, 0.42f), MaterialLibrary.Paper, false);
            }

            var cabinetAnchor = CreateAnchor(root, "Armario_Cabos", new Vector3(-9.2f, 0.72f, 2.45f));
            PropFactory.CreateBox(cabinetAnchor, "Armario", Vector3.zero, new Vector3(1.8f, 1.45f, 0.7f), MaterialLibrary.WoodDark);
            for (var i = 0; i < 3; i++)
            {
                PropFactory.CreateBox(cabinetAnchor, "Gaveta_" + i, new Vector3(0f, 0.4f - i * 0.38f, -0.38f), new Vector3(1.55f, 0.3f, 0.04f), MaterialLibrary.Wood, false);
                PropFactory.CreateBox(cabinetAnchor, "Puxador_Gaveta_" + i, new Vector3(0f, 0.4f - i * 0.38f, -0.42f), new Vector3(0.22f, 0.035f, 0.03f), MaterialLibrary.Metal, false);
            }

            var spool = PropFactory.CreateCylinder(root, "Cabo_214", new Vector3(-9.2f, 1.08f, 1.92f), new Vector3(0.2f, 0.2f, 0.2f), MaterialLibrary.Cable);
            spool.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var pickup = spool.AddComponent<CablePickup>();
            pickup.cableLabel = "CABO 214";
            pickup.cableLength = 4;
            context.CableSpawn = spool.transform;
            context.Pickups[pickup.cableLabel] = pickup;

            // Posições alternativas do cabo (variação por seed nos capítulos 1 e 2).
            Anchor(root, context, "cable_drawer_top", new Vector3(-9.2f, 1.08f, 1.92f), 0f);
            Anchor(root, context, "cable_archive_table", new Vector3(-11.5f, 0.86f, 0.4f), 0f);
            Anchor(root, context, "cable_archive_shelf", new Vector3(-13.2f, 1.03f, 1.6f), 0f);

            Label(cabinetAnchor, "CABOS / 200–219", new Vector3(0f, 0.5f, -0.4f), 0f, 0.032f, new Color(0.85f, 0.75f, 0.55f));

            var register = Document(root, context, "Registro_1987", new Vector3(-11.8f, 0.9f, -1.2f), new Vector3(0.42f, 0.05f, 0.3f), -22f,
                "REGISTRO DE 1987", "A extensão 214 foi removida do painel após o incêndio. O tronco zero permaneceu ativo. Nenhum técnico assinou a remoção.",
                "call_214_origin");
            register.category = EvidenceCategory.Document;

            PropFactory.CreateBox(root, "Mesa_Arquivo", new Vector3(-11.5f, 0.4f, 0.2f), new Vector3(1.8f, 0.8f, 0.9f), MaterialLibrary.Wood);
            WallPhone(root, context, 211, "ARQUIVO DE VOZES", new Vector3(-12.4f, 1.45f, -2.84f), 0f);
        }

        private static void BuildCorridorProps(Transform root, WorldContext context)
        {
            var station = PropFactory.CreateBox(root, "Estacao_Bateria", new Vector3(1.42f, 1.2f, 11.2f), new Vector3(0.3f, 0.62f, 0.42f), MaterialLibrary.Metal);
            station.AddComponent<BatteryStation>();
            Label(root, "ENERGIA", new Vector3(1.25f, 1.58f, 11.2f), 90f, 0.035f, new Color(0.75f, 0.85f, 0.7f));

            for (var i = 0; i < 4; i++)
            {
                PropFactory.CreateBox(root, "Caixa_Corredor_" + i, new Vector3(-1.25f, 0.28f, 8f + i * 2.6f), new Vector3(0.45f, 0.56f, 0.5f), MaterialLibrary.WoodDark);
            }

            Label(root, "ARQUIVO MORTO", new Vector3(1.48f, 2.35f, 14.4f), 90f, 0.03f, new Color(0.65f, 0.62f, 0.55f));
            Label(root, "SUBSOLO", new Vector3(0f, 2.35f, 18.88f), 0f, 0.035f, new Color(0.75f, 0.45f, 0.35f));
            WallPhone(root, context, 216, "CORREDOR", new Vector3(-1.45f, 1.45f, 12.8f), 90f);
        }

        private static void BuildReception(Transform root, WorldContext context)
        {
            const float h = 3.2f;
            Slab(root, "Piso_Recepcao", new Vector3(0f, -0.06f, -10f), new Vector3(12f, 0.12f, 8f), MaterialLibrary.Floor);
            Slab(root, "Teto_Recepcao", new Vector3(0f, h + 0.06f, -10f), new Vector3(12f, 0.12f, 8f), MaterialLibrary.Ceiling);
            Wall(root, "Recepcao_Oeste", new Vector3(-6f, h * 0.5f, -10f), new Vector3(0.2f, h, 8f));
            Wall(root, "Recepcao_Leste", new Vector3(6f, h * 0.5f, -10f), new Vector3(0.2f, h, 8f));
            Wall(root, "Recepcao_Sul", new Vector3(0f, h * 0.5f, -14f), new Vector3(12f, h, 0.2f));

            // Saída externa: porta dupla trancada (o jogo nunca sai da estação).
            PropFactory.CreateBox(root, "Porta_Saida", new Vector3(0f, 1.15f, -13.86f), new Vector3(2.2f, 2.3f, 0.1f), MaterialLibrary.MetalDark, false);
            Label(root, "SAÍDA — FECHADA PELA RENALTA", new Vector3(0f, 2.55f, -13.84f), 180f, 0.03f, new Color(0.8f, 0.4f, 0.32f));

            PropFactory.CreateBox(root, "Balcao", new Vector3(2.6f, 0.55f, -9.2f), new Vector3(3.2f, 1.1f, 0.8f), MaterialLibrary.Wood);
            PropFactory.CreateBox(root, "Balcao_Tampo", new Vector3(2.6f, 1.12f, -9.2f), new Vector3(3.3f, 0.05f, 0.9f), MaterialLibrary.WoodDark, false);
            for (var i = 0; i < 3; i++)
            {
                PropFactory.CreateBox(root, "Cadeira_Espera_" + i, new Vector3(-4.6f, 0.45f, -12.2f + i * 0.9f), new Vector3(0.55f, 0.9f, 0.55f), MaterialLibrary.WoodDark);
            }

            PropFactory.CreateCylinder(root, "Extintor", new Vector3(5.6f, 0.4f, -13.4f), new Vector3(0.18f, 0.4f, 0.18f), MaterialLibrary.Emergency);
            Label(root, "SERRA DO SILÊNCIO — RENALTA", new Vector3(0f, 2.75f, -6.14f), 0f, 0.05f, new Color(0.82f, 0.78f, 0.62f));
            Label(root, "RELÓGIO PARADO — 03:12", new Vector3(-5.86f, 2.4f, -10f), -90f, 0.03f, new Color(0.72f, 0.7f, 0.6f));

            var radio = PropFactory.CreateBox(root, "Radio_Apoio", new Vector3(1.8f, 1.27f, -9.2f), new Vector3(0.42f, 0.24f, 0.22f), MaterialLibrary.MetalDark);
            var radioSwitch = radio.AddComponent<MissionSwitch>();
            radioSwitch.eventId = "radio";
            radioSwitch.label = "Ouvir o rádio de apoio";
            context.Switches["radio"] = radioSwitch;

            Document(root, context, "Manual_Operacao", new Vector3(3.3f, 1.16f, -9.1f), new Vector3(0.32f, 0.03f, 0.24f), 12f,
                "MANUAL DE OPERAÇÃO", "\"Conexões de emergência usam o tronco zero. Nunca use a linha 9 para tráfego.\" Assinado: Í. Vilar.", "manual_trunk0");
            Document(root, context, "Quadro_Planta", new Vector3(5.88f, 1.6f, -11f), new Vector3(0.05f, 1.0f, 1.4f), 0f,
                "PLANTA DA ESTAÇÃO", "Recepção, central, arquivo de vozes, corredor técnico, arquivo morto e acesso ao subsolo. A central original aparece riscada a lápis.",
                "map_board").category = EvidenceCategory.Location;

            WallPhone(root, context, 205, "RECEPÇÃO", new Vector3(-5.86f, 1.45f, -8.2f), 90f);
            Anchor(root, context, "spawn_reception", new Vector3(0f, 0.12f, -12.5f), 0f);
            Anchor(root, context, "cable_reception", new Vector3(3.1f, 1.2f, -9.3f), 0f);
        }

        private static void BuildDeadArchive(Transform root, WorldContext context)
        {
            const float h = 3f;
            Slab(root, "Piso_Arquivo_Morto", new Vector3(5.1f, -0.06f, 14.4f), new Vector3(7f, 0.12f, 6f), MaterialLibrary.Floor);
            Slab(root, "Teto_Arquivo_Morto", new Vector3(5.1f, h + 0.06f, 14.4f), new Vector3(7f, 0.12f, 6f), MaterialLibrary.Ceiling);
            Wall(root, "Arquivo_Morto_Leste", new Vector3(8.6f, h * 0.5f, 14.4f), new Vector3(0.2f, h, 6f));
            Wall(root, "Arquivo_Morto_Norte", new Vector3(5.1f, h * 0.5f, 17.4f), new Vector3(7f, h, 0.2f));
            Wall(root, "Arquivo_Morto_Sul", new Vector3(5.1f, h * 0.5f, 11.4f), new Vector3(7f, h, 0.2f));

            Door(root, context, "porta_arquivo_morto", new Vector3(1.6f, 0f, 13.8f), 1.2f, false, true,
                "Trancada. A etiqueta diz: ARQUIVO MORTO — ACESSO COM AUTORIZAÇÃO.");

            for (var i = 0; i < 3; i++)
            {
                PropFactory.CreateBox(root, "Estante_Fitas_" + i, new Vector3(8.1f, 1.2f, 12.6f + i * 1.8f), new Vector3(0.6f, 2.4f, 1.5f), MaterialLibrary.WoodDark);
            }

            PropFactory.CreateBox(root, "Mesa_Fitas", new Vector3(4.6f, 0.4f, 15.6f), new Vector3(2.2f, 0.8f, 0.9f), MaterialLibrary.Wood);
            PropFactory.CreateBox(root, "Gravador_Rolo", new Vector3(4.6f, 0.95f, 15.75f), new Vector3(0.8f, 0.3f, 0.45f), MaterialLibrary.MetalDark, false);

            Tape(root, context, "tape_1979", "FITA 1979", "ch2_tape_1979", new Vector3(4.1f, 0.88f, 15.4f));
            Tape(root, context, "tape_1987", "FITA 1987", "ch2_tape_1987", new Vector3(5.1f, 0.88f, 15.4f));

            Document(root, context, "Escala_1987", new Vector3(3.2f, 0.82f, 15.5f), new Vector3(0.3f, 0.015f, 0.22f), -10f,
                "ESCALA DE PLANTÃO — 1987", "Noite da tempestade. C. Prado — operadora. A. Prado — estagiária. Emergência do povoado: rotear pela 212.",
                "duty_roster_1987");
            Document(root, context, "Ordem_Corte_1998", new Vector3(5.9f, 0.82f, 15.45f), new Vector3(0.3f, 0.015f, 0.22f), 14f,
                "ORDEM DE CORTE — 1998", "Desmontagem da rede analógica. Itens 1–219: executados. Item 0 (tronco zero): \"não executado\". Sem assinatura.",
                "cut_order_1998");

            WallPhone(root, context, 207, "ARQUIVO MORTO", new Vector3(8.46f, 1.45f, 15.9f), -90f);
            Anchor(root, context, "archive_dead", new Vector3(7.4f, 0f, 16.6f), -120f);
            Anchor(root, context, "cable_dead_archive", new Vector3(3.6f, 0.86f, 15.7f), 0f);
            Zone(root, context, "dead_archive", new Vector3(3.2f, 1.2f, 14.4f), new Vector3(1.2f, 2.4f, 2.4f));
        }

        private static void BuildStairwell(Transform root, WorldContext context)
        {
            // Rampa contínua (≈20°) do corredor (y=0, z=19) aos túneis (y=-3, z=27).
            var length = Mathf.Sqrt(8f * 8f + 3f * 3f);
            var angle = Mathf.Atan2(3f, 8f) * Mathf.Rad2Deg;
            var rotation = Quaternion.Euler(angle, 0f, 0f);
            var up = rotation * Vector3.up;
            var ramp = PropFactory.CreateBox(root, "Rampa_Subsolo", new Vector3(0f, -1.5f, 23f) - up * 0.06f, new Vector3(3.2f, 0.12f, length + 0.1f), MaterialLibrary.Floor);
            ramp.transform.localRotation = rotation;
            ramp.isStatic = true;

            Wall(root, "Rampa_Oeste", new Vector3(-1.6f, 0f, 23f), new Vector3(0.2f, 6f, 8f));
            Wall(root, "Rampa_Leste", new Vector3(1.6f, 0f, 23f), new Vector3(0.2f, 6f, 8f));
            Slab(root, "Teto_Rampa", new Vector3(0f, 3.06f, 23f), new Vector3(3.2f, 0.12f, 8f), MaterialLibrary.Ceiling);
            Wall(root, "Rampa_Cabeceira", new Vector3(0f, (TunnelFloor + TunnelHeight + 3f) * 0.5f, 27f), new Vector3(3.2f, 3f - (TunnelFloor + TunnelHeight), 0.2f));

            Door(root, context, "porta_subsolo", new Vector3(-0.9f, 0f, 19f), 1.8f, true, true,
                "Trancada por fora. Uma corrente nova, sem poeira, passa pela maçaneta.");
            Zone(root, context, "ramp", new Vector3(0f, 1.2f, 19.8f), new Vector3(3f, 2.4f, 0.6f));
        }

        private static void BuildTunnels(Transform root, WorldContext context)
        {
            var y = TunnelFloor;
            var cy = y + TunnelHeight * 0.5f;

            // Túnel principal (norte–sul) com cruzamento em z ∈ [35,4; 38,6].
            Slab(root, "Piso_T1", new Vector3(0f, y - 0.06f, 37f), new Vector3(3.2f, 0.12f, 20f), MaterialLibrary.Floor);
            Slab(root, "Teto_T1", new Vector3(0f, y + TunnelHeight + 0.06f, 37f), new Vector3(3.2f, 0.12f, 20f), MaterialLibrary.Ceiling);
            Wall(root, "T1_Oeste_Sul", new Vector3(-1.6f, cy, 31.2f), new Vector3(0.2f, TunnelHeight, 8.4f), MaterialLibrary.WallDark);
            Wall(root, "T1_Oeste_Norte", new Vector3(-1.6f, cy, 42.8f), new Vector3(0.2f, TunnelHeight, 8.4f), MaterialLibrary.WallDark);
            Wall(root, "T1_Leste_Sul", new Vector3(1.6f, cy, 31.2f), new Vector3(0.2f, TunnelHeight, 8.4f), MaterialLibrary.WallDark);
            Wall(root, "T1_Leste_Norte", new Vector3(1.6f, cy, 42.8f), new Vector3(0.2f, TunnelHeight, 8.4f), MaterialLibrary.WallDark);
            Wall(root, "T1_Fundo_Esq", new Vector3(-1.25f, cy, 47f), new Vector3(0.7f, TunnelHeight, 0.2f), MaterialLibrary.WallDark);
            Wall(root, "T1_Fundo_Dir", new Vector3(1.25f, cy, 47f), new Vector3(0.7f, TunnelHeight, 0.2f), MaterialLibrary.WallDark);
            Lintel(root, "Verga_T1", new Vector3(0f, y, 47f), 1.8f, TunnelHeight, true);

            // Túnel transversal (oeste–leste).
            Slab(root, "Piso_T2_Oeste", new Vector3(-6.8f, y - 0.06f, 37f), new Vector3(10.4f, 0.12f, 3.2f), MaterialLibrary.Floor);
            Slab(root, "Piso_T2_Leste", new Vector3(6.8f, y - 0.06f, 37f), new Vector3(10.4f, 0.12f, 3.2f), MaterialLibrary.Floor);
            Slab(root, "Teto_T2_Oeste", new Vector3(-6.8f, y + TunnelHeight + 0.06f, 37f), new Vector3(10.4f, 0.12f, 3.2f), MaterialLibrary.Ceiling);
            Slab(root, "Teto_T2_Leste", new Vector3(6.8f, y + TunnelHeight + 0.06f, 37f), new Vector3(10.4f, 0.12f, 3.2f), MaterialLibrary.Ceiling);
            Wall(root, "T2_Norte_Oeste", new Vector3(-6.8f, cy, 38.6f), new Vector3(10.4f, TunnelHeight, 0.2f), MaterialLibrary.WallDark);
            Wall(root, "T2_Norte_Leste", new Vector3(6.8f, cy, 38.6f), new Vector3(10.4f, TunnelHeight, 0.2f), MaterialLibrary.WallDark);
            Wall(root, "T2_Sul_Oeste", new Vector3(-6.8f, cy, 35.4f), new Vector3(10.4f, TunnelHeight, 0.2f), MaterialLibrary.WallDark);
            Wall(root, "T2_Sul_Leste", new Vector3(6.8f, cy, 35.4f), new Vector3(10.4f, TunnelHeight, 0.2f), MaterialLibrary.WallDark);

            // Fim oeste: vidro de inspeção com nicho escuro atrás (manifestação por reflexo).
            var glass = PropFactory.CreateBox(root, "Vidro_Inspecao", new Vector3(-12f, cy, 37f), new Vector3(0.06f, TunnelHeight, 3.2f), MaterialLibrary.Glass);
            glass.isStatic = true;
            Slab(root, "Piso_Nicho", new Vector3(-13.6f, y - 0.06f, 37f), new Vector3(3.2f, 0.12f, 3.2f), MaterialLibrary.Floor);
            Slab(root, "Teto_Nicho", new Vector3(-13.6f, y + TunnelHeight + 0.06f, 37f), new Vector3(3.2f, 0.12f, 3.2f), MaterialLibrary.Ceiling);
            Wall(root, "Nicho_Oeste", new Vector3(-15.2f, cy, 37f), new Vector3(0.2f, TunnelHeight, 3.2f), MaterialLibrary.WallDark);
            Wall(root, "Nicho_Norte", new Vector3(-13.6f, cy, 38.6f), new Vector3(3.2f, TunnelHeight, 0.2f), MaterialLibrary.WallDark);
            Wall(root, "Nicho_Sul", new Vector3(-13.6f, cy, 35.4f), new Vector3(3.2f, TunnelHeight, 0.2f), MaterialLibrary.WallDark);
            Anchor(root, context, "reflection", new Vector3(-13.6f, y, 37f), 90f);
            Zone(root, context, "glass", new Vector3(-9.5f, cy, 37f), new Vector3(0.6f, TunnelHeight, 3f));

            // Fim leste: parede com vão para a sala de energia (z ∈ [36,1; 37,9]).
            Wall(root, "T2_Fundo_Sul", new Vector3(12f, cy, 35.75f), new Vector3(0.2f, TunnelHeight, 0.7f), MaterialLibrary.WallDark);
            Wall(root, "T2_Fundo_Norte", new Vector3(12f, cy, 38.25f), new Vector3(0.2f, TunnelHeight, 0.7f), MaterialLibrary.WallDark);
            Lintel(root, "Verga_Energia", new Vector3(12f, y, 37f), 1.8f, TunnelHeight, false);

            // Eletrocalhas: leitura de "túnel de cabos" barata.
            PropFactory.CreateBox(root, "Eletrocalha_T1", new Vector3(1.3f, y + 2.3f, 37f), new Vector3(0.25f, 0.08f, 20f), MaterialLibrary.Metal, false);
            PropFactory.CreateBox(root, "Eletrocalha_T2", new Vector3(0f, y + 2.3f, 38.3f), new Vector3(24f, 0.08f, 0.25f), MaterialLibrary.Metal, false);

            // Seis caixas de passagem (posições fixas; quais estão contaminadas depende da seed).
            var boxes = new[]
            {
                (new Vector3(-1.45f, y + 1.3f, 30f), 90f),
                (new Vector3(1.45f, y + 1.3f, 33f), -90f),
                (new Vector3(-6f, y + 1.3f, 38.45f), 180f),
                (new Vector3(-9f, y + 1.3f, 35.55f), 0f),
                (new Vector3(7f, y + 1.3f, 38.45f), 180f),
                (new Vector3(1.45f, y + 1.3f, 43f), -90f)
            };
            for (var i = 0; i < boxes.Length; i++)
            {
                var box = Junction(root, $"CX {i + 1}", "junction", i, boxes[i].Item1, boxes[i].Item2);
                context.TunnelBoxes.Add(box);
            }

            WallPhone(root, context, 218, "TÚNEIS", new Vector3(-4f, y + 1.45f, 35.56f), 0f);
            Label(root, "TÚNEL DE CABOS — LINHA 0", new Vector3(-1.48f, y + 2.2f, 29.5f), -90f, 0.03f, new Color(0.7f, 0.55f, 0.45f));
            Anchor(root, context, "spawn_tunnels", new Vector3(0f, y + 0.12f, 28.6f), 0f);
            Anchor(root, context, "tunnel_east", new Vector3(10.6f, y, 37f), -90f);
            Anchor(root, context, "tunnel_north", new Vector3(0f, y, 45.6f), 180f);
            Anchor(root, context, "tunnel_cross", new Vector3(0f, y, 37f), 0f);
            Zone(root, context, "tunnels", new Vector3(0f, y + 1.2f, 28f), new Vector3(3f, 2.4f, 0.8f));

            var station = PropFactory.CreateBox(root, "Estacao_Bateria_Tunel", new Vector3(-1.42f, y + 1.2f, 41f), new Vector3(0.3f, 0.62f, 0.42f), MaterialLibrary.Metal);
            station.AddComponent<BatteryStation>();
        }

        private static void BuildEnergyRoom(Transform root, WorldContext context)
        {
            var y = TunnelFloor;
            const float h = 3.2f;
            Slab(root, "Piso_Energia", new Vector3(16f, y - 0.06f, 37f), new Vector3(8f, 0.12f, 8f), MaterialLibrary.Floor);
            Slab(root, "Teto_Energia", new Vector3(16f, y + h + 0.06f, 37f), new Vector3(8f, 0.12f, 8f), MaterialLibrary.Ceiling);
            Wall(root, "Energia_Leste", new Vector3(20f, y + h * 0.5f, 37f), new Vector3(0.2f, h, 8f), MaterialLibrary.WallDark);
            Wall(root, "Energia_Norte", new Vector3(16f, y + h * 0.5f, 41f), new Vector3(8f, h, 0.2f), MaterialLibrary.WallDark);
            Wall(root, "Energia_Sul", new Vector3(16f, y + h * 0.5f, 33f), new Vector3(8f, h, 0.2f), MaterialLibrary.WallDark);
            Wall(root, "Energia_Oeste_Sul", new Vector3(12f, y + h * 0.5f, 34.55f), new Vector3(0.2f, h, 3.1f), MaterialLibrary.WallDark);
            Wall(root, "Energia_Oeste_Norte", new Vector3(12f, y + h * 0.5f, 39.45f), new Vector3(0.2f, h, 3.1f), MaterialLibrary.WallDark);
            PropFactory.CreateBox(root, "Energia_Verga", new Vector3(12f, y + TunnelHeight + (h - TunnelHeight) * 0.5f, 37f), new Vector3(0.2f, h - TunnelHeight, 1.8f), MaterialLibrary.WallDark, false);

            Door(root, context, "porta_energia", new Vector3(12f, y, 36.1f), 1.8f, false, true,
                "Trancada. Placa: SALA DE ENERGIA — RISCO DE MORTE. O cadeado está quente.");

            PropFactory.CreateBox(root, "Gerador", new Vector3(17.2f, y + 0.9f, 39.2f), new Vector3(3.2f, 1.8f, 1.4f), MaterialLibrary.MetalDark);
            PropFactory.CreateCylinder(root, "Gerador_Exaustor", new Vector3(18.4f, y + 2.2f, 39.2f), new Vector3(0.3f, 0.6f, 0.3f), MaterialLibrary.Metal, false);

            var ids = new[] { "A", "B", "C" };
            for (var i = 0; i < ids.Length; i++)
            {
                var anchor = CreateAnchor(root, "Disjuntor_" + ids[i], new Vector3(14.6f + i * 1.4f, y + 1.4f, 33.16f));
                PropFactory.CreateBox(anchor, "Caixa", Vector3.zero, new Vector3(0.6f, 0.9f, 0.16f), MaterialLibrary.Metal);
                var handle = PropFactory.CreateBox(anchor, "Manopla", new Vector3(0f, 0f, 0.14f), new Vector3(0.08f, 0.42f, 0.08f), MaterialLibrary.Emergency);
                var breaker = anchor.gameObject.AddComponent<BreakerSwitch>();
                breaker.breakerId = ids[i];
                breaker.handle = handle.transform;
                breaker.SetOn(true);
                context.Breakers[ids[i]] = breaker;
                Label(anchor, "DISJUNTOR " + ids[i], new Vector3(0f, 0.6f, 0.1f), 180f, 0.03f, new Color(0.9f, 0.5f, 0.38f));
            }

            Document(root, context, "Manual_Desligamento", new Vector3(19.86f, y + 1.5f, 34.6f), new Vector3(0.05f, 0.6f, 0.45f), 0f,
                "MANUAL DE DESLIGAMENTO", "\"Para isolar a linha zero: desligue A e C. Nunca desligue B sob carga — B alimenta a iluminação de emergência.\"",
                "breaker_manual");
            WallPhone(root, context, 209, "SALA DE ENERGIA", new Vector3(19.86f, y + 1.45f, 38f), -90f);
            Zone(root, context, "energy", new Vector3(13.2f, y + 1.2f, 37f), new Vector3(0.6f, 2.4f, 2f));
        }

        private static void BuildOriginalCentral(Transform root, WorldContext context)
        {
            var y = TunnelFloor;
            const float h = 3.4f;
            Slab(root, "Piso_Original", new Vector3(0f, y - 0.06f, 52f), new Vector3(12f, 0.12f, 10f), MaterialLibrary.Floor);
            Slab(root, "Teto_Original", new Vector3(0f, y + h + 0.06f, 52f), new Vector3(12f, 0.12f, 10f), MaterialLibrary.Ceiling);
            Wall(root, "Original_Oeste", new Vector3(-6f, y + h * 0.5f, 52f), new Vector3(0.2f, h, 10f), MaterialLibrary.WallDark);
            Wall(root, "Original_Leste", new Vector3(6f, y + h * 0.5f, 52f), new Vector3(0.2f, h, 10f), MaterialLibrary.WallDark);
            Wall(root, "Original_Norte", new Vector3(0f, y + h * 0.5f, 57f), new Vector3(12f, h, 0.2f), MaterialLibrary.WallDark);
            Wall(root, "Original_Sul_Esq", new Vector3(-3.8f, y + h * 0.5f, 47f), new Vector3(4.4f, h, 0.2f), MaterialLibrary.WallDark);
            Wall(root, "Original_Sul_Dir", new Vector3(3.8f, y + h * 0.5f, 47f), new Vector3(4.4f, h, 0.2f), MaterialLibrary.WallDark);
            PropFactory.CreateBox(root, "Original_Verga", new Vector3(0f, y + TunnelHeight + (h - TunnelHeight) * 0.5f, 47f), new Vector3(3.2f, h - TunnelHeight, 0.2f), MaterialLibrary.WallDark, false);

            Door(root, context, "porta_central_original", new Vector3(-0.9f, y, 47f), 1.8f, true, true,
                "Não abre. Do outro lado, um tom de discagem contínuo atravessa a madeira.");

            // Painel de 1987, queimado: circuitos isoláveis e o plugue da linha original.
            var panel = CreateAnchor(root, "Central_1987", new Vector3(0f, y + 1.8f, 56.7f));
            PropFactory.CreateBox(panel, "Painel_1987", Vector3.zero, new Vector3(7f, 2.4f, 0.3f), MaterialLibrary.MetalDark);
            for (var i = 0; i < 5; i++)
            {
                var box = Junction(root, $"CIRCUITO {i + 1}", "circuit", i, new Vector3(-2.8f + i * 1.4f, y + 2.05f, 56.5f), 180f);
                context.OriginalCircuits.Add(box);
            }

            for (var i = 0; i < 6; i++)
            {
                PropFactory.CreateBox(root, "Marca_Queimado_" + i, new Vector3(-3f + i * 1.2f, y + 0.02f, 55.6f - (i % 2) * 0.7f),
                    new Vector3(0.9f, 0.02f, 0.6f), MaterialLibrary.Ceiling, false);
            }

            var restore = PropFactory.CreateBox(root, "Plugue_Original", new Vector3(0f, y + 1.15f, 56.45f), new Vector3(0.5f, 0.3f, 0.2f), MaterialLibrary.Emergency);
            var restoreSwitch = restore.AddComponent<MissionSwitch>();
            restoreSwitch.eventId = "restore.trunk";
            restoreSwitch.label = "Religar o tronco zero à linha original";
            context.Switches["restore.trunk"] = restoreSwitch;
            Label(root, "LINHA ORIGINAL — 0", new Vector3(0f, y + 1.48f, 56.5f), 0f, 0.03f, new Color(0.95f, 0.6f, 0.4f));

            // Três conclusões.
            var cut = PropFactory.CreateBox(root, "Chave_Geral", new Vector3(-5.85f, y + 1.4f, 52f), new Vector3(0.18f, 0.9f, 0.6f), MaterialLibrary.Emergency);
            Ending(context, cut, EndingKind.Isolate, "Chave geral de corte");
            var plug = PropFactory.CreateBox(root, "Plugue_Ana", new Vector3(1.6f, y + 1.15f, 56.45f), new Vector3(0.4f, 0.3f, 0.2f), MaterialLibrary.Metal);
            Ending(context, plug, EndingKind.Restore, "Completar a chamada de 1987");

            PropFactory.CreateBox(root, "Mesa_Operadora", new Vector3(4.2f, y + 0.4f, 52.5f), new Vector3(1.4f, 0.8f, 2.4f), MaterialLibrary.Wood);
            var headset = PropFactory.CreateBox(root, "Fone_Operadora", new Vector3(4.2f, y + 0.88f, 52.9f), new Vector3(0.36f, 0.12f, 0.3f), MaterialLibrary.MetalDark);
            Ending(context, headset, EndingKind.Answer, "Assumir a linha");

            var zeroPhone = PropFactory.CreateBox(root, "Telefone_Zero", new Vector3(4.2f, y + 0.88f, 52.0f), new Vector3(0.46f, 0.14f, 0.3f), MaterialLibrary.Metal);
            var zero = zeroPhone.AddComponent<Phone>();
            zero.extension = 1000;
            zero.isOperatorPhone = false;
            zero.locationLabel = "CENTRAL ORIGINAL";
            context.Phones[1000] = zero;

            Document(root, context, "Registros_Incendio", new Vector3(-4.2f, y + 0.82f, 50.2f), new Vector3(0.36f, 0.02f, 0.26f), 8f,
                "REGISTROS DO INCÊNDIO", "Relatório interno de 1987: a sobrecarga começou no tronco zero, mantido aberto além do protocolo. O laudo de 1979 foi copiado, com outra data.",
                "fire_records");
            PropFactory.CreateBox(root, "Mesa_Registros", new Vector3(-4.2f, y + 0.4f, 50.2f), new Vector3(1.2f, 0.8f, 0.8f), MaterialLibrary.Wood);
            Document(root, context, "Cracha_Ana", new Vector3(-1.9f, y + 1.02f, 56.42f), new Vector3(0.1f, 0.14f, 0.02f), 0f,
                "CRACHÁ QUEIMADO", "ANA PRADO — ESTAGIÁRIA DE OPERAÇÃO. O plástico derreteu sobre o painel.", "ana_badge");
            Tape(root, context, "tape_original", "GRAVAÇÃO ORIGINAL", "ch5_original", new Vector3(4.2f, y + 0.86f, 51.5f));

            Anchor(root, context, "original_center", new Vector3(0f, y, 54.6f), 180f);
            Zone(root, context, "original", new Vector3(0f, y + 1.2f, 48.4f), new Vector3(3f, 2.4f, 0.8f));
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

            var equipment = PlayerEquipment.Attach(controller, true);
            var interactor = player.AddComponent<PlayerInteractor>();
            interactor.Initialize(controller.Camera, equipment, true);

            context.Player = player;
            context.PlayerController = controller;
            context.SetPlayerActive(true);
        }

        /// <summary>Static batching só do que é realmente estático (paredes, pisos, tetos).</summary>
        private static void CombineStatic(GameObject root)
        {
            var list = new System.Collections.Generic.List<GameObject>();
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.gameObject.isStatic) list.Add(renderer.gameObject);
            }

            if (list.Count > 0) StaticBatchingUtility.Combine(list.ToArray(), root);
        }

        // ================================================================ setores

        private static void BuildSectors(WorldContext context)
        {
            AddSector(context, "reception", "RECEPÇÃO", new Vector3(0f, 1.5f, -10f), new Vector3(12f, 4f, 8f), "recepcao",
                new[] { "central" }, new Vector2(0f, -2f));
            AddSector(context, "central", "SALA DA CENTRAL", new Vector3(0f, 1.5f, 0f), new Vector3(14f, 4f, 12f), "sala_central",
                new[] { "reception", "archive", "corridor" }, new Vector2(0f, 0f));
            AddSector(context, "archive", "ARQUIVO DE VOZES", new Vector3(-11f, 1.5f, 0f), new Vector3(8f, 4f, 6f), "arquivo",
                new[] { "central" }, new Vector2(-2f, 0f));
            AddSector(context, "corridor", "CORREDOR TÉCNICO", new Vector3(0f, 1.5f, 12.5f), new Vector3(3.2f, 4f, 13f), "corredor",
                new[] { "central", "dead_archive", "stairs" }, new Vector2(0f, 2f));
            AddSector(context, "dead_archive", "ARQUIVO MORTO", new Vector3(5.1f, 1.5f, 14.4f), new Vector3(7f, 4f, 6f), "arquivo",
                new[] { "corridor" }, new Vector2(1.6f, 2f));
            AddSector(context, "stairs", "ACESSO AO SUBSOLO", new Vector3(0f, 0f, 23f), new Vector3(3.2f, 7f, 8f), "corredor",
                new[] { "corridor", "tunnels" }, new Vector2(0f, 3f));
            AddSector(context, "tunnels", "TÚNEIS DE CABOS", new Vector3(-1.5f, -1.5f, 37f), new Vector3(27.5f, 3.4f, 20f), "tuneis",
                new[] { "stairs", "energy", "original" }, new Vector2(0f, 4.4f));
            AddSector(context, "energy", "SALA DE ENERGIA", new Vector3(16f, -1.5f, 37f), new Vector3(8f, 3.6f, 8f), "energia",
                new[] { "tunnels" }, new Vector2(2.2f, 4.4f));
            AddSector(context, "original", "CENTRAL ORIGINAL", new Vector3(0f, -1.5f, 52f), new Vector3(12f, 3.8f, 10f), "original",
                new[] { "tunnels" }, new Vector2(0f, 5.8f));

            // Distribui as luzes já criadas pelo LightingRig entre os setores.
            foreach (var light in context.CeilingLights)
            {
                if (light == null) continue;
                var sector = context.SectorAt(light.transform.position);
                if (sector != null) sector.Lights.Add(light);
            }
        }

        private static void AddSector(WorldContext context, string id, string label, Vector3 center, Vector3 size, string ambience,
            string[] neighbors, Vector2 map)
        {
            context.Sectors.Add(new SectorInfo
            {
                Id = id,
                Label = label,
                Bounds = new Bounds(center, size),
                Ambience = ambience,
                Neighbors = neighbors,
                MapPosition = map
            });
        }

        private static void BuildReverbZones(Transform root, WorldContext context)
        {
            foreach (var sector in context.Sectors)
            {
                var go = new GameObject("Reverb_" + sector.Id);
                go.transform.SetParent(root, false);
                go.transform.position = sector.Bounds.center;
                var zone = go.AddComponent<AudioReverbZone>();
                var radius = Mathf.Max(sector.Bounds.extents.x, sector.Bounds.extents.z);
                zone.minDistance = radius * 0.6f;
                zone.maxDistance = radius * 1.1f;
                switch (sector.Id)
                {
                    case "corridor":
                    case "stairs": zone.reverbPreset = AudioReverbPreset.Hallway; break;
                    case "tunnels": zone.reverbPreset = AudioReverbPreset.SewerPipe; break;
                    case "energy": zone.reverbPreset = AudioReverbPreset.Hangar; break;
                    case "original": zone.reverbPreset = AudioReverbPreset.Cave; break;
                    case "archive":
                    case "dead_archive": zone.reverbPreset = AudioReverbPreset.Livingroom; break;
                    default: zone.reverbPreset = AudioReverbPreset.Room; break;
                }
            }
        }

        // ================================================================ helpers

        private static Transform CreateAnchor(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go.transform;
        }

        private static Transform Anchor(Transform root, WorldContext context, string id, Vector3 position, float yaw)
        {
            var anchor = CreateAnchor(root, "Ancora_" + id, position);
            anchor.localRotation = Quaternion.Euler(0f, yaw, 0f);
            context.Anchors[id] = anchor;
            return anchor;
        }

        private static void Wall(Transform root, string name, Vector3 position, Vector3 size, Material material = null)
        {
            var wall = PropFactory.CreateBox(root, name, position, size, material != null ? material : MaterialLibrary.Wall);
            wall.isStatic = true;
        }

        private static void Slab(Transform root, string name, Vector3 position, Vector3 size, Material material)
        {
            var slab = PropFactory.CreateBox(root, name, position, size, material);
            slab.isStatic = true;
        }

        /// <summary>Verga acima de um vão de porta (2,1 m até o teto).</summary>
        private static void Lintel(Transform root, string name, Vector3 baseCenter, float width, float ceiling, bool alongX)
        {
            var height = ceiling - 2.1f;
            if (height <= 0.01f) return;
            var size = alongX ? new Vector3(width, height, 0.2f) : new Vector3(0.2f, height, width);
            Wall(root, name, baseCenter + new Vector3(0f, 2.1f + height * 0.5f, 0f), size);
        }

        /// <summary>
        /// Porta com pivô na dobradiça. <paramref name="alongX"/>: a folha se estende em +X a partir
        /// da dobradiça (parede perpendicular a Z); senão, em +Z.
        /// </summary>
        private static DoorInteractable Door(Transform root, WorldContext context, string id, Vector3 hinge, float width,
            bool alongX, bool locked, string lockedMessage)
        {
            var pivot = CreateAnchor(root, "Porta_" + id, hinge);
            var size = alongX ? new Vector3(width, 2.1f, 0.12f) : new Vector3(0.12f, 2.1f, width);
            var center = alongX ? new Vector3(width * 0.5f, 1.05f, 0f) : new Vector3(0f, 1.05f, width * 0.5f);
            PropFactory.CreateBox(pivot, "Folha", center, size, locked ? MaterialLibrary.MetalDark : MaterialLibrary.WoodDark);
            var knob = alongX ? new Vector3(width - 0.12f, 1.05f, -0.09f) : new Vector3(-0.09f, 1.05f, width - 0.12f);
            PropFactory.CreateSphere(pivot, "Macaneta", knob, 0.08f, MaterialLibrary.Metal);
            var door = pivot.gameObject.AddComponent<DoorInteractable>();
            door.doorId = id;
            door.openAngle = 95f;
            door.locked = locked;
            door.lockedMessage = string.IsNullOrEmpty(lockedMessage) ? "Trancada." : lockedMessage;
            context.Doors[id] = door;
            return door;
        }

        private static void Label(Transform root, string text, Vector3 position, float yaw, float size, Color color)
        {
            PropFactory.CreateLabel(root, text, position, Quaternion.Euler(0f, yaw, 0f), size, color);
        }

        private static ExamineInteractable Document(Transform root, WorldContext context, string name, Vector3 position, Vector3 size, float yaw,
            string title, string body, string evidenceId)
        {
            var go = PropFactory.CreateBox(root, name, position, size, MaterialLibrary.Paper);
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var examine = go.AddComponent<ExamineInteractable>();
            examine.title = title;
            examine.body = body;
            examine.evidenceId = evidenceId;
            examine.category = EvidenceCategory.Document;
            if (!string.IsNullOrEmpty(evidenceId) && !context.Documents.ContainsKey(evidenceId)) context.Documents[evidenceId] = examine;
            return examine;
        }

        private static Phone WallPhone(Transform root, WorldContext context, int extension, string location, Vector3 position, float yaw)
        {
            var anchor = CreateAnchor(root, $"Telefone_Parede_{extension}", position);
            anchor.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var body = PropFactory.CreateBox(anchor, "Corpo", Vector3.zero, new Vector3(0.24f, 0.36f, 0.14f), MaterialLibrary.Metal);
            PropFactory.CreateBox(anchor, "Fone", new Vector3(0.15f, 0f, 0.02f), new Vector3(0.06f, 0.3f, 0.08f), MaterialLibrary.MetalDark, false);
            var phone = body.AddComponent<Phone>();
            phone.extension = extension;
            phone.isOperatorPhone = false;
            phone.locationLabel = location;
            context.Phones[extension] = phone;
            PropFactory.CreateLabel(anchor, extension.ToString("000"), new Vector3(0f, 0.26f, 0f), Quaternion.Euler(0f, 180f, 0f), 0.025f,
                new Color(0.8f, 0.75f, 0.6f));
            return phone;
        }

        private static void Tape(Transform root, WorldContext context, string id, string label, string callId, Vector3 position)
        {
            var go = PropFactory.CreateCylinder(root, "Fita_" + id, position, new Vector3(0.26f, 0.02f, 0.26f), MaterialLibrary.MetalDark);
            var tape = go.AddComponent<TapeReel>();
            tape.tapeId = id;
            tape.label = label;
            tape.callId = callId;
            context.Tapes[id] = tape;
        }

        private static JunctionBox Junction(Transform root, string label, string prefix, int index, Vector3 position, float yaw)
        {
            var anchor = CreateAnchor(root, "Caixa_" + prefix + "_" + index, position);
            anchor.localRotation = Quaternion.Euler(0f, yaw, 0f);
            PropFactory.CreateBox(anchor, "Corpo", Vector3.zero, new Vector3(0.5f, 0.6f, 0.18f), MaterialLibrary.Metal);
            var led = PropFactory.CreateSphere(anchor, "Led", new Vector3(0f, 0.2f, 0.1f), 0.07f, MaterialLibrary.IndicatorIdle);
            PropFactory.CreateLabel(anchor, label, new Vector3(0f, -0.4f, 0.1f), Quaternion.Euler(0f, 180f, 0f), 0.025f, new Color(0.82f, 0.75f, 0.6f));
            var box = anchor.gameObject.AddComponent<JunctionBox>();
            box.index = index;
            box.label = label;
            box.eventPrefix = prefix;
            box.led = led.GetComponent<Renderer>();
            box.SetState(JunctionState.Clean);
            return box;
        }

        private static void Ending(WorldContext context, GameObject go, EndingKind ending, string label)
        {
            var console = go.AddComponent<EndingConsole>();
            console.ending = ending;
            console.objectLabel = label;
            context.EndingConsoles.Add(console);
        }

        private static void Zone(Transform root, WorldContext context, string id, Vector3 position, Vector3 size)
        {
            var trigger = PropFactory.CreateTrigger(root, "Zona_" + id, position, size);
            var zone = trigger.AddComponent<ZoneTrigger>();
            zone.zoneId = id;
            context.Zones.Add(zone);
        }
    }
}
