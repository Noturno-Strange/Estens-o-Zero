using System.Collections.Generic;
using UnityEngine;

namespace ExtensaoZero.Telephone
{
    /// <summary>
    /// Catálogo em memória de todas as chamadas e gravações da campanha. Nenhuma voz humana
    /// foi gravada: isPlaceholder é explícito e a UI sempre exibe legendas com o conteúdo real.
    /// Chamadas que dependem da seed são registradas pela missão em <c>SetupWorld</c>, que roda
    /// igual em todos os peers; por isso o host só precisa enviar o id.
    /// </summary>
    public static class CallCatalog
    {
        public const string FirstCallId = "first_call";
        public const string SecondCallId = "second_call";
        public const string BusyLineId = "busy_line";

        private static Dictionary<string, CallDefinition> s_Calls;

        public static CallDefinition FirstCall => Get(FirstCallId);
        public static CallDefinition SecondCall => Get(SecondCallId);
        public static CallDefinition BusyLine => Get(BusyLineId);

        public static CallDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureBuilt();
            return s_Calls.TryGetValue(id, out var call) ? call : null;
        }

        /// <summary>Registra (ou substitui) uma chamada gerada em runtime.</summary>
        public static void Register(CallDefinition call)
        {
            if (call == null || string.IsNullOrEmpty(call.id)) return;
            EnsureBuilt();
            s_Calls[call.id] = call;
        }

        public static IEnumerable<string> Ids
        {
            get
            {
                EnsureBuilt();
                return s_Calls.Keys;
            }
        }

        public static CallDefinition Make(string id, string title, string speaker, string voice, string ambience,
            string signals, string interference, float duration, string dtmf, string group = "")
        {
            var call = ScriptableObject.CreateInstance<CallDefinition>();
            call.name = "Call_" + id;
            call.id = id;
            call.title = title;
            call.voiceSpeaker = speaker;
            call.voice = voice;
            call.ambience = ambience;
            call.signals = signals;
            call.interference = interference;
            call.duration = duration;
            call.dtmf = dtmf;
            call.group = group;
            call.isPlaceholder = true;
            return call;
        }

        private static void EnsureBuilt()
        {
            if (s_Calls != null) return;
            s_Calls = new Dictionary<string, CallDefinition>();
            foreach (var call in BuildAll()) s_Calls[call.id] = call;
        }

