namespace Genometry.NeuralNetworking
{
    public enum ActivationFunction
    {
        /// <summary>
        /// x = y
        /// </summary>
        Lineair,
        /// <summary>
        /// x = ((y &gt;= 1) ? 1 : (y &lt;= -1) ? -1 : y)
        /// </summary>
        LineairTruncated,
        /// <summary>
        /// x = ((y &gt;= 1) ? 1 : (y &lt;= 0) ? 0 : y)
        /// </summary>
        LineairTruncatedAtZero,
        /// <summary>
        /// x = (y &gt;= 0.5 ? 1 : 0)
        /// </summary>
        Binairy,
        /// <summary>
        /// x = (y &gt;= 0 ? 1 : -1)
        /// </summary>
        PositiveNegativeBinairy,
        /// <summary>
        /// x = 1/(1+e^-y)
        /// </summary>
        Sigmoid,
        /// <summary>
        /// x = tanh(y)
        /// </summary>
        HyperbolicTangent,
    }
}
