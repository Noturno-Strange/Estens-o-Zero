using UnityEngine;

namespace ExtensaoZero.Telephone
{
    /// <summary>
    /// Catálogo em memória. Nenhuma voz humana foi gravada: o campo isPlaceholder é
    /// explícito e a UI sempre exibe legendas com o conteúdo real.
    /// </summary>
    public static class CallCatalog
    {
        private static CallDefinition s_First;
        private static CallDefinition s_Second;
        private static CallDefinition s_Busy;

        public static CallDefinition FirstCall => s_First ??= BuildFirst();
        public static CallDefinition SecondCall => s_Second ??= BuildSecond();
        public static CallDefinition BusyLine => s_Busy ??= BuildBusy();

        private static CallDefinition BuildFirst()
        {
            var call = ScriptableObject.CreateInstance<CallDefinition>();
            call.name = "Call_First";
            call.id = "first_call";
            call.title = "CHAMADA 1 — EXTENSÃO 214";
            call.voice = "A extensão 214 está fora do painel. Reconecte ao tronco zero antes que o sinal caia.";
            call.ambience = "Três batidas lentas. Uma porta se fecha. Água corre em algum lugar abaixo.";
            call.signals = "DTMF: 2 — 1 — 4. Pulso curto após o quarto tom.";
            call.interference = "\"Não use a linha nove.\" A frase se repete, mais baixa, fora de sincronia.";
            call.duration = 30f;
            call.dtmf = "214";
            call.targetExtension = 214;
            return call;
        }

        private static CallDefinition BuildSecond()
        {
            var call = ScriptableObject.CreateInstance<CallDefinition>();
            call.name = "Call_Second";
            call.id = "second_call";
            call.title = "CHAMADA 2 — AGRADECIMENTO";
            call.voice = "Obrigado. A linha voltou. Ela já sabe que vocês estão aqui.";
            call.ambience = "Passos no corredor. Param quando você para. Recomeçam atrás da parede.";
            call.signals = "Tom contínuo em 214 Hz. Interrupção de 0,5 s a cada 4 s.";
            call.interference = "Uma segunda voz tenta falar por cima: \"não terminem a conexão\".";
            call.duration = 22f;
            call.dtmf = "214";
            call.targetExtension = 214;
            return call;
        }

        private static CallDefinition BuildBusy()
        {
            var call = ScriptableObject.CreateInstance<CallDefinition>();
            call.name = "Call_Busy";
            call.id = "busy_line";
            call.title = "LINHA OCUPADA";
            call.voice = "Estática. Uma frequência de retorno pulsa em intervalos regulares.";
            call.ambience = "Nenhum ambiente identificável. A sala parece menor do que deveria.";
            call.signals = "Retorno em 1 — 4 — 7 Hz. Sintonize a frequência para sair.";
            call.interference = "A sua própria voz, atrasada dois segundos, pedindo para não desligar.";
            call.duration = 20f;
            call.dtmf = "147";
            call.targetExtension = 9;
            return call;
        }
    }
}