        private static IEnumerable<CallDefinition> BuildAll()
        {
            // ---------------------------------------------------------------- prólogo
            yield return Make(FirstCallId, "CHAMADA 1 — EXTENSÃO 214", "DESCONHECIDA",
                "Extensão 214. Está fora do painel. Reconecte ao tronco zero antes que o sinal caia.",
                "Três batidas lentas. Uma porta. Um zumbido de 60 Hz que não para.",
                "DTMF: 2 — 1 — 4, repetido três vezes.",
                "\"...não use a linha nove... não use a linha nove...\" A frase volta, mais baixa, fora de sincronia.",
                30f, "214214214");

            yield return Make(SecondCallId, "CHAMADA 2 — AGRADECIMENTO", "DESCONHECIDA",
                "Obrigada. A linha está estável. Ela já sabe que vocês estão aqui.",
                "Passos no corredor. Param quando a gravação termina.",
                "DTMF: 0 — 0 — 0.",
                "Uma respiração muito perto do microfone. Ninguém fala.",
                22f, "000");

            yield return Make(BusyLineId, "LINHA OCUPADA", "",
                "Estática. Uma frequência de retorno pulsa em intervalos regulares.",
                "Nenhum ambiente identificável. A sala parece menor do que deveria.",
                "Retorno pulsando. Sintonize a frequência indicada para sair.",
                "Um tom de ocupado, longe, como se viesse de outro prédio.",
                20f, "147");

            // ---------------------------------------------------------------- capítulo 1
            yield return Make("ch1_intro", "CENTRAL — TRÁFEGO FANTASMA", "VOZ DE ARQUIVO",
                "Central Serra do Silêncio. Três extensões fora de serviço registram tráfego. Rastreio pelo tronco zero.",
                "Relés disparando sozinhos, um depois do outro, em salas diferentes.",
                "DTMF: 0. Depois, três pulsos curtos.",
                "\"...uma delas não é extensão...\"",
                24f, "0");

            yield return Make("ch1_phantom_205", "EXTENSÃO 205", "DESCONHECIDA",
                "Alô? Alguém na recepção? Estou esperando há muito tempo.",
                "Um relógio de parede. Chuva em vidro. Uma cadeira arrastada.",
                "DTMF: 2 — 0 — 5.",
                "\"...não é a recepção de agora...\"",
                22f, "205");

            yield return Make("ch1_phantom_211", "EXTENSÃO 211", "DESCONHECIDA",
                "Está tudo arquivado. Ninguém lê. Por que vocês vieram ler?",
                "Gavetas de metal. Papel virando sozinho. Fluorescente falhando.",
                "DTMF: 2 — 1 — 1.",
                "\"...a gaveta de cima não fecha...\"",
                22f, "211");

            yield return Make("ch1_phantom_216", "EXTENSÃO 216", "DESCONHECIDA",
                "Eu andei o corredor inteiro. Ele fica mais comprido à noite.",
                "Eco longo. Canos estalando. Passos que voltam.",
                "DTMF: 2 — 1 — 6.",
                "\"...conte as portas...\"",
                22f, "216");

            yield return Make("ch1_source_205", "TELEFONE DA RECEPÇÃO", "DESCONHECIDA",
                "Você atendeu. Ninguém atendia aqui desde que fecharam.",
                "O mesmo relógio, agora atrás de você.",
                "Tom contínuo. Nenhum dígito.",
                "Estática limpa.",
                16f, "");

            yield return Make("ch1_source_211", "TELEFONE DO ARQUIVO", "DESCONHECIDA",
                "As fichas de 1987 estão fora de ordem. Alguém procurou o mesmo nome muitas vezes.",
                "Papel. Uma gaveta fecha devagar.",
                "Tom contínuo. Nenhum dígito.",
                "Estática limpa.",
                16f, "");

            yield return Make("ch1_source_216", "TELEFONE DO CORREDOR", "DESCONHECIDA",
                "Você contou as portas? Tem uma a mais quando ninguém olha.",
                "Passos que param quando você para.",
                "Tom contínuo. Nenhum dígito.",
                "Estática limpa.",
                16f, "");

            // ---------------------------------------------------------------- capítulo 2
            yield return Make("ch2_tape_1979", "FITA — 1979", "CELINA PRADO",
                "Central Serra do Silêncio. Transfira a emergência para a 207 e mantenha o tronco zero livre.",
                "Chuva fraca. Um alarme de incêndio distante, que ninguém desliga.",
                "DTMF: 2 — 0 — 7.",
                "Por baixo da voz, outra voz repete o número, mas erra o último dígito.",
                26f, "207", "tape_diff");

            yield return Make("ch2_tape_1987", "FITA — 1987", "CELINA PRADO",
                "Central Serra do Silêncio. Transfira a emergência para a 212 e mantenha o tronco zero aberto.",
                "Tempestade. Uma voz jovem ao fundo: \"mãe?\"",
                "DTMF: 2 — 1 — 2.",
                "\"...ela trocou o número...\" A fita estica e a frase se arrasta.",
                26f, "212", "tape_diff");

            yield return Make("ch2_reply", "EXTENSÃO 207", "VOZ JOVEM",
                "Mãe, a linha zero ainda está aberta. Não deixa cair.",
                "Painel estalando. Cheiro de borracha queimada descrito por alguém ao fundo.",
                "DTMF: 0 — 1 — 9 — 8 — 7.",
                "A voz de Celina, gravada, responde a uma pergunta que ninguém fez.",
                22f, "01987");

            // ---------------------------------------------------------------- capítulo 3
            // ch3_tunnel é gerada pela missão a partir da seed (caixas contaminadas).
            yield return Make("ch3_after", "TÚNEL — LINHA MORTA", "DESCONHECIDA",
                "Vocês cortaram três de mim. A linha morta respira mais devagar agora.",
                "Água pingando. Um transformador perde o ritmo.",
                "Pulsos lentos, cada vez mais espaçados.",
                "Uma voz conta até seis e para no três.",
                20f, "333");

            // ---------------------------------------------------------------- capítulo 4
            yield return Make("ch4_prediction", "SALA DE ENERGIA — EXTENSÃO 209", "DESCONHECIDA",
                "Daqui a pouco alguém vai abrir a porta da sala de energia. A luz vermelha vai apagar duas vezes.",
                "Uma porta pesada. Duas quedas de tensão.",
                "DTMF: 2 — 0 — 9.",
                "\"...vocês sempre abrem...\"",
                22f, "209");

            yield return Make("ch4_breakers", "SALA DE ENERGIA — INSTRUÇÕES", "DESCONHECIDA",
                "Não desliguem o disjuntor B. Se desligarem, um de vocês fica do outro lado.",
                "Três alavancas. Uma delas faz um som diferente ao descer.",
                "DTMF: 1 — 0 — 1. Pausa. 1 — 0 — 1.",
                "\"...desliguem o B... o B alimenta a linha dela...\"",
                26f, "101101");

            yield return Make("ch4_personal", "CHAMADA PARTICULAR", "VOZ JOVEM",
                "\"Três batidas.\" \"Não use a linha nove.\" \"Silhueta no corredor.\" Vocês anotaram tudo. Eu também.",
                "O som de uma caneta escrevendo em papel, no ritmo de quem está ouvindo.",
                "DTMF: os mesmos tons que vocês ouviram no primeiro dia, na ordem inversa.",
                "Frases do caderno de vocês, lidas com a pausa errada.",
                24f, "412");

            // ---------------------------------------------------------------- capítulo 5
            yield return Make("ch5_original", "GRAVAÇÃO ORIGINAL — 1987", "CELINA E ANA PRADO",
                "Celina: \"Segura a linha, Ana. Não deixa cair.\" Ana: \"Estou segurando. Está quente, mãe.\"",
                "A central queimando. Disjuntores estourando em sequência. Chuva entrando pelo teto.",
                "DTMF: 0 — 0 — 0, sem parar, até a fita acabar.",
                "Uma terceira voz, de longe, pede socorro do povoado. A ligação de emergência nunca terminou.",
                32f, "000000", "original");

            yield return Make("ch5_operator", "EXTENSÃO ZERO", "ANA PRADO",
                "Vocês vieram atender? Faz tanto tempo que eu seguro essa linha.",
                "Nenhum ruído. Um silêncio perfeito demais.",
                "Um tom de discagem antigo, contínuo.",
                "Todas as chamadas da campanha, sobrepostas, muito baixas.",
                24f, "0");
        }
    }
}
