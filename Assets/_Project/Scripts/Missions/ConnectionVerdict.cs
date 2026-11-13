namespace ExtensaoZero.Missions
{
    /// <summary>Resultado de uma conexão física na central, decidido pela missão ativa.</summary>
    public enum ConnectionVerdict
    {
        /// <summary>Sem julgamento local (cliente em rede: o host anuncia o resultado).</summary>
        Neutral,
        /// <summary>Conexão que resolve o objetivo atual.</summary>
        Solution,
        /// <summary>Conexão que atende uma etapa intermediária (rastreio, encaminhamento).</summary>
        Progress,
        /// <summary>Conexão aceita, mas sem resposta.</summary>
        NoAnswer,
        /// <summary>Conexão que contamina o circuito (linha 9 ou circuito marcado).</summary>
        Contaminated
    }
}
