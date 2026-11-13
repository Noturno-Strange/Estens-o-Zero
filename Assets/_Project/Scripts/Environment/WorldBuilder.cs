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
    /// Constrói a Estação de Retransmissão Serra do Silêncio em runtime (projeto.md §15):
    /// área externa de retransmissão, recepção, alojamentos antigos, sala da central, salas de
    /// operação, arquivo de vozes, corredor técnico, depósito, arquivo morto, acesso ao subsolo,
    /// túneis de cabos, sala de energia e central original.
    ///
    /// Kit modular com texturas PBR em escala de mundo, modelos CC0 (CREDITS.md) e luminárias
    /// com cone para baixo e sombra, para a luz não atravessar paredes. Geometria determinística:
    /// a mesma ordem de construção gera os mesmos IDs de interação em todos os peers. As
    /// coordenadas usadas pelas missões e pelos testes são preservadas.
    /// </summary>
    public static class WorldBuilder
    {
        private const float WallHeight = 3.4f;
        private const float CorridorHeight = 3.0f;
        private const float TunnelFloor = -3f;
        private const float TunnelHeight = 2.8f;
        private const float WallThickness = 0.2f;

        private enum WallKind
        {
            Institutional,
            Peeling,
            Concrete,
            Tunnel,
            Burned,
            Metal,
            Facade
        }

        public static WorldContext Build(GameManager manager)
        {
            InteractionRegistry.Clear();
            SignalRegistry.Clear();
            var context = new WorldContext
            {
                Seed = 1337,
                AmbientZone = "sala_central",
                PlayerSpawn = new Vector3(0f, 0.12f, -27f),
                PlayerSpawnRotation = Quaternion.identity
            };

            var root = new GameObject("World");
            context.Root = root;
            var t = root.transform;

            // A ordem de construção define os IDs de interação: não reordenar sem considerar a rede.
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
            BuildExterior(t, context);
            BuildDorms(t, context);
            BuildOperations(t, context);
            BuildDepot(t, context);
            BuildPlayer(t, context);
            LightingRig.Build(t, context);
            BuildSectors(context);
            BuildReverbZones(t, context);
            CombineStatic(root);
            Debug.Log($"[EZ] Mundo construído: {context.SwitchboardSockets.Count} soquetes, {context.Phones.Count} telefones, " +
                      $"{context.Doors.Count} portas, {context.CeilingLights.Count} luzes, {context.Sectors.Count} setores.");
            return context;
        }

        // ================================================================ sala da central

        private static void BuildCentralRoom(Transform root, WorldContext context)
        {
            Slab(root, "Piso_Central", new Vector3(0f, -0.06f, 0f), new Vector3(14f, 0.12f, 12f), Surface.FloorLinoleum);
            Slab(root, "Teto_Central", new Vector3(0f, WallHeight + 0.06f, 0f), new Vector3(14f, 0.12f, 12f), Surface.Ceiling);

            // Parede sul com vão para a recepção (x ∈ [-0,9; 0,9]).
            Wall(root, "Parede_Sul_Esq", new Vector3(-3.95f, WallHeight * 0.5f, -6f), new Vector3(6.1f, WallHeight, WallThickness));
            Wall(root, "Parede_Sul_Dir", new Vector3(3.95f, WallHeight * 0.5f, -6f), new Vector3(6.1f, WallHeight, WallThickness));
            Lintel(root, "Verga_Sul", new Vector3(0f, 0f, -6f), 1.8f, WallHeight, true);

            // Parede leste com vão para as salas de operação (z ∈ [-0,9; 0,9]).
            Wall(root, "Parede_Leste_Sul", new Vector3(7f, WallHeight * 0.5f, -3.45f), new Vector3(WallThickness, WallHeight, 5.1f));
            Wall(root, "Parede_Leste_Norte", new Vector3(7f, WallHeight * 0.5f, 3.45f), new Vector3(WallThickness, WallHeight, 5.1f));
            Lintel(root, "Verga_Leste", new Vector3(7f, 0f, 0f), 1.8f, WallHeight, false);

            Wall(root, "Parede_Norte_Esq", new Vector3(-4.1f, WallHeight * 0.5f, 6f), new Vector3(5.8f, WallHeight, WallThickness));
            Wall(root, "Parede_Norte_Dir", new Vector3(4.1f, WallHeight * 0.5f, 6f), new Vector3(5.8f, WallHeight, WallThickness));
            Wall(root, "Parede_Oeste_Norte", new Vector3(-7f, WallHeight * 0.5f, 3.5f), new Vector3(WallThickness, WallHeight, 5f));
            Wall(root, "Parede_Oeste_Sul", new Vector3(-7f, WallHeight * 0.5f, -3.5f), new Vector3(WallThickness, WallHeight, 5f));
            Lintel(root, "Verga_Oeste", new Vector3(-7f, 0f, 0f), 2f, WallHeight, false);

            // Batentes entre o vão do corredor (2,4 m) e a porta (1,8 m).
            Wall(root, "Batente_Corredor_Esq", new Vector3(-1.05f, WallHeight * 0.5f, 6f), new Vector3(0.3f, WallHeight, WallThickness));
            Wall(root, "Batente_Corredor_Dir", new Vector3(1.05f, WallHeight * 0.5f, 6f), new Vector3(0.3f, WallHeight, WallThickness));
            Lintel(root, "Verga_Norte", new Vector3(0f, 0f, 6f), 1.8f, WallHeight, true);

            // Vigas, eletrocalha e conduíte: leitura de escala industrial.
            Trim(root, "Viga_Central", new Vector3(0f, 3.18f, 0f), new Vector3(14f, 0.32f, 0.26f), Surface.MetalPanel);
            Trim(root, "Viga_Central_2", new Vector3(0f, 3.18f, -3f), new Vector3(14f, 0.32f, 0.26f), Surface.MetalPanel);
            Trim(root, "Eletrocalha_Central", new Vector3(-6.6f, 3.0f, 0f), new Vector3(0.3f, 0.08f, 11.6f), Surface.Trim);
            Pipe(root, "Conduite_Central", new Vector3(-6.78f, 2.85f, 0f), 11.6f, 0.05f, false);

            Door(root, context, "porta_recepcao", new Vector3(-0.9f, 0f, -6f), 1.8f, true, false, "", Surface.DoorWood);
            DoorFrame(root, new Vector3(0f, 0f, -6f), 1.8f, true);
            DoorFrame(root, new Vector3(7f, 0f, 0f), 1.8f, false);
            DoorFrame(root, new Vector3(-7f, 0f, 0f), 2f, false);
            Anchor(root, context, "central_center", new Vector3(0f, 0f, 0f), 0f);

            // Mobiliário de apoio: arquivos, estante, relógio, quadro de força e avisos.
            ArtLibrary.Prop(root, "drawer_cabinet", new Vector3(6.6f, 0f, -4.6f), -90f);
            ArtLibrary.Prop(root, "drawer_cabinet", new Vector3(6.6f, 0f, -3.3f), -90f);
            ArtLibrary.Prop(root, "wooden_bookshelf_worn", new Vector3(-6.6f, 0f, -4.4f), 90f);
            ArtLibrary.Prop(root, "cardboard_box_01", new Vector3(-6.6f, 2.06f, -4.6f), 80f, false);
            ArtLibrary.Prop(root, "vintage_telephone_wall_clock", new Vector3(0f, 2.45f, -5.89f), 0f, false);
            ArtLibrary.Prop(root, "power_box_01", new Vector3(6.85f, 1.6f, 2.2f), -90f, false);
            ArtLibrary.Prop(root, "korean_fire_extinguisher_01", new Vector3(6.75f, 0f, -1.5f), -90f);
            Poster(root, "AVISO", "RENALTA — NORMA 12\nToda chamada de emergência\ndeve ser roteada pelo tronco zero.", new Vector3(-6.88f, 1.7f, 1.6f), -90f);
            Poster(root, "ESCALA", "TURNO DA NOITE\nOPERADORA: C. PRADO\nAPOIO: A. PRADO", new Vector3(4.6f, 1.75f, -5.88f), 180f);
        }

        private static void BuildArchive(Transform root, WorldContext context)
        {
            Slab(root, "Piso_Arquivo", new Vector3(-11f, -0.06f, 0f), new Vector3(8f, 0.12f, 6f), Surface.FloorWood);
            Slab(root, "Teto_Arquivo", new Vector3(-11f, 3.06f, 0f), new Vector3(8f, 0.12f, 6f), Surface.Ceiling);
            Wall(root, "Arquivo_Sul", new Vector3(-11f, 1.5f, -3f), new Vector3(8f, 3f, WallThickness), WallKind.Peeling);
            Wall(root, "Arquivo_Norte", new Vector3(-11f, 1.5f, 3f), new Vector3(8f, 3f, WallThickness), WallKind.Peeling);
            Wall(root, "Arquivo_Oeste", new Vector3(-15f, 1.5f, 0f), new Vector3(WallThickness, 3f, 6f), WallKind.Peeling);

            Door(root, context, "porta_arquivo", new Vector3(-7f, 0f, -0.9f), 1.8f, false, false, "", Surface.DoorWood);
        }

        private static void BuildCorridor(Transform root, WorldContext context)
        {
            Slab(root, "Piso_Corredor", new Vector3(0f, -0.06f, 12.5f), new Vector3(3.2f, 0.12f, 13f), Surface.FloorConcrete);
            Slab(root, "Teto_Corredor", new Vector3(0f, CorridorHeight + 0.06f, 12.5f), new Vector3(3.2f, 0.12f, 13f), Surface.Ceiling);

            // Parede oeste com vão para o depósito (z ∈ [8,6; 9,8]).
            Wall(root, "Corredor_Oeste_Sul", new Vector3(-1.6f, CorridorHeight * 0.5f, 7.3f), new Vector3(WallThickness, CorridorHeight, 2.6f));
            Wall(root, "Corredor_Oeste_Norte", new Vector3(-1.6f, CorridorHeight * 0.5f, 14.4f), new Vector3(WallThickness, CorridorHeight, 9.2f));
            Lintel(root, "Verga_Deposito", new Vector3(-1.6f, 0f, 9.2f), 1.2f, CorridorHeight, false);

            // Parede leste com vão para o arquivo morto (z ∈ [13,8; 15,0]).
            Wall(root, "Corredor_Leste_Sul", new Vector3(1.6f, CorridorHeight * 0.5f, 9.9f), new Vector3(WallThickness, CorridorHeight, 7.8f));
            Wall(root, "Corredor_Leste_Norte", new Vector3(1.6f, CorridorHeight * 0.5f, 17f), new Vector3(WallThickness, CorridorHeight, 4f));
            Lintel(root, "Verga_Arquivo_Morto", new Vector3(1.6f, 0f, 14.4f), 1.2f, CorridorHeight, false);

            // Parede do fundo com vão para o subsolo (x ∈ [-0,9; 0,9]).
            Wall(root, "Corredor_Fundo_Esq", new Vector3(-1.25f, CorridorHeight * 0.5f, 19f), new Vector3(0.7f, CorridorHeight, WallThickness));
            Wall(root, "Corredor_Fundo_Dir", new Vector3(1.25f, CorridorHeight * 0.5f, 19f), new Vector3(0.7f, CorridorHeight, WallThickness));
            Lintel(root, "Verga_Subsolo", new Vector3(0f, 0f, 19f), 1.8f, CorridorHeight, true);

            // Abre para dentro do corredor: para o outro lado a folha atravessaria o painel da central.
            Door(root, context, "porta_corredor", new Vector3(-0.9f, 0f, 6f), 1.8f, true, false, "", Surface.DoorMetal).openAngle = -95f;
            DoorFrame(root, new Vector3(0f, 0f, 6f), 1.8f, true);
            DoorFrame(root, new Vector3(0f, 0f, 19f), 1.8f, true);
            DoorFrame(root, new Vector3(1.6f, 0f, 14.4f), 1.2f, false);
            DoorFrame(root, new Vector3(-1.6f, 0f, 9.2f), 1.2f, false);

            // Tubulação e eletrocalha pelo teto do corredor.
            Pipe(root, "Cano_A", new Vector3(-1.15f, 2.72f, 12.5f), 13f, 0.09f, true);
            Pipe(root, "Cano_B", new Vector3(-0.9f, 2.8f, 12.5f), 13f, 0.05f, false);
            Trim(root, "Eletrocalha_Corredor", new Vector3(1.2f, 2.78f, 12.5f), new Vector3(0.3f, 0.08f, 13f), Surface.Trim);

            var trigger = PropFactory.CreateTrigger(root, "Gatilho_Corredor", new Vector3(0f, 1.2f, 6.8f), new Vector3(2.8f, 2.4f, 0.6f));
            context.CorridorTrigger = trigger.AddComponent<CorridorTrigger>();

            context.CorridorAnchor = Anchor(root, context, "corridor_mid", new Vector3(0f, 0f, 10.5f), 0f);
            context.SubscriberAnchor = Anchor(root, context, "corridor_end", new Vector3(0f, 0f, 17.9f), 180f);
            Anchor(root, context, "spawn_corridor", new Vector3(0f, 0.12f, 15.5f), 0f);
        }

        private static void BuildSwitchboard(Transform root, WorldContext context)
        {
            // Mesa telefônica de cordão: gabinete de madeira, painel de jacks, medidores e prateleira de chaves.
            var wood = ArtLibrary.Get(Surface.DoorWood);
            var panelAnchor = CreateAnchor(root, "Central_Telefonica", new Vector3(-2.7f, 1.95f, 5.8f));
            PropFactory.CreateBox(panelAnchor, "Painel", Vector3.zero, new Vector3(7.4f, 2.55f, 0.26f), ArtLibrary.Get(Surface.MetalPanel));
            PropFactory.CreateBox(panelAnchor, "Gabinete_Topo", new Vector3(0f, 1.42f, -0.06f), new Vector3(7.9f, 0.3f, 0.42f), wood);
            PropFactory.CreateBox(panelAnchor, "Gabinete_Esq", new Vector3(-3.85f, -0.4f, -0.06f), new Vector3(0.2f, 3.4f, 0.42f), wood);
            PropFactory.CreateBox(panelAnchor, "Gabinete_Dir", new Vector3(3.85f, -0.4f, -0.06f), new Vector3(0.2f, 3.4f, 0.42f), wood);
            PropFactory.CreateBox(panelAnchor, "Prateleira_Chaves", new Vector3(0f, -1.05f, -0.42f), new Vector3(7.5f, 0.06f, 0.62f), wood)
                .transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
            PropFactory.CreateBox(panelAnchor, "Base_Gabinete", new Vector3(0f, -1.55f, -0.3f), new Vector3(7.5f, 0.85f, 0.5f), wood);
            for (var i = 0; i < 14; i++)
            {
                PropFactory.CreateCylinder(panelAnchor, "Plugue_" + i, new Vector3(-3.3f + i * 0.5f, -0.98f, -0.5f), new Vector3(0.05f, 0.07f, 0.05f), MaterialLibrary.MetalDark, false);
                PropFactory.CreateSphere(panelAnchor, "Chave_" + i, new Vector3(-3.3f + i * 0.5f, -0.99f, -0.66f), 0.05f, i % 4 == 0 ? MaterialLibrary.Emergency : MaterialLibrary.Metal);
            }

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

            var jackHole = MaterialLibrary.Get("jack_hole", new Color(0.02f, 0.02f, 0.02f), 0.6f);
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

                PropFactory.CreateCylinder(panelAnchor, $"Jack_{ext:000}", new Vector3(x, y, -0.235f), new Vector3(0.07f, 0.01f, 0.07f), jackHole, false)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                var indicator = PropFactory.CreateSphere(panelAnchor, $"Led_{ext:000}", new Vector3(x, y + 0.17f, -0.2f), 0.065f, MaterialLibrary.IndicatorIdle);
                socket.indicator = indicator.GetComponent<Renderer>();
                socket.SetContaminatedMark(ext == 9);
                socket.SetConnected(false);

                PropFactory.CreateLabel(panelAnchor, ext == 0 ? "TRONCO 0" : ext == 9 ? "LINHA 9" : ext.ToString("000"),
                    new Vector3(x, y - 0.2f, -0.2f), Quaternion.identity, 0.035f,
                    ext == 9 ? new Color(0.9f, 0.45f, 0.35f) : ext == 0 ? new Color(0.95f, 0.78f, 0.4f) : new Color(0.72f, 0.7f, 0.62f));

                context.SwitchboardSockets.Add(socket);
            }

            // Medidores analógicos e controles de frequência (projeto.md §7).
            for (var i = 0; i < 4; i++)
            {
                var gx = -2.6f + i * 1.7f;
                PropFactory.CreateCylinder(panelAnchor, "Medidor_" + i, new Vector3(gx, 1.05f, -0.16f), new Vector3(0.36f, 0.02f, 0.36f), ArtLibrary.Get(Surface.Trim), false)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                PropFactory.CreateCylinder(panelAnchor, "Mostrador_" + i, new Vector3(gx, 1.05f, -0.18f), new Vector3(0.3f, 0.005f, 0.3f), MaterialLibrary.Paper, false)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                PropFactory.CreateBox(panelAnchor, "Agulha_" + i, new Vector3(gx, 1.08f, -0.19f), new Vector3(0.012f, 0.13f, 0.005f), MaterialLibrary.Emergency, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, -40f + i * 23f);
                PropFactory.CreateCylinder(panelAnchor, "Dial_Freq_" + i, new Vector3(gx + 0.5f, 1.0f, -0.18f), new Vector3(0.12f, 0.04f, 0.12f), MaterialLibrary.MetalDark, false)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }

            // Cordões pendurados na prateleira (decorativos, sem física).
            var cordMaterial = MaterialLibrary.Get("cord", new Color(0.12f, 0.1f, 0.08f), 0.4f);
            for (var i = 0; i < 5; i++)
            {
                var cord = new GameObject("Cordao_" + i);
                cord.transform.SetParent(panelAnchor, false);
                var lr = cord.AddComponent<LineRenderer>();
                lr.material = cordMaterial;
                lr.widthMultiplier = 0.02f;
                lr.useWorldSpace = false;
                lr.positionCount = 6;
                var sx = -3f + i * 1.3f;
                for (var p = 0; p < 6; p++)
                {
                    var tt = p / 5f;
                    lr.SetPosition(p, new Vector3(sx + tt * 0.3f, -0.98f - Mathf.Sin(tt * Mathf.PI) * 0.35f, -0.52f - tt * 0.05f));
                }
            }

            Label(root, "TRONCO 0 — LINHA MORTA", new Vector3(-6.0f, 2.95f, 5.55f), 0f, 0.055f, new Color(0.9f, 0.78f, 0.45f));
            Label(root, "CENTRAL 214", new Vector3(-2.7f, 3.15f, 5.5f), 0f, 0.075f, new Color(0.82f, 0.78f, 0.62f));
            Label(root, "NÃO CONECTE O 9", new Vector3(1.35f, 2.95f, 5.55f), 0f, 0.045f, new Color(0.82f, 0.36f, 0.28f));

            Document(root, context, "Placa_Tronco_Zero", new Vector3(-5.98f, 1.15f, 5.66f), new Vector3(0.62f, 0.22f, 0.04f), 0f,
                "PLACA DO TRONCO 0", "TRONCO 0 — LINHA MORTA. Fora da rede ativa. Use apenas para reconexões de emergência.", "plate_trunk0");

            var leverAnchor = CreateAnchor(root, "Alavanca_Isolamento", new Vector3(-6.45f, 1.45f, 5.66f));
            PropFactory.CreateBox(leverAnchor, "Base", Vector3.zero, new Vector3(0.32f, 0.62f, 0.18f), ArtLibrary.Get(Surface.Rust));
            var handle = PropFactory.CreateBox(leverAnchor, "Manopla", new Vector3(0f, 0.05f, -0.14f), new Vector3(0.08f, 0.5f, 0.08f), MaterialLibrary.Emergency);
            var lever = leverAnchor.gameObject.AddComponent<IsolatorLever>();
            lever.handle = handle.transform;
            context.IsolatorAnchor = leverAnchor;
            context.Isolator = lever;
            Label(root, "ISOLAMENTO", new Vector3(-6.45f, 1.92f, 5.56f), 0f, 0.04f, new Color(0.9f, 0.5f, 0.38f));

            ArtLibrary.Prop(root, "painted_wooden_chair_01", new Vector3(-4.2f, 0f, 4.4f), 170f);
            ArtLibrary.Prop(root, "painted_wooden_chair_01", new Vector3(-1.3f, 0f, 4.5f), 190f);
        }

        private static void BuildDesk(Transform root, WorldContext context)
        {
            PropFactory.CreateBox(root, "Mesa_Central", new Vector3(3.35f, 0.44f, 4.65f), new Vector3(2.3f, 0.88f, 1.25f), ArtLibrary.Get(Surface.DoorWood));
            PropFactory.CreateBox(root, "Mesa_Tampo", new Vector3(3.35f, 0.895f, 4.65f), new Vector3(2.4f, 0.03f, 1.35f), ArtLibrary.Get(Surface.FloorWood), false);

            var phone = PhoneBody(root, "Telefone_Central", new Vector3(3.35f, 0.97f, 4.55f), 180f);
            var telephone = phone.AddComponent<Phone>();
            telephone.extension = 0;
            telephone.isOperatorPhone = true;
            telephone.locationLabel = "CENTRAL";
            context.OfficePhone = telephone;
            context.Phones[0] = telephone;

            Label(root, "CENTRAL", new Vector3(3.35f, 1.22f, 4.3f), 0f, 0.045f, new Color(0.85f, 0.75f, 0.55f));

            Document(root, context, "Anotacao_Linha_9", new Vector3(2.35f, 0.92f, 4.35f), new Vector3(0.3f, 0.01f, 0.22f), 18f,
                "ANOTAÇÃO", "\"Não conecte o 9.\" A tinta é recente. A letra não é de ninguém da equipe.", "note_line9");

            ArtLibrary.Prop(root, "desk_lamp_arm_01", new Vector3(4.2f, 0.91f, 4.9f), 200f, false);
            ArtLibrary.Prop(root, "painted_wooden_chair_01", new Vector3(3.3f, 0f, 3.6f), 10f);
            ArtLibrary.Prop(root, "drawer_cabinet", new Vector3(6.6f, 0f, 4.2f), -90f);
        }

        private static void BuildArchiveProps(Transform root, WorldContext context)
        {
            for (var i = 0; i < 3; i++)
            {
                var z = -1.6f + i * 1.45f;
                ArtLibrary.Prop(root, "wooden_bookshelf_worn", new Vector3(-14.55f, 0f, z), 90f);
                ArtLibrary.Prop(root, "cardboard_box_01", new Vector3(-14.5f, 2.06f, z + 0.1f), 90f + i * 23f, false);
            }

            var cabinetAnchor = CreateAnchor(root, "Armario_Cabos", new Vector3(-9.2f, 0.72f, 2.45f));
            PropFactory.CreateBox(cabinetAnchor, "Armario", Vector3.zero, new Vector3(1.8f, 1.45f, 0.7f), ArtLibrary.Get(Surface.DoorWood));
            for (var i = 0; i < 3; i++)
            {
                PropFactory.CreateBox(cabinetAnchor, "Gaveta_" + i, new Vector3(0f, 0.4f - i * 0.38f, -0.38f), new Vector3(1.55f, 0.3f, 0.04f), ArtLibrary.Get(Surface.FloorWood), false);
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
            Anchor(root, context, "cable_archive_shelf", new Vector3(-12.6f, 0.45f, -2.4f), 0f);

            Label(cabinetAnchor, "CABOS / 200–219", new Vector3(0f, 0.5f, -0.4f), 0f, 0.032f, new Color(0.85f, 0.75f, 0.55f));

            PropFactory.CreateBox(root, "Mesa_Arquivo", new Vector3(-11.5f, 0.4f, 0.2f), new Vector3(1.8f, 0.8f, 0.9f), ArtLibrary.Get(Surface.DoorWood));
            var register = Document(root, context, "Registro_1987", new Vector3(-11.8f, 0.815f, 0f), new Vector3(0.42f, 0.02f, 0.3f), -22f,
                "REGISTRO DE 1987", "A extensão 214 foi removida do painel após o incêndio. O tronco zero permaneceu ativo. Nenhum técnico assinou a remoção.",
                "call_214_origin");
            register.category = EvidenceCategory.Document;
            ArtLibrary.Prop(root, "desk_lamp_arm_01", new Vector3(-10.8f, 0.8f, 0.4f), 160f, false);
            ArtLibrary.Prop(root, "painted_wooden_chair_01", new Vector3(-11.4f, 0f, -0.6f), 0f);
            ArtLibrary.Prop(root, "cardboard_box_01", new Vector3(-12.6f, 0f, -2.4f), 15f);
            ArtLibrary.Prop(root, "cardboard_box_01", new Vector3(-13.1f, 0f, -2.3f), -20f);

            WallPhone(root, context, 211, "ARQUIVO DE VOZES", new Vector3(-12.4f, 1.45f, -2.84f), 0f);
        }

        private static void BuildCorridorProps(Transform root, WorldContext context)
        {
            var station = PropFactory.CreateBox(root, "Estacao_Bateria", new Vector3(1.38f, 1.2f, 11.2f), new Vector3(0.3f, 0.62f, 0.42f), ArtLibrary.Get(Surface.MetalPanel));
            station.AddComponent<BatteryStation>();
            PropFactory.CreateSphere(root, "Led_Bateria", new Vector3(1.22f, 1.45f, 11.2f), 0.05f, MaterialLibrary.Emissive("battery_led", new Color(0.3f, 0.9f, 0.4f), 1.5f));
            Label(root, "ENERGIA", new Vector3(1.22f, 1.62f, 11.2f), 90f, 0.035f, new Color(0.75f, 0.85f, 0.7f));

            ArtLibrary.Prop(root, "wooden_crate_01", new Vector3(-1.1f, 0f, 11.4f), 90f);
            ArtLibrary.Prop(root, "cardboard_box_01", new Vector3(-1.15f, 0.35f, 11.4f), 75f, false);
            ArtLibrary.Prop(root, "korean_fire_extinguisher_01", new Vector3(1.35f, 0f, 7.2f), -90f);
            ArtLibrary.Prop(root, "utility_box_01", new Vector3(-1.3f, 0f, 16.4f), 90f);
            ArtLibrary.Prop(root, "power_box_01", new Vector3(1.45f, 1.9f, 17.6f), -90f, false);

            Label(root, "ARQUIVO MORTO", new Vector3(1.48f, 2.35f, 14.4f), 90f, 0.03f, new Color(0.65f, 0.62f, 0.55f));
            Label(root, "DEPÓSITO", new Vector3(-1.48f, 2.35f, 9.2f), -90f, 0.03f, new Color(0.65f, 0.62f, 0.55f));
            Label(root, "SUBSOLO", new Vector3(0f, 2.35f, 18.88f), 0f, 0.035f, new Color(0.75f, 0.45f, 0.35f));
            WallPhone(root, context, 216, "CORREDOR", new Vector3(-1.45f, 1.45f, 12.8f), 90f);
        }

        // ================================================================ recepção e área externa

        private static void BuildReception(Transform root, WorldContext context)
        {
            const float h = 3.2f;
            Slab(root, "Piso_Recepcao", new Vector3(0f, -0.06f, -10f), new Vector3(12f, 0.12f, 8f), Surface.FloorTiles);
            Slab(root, "Teto_Recepcao", new Vector3(0f, h + 0.06f, -10f), new Vector3(12f, 0.12f, 8f), Surface.Ceiling);

            // Parede oeste com vão para os alojamentos (z ∈ [-12; -10,8]).
            Wall(root, "Recepcao_Oeste_Sul", new Vector3(-6f, h * 0.5f, -13f), new Vector3(WallThickness, h, 2f));
            Wall(root, "Recepcao_Oeste_Norte", new Vector3(-6f, h * 0.5f, -8.4f), new Vector3(WallThickness, h, 4.8f));
            Lintel(root, "Verga_Alojamentos", new Vector3(-6f, 0f, -11.4f), 1.2f, h, false);
            Wall(root, "Recepcao_Leste", new Vector3(6f, h * 0.5f, -10f), new Vector3(WallThickness, h, 8f));

            // Parede sul com a entrada principal (x ∈ [-0,9; 0,9]).
            Wall(root, "Recepcao_Sul_Esq", new Vector3(-3.45f, h * 0.5f, -14f), new Vector3(5.1f, h, WallThickness));
            Wall(root, "Recepcao_Sul_Dir", new Vector3(3.45f, h * 0.5f, -14f), new Vector3(5.1f, h, WallThickness));
            Lintel(root, "Verga_Entrada", new Vector3(0f, 0f, -14f), 1.8f, h, true);
            Door(root, context, "porta_entrada", new Vector3(-0.9f, 0f, -14f), 1.8f, true, false, "", Surface.DoorMetal).openAngle = -95f;
            DoorFrame(root, new Vector3(0f, 0f, -14f), 1.8f, true);
            DoorFrame(root, new Vector3(-6f, 0f, -11.4f), 1.2f, false);

            PropFactory.CreateBox(root, "Balcao", new Vector3(2.6f, 0.55f, -9.2f), new Vector3(3.2f, 1.1f, 0.8f), ArtLibrary.Get(Surface.DoorWood));
            PropFactory.CreateBox(root, "Balcao_Tampo", new Vector3(2.6f, 1.125f, -9.2f), new Vector3(3.35f, 0.05f, 0.95f), ArtLibrary.Get(Surface.FloorWood), false);
            ArtLibrary.Prop(root, "painted_wooden_chair_01", new Vector3(2.6f, 0f, -8.3f), 180f);
            for (var i = 0; i < 3; i++)
            {
                ArtLibrary.Prop(root, "painted_wooden_chair_01", new Vector3(-4.9f, 0f, -9.6f + i * 0.75f), 90f);
            }

            ArtLibrary.Prop(root, "korean_fire_extinguisher_01", new Vector3(5.7f, 0f, -13.5f), -90f);
            ArtLibrary.Prop(root, "vintage_telephone_wall_clock", new Vector3(5.89f, 2.2f, -10f), -90f, false);
            Label(root, "03:12 — PARADO", new Vector3(5.86f, 1.85f, -10f), 90f, 0.025f, new Color(0.72f, 0.7f, 0.6f));
            Label(root, "SERRA DO SILÊNCIO — RENALTA", new Vector3(0f, 2.75f, -6.14f), 0f, 0.05f, new Color(0.82f, 0.78f, 0.62f));
            ArtLibrary.Prop(root, "drawer_cabinet", new Vector3(4.9f, 0f, -6.4f), 180f);

            // Rádio de apoio: Helena repete o briefing.
            var radio = ArtLibrary.Prop(root, "vintage_radio_transceiver", new Vector3(1.6f, 1.15f, -9.25f), 180f, true, 1f, new Color(0.15f, 0.17f, 0.15f));
            var radioSwitch = radio.AddComponent<MissionSwitch>();
            radioSwitch.eventId = "radio";
            radioSwitch.label = "Ouvir o rádio de apoio";
            context.Switches["radio"] = radioSwitch;

            Document(root, context, "Manual_Operacao", new Vector3(3.3f, 1.16f, -9.1f), new Vector3(0.32f, 0.03f, 0.24f), 12f,
                "MANUAL DE OPERAÇÃO", "\"Conexões de emergência usam o tronco zero. Nunca use a linha 9 para tráfego.\" Assinado: Í. Vilar.", "manual_trunk0");
            Document(root, context, "Quadro_Planta", new Vector3(5.88f, 1.6f, -12.2f), new Vector3(0.05f, 1.0f, 1.4f), 0f,
                "PLANTA DA ESTAÇÃO", "Recepção, alojamentos, central, salas de operação, arquivo de vozes, corredor técnico, depósito, arquivo morto e acesso ao subsolo. A central original aparece riscada a lápis.",
                "map_board").category = EvidenceCategory.Location;

            WallPhone(root, context, 205, "RECEPÇÃO", new Vector3(-5.86f, 1.45f, -8.2f), 90f);
            Anchor(root, context, "cable_reception", new Vector3(3.1f, 1.2f, -9.3f), 0f);
        }

        private static void BuildExterior(Transform root, WorldContext context)
        {
            // Pátio da estação de retransmissão, cercado. Noite, névoa, torre com baliza vermelha.
            Slab(root, "Terreno", new Vector3(0f, -0.08f, -25.2f), new Vector3(34f, 0.12f, 21.6f), Surface.Ground);
            Slab(root, "Caminho", new Vector3(0f, -0.01f, -25f), new Vector3(3f, 0.02f, 21f), Surface.Asphalt);

            // Fachada de dois pavimentos (face externa; o interior não muda).
            const float facadeZ = -14.27f;
            // A fachada vai até a cerca (x = ±17,2): não sobra fresta para sair do mapa.
            Wall(root, "Fachada_Oeste", new Vector3(-9.05f, 3f, facadeZ), new Vector3(16.3f, 6f, 0.34f), WallKind.Facade);
            Wall(root, "Fachada_Leste", new Vector3(9.05f, 3f, facadeZ), new Vector3(16.3f, 6f, 0.34f), WallKind.Facade);
            Wall(root, "Fachada_Verga", new Vector3(0f, 4.05f, facadeZ), new Vector3(1.8f, 3.9f, 0.34f), WallKind.Facade);
            Wall(root, "Fachada_Lateral_Oeste", new Vector3(-16f, 3f, -10f), new Vector3(0.34f, 6f, 8.6f), WallKind.Facade);
            Wall(root, "Fachada_Lateral_Leste", new Vector3(16f, 3f, -10f), new Vector3(0.34f, 6f, 8.6f), WallKind.Facade);
            Trim(root, "Fachada_Cornija", new Vector3(0f, 6.1f, facadeZ - 0.12f), new Vector3(34.6f, 0.25f, 0.6f), Surface.MetalPanel);
            Slab(root, "Telhado", new Vector3(0f, 6.1f, -10f), new Vector3(32.4f, 0.2f, 8.6f), Surface.Rust);
            Trim(root, "Marquise", new Vector3(0f, 2.75f, -15.2f), new Vector3(3.2f, 0.12f, 1.6f), Surface.Rust);

            // Janelas do segundo pavimento, tapadas com tábuas.
            var windowDark = MaterialLibrary.Get("window_dark", new Color(0.02f, 0.025f, 0.03f), 0.9f);
            for (var i = 0; i < 6; i++)
            {
                var wx = -13f + i * 5.2f;
                if (Mathf.Abs(wx) < 2f) continue;
                PropFactory.CreateBox(root, "Janela_" + i, new Vector3(wx, 4.3f, facadeZ - 0.18f), new Vector3(1.4f, 1.2f, 0.04f), windowDark, false);
                for (var p = 0; p < 3; p++)
                {
                    PropFactory.CreateBox(root, $"Tabua_{i}_{p}", new Vector3(wx, 3.95f + p * 0.35f, facadeZ - 0.22f), new Vector3(1.6f, 0.16f, 0.04f), ArtLibrary.Get(Surface.DoorWood), false)
                        .transform.localRotation = Quaternion.Euler(0f, 0f, (p - 1) * 7f);
                }
            }

            Label(root, "ESTAÇÃO DE RETRANSMISSÃO SERRA DO SILÊNCIO", new Vector3(0f, 3.25f, facadeZ - 0.2f), 180f, 0.06f, new Color(0.78f, 0.74f, 0.62f));
            Label(root, "RENALTA — REDE NACIONAL DE TELECOMUNICAÇÕES ANALÓGICAS", new Vector3(0f, 2.98f, facadeZ - 0.2f), 180f, 0.03f, new Color(0.6f, 0.58f, 0.5f));
            ArtLibrary.Prop(root, "korean_public_payphone_01", new Vector3(3.4f, 1.0f, facadeZ - 0.29f), 180f, false);
            Label(root, "TELEFONE PÚBLICO", new Vector3(3.4f, 1.9f, facadeZ - 0.2f), 180f, 0.025f, new Color(0.7f, 0.68f, 0.6f));

            // Cerca de chapa ondulada com postes; portão fechado ao sul.
            Fence(root, "Cerca_Oeste", new Vector3(-17f, 0f, -25.2f), new Vector3(0.06f, 2.4f, 21.6f));
            Fence(root, "Cerca_Leste", new Vector3(17f, 0f, -25.2f), new Vector3(0.06f, 2.4f, 21.6f));
            Fence(root, "Cerca_Sul", new Vector3(0f, 0f, -36f), new Vector3(34f, 2.4f, 0.06f));
            Label(root, "PORTÃO — ACESSO RESTRITO", new Vector3(0f, 2.6f, -35.9f), 0f, 0.035f, new Color(0.8f, 0.4f, 0.32f));

            BuildAntenna(root, context, new Vector3(10.5f, 0f, -28f), 18f);

            // Abrigo de equipamentos e sucata no pátio.
            Wall(root, "Abrigo", new Vector3(-11f, 1.3f, -27f), new Vector3(4f, 2.6f, 3f), WallKind.Metal);
            Trim(root, "Abrigo_Telhado", new Vector3(-11f, 2.68f, -27f), new Vector3(4.4f, 0.1f, 3.4f), Surface.Corrugated);
            ArtLibrary.Prop(root, "rollershutter_door", new Vector3(-10.7f, 0f, -28.53f), 180f, false, 0.85f);
            ArtLibrary.Prop(root, "portable_generator", new Vector3(-8.4f, 0f, -26.2f), 30f);
            ArtLibrary.Prop(root, "barrel_03", new Vector3(-8.2f, 0f, -28.6f), 0f);
            ArtLibrary.Prop(root, "barrel_03", new Vector3(-7.5f, 0f, -29.2f), 40f);
            ArtLibrary.Prop(root, "metal_jerrycan", new Vector3(-8.9f, 0f, -29.3f), 70f);
            ArtLibrary.Prop(root, "old_military_crate", new Vector3(6f, 0f, -18.5f), 12f);
            ArtLibrary.Prop(root, "wooden_crate_01", new Vector3(6.2f, 0.3f, -18.5f), -5f);

            // Postes: o pátio é escuro, mas legível.
            ArtLibrary.Prop(root, "street_lamp_01", new Vector3(-3.2f, 0f, -22f), 90f);
            ArtLibrary.Prop(root, "street_lamp_01", new Vector3(3.2f, 0f, -31f), -90f);
            LightingRig.SpotDown(root, context, "Luz_Poste_A", new Vector3(-2.9f, 3.7f, -22f), new Color(1f, 0.78f, 0.5f), 2.2f, 11f, 120f, true);
            LightingRig.SpotDown(root, context, "Luz_Poste_B", new Vector3(2.9f, 3.7f, -31f), new Color(0.75f, 0.85f, 1f), 1.6f, 10f, 120f, false);
            LightingRig.SpotDown(root, context, "Luz_Marquise", new Vector3(0f, 2.6f, -15.3f), new Color(1f, 0.72f, 0.45f), 1.8f, 7f, 110f, true);

            Anchor(root, context, "spawn_reception", new Vector3(0f, 0.12f, -27f), 0f);
        }

        private static void BuildAntenna(Transform root, WorldContext context, Vector3 basePosition, float height)
        {
            var anchor = CreateAnchor(root, "Torre_Retransmissao", basePosition);
            var rust = ArtLibrary.Get(Surface.Rust);
            const float half = 0.8f;
            var corners = new[] { new Vector3(-half, 0f, -half), new Vector3(half, 0f, -half), new Vector3(half, 0f, half), new Vector3(-half, 0f, half) };
            foreach (var corner in corners)
            {
                PropFactory.CreateBox(anchor, "Montante", corner + Vector3.up * height * 0.5f, new Vector3(0.1f, height, 0.1f), rust).isStatic = true;
            }

            // Treliça: travessas horizontais a cada 2 m e diagonais alternadas entre elas.
            for (var level = 0f; level <= height - 2f; level += 2f)
            {
                for (var side = 0; side < 4; side++)
                {
                    var a = corners[side];
                    var b = corners[(side + 1) % 4];
                    var horizontal = b - a;
                    var travessa = horizontal.x != 0f ? new Vector3(2f * half, 0.05f, 0.05f) : new Vector3(0.05f, 0.05f, 2f * half);
                    PropFactory.CreateBox(anchor, "Travessa", (a + b) * 0.5f + Vector3.up * level, travessa, rust, false).isStatic = true;

                    var flip = (Mathf.RoundToInt(level / 2f) + side) % 2 == 0;
                    var start = (flip ? a : b) + Vector3.up * level;
                    var end = (flip ? b : a) + Vector3.up * (level + 2f);
                    var direction = end - start;
                    var brace = PropFactory.CreateBox(anchor, "Diagonal", (start + end) * 0.5f, new Vector3(0.04f, direction.magnitude, 0.04f), rust, false);
                    brace.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
                    brace.isStatic = true;
                }
            }

            PropFactory.CreateCylinder(anchor, "Mastro", new Vector3(0f, height + 1.5f, 0f), new Vector3(0.08f, 1.5f, 0.08f), rust, false);
            var panelMaterial = MaterialLibrary.Get("antenna_panel", new Color(0.55f, 0.56f, 0.52f), 0.3f);
            for (var i = 0; i < 3; i++)
            {
                var direction = Quaternion.Euler(0f, i * 120f, 0f) * Vector3.forward;
                PropFactory.CreateBox(anchor, "Antena_Painel_" + i, new Vector3(0f, height - 1.2f, 0f) + direction * 0.55f, new Vector3(0.35f, 1.4f, 0.08f), panelMaterial, false)
                    .transform.localRotation = Quaternion.Euler(0f, i * 120f, 0f);
            }

            var beacon = PropFactory.CreateSphere(anchor, "Baliza", new Vector3(0f, height + 3.05f, 0f), 0.22f, MaterialLibrary.Emissive("beacon", new Color(1f, 0.1f, 0.06f), 3f));
            var light = PropFactory.CreatePointLight(anchor, "Luz_Baliza", new Vector3(0f, height + 3.05f, 0f), new Color(1f, 0.12f, 0.08f), 2.2f, 9f);
            context.CeilingLights.Add(light);
            var blink = beacon.AddComponent<BeaconBlink>();
            blink.target = light;

            var wire = MaterialLibrary.Get("guy_wire", new Color(0.2f, 0.2f, 0.19f), 0.4f);
            for (var i = 0; i < 3; i++)
            {
                var guy = new GameObject("Estai_" + i);
                guy.transform.SetParent(anchor, false);
                var lr = guy.AddComponent<LineRenderer>();
                lr.material = wire;
                lr.widthMultiplier = 0.02f;
                lr.useWorldSpace = false;
                lr.positionCount = 2;
                var direction = Quaternion.Euler(0f, i * 120f + 30f, 0f) * Vector3.forward;
                lr.SetPosition(0, new Vector3(0f, height * 0.85f, 0f));
                lr.SetPosition(1, direction * 5.5f);
            }
        }

        private static void Fence(Transform root, string name, Vector3 baseCenter, Vector3 size)
        {
            var panel = PropFactory.CreateBox(root, name, baseCenter + Vector3.up * size.y * 0.5f, size, ArtLibrary.Get(Surface.Corrugated));
            panel.isStatic = true;
            var alongX = size.x > size.z;
            var length = alongX ? size.x : size.z;
            var count = Mathf.CeilToInt(length / 3f);
            var rust = ArtLibrary.Get(Surface.Rust);
            for (var i = 0; i <= count; i++)
            {
                var offset = -length * 0.5f + i * (length / count);
                var position = baseCenter + (alongX ? new Vector3(offset, 1.3f, 0f) : new Vector3(0f, 1.3f, offset));
                PropFactory.CreateCylinder(root, name + "_Poste_" + i, position, new Vector3(0.1f, 1.3f, 0.1f), rust, false).isStatic = true;
            }
        }

        // ================================================================ alojamentos antigos

        private static void BuildDorms(Transform root, WorldContext context)
        {
            const float h = 3f;
            Slab(root, "Piso_Alojamentos", new Vector3(-10f, -0.06f, -10f), new Vector3(8f, 0.12f, 8f), Surface.FloorWood);
            Slab(root, "Teto_Alojamentos", new Vector3(-10f, h + 0.06f, -10f), new Vector3(8f, 0.12f, 8f), Surface.Ceiling);
            Wall(root, "Alojamentos_Oeste", new Vector3(-14f, h * 0.5f, -10f), new Vector3(WallThickness, h, 8f), WallKind.Peeling);
            Wall(root, "Alojamentos_Norte", new Vector3(-10.5f, h * 0.5f, -6f), new Vector3(7f, h, WallThickness), WallKind.Peeling);
            Wall(root, "Alojamentos_Sul", new Vector3(-10f, h * 0.5f, -14f), new Vector3(8f, h, WallThickness), WallKind.Peeling);
            Door(root, context, "porta_alojamentos", new Vector3(-6f, 0f, -12f), 1.2f, false, false, "", Surface.DoorWood).openAngle = -95f;

            for (var i = 0; i < 2; i++)
            {
                ArtLibrary.Prop(root, "old_bed_frame", new Vector3(-12.9f, 0f, -12.6f + i * 3.2f), 90f);
                ArtLibrary.Prop(root, "old_bed_frame", new Vector3(-9.3f, 0f, -12.6f + i * 3.2f), 90f);
            }

            ArtLibrary.Prop(root, "drawer_cabinet", new Vector3(-12.8f, 0f, -6.4f), 180f);
            ArtLibrary.Prop(root, "drawer_cabinet", new Vector3(-11.5f, 0f, -6.4f), 180f);
            ArtLibrary.Prop(root, "old_gas_mask", new Vector3(-13.85f, 2.0f, -8f), 90f, false);
            ArtLibrary.Prop(root, "old_gas_mask", new Vector3(-13.85f, 2.0f, -7.4f), 90f, false);
            ArtLibrary.Prop(root, "wooden_crate_01", new Vector3(-7.1f, 0f, -7.2f), 0f);
            ArtLibrary.Prop(root, "painted_wooden_chair_01", new Vector3(-8f, 0f, -7.6f), -40f);
            Poster(root, "QUARTO 3", "C. PRADO\nA. PRADO (ESTAGIÁRIA)", new Vector3(-13.88f, 1.65f, -9.3f), -90f);

            Document(root, context, "Escala_Quartos", new Vector3(-7.1f, 0.36f, -7.2f), new Vector3(0.3f, 0.01f, 0.22f), 20f,
                "ESCALA DE QUARTOS — 1987", "Quarto 3: C. Prado e A. Prado (estagiária, 1983–1987). Na cama de baixo, um cobertor dobrado que ninguém recolheu.",
                "dorm_roster");
        }

        // ================================================================ salas de operação

        private static void BuildOperations(Transform root, WorldContext context)
        {
            Slab(root, "Piso_Operacao", new Vector3(11f, -0.06f, 0f), new Vector3(8f, 0.12f, 12f), Surface.FloorLinoleum);
            Slab(root, "Teto_Operacao", new Vector3(11f, WallHeight + 0.06f, 0f), new Vector3(8f, 0.12f, 12f), Surface.Ceiling);
            Wall(root, "Operacao_Leste", new Vector3(15f, WallHeight * 0.5f, 0f), new Vector3(WallThickness, WallHeight, 12f));
            Wall(root, "Operacao_Norte", new Vector3(11f, WallHeight * 0.5f, 6f), new Vector3(8f, WallHeight, WallThickness));
            Wall(root, "Operacao_Sul", new Vector3(11f, WallHeight * 0.5f, -6f), new Vector3(8f, WallHeight, WallThickness));
            Door(root, context, "porta_operacao", new Vector3(7f, 0f, -0.9f), 1.8f, false, false, "", Surface.DoorWood);

            // Três posições de operadora: mesa, telefone, luminária e cadeira.
            for (var i = 0; i < 3; i++)
            {
                var z = -3.6f + i * 3.6f;
                ArtLibrary.Prop(root, "metal_office_desk", new Vector3(14.35f, 0f, z), -90f);
                PhoneBody(root, "Telefone_Operacao_" + i, new Vector3(14.3f, 0.86f, z - 0.45f), -90f);
                ArtLibrary.Prop(root, "desk_lamp_arm_01", new Vector3(14.45f, 0.79f, z + 0.6f), -110f, false);
                ArtLibrary.Prop(root, "painted_wooden_chair_01", new Vector3(13.2f, 0f, z), 80f + i * 9f);
            }

            ArtLibrary.Prop(root, "drawer_cabinet", new Vector3(8f, 0f, 5.6f), 180f);
            ArtLibrary.Prop(root, "worn_metal_rack", new Vector3(9.4f, 0f, 5.55f), 180f);
            ArtLibrary.Prop(root, "cardboard_box_01", new Vector3(9.4f, 0.95f, 5.55f), 170f, false);
            ArtLibrary.Prop(root, "vintage_telephone_wall_clock", new Vector3(11f, 2.4f, -5.89f), 0f, false);

            // Mapa da rede regional na parede norte.
            PropFactory.CreateBox(root, "Mapa_Rede", new Vector3(12.3f, 1.8f, 5.88f), new Vector3(2.4f, 1.4f, 0.03f), MaterialLibrary.Paper, false);
            Label(root, "REDE SERRA DO SILÊNCIO — TRONCOS E EXTENSÕES", new Vector3(12.3f, 2.4f, 5.85f), 0f, 0.025f, new Color(0.25f, 0.22f, 0.18f));
            for (var i = 0; i < 7; i++)
            {
                PropFactory.CreateSphere(root, "Alfinete_" + i, new Vector3(11.4f + (i * 0.37f) % 2f, 1.3f + (i * 0.23f) % 0.9f, 5.85f), 0.04f, i == 3 ? MaterialLibrary.Emergency : MaterialLibrary.IndicatorOn);
            }

            Document(root, context, "Registro_Chamadas", new Vector3(14.3f, 0.8f, 0.4f), new Vector3(0.36f, 0.02f, 0.26f), -80f,
                "REGISTRO DE CHAMADAS — 14/03/1987", "02:51 — emergência do povoado, transferida para o tronco 0. 03:05 — queda de tensão. 03:12 — (sem registro). A página seguinte foi arrancada.",
                "call_log_1987");
        }

        // ================================================================ depósito

        private static void BuildDepot(Transform root, WorldContext context)
        {
            const float h = 3.2f;
            Slab(root, "Piso_Deposito", new Vector3(-6.3f, -0.06f, 12.5f), new Vector3(9.4f, 0.12f, 11f), Surface.FloorConcrete);
            Slab(root, "Teto_Deposito", new Vector3(-6.3f, h + 0.06f, 12.5f), new Vector3(9.4f, 0.12f, 11f), Surface.Ceiling);
            Wall(root, "Deposito_Oeste", new Vector3(-11f, h * 0.5f, 12.5f), new Vector3(WallThickness, h, 11f), WallKind.Concrete);
            Wall(root, "Deposito_Norte", new Vector3(-6.3f, h * 0.5f, 18f), new Vector3(9.4f, h, WallThickness), WallKind.Concrete);
            Wall(root, "Deposito_Sul", new Vector3(-6.3f, h * 0.5f, 7f), new Vector3(9.4f, h, WallThickness), WallKind.Concrete);
            Door(root, context, "porta_deposito", new Vector3(-1.6f, 0f, 8.6f), 1.2f, false, false, "", Surface.DoorMetal).openAngle = -95f;

            for (var i = 0; i < 4; i++)
            {
                var z = 9.6f + i * 2.1f;
                ArtLibrary.Prop(root, "worn_metal_rack", new Vector3(-10.55f, 0f, z), 90f);
                ArtLibrary.Prop(root, "cardboard_box_01", new Vector3(-10.55f, 0.98f, z + 0.1f), 90f + i * 31f, false);
                ArtLibrary.Prop(root, "cardboard_box_01", new Vector3(-10.55f, 1.42f, z - 0.2f), 70f + i * 17f, false);
            }

            ArtLibrary.Prop(root, "rollershutter_door", new Vector3(-7.6f, 0f, 17.85f), 180f, false);
            ArtLibrary.Prop(root, "old_military_crate", new Vector3(-6.2f, 0f, 9.2f), 8f);
            ArtLibrary.Prop(root, "wooden_crate_01", new Vector3(-6.3f, 0.3f, 9.1f), -12f);
            ArtLibrary.Prop(root, "wooden_crate_01", new Vector3(-5.4f, 0f, 13.6f), 30f);
            ArtLibrary.Prop(root, "barrel_03", new Vector3(-3.0f, 0f, 16.9f), 0f);
            ArtLibrary.Prop(root, "barrel_03", new Vector3(-3.7f, 0f, 17.2f), 50f);
            ArtLibrary.Prop(root, "metal_jerrycan", new Vector3(-4.3f, 0f, 16.8f), 20f);
            ArtLibrary.Prop(root, "metal_tool_chest", new Vector3(-2.6f, 0f, 12.6f), -90f);
            ArtLibrary.Prop(root, "portable_generator", new Vector3(-7.4f, 0f, 14.6f), 140f);

            // Bobinas de cabo telefônico de reposição.
            var coilWire = MaterialLibrary.Get("coil", new Color(0.08f, 0.07f, 0.06f), 0.5f);
            for (var i = 0; i < 3; i++)
            {
                var position = new Vector3(-8.6f + i * 0.9f, 0.4f, 11.4f);
                PropFactory.CreateCylinder(root, "Bobina_" + i, position, new Vector3(0.8f, 0.2f, 0.8f), ArtLibrary.Get(Surface.DoorWood))
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                PropFactory.CreateCylinder(root, "Bobina_Cabo_" + i, position, new Vector3(0.62f, 0.205f, 0.62f), coilWire, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        // ================================================================ arquivo morto e subsolo

        private static void BuildDeadArchive(Transform root, WorldContext context)
        {
            const float h = 3f;
            Slab(root, "Piso_Arquivo_Morto", new Vector3(5.1f, -0.06f, 14.4f), new Vector3(7f, 0.12f, 6f), Surface.FloorWood);
            Slab(root, "Teto_Arquivo_Morto", new Vector3(5.1f, h + 0.06f, 14.4f), new Vector3(7f, 0.12f, 6f), Surface.Ceiling);
            Wall(root, "Arquivo_Morto_Leste", new Vector3(8.6f, h * 0.5f, 14.4f), new Vector3(WallThickness, h, 6f), WallKind.Peeling);
            Wall(root, "Arquivo_Morto_Norte", new Vector3(5.1f, h * 0.5f, 17.4f), new Vector3(7f, h, WallThickness), WallKind.Peeling);
            Wall(root, "Arquivo_Morto_Sul", new Vector3(5.1f, h * 0.5f, 11.4f), new Vector3(7f, h, WallThickness), WallKind.Peeling);

            Door(root, context, "porta_arquivo_morto", new Vector3(1.6f, 0f, 13.8f), 1.2f, false, true,
                "Trancada. A etiqueta diz: ARQUIVO MORTO — ACESSO COM AUTORIZAÇÃO.", Surface.DoorMetal);

            for (var i = 0; i < 3; i++)
            {
                ArtLibrary.Prop(root, "worn_metal_rack", new Vector3(8.2f, 0f, 12.4f + i * 1.7f), -90f);
                ArtLibrary.Prop(root, "cardboard_box_01", new Vector3(8.2f, 0.98f, 12.4f + i * 1.7f), -90f + i * 13f, false);
            }

            PropFactory.CreateBox(root, "Mesa_Fitas", new Vector3(4.6f, 0.4f, 15.6f), new Vector3(2.2f, 0.8f, 0.9f), ArtLibrary.Get(Surface.DoorWood));
            PropFactory.CreateBox(root, "Gravador_Rolo", new Vector3(4.6f, 0.95f, 15.75f), new Vector3(0.8f, 0.3f, 0.45f), ArtLibrary.Get(Surface.MetalPanel), false);
            for (var i = 0; i < 2; i++)
            {
                PropFactory.CreateCylinder(root, "Rolo_" + i, new Vector3(4.35f + i * 0.5f, 1.11f, 15.8f), new Vector3(0.24f, 0.01f, 0.24f), MaterialLibrary.MetalDark, false);
            }

            ArtLibrary.Prop(root, "portable_cassette_player", new Vector3(5.6f, 0.86f, 15.8f), 180f, false);
            ArtLibrary.Prop(root, "desk_lamp_arm_01", new Vector3(3.6f, 0.8f, 15.9f), 150f, false);
            ArtLibrary.Prop(root, "painted_wooden_chair_01", new Vector3(4.6f, 0f, 14.8f), 0f);

            Tape(root, context, "tape_1979", "FITA 1979", "ch2_tape_1979", new Vector3(4.1f, 0.81f, 15.4f));
            Tape(root, context, "tape_1987", "FITA 1987", "ch2_tape_1987", new Vector3(5.1f, 0.81f, 15.4f));

            Document(root, context, "Escala_1987", new Vector3(3.2f, 0.81f, 15.5f), new Vector3(0.3f, 0.01f, 0.22f), -10f,
                "ESCALA DE PLANTÃO — 1987", "Noite da tempestade. C. Prado — operadora. A. Prado — estagiária. Emergência do povoado: rotear pela 212.",
                "duty_roster_1987");
            Document(root, context, "Ordem_Corte_1998", new Vector3(5.9f, 0.81f, 15.45f), new Vector3(0.3f, 0.01f, 0.22f), 14f,
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
            var ramp = PropFactory.CreateBox(root, "Rampa_Subsolo", new Vector3(0f, -1.5f, 23f) - up * 0.06f, new Vector3(3.2f, 0.12f, length + 0.1f), ArtLibrary.Get(Surface.FloorTunnel));
            ramp.transform.localRotation = rotation;
            ramp.isStatic = true;
            for (var i = 0; i < 2; i++)
            {
                var rail = PropFactory.CreateBox(root, "Corrimao_" + i, new Vector3(i == 0 ? -1.42f : 1.42f, -0.6f, 23f), new Vector3(0.05f, 0.05f, length), ArtLibrary.Get(Surface.Rust), false);
                rail.transform.localRotation = rotation;
            }

            Wall(root, "Rampa_Oeste", new Vector3(-1.6f, 0f, 23f), new Vector3(WallThickness, 6f, 8f), WallKind.Tunnel);
            Wall(root, "Rampa_Leste", new Vector3(1.6f, 0f, 23f), new Vector3(WallThickness, 6f, 8f), WallKind.Tunnel);
            Slab(root, "Teto_Rampa", new Vector3(0f, 3.06f, 23f), new Vector3(3.2f, 0.12f, 8f), Surface.CeilingTunnel);
            Wall(root, "Rampa_Cabeceira", new Vector3(0f, (TunnelFloor + TunnelHeight + 3f) * 0.5f, 27f), new Vector3(3.2f, 3f - (TunnelFloor + TunnelHeight), WallThickness), WallKind.Tunnel);

            Door(root, context, "porta_subsolo", new Vector3(-0.9f, 0f, 19f), 1.8f, true, true,
                "Trancada por fora. Uma corrente nova, sem poeira, passa pela maçaneta.", Surface.DoorMetal);
            Zone(root, context, "ramp", new Vector3(0f, 1.2f, 19.8f), new Vector3(3f, 2.4f, 0.6f));
        }

        private static void BuildTunnels(Transform root, WorldContext context)
        {
            var y = TunnelFloor;
            var cy = y + TunnelHeight * 0.5f;

            Slab(root, "Piso_T1", new Vector3(0f, y - 0.06f, 37f), new Vector3(3.2f, 0.12f, 20f), Surface.FloorTunnel);
            Slab(root, "Teto_T1", new Vector3(0f, y + TunnelHeight + 0.06f, 37f), new Vector3(3.2f, 0.12f, 20f), Surface.CeilingTunnel);
            Wall(root, "T1_Oeste_Sul", new Vector3(-1.6f, cy, 31.2f), new Vector3(WallThickness, TunnelHeight, 8.4f), WallKind.Tunnel);
            Wall(root, "T1_Oeste_Norte", new Vector3(-1.6f, cy, 42.8f), new Vector3(WallThickness, TunnelHeight, 8.4f), WallKind.Tunnel);
            Wall(root, "T1_Leste_Sul", new Vector3(1.6f, cy, 31.2f), new Vector3(WallThickness, TunnelHeight, 8.4f), WallKind.Tunnel);
            Wall(root, "T1_Leste_Norte", new Vector3(1.6f, cy, 42.8f), new Vector3(WallThickness, TunnelHeight, 8.4f), WallKind.Tunnel);
            Wall(root, "T1_Fundo_Esq", new Vector3(-1.25f, cy, 47f), new Vector3(0.7f, TunnelHeight, WallThickness), WallKind.Tunnel);
            Wall(root, "T1_Fundo_Dir", new Vector3(1.25f, cy, 47f), new Vector3(0.7f, TunnelHeight, WallThickness), WallKind.Tunnel);
            Lintel(root, "Verga_T1", new Vector3(0f, y, 47f), 1.8f, TunnelHeight, true, WallKind.Tunnel);

            Slab(root, "Piso_T2_Oeste", new Vector3(-6.8f, y - 0.06f, 37f), new Vector3(10.4f, 0.12f, 3.2f), Surface.FloorTunnel);
            Slab(root, "Piso_T2_Leste", new Vector3(6.8f, y - 0.06f, 37f), new Vector3(10.4f, 0.12f, 3.2f), Surface.FloorTunnel);
            Slab(root, "Teto_T2_Oeste", new Vector3(-6.8f, y + TunnelHeight + 0.06f, 37f), new Vector3(10.4f, 0.12f, 3.2f), Surface.CeilingTunnel);
            Slab(root, "Teto_T2_Leste", new Vector3(6.8f, y + TunnelHeight + 0.06f, 37f), new Vector3(10.4f, 0.12f, 3.2f), Surface.CeilingTunnel);
            Wall(root, "T2_Norte_Oeste", new Vector3(-6.8f, cy, 38.6f), new Vector3(10.4f, TunnelHeight, WallThickness), WallKind.Tunnel);
            Wall(root, "T2_Norte_Leste", new Vector3(6.8f, cy, 38.6f), new Vector3(10.4f, TunnelHeight, WallThickness), WallKind.Tunnel);
            Wall(root, "T2_Sul_Oeste", new Vector3(-6.8f, cy, 35.4f), new Vector3(10.4f, TunnelHeight, WallThickness), WallKind.Tunnel);
            Wall(root, "T2_Sul_Leste", new Vector3(6.8f, cy, 35.4f), new Vector3(10.4f, TunnelHeight, WallThickness), WallKind.Tunnel);

            // Fim oeste: vidro de inspeção com nicho escuro atrás (manifestação por reflexo).
            var glass = PropFactory.CreateBox(root, "Vidro_Inspecao", new Vector3(-12f, cy, 37f), new Vector3(0.06f, TunnelHeight, 3.2f), MaterialLibrary.Glass);
            glass.isStatic = true;
            Trim(root, "Moldura_Vidro", new Vector3(-12f, y + TunnelHeight - 0.05f, 37f), new Vector3(0.16f, 0.1f, 3.2f), Surface.Rust);
            Slab(root, "Piso_Nicho", new Vector3(-13.6f, y - 0.06f, 37f), new Vector3(3.2f, 0.12f, 3.2f), Surface.FloorTunnel);
            Slab(root, "Teto_Nicho", new Vector3(-13.6f, y + TunnelHeight + 0.06f, 37f), new Vector3(3.2f, 0.12f, 3.2f), Surface.CeilingTunnel);
            Wall(root, "Nicho_Oeste", new Vector3(-15.2f, cy, 37f), new Vector3(WallThickness, TunnelHeight, 3.2f), WallKind.Tunnel);
            Wall(root, "Nicho_Norte", new Vector3(-13.6f, cy, 38.6f), new Vector3(3.2f, TunnelHeight, WallThickness), WallKind.Tunnel);
            Wall(root, "Nicho_Sul", new Vector3(-13.6f, cy, 35.4f), new Vector3(3.2f, TunnelHeight, WallThickness), WallKind.Tunnel);
            Anchor(root, context, "reflection", new Vector3(-13.6f, y, 37f), 90f);
            Zone(root, context, "glass", new Vector3(-9.5f, cy, 37f), new Vector3(0.6f, TunnelHeight, 3f));

            Wall(root, "T2_Fundo_Sul", new Vector3(12f, cy, 35.75f), new Vector3(WallThickness, TunnelHeight, 0.7f), WallKind.Tunnel);
            Wall(root, "T2_Fundo_Norte", new Vector3(12f, cy, 38.25f), new Vector3(WallThickness, TunnelHeight, 0.7f), WallKind.Tunnel);
            Lintel(root, "Verga_Energia", new Vector3(12f, y, 37f), 1.8f, TunnelHeight, false, WallKind.Tunnel);

            // Eletrocalhas, feixes de cabos e tubulação ao longo dos túneis.
            Trim(root, "Eletrocalha_T1", new Vector3(1.3f, y + 2.3f, 37f), new Vector3(0.3f, 0.08f, 20f), Surface.Trim);
            Trim(root, "Eletrocalha_T2", new Vector3(0f, y + 2.3f, 38.3f), new Vector3(24f, 0.08f, 0.3f), Surface.Trim);
            var cable = MaterialLibrary.Get("tunnel_cable", new Color(0.05f, 0.05f, 0.05f), 0.5f);
            for (var i = 0; i < 4; i++)
            {
                Pipe(root, "Cabo_T1_" + i, new Vector3(1.2f + i * 0.06f, y + 2.38f, 37f), 20f, 0.025f, false, cable);
                Pipe(root, "Cabo_T2_" + i, new Vector3(0f, y + 2.38f, 38.2f + i * 0.06f), 24f, 0.025f, false, cable, true);
            }

            Pipe(root, "Cano_T1", new Vector3(-1.3f, y + 0.4f, 37f), 20f, 0.12f, true);
            var puddle = MaterialLibrary.Get("puddle", new Color(0.02f, 0.025f, 0.03f), 0.95f, 0.2f);
            for (var i = 0; i < 3; i++)
            {
                PropFactory.CreateBox(root, "Poca_" + i, new Vector3(-0.4f + i * 0.5f, y + 0.005f, 30.5f + i * 6.3f), new Vector3(1.1f, 0.01f, 0.8f), puddle, false);
            }

            ArtLibrary.Prop(root, "barrel_03", new Vector3(-10.6f, y, 36.1f), 20f);
            ArtLibrary.Prop(root, "wooden_crate_01", new Vector3(9.6f, y, 36.0f), 90f);
            ArtLibrary.Prop(root, "old_military_crate", new Vector3(0.6f, y, 45.6f), 90f);
            ArtLibrary.Prop(root, "metal_jerrycan", new Vector3(-1.2f, y, 33.6f), 60f);

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

            var station = PropFactory.CreateBox(root, "Estacao_Bateria_Tunel", new Vector3(-1.38f, y + 1.2f, 41f), new Vector3(0.3f, 0.62f, 0.42f), ArtLibrary.Get(Surface.MetalPanel));
            station.AddComponent<BatteryStation>();
            PropFactory.CreateSphere(root, "Led_Bateria_Tunel", new Vector3(-1.22f, y + 1.45f, 41f), 0.05f, MaterialLibrary.Emissive("battery_led", new Color(0.3f, 0.9f, 0.4f), 1.5f));
        }

        private static void BuildEnergyRoom(Transform root, WorldContext context)
        {
            var y = TunnelFloor;
            const float h = 3.2f;
            Slab(root, "Piso_Energia", new Vector3(16f, y - 0.06f, 37f), new Vector3(8f, 0.12f, 8f), Surface.FloorMetal);
            Slab(root, "Teto_Energia", new Vector3(16f, y + h + 0.06f, 37f), new Vector3(8f, 0.12f, 8f), Surface.CeilingTunnel);
            Wall(root, "Energia_Leste", new Vector3(20f, y + h * 0.5f, 37f), new Vector3(WallThickness, h, 8f), WallKind.Concrete);
            Wall(root, "Energia_Norte", new Vector3(16f, y + h * 0.5f, 41f), new Vector3(8f, h, WallThickness), WallKind.Concrete);
            Wall(root, "Energia_Sul", new Vector3(16f, y + h * 0.5f, 33f), new Vector3(8f, h, WallThickness), WallKind.Concrete);
            Wall(root, "Energia_Oeste_Sul", new Vector3(12f, y + h * 0.5f, 34.55f), new Vector3(WallThickness, h, 3.1f), WallKind.Concrete);
            Wall(root, "Energia_Oeste_Norte", new Vector3(12f, y + h * 0.5f, 39.45f), new Vector3(WallThickness, h, 3.1f), WallKind.Concrete);
            PropFactory.CreateBox(root, "Energia_Verga", new Vector3(12f, y + TunnelHeight + (h - TunnelHeight) * 0.5f, 37f), new Vector3(WallThickness, h - TunnelHeight, 1.8f), ArtLibrary.Get(Surface.WallConcrete), false);

            Door(root, context, "porta_energia", new Vector3(12f, y, 36.1f), 1.8f, false, true,
                "Trancada. Placa: SALA DE ENERGIA — RISCO DE MORTE. O cadeado está quente.", Surface.DoorMetal);
            Label(root, "PERIGO — ALTA TENSÃO", new Vector3(11.88f, y + 2.45f, 37f), -90f, 0.03f, new Color(0.95f, 0.75f, 0.2f));

            PropFactory.CreateBox(root, "Gerador", new Vector3(17.2f, y + 0.9f, 39.2f), new Vector3(3.2f, 1.8f, 1.4f), ArtLibrary.Get(Surface.MetalPanel));
            PropFactory.CreateBox(root, "Gerador_Base", new Vector3(17.2f, y + 0.08f, 39.2f), new Vector3(3.6f, 0.16f, 1.8f), ArtLibrary.Get(Surface.Rust), false);
            PropFactory.CreateCylinder(root, "Gerador_Exaustor", new Vector3(18.4f, y + 2.4f, 39.2f), new Vector3(0.3f, 0.6f, 0.3f), ArtLibrary.Get(Surface.Rust), false);
            ArtLibrary.Prop(root, "portable_generator", new Vector3(13.6f, y, 40.2f), 150f);
            ArtLibrary.Prop(root, "utility_box_01", new Vector3(19.6f, y, 34.2f), -90f);
            ArtLibrary.Prop(root, "power_box_01", new Vector3(19.85f, y + 1.7f, 36.2f), -90f, false);
            ArtLibrary.Prop(root, "metal_tool_chest", new Vector3(13.4f, y, 34f), 90f);

            var ids = new[] { "A", "B", "C" };
            for (var i = 0; i < ids.Length; i++)
            {
                var anchor = CreateAnchor(root, "Disjuntor_" + ids[i], new Vector3(14.6f + i * 1.4f, y + 1.4f, 33.16f));
                PropFactory.CreateBox(anchor, "Caixa", Vector3.zero, new Vector3(0.6f, 0.9f, 0.16f), ArtLibrary.Get(Surface.MetalPanel));
                var handle = PropFactory.CreateBox(anchor, "Manopla", new Vector3(0f, 0f, 0.14f), new Vector3(0.08f, 0.42f, 0.08f), MaterialLibrary.Emergency);
                var breaker = anchor.gameObject.AddComponent<BreakerSwitch>();
                breaker.breakerId = ids[i];
                breaker.handle = handle.transform;
                breaker.SetOn(true);
                context.Breakers[ids[i]] = breaker;
                Label(anchor, "DISJUNTOR " + ids[i], new Vector3(0f, 0.6f, 0.1f), 180f, 0.03f, new Color(0.9f, 0.5f, 0.38f));
            }

            Document(root, context, "Manual_Desligamento", new Vector3(19.86f, y + 1.5f, 34.9f), new Vector3(0.05f, 0.6f, 0.45f), 0f,
                "MANUAL DE DESLIGAMENTO", "\"Para isolar a linha zero: desligue A e C. Nunca desligue B sob carga — B alimenta a iluminação de emergência.\"",
                "breaker_manual");
            WallPhone(root, context, 209, "SALA DE ENERGIA", new Vector3(19.86f, y + 1.45f, 38f), -90f);
            Zone(root, context, "energy", new Vector3(13.2f, y + 1.2f, 37f), new Vector3(0.6f, 2.4f, 2f));
        }

        private static void BuildOriginalCentral(Transform root, WorldContext context)
        {
            var y = TunnelFloor;
            const float h = 3.4f;
            var burnedWood = MaterialLibrary.Get("burned_wood", new Color(0.06f, 0.045f, 0.035f), 0.15f);
            Slab(root, "Piso_Original", new Vector3(0f, y - 0.06f, 52f), new Vector3(12f, 0.12f, 10f), Surface.FloorWood);
            Slab(root, "Teto_Original", new Vector3(0f, y + h + 0.06f, 52f), new Vector3(12f, 0.12f, 10f), Surface.CeilingTunnel);
            Wall(root, "Original_Oeste", new Vector3(-6f, y + h * 0.5f, 52f), new Vector3(WallThickness, h, 10f), WallKind.Burned);
            Wall(root, "Original_Leste", new Vector3(6f, y + h * 0.5f, 52f), new Vector3(WallThickness, h, 10f), WallKind.Burned);
            Wall(root, "Original_Norte", new Vector3(0f, y + h * 0.5f, 57f), new Vector3(12f, h, WallThickness), WallKind.Burned);
            Wall(root, "Original_Sul_Esq", new Vector3(-3.8f, y + h * 0.5f, 47f), new Vector3(4.4f, h, WallThickness), WallKind.Burned);
            Wall(root, "Original_Sul_Dir", new Vector3(3.8f, y + h * 0.5f, 47f), new Vector3(4.4f, h, WallThickness), WallKind.Burned);
            PropFactory.CreateBox(root, "Original_Verga", new Vector3(0f, y + TunnelHeight + (h - TunnelHeight) * 0.5f, 47f), new Vector3(3.2f, h - TunnelHeight, WallThickness), ArtLibrary.Get(Surface.WallBurned), false);

            Door(root, context, "porta_central_original", new Vector3(-0.9f, y, 47f), 1.8f, true, true,
                "Não abre. Do outro lado, um tom de discagem contínuo atravessa a madeira.", Surface.DoorWood);

            var panel = CreateAnchor(root, "Central_1987", new Vector3(0f, y + 1.8f, 56.7f));
            PropFactory.CreateBox(panel, "Painel_1987", Vector3.zero, new Vector3(7f, 2.4f, 0.3f), ArtLibrary.Get(Surface.WallBurned));
            PropFactory.CreateBox(panel, "Gabinete_1987", new Vector3(0f, 1.32f, -0.06f), new Vector3(7.4f, 0.26f, 0.42f), burnedWood);
            for (var i = 0; i < 5; i++)
            {
                var box = Junction(root, $"CIRCUITO {i + 1}", "circuit", i, new Vector3(-2.8f + i * 1.4f, y + 2.05f, 56.5f), 180f);
                context.OriginalCircuits.Add(box);
            }

            var soot = MaterialLibrary.Get("soot", new Color(0.015f, 0.012f, 0.01f), 0.05f);
            for (var i = 0; i < 6; i++)
            {
                PropFactory.CreateBox(root, "Marca_Queimado_" + i, new Vector3(-3f + i * 1.2f, y + 0.005f, 55.6f - (i % 2) * 0.7f), new Vector3(0.9f, 0.01f, 0.6f), soot, false);
            }

            var restore = PropFactory.CreateBox(root, "Plugue_Original", new Vector3(0f, y + 1.15f, 56.45f), new Vector3(0.5f, 0.3f, 0.2f), MaterialLibrary.Emergency);
            var restoreSwitch = restore.AddComponent<MissionSwitch>();
            restoreSwitch.eventId = "restore.trunk";
            restoreSwitch.label = "Religar o tronco zero à linha original";
            context.Switches["restore.trunk"] = restoreSwitch;
            Label(root, "LINHA ORIGINAL — 0", new Vector3(0f, y + 1.48f, 56.5f), 0f, 0.03f, new Color(0.95f, 0.6f, 0.4f));

            // Três conclusões.
            var cut = PropFactory.CreateBox(root, "Chave_Geral", new Vector3(-5.85f, y + 1.4f, 52f), new Vector3(0.18f, 0.9f, 0.6f), ArtLibrary.Get(Surface.Rust));
            PropFactory.CreateBox(root, "Chave_Geral_Manopla", new Vector3(-5.7f, y + 1.55f, 52f), new Vector3(0.12f, 0.4f, 0.08f), MaterialLibrary.Emergency, false);
            Ending(context, cut, EndingKind.Isolate, "Chave geral de corte");
            var plug = PropFactory.CreateBox(root, "Plugue_Ana", new Vector3(1.6f, y + 1.15f, 56.45f), new Vector3(0.4f, 0.3f, 0.2f), MaterialLibrary.Metal);
            Ending(context, plug, EndingKind.Restore, "Completar a chamada de 1987");

            PropFactory.CreateBox(root, "Mesa_Operadora", new Vector3(4.2f, y + 0.4f, 52.5f), new Vector3(1.4f, 0.8f, 2.4f), burnedWood);
            var headset = PropFactory.CreateBox(root, "Fone_Operadora", new Vector3(4.2f, y + 0.86f, 52.9f), new Vector3(0.36f, 0.12f, 0.3f), MaterialLibrary.MetalDark);
            Ending(context, headset, EndingKind.Answer, "Assumir a linha");
            ArtLibrary.Prop(root, "painted_wooden_chair_01", new Vector3(3.2f, y, 52.6f), 95f);

            var zeroPhone = PhoneBody(root, "Telefone_Zero", new Vector3(4.2f, y + 0.86f, 52.0f), -90f);
            var zero = zeroPhone.AddComponent<Phone>();
            zero.extension = 1000;
            zero.isOperatorPhone = false;
            zero.locationLabel = "CENTRAL ORIGINAL";
            context.Phones[1000] = zero;

            PropFactory.CreateBox(root, "Mesa_Registros", new Vector3(-4.2f, y + 0.4f, 50.2f), new Vector3(1.2f, 0.8f, 0.8f), burnedWood);
            Document(root, context, "Registros_Incendio", new Vector3(-4.2f, y + 0.81f, 50.2f), new Vector3(0.36f, 0.02f, 0.26f), 8f,
                "REGISTROS DO INCÊNDIO", "Relatório interno de 1987: a sobrecarga começou no tronco zero, mantido aberto além do protocolo. O laudo de 1979 foi copiado, com outra data.",
                "fire_records");
            Document(root, context, "Cracha_Ana", new Vector3(-1.9f, y + 1.02f, 56.42f), new Vector3(0.1f, 0.14f, 0.02f), 0f,
                "CRACHÁ QUEIMADO", "ANA PRADO — ESTAGIÁRIA DE OPERAÇÃO. O plástico derreteu sobre o painel.", "ana_badge");
            Tape(root, context, "tape_original", "GRAVAÇÃO ORIGINAL", "ch5_original", new Vector3(4.2f, y + 0.81f, 51.5f));

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

        /// <summary>Static batching só do que é realmente estático (paredes, pisos, tetos, frisos).</summary>
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
            AddSector(context, "exterior", "ÁREA EXTERNA DE RETRANSMISSÃO", new Vector3(0f, 6f, -25.2f), new Vector3(34.2f, 16f, 21.6f), "externo",
                new[] { "reception" }, new Vector2(0f, -3.4f));
            AddSector(context, "reception", "RECEPÇÃO", new Vector3(0f, 1.5f, -10f), new Vector3(12f, 4f, 8f), "recepcao",
                new[] { "central", "dorms", "exterior" }, new Vector2(0f, -2f));
            AddSector(context, "dorms", "ALOJAMENTOS ANTIGOS", new Vector3(-10f, 1.5f, -10f), new Vector3(8f, 4f, 8f), "arquivo",
                new[] { "reception" }, new Vector2(-1.6f, -2f));
            AddSector(context, "central", "SALA DA CENTRAL", new Vector3(0f, 1.5f, 0f), new Vector3(14f, 4f, 12f), "sala_central",
                new[] { "reception", "archive", "corridor", "operations" }, new Vector2(0f, 0f));
            AddSector(context, "operations", "SALAS DE OPERAÇÃO", new Vector3(11f, 1.5f, 0f), new Vector3(8f, 4f, 12f), "sala_central",
                new[] { "central" }, new Vector2(1.8f, 0f));
            AddSector(context, "archive", "ARQUIVO DE VOZES", new Vector3(-11f, 1.5f, 0f), new Vector3(8f, 4f, 6f), "arquivo",
                new[] { "central" }, new Vector2(-2f, 0f));
            AddSector(context, "corridor", "CORREDOR TÉCNICO", new Vector3(0f, 1.5f, 12.5f), new Vector3(3.2f, 4f, 13f), "corredor",
                new[] { "central", "dead_archive", "stairs", "depot" }, new Vector2(0f, 2f));
            AddSector(context, "depot", "DEPÓSITO", new Vector3(-6.3f, 1.5f, 12.5f), new Vector3(9.4f, 4f, 11f), "deposito",
                new[] { "corridor" }, new Vector2(-1.6f, 2f));
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
                    case "energy":
                    case "depot": zone.reverbPreset = AudioReverbPreset.Hangar; break;
                    case "original": zone.reverbPreset = AudioReverbPreset.Cave; break;
                    case "exterior": zone.reverbPreset = AudioReverbPreset.Plain; break;
                    case "archive":
                    case "dead_archive":
                    case "dorms": zone.reverbPreset = AudioReverbPreset.Livingroom; break;
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

        /// <summary>
        /// Parede do kit. Paredes institucionais inteiras ganham barrado escuro até 1,1 m, friso
        /// metálico e rodapé (ART_DIRECTION §3: verdes institucionais envelhecidos).
        /// </summary>
        private static void Wall(Transform root, string name, Vector3 position, Vector3 size, WallKind kind = WallKind.Institutional)
        {
            var bottom = position.y - size.y * 0.5f;
            var onFloor = Mathf.Abs(bottom) < 0.05f || Mathf.Abs(bottom - TunnelFloor) < 0.05f;
            var fullHeight = size.y > 1.6f && onFloor;
            if (kind != WallKind.Institutional || !fullHeight)
            {
                var single = PropFactory.CreateBox(root, name, position, size, ArtLibrary.Get(SurfaceOf(kind)));
                single.isStatic = true;
                if (fullHeight && kind != WallKind.Facade && kind != WallKind.Metal) Baseboard(root, name, position, size, bottom);
                return;
            }

            const float lower = 1.1f;
            var alongX = size.x >= size.z;
            var lowerGo = PropFactory.CreateBox(root, name + "_Barrado", new Vector3(position.x, bottom + lower * 0.5f, position.z),
                new Vector3(size.x, lower, size.z), ArtLibrary.Get(Surface.WallLower));
            var upperHeight = size.y - lower;
            var upperGo = PropFactory.CreateBox(root, name, new Vector3(position.x, bottom + lower + upperHeight * 0.5f, position.z),
                new Vector3(size.x, upperHeight, size.z), ArtLibrary.Get(Surface.WallUpper));
            lowerGo.isStatic = true;
            upperGo.isStatic = true;

            var railSize = alongX ? new Vector3(size.x, 0.05f, size.z + 0.05f) : new Vector3(size.x + 0.05f, 0.05f, size.z);
            Trim(root, name + "_Friso", new Vector3(position.x, bottom + lower, position.z), railSize, Surface.Trim);
            Baseboard(root, name, position, size, bottom);
        }

        private static void Baseboard(Transform root, string name, Vector3 position, Vector3 size, float bottom)
        {
            var alongX = size.x >= size.z;
            var baseSize = alongX ? new Vector3(size.x, 0.12f, size.z + 0.04f) : new Vector3(size.x + 0.04f, 0.12f, size.z);
            Trim(root, name + "_Rodape", new Vector3(position.x, bottom + 0.06f, position.z), baseSize, Surface.Trim);
        }

        private static Surface SurfaceOf(WallKind kind)
        {
            switch (kind)
            {
                case WallKind.Peeling: return Surface.WallPeeling;
                case WallKind.Concrete: return Surface.WallConcrete;
                case WallKind.Tunnel: return Surface.WallTunnel;
                case WallKind.Burned: return Surface.WallBurned;
                case WallKind.Metal: return Surface.MetalPanel;
                case WallKind.Facade: return Surface.WallFacade;
                default: return Surface.WallUpper;
            }
        }

        private static void Slab(Transform root, string name, Vector3 position, Vector3 size, Surface surface)
        {
            var slab = PropFactory.CreateBox(root, name, position, size, ArtLibrary.Get(surface));
            slab.isStatic = true;
        }

        private static void Trim(Transform root, string name, Vector3 position, Vector3 size, Surface surface)
        {
            var trim = PropFactory.CreateBox(root, name, position, size, ArtLibrary.Get(surface), false);
            trim.isStatic = true;
        }

        /// <summary>Cano ou cabo reto ao longo de Z (ou de X quando <paramref name="alongX"/>).</summary>
        private static void Pipe(Transform root, string name, Vector3 center, float length, float radius, bool rusty, Material material = null, bool alongX = false)
        {
            var pipe = PropFactory.CreateCylinder(root, name, center, new Vector3(radius * 2f, length * 0.5f, radius * 2f),
                material != null ? material : ArtLibrary.Get(rusty ? Surface.Rust : Surface.Trim), false);
            pipe.transform.localRotation = alongX ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(90f, 0f, 0f);
            pipe.isStatic = true;
        }

        /// <summary>Verga acima de um vão de porta (de 2,1 m até o teto).</summary>
        private static void Lintel(Transform root, string name, Vector3 baseCenter, float width, float ceiling, bool alongX, WallKind kind = WallKind.Institutional)
        {
            var height = ceiling - 2.1f;
            if (height <= 0.01f) return;
            var size = alongX ? new Vector3(width, height, WallThickness) : new Vector3(WallThickness, height, width);
            Wall(root, name, baseCenter + new Vector3(0f, 2.1f + height * 0.5f, 0f), size, kind);
        }

        /// <summary>Batente: dois montantes e uma travessa salientes nas duas faces da parede.</summary>
        private static void DoorFrame(Transform root, Vector3 baseCenter, float width, bool alongX)
        {
            var depth = WallThickness + 0.08f;
            var jamb = alongX ? new Vector3(0.1f, 2.18f, depth) : new Vector3(depth, 2.18f, 0.1f);
            var offset = (width * 0.5f + 0.05f) * (alongX ? Vector3.right : Vector3.forward);
            Trim(root, "Batente_A", baseCenter + offset + Vector3.up * 1.09f, jamb, Surface.DoorWood);
            Trim(root, "Batente_B", baseCenter - offset + Vector3.up * 1.09f, jamb, Surface.DoorWood);
            var head = alongX ? new Vector3(width + 0.3f, 0.1f, depth) : new Vector3(depth, 0.1f, width + 0.3f);
            Trim(root, "Batente_Topo", baseCenter + Vector3.up * 2.15f, head, Surface.DoorWood);
        }

        /// <summary>
        /// Porta com pivô na dobradiça. <paramref name="alongX"/>: a folha se estende em +X a partir
        /// da dobradiça (parede perpendicular a Z); senão, em +Z.
        /// </summary>
        private static DoorInteractable Door(Transform root, WorldContext context, string id, Vector3 hinge, float width,
            bool alongX, bool locked, string lockedMessage, Surface surface)
        {
            var pivot = CreateAnchor(root, "Porta_" + id, hinge);
            var size = alongX ? new Vector3(width, 2.1f, 0.08f) : new Vector3(0.08f, 2.1f, width);
            var center = alongX ? new Vector3(width * 0.5f, 1.05f, 0f) : new Vector3(0f, 1.05f, width * 0.5f);
            PropFactory.CreateBox(pivot, "Folha", center, size, ArtLibrary.Get(surface));
            var knob = alongX ? new Vector3(width - 0.12f, 1.0f, 0f) : new Vector3(0f, 1.0f, width - 0.12f);
            var knobSize = alongX ? new Vector3(0.12f, 0.03f, 0.2f) : new Vector3(0.2f, 0.03f, 0.12f);
            PropFactory.CreateBox(pivot, "Macaneta", knob, knobSize, MaterialLibrary.Metal, false);
            var plate = alongX ? new Vector3(width * 0.5f, 1.7f, 0f) : new Vector3(0f, 1.7f, width * 0.5f);
            var plateSize = alongX ? new Vector3(0.5f, 0.12f, 0.1f) : new Vector3(0.1f, 0.12f, 0.5f);
            PropFactory.CreateBox(pivot, "Placa", plate, plateSize, MaterialLibrary.Paper, false);
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

        /// <summary>
        /// Cartaz de papel. <paramref name="yaw"/> é a direção para a qual a parede "olha" a partir
        /// do leitor (+Z local aponta para dentro da parede; o texto fica legível do lado -Z).
        /// </summary>
        private static void Poster(Transform root, string title, string body, Vector3 position, float yaw)
        {
            var anchor = CreateAnchor(root, "Cartaz_" + title, position);
            anchor.localRotation = Quaternion.Euler(0f, yaw, 0f);
            PropFactory.CreateBox(anchor, "Papel", Vector3.zero, new Vector3(0.6f, 0.8f, 0.01f), MaterialLibrary.Paper, false);
            PropFactory.CreateLabel(anchor, title, new Vector3(0f, 0.28f, -0.01f), Quaternion.identity, 0.03f, new Color(0.55f, 0.12f, 0.08f));
            PropFactory.CreateLabel(anchor, body, new Vector3(0f, -0.02f, -0.01f), Quaternion.identity, 0.016f, new Color(0.18f, 0.16f, 0.13f));
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

        /// <summary>Telefone de mesa de baquelite: corpo, disco e monofone. Devolve o corpo (com colisor).</summary>
        private static GameObject PhoneBody(Transform root, string name, Vector3 position, float yaw)
        {
            var bakelite = MaterialLibrary.Get("bakelite", new Color(0.035f, 0.032f, 0.03f), 0.75f);
            var body = PropFactory.CreateBox(root, name, position, new Vector3(0.26f, 0.12f, 0.3f), bakelite);
            body.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            PropFactory.CreateCylinder(body.transform, "Disco", new Vector3(0f, 0.065f, -0.05f), new Vector3(0.16f, 0.008f, 0.16f), MaterialLibrary.Paper, false);
            PropFactory.CreateCylinder(body.transform, "Disco_Centro", new Vector3(0f, 0.072f, -0.05f), new Vector3(0.06f, 0.004f, 0.06f), MaterialLibrary.Metal, false);
            PropFactory.CreateBox(body.transform, "Monofone", new Vector3(0f, 0.105f, 0.07f), new Vector3(0.24f, 0.035f, 0.05f), bakelite, false);
            PropFactory.CreateBox(body.transform, "Monofone_Ouvido", new Vector3(-0.11f, 0.09f, 0.07f), new Vector3(0.06f, 0.05f, 0.07f), bakelite, false);
            PropFactory.CreateBox(body.transform, "Monofone_Fala", new Vector3(0.11f, 0.09f, 0.07f), new Vector3(0.06f, 0.05f, 0.07f), bakelite, false);
            return body;
        }

        private static Phone WallPhone(Transform root, WorldContext context, int extension, string location, Vector3 position, float yaw)
        {
            var anchor = CreateAnchor(root, $"Telefone_Parede_{extension}", position);
            anchor.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var bakelite = MaterialLibrary.Get("bakelite", new Color(0.035f, 0.032f, 0.03f), 0.75f);
            var body = PropFactory.CreateBox(anchor, "Corpo", Vector3.zero, new Vector3(0.24f, 0.36f, 0.14f), bakelite);
            PropFactory.CreateBox(anchor, "Fone", new Vector3(0.15f, 0f, 0.02f), new Vector3(0.06f, 0.3f, 0.08f), bakelite, false);
            PropFactory.CreateCylinder(anchor, "Disco", new Vector3(0f, -0.04f, 0.075f), new Vector3(0.14f, 0.006f, 0.14f), MaterialLibrary.Paper, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
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
            PropFactory.CreateBox(anchor, "Corpo", Vector3.zero, new Vector3(0.5f, 0.6f, 0.18f), ArtLibrary.Get(Surface.MetalPanel));
            PropFactory.CreateBox(anchor, "Tampa", new Vector3(0f, 0f, 0.1f), new Vector3(0.44f, 0.54f, 0.02f), ArtLibrary.Get(Surface.Rust), false);
            var led = PropFactory.CreateSphere(anchor, "Led", new Vector3(0f, 0.2f, 0.12f), 0.07f, MaterialLibrary.IndicatorIdle);
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
