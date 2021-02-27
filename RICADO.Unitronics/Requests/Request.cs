using System.Threading;
using System.Threading.Tasks;

namespace RICADO.Unitronics.Requests
{
    public abstract class Request
    {
        #region Private Fields

        private byte _unitId;

        #endregion


        #region Public Properties

        public byte UnitID => _unitId;

        #endregion


        #region Constructor

        protected Request(UnitronicsPLC plc)
        {
            _unitId = plc.UnitID;
        }

        #endregion


        #region Internal Methods

        internal abstract Task<ProcessRequestResult> ProcessRequest(UnitronicsPLC plc, CancellationToken cancellationToken);

        #endregion
    }
}
