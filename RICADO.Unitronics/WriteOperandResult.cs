namespace RICADO.Unitronics
{
    public class WriteOperandResult : RequestResult
    {
        #region Constructor

        internal WriteOperandResult(Channels.ProcessMessageResult result) : base(result)
        {
        }

        #endregion
    }
}
