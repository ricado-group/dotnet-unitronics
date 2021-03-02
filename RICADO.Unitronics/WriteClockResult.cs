namespace RICADO.Unitronics
{
    public class WriteClockResult : RequestResult
    {
        #region Constructor

        internal WriteClockResult(Channels.ProcessMessageResult result) : base(result)
        {
        }

        #endregion
    }
}
