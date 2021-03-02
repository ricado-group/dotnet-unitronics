using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RICADO.Unitronics.PComA
{
    internal class GetIdentificationResponse : Response
    {
        #region Constants

        private const string OldVersionRegex = "^(.{4})(.)(.{3})(.{3})(.{2})B(.{3})(.{3})(.{2})P(.{3})(.{3})(.{2})F(.)(.)(.{2}).{2}(.{2})(FT(.{5})(.{5}))?$";
        private const string NewVersionRegex = "^(.{6})(.)(.{3})(.{3})(.{2})B(.{3})(.{3})(.{2})P(.{3})(.{3})(.{2})F(.)(.)(.{2}).{2}(.{2})(FT(.{5})(.{5}))?$";
        private const string ShortVersionRegex = "^(.{4})(.)(.)(.{2})(.{2})$";

        #endregion


        #region Private Fields

        private PLCModel _model;
        private Version _version;

        #endregion


        #region Public Properties

        public PLCModel Model => _model;

        public Version Version => _version;

        #endregion


        #region Constructor

        protected GetIdentificationResponse(Request request, Memory<byte> responseMessage) : base(request, responseMessage)
        {
        }

        #endregion


        #region Public Methods

        public static GetIdentificationResponse UnpackResponseMessage(GetIdentificationRequest request, Memory<byte> responseMessage)
        {
            return new GetIdentificationResponse(request, responseMessage);
        }

        #endregion


        #region Protected Methods

        protected override void UnpackMessageDetail(string messageDetail)
        {
            string[] splitInformation;

            if (Regex.IsMatch(messageDetail, OldVersionRegex))
            {
                splitInformation = Regex.Split(messageDetail, OldVersionRegex);
            }
            else if (Regex.IsMatch(messageDetail, NewVersionRegex))
            {
                splitInformation = Regex.Split(messageDetail, NewVersionRegex);
            }
            else if (Regex.IsMatch(messageDetail, ShortVersionRegex))
            {
                splitInformation = Regex.Split(messageDetail, ShortVersionRegex);
            }
            else
            {
                throw new PComAException("The Get Identification Response Message Format was Invalid");
            }

            if (int.TryParse(splitInformation[3], out int majorVersion) == false || int.TryParse(splitInformation[4], out int minorVersion) == false || int.TryParse(splitInformation[5], out int buildVersion) == false)
            {
                throw new PComAException("The Get Identification Response Message Version was Invalid");
            }

            _version = new Version(majorVersion, minorVersion, buildVersion);

            _model = extractPLCModel(splitInformation[1]);
        }

        #endregion


        #region Private Methods

        private static PLCModel extractPLCModel(string modelString)
        {
            if (modelString == null || modelString.Length < 4)
            {
                return PLCModel.Unknown;
            }

            if (modelString.Contains("BOOT"))
            {
                return PLCModel.Unknown;
            }

            switch (modelString)
            {
                case "B1  ":
                case "B1A ":
                case "R1  ":
                case "R1C ":
                case "R2C ":
                case "T   ":
                case "T1  ":
                case "T1C ":
                case "TA2C":
                case "TA3C":
                case "7B1 ":
                case "7B1A":
                case "7R1":
                case "7R1C":
                case "7T  ":
                case "7T1 ":
                case "7T1C":
                case "7TA2":
                case "7TA3":
                    return PLCModel.M90;

                case "1TC2":
                case "1UN2":
                case "1R1 ":
                case "1R2 ":
                case "1R2C":
                case "1T1 ":
                case "1UA2":
                case "1T2C":
                case "8TC2":
                case "8UN2":
                case "8R1 ":
                case "8R2 ":
                case "8R2C":
                case "8T1 ":
                case "8UA2":
                case "8T38":
                case "8T2C":
                case "8R6C":
                case "8R34":
                case "8A19":
                case "8A22":
                case "1T38":
                case "8RZ ":
                    return PLCModel.M91;

                case "JR14":
                case "JR17":
                case "JR10":
                case "JR16":
                case "JT10":
                case "JT17":
                case "JEW1":
                case "JE10":
                case "JR31":
                case "JT40":
                case "JP15":
                case "JE13":
                case "JA24":
                case "JN20":
                case "NR10":
                case "NR16":
                case "NR31":
                case "NT10":
                case "NT18":
                case "NT20":
                case "NT40":
                    return PLCModel.Jazz;

                case "2320":
                    return PLCModel.V230;

                case "2620":
                    return PLCModel.V260;

                case "2820":
                    return PLCModel.V280;

                case "2920":
                    return PLCModel.V290;

                case "VUN2":
                case "VR1 ":
                case "VR2C":
                case "VUA2":
                case "VT1 ":
                case "VT40":
                case "VT2C":
                case "VT38":
                case "WUN2":
                case "WR1 ":
                case "WR2C":
                case "WUA2":
                case "WT1 ":
                case "WT40":
                case "WT2C":
                case "WT38":
                case "WR6C":
                case "WR34":
                case "WA19":
                case "WA22":
                    return PLCModel.V120;

                case "ERC1":
                    return PLCModel.EX_RC1;

                case "5320":
                    return PLCModel.V530;

                case "49C3":
                case "57C3":
                case "49T3":
                case "57T3":
                case "49T2":
                case "57T2":
                case "49T4":
                case "57T4":
                    return PLCModel.V570;

                case "56C3":
                case "56T4":
                case "56T3":
                case "56T2":
                    return PLCModel.V560;

                case "13R2  ":
                case "13R34 ":
                case "13T2  ":
                case "13T38 ":
                case "13RA22":
                case "13TA24":
                case "13B1  ":
                case "13T40 ":
                case "13R6  ":
                case "13TR34":
                case "13TR22":
                case "13TR20":
                case "13TR6 ":
                case "13TU24":
                case "13XXXX":
                    return PLCModel.V130;

                case "35R2  ":
                case "35R34 ":
                case "35T2  ":
                case "35T38 ":
                case "35RA22":
                case "35TA24":
                case "35B1  ":
                case "35T40 ":
                case "35R6  ":
                case "35TR34":
                case "35TR22":
                case "35TR20":
                case "35TR6 ":
                case "35TU24":
                case "35XXXX":
                    return PLCModel.V350;

                case "43RH2 ":
                    return PLCModel.V430;

                case "S3T20 ":
                case "S3TA2 ":
                case "S3R20 ":
                    return PLCModel.Samba35;

                case "S4T20 ":
                case "S4TA2 ":
                case "S4R20 ":
                    return PLCModel.Samba43;

                case "70T2":
                    return PLCModel.V700;

                case "EC15  ":
                    return PLCModel.EXF_RC15;

                case "10T2":
                    return PLCModel.V1040;

                case "12T2":
                    return PLCModel.V1210;
            }

            if (modelString.StartsWith("13"))
            {
                return PLCModel.V130;
            }

            if (modelString.StartsWith("35"))
            {
                return PLCModel.V350;
            }

            if (modelString.StartsWith("43"))
            {
                return PLCModel.V430;
            }

            if (modelString.StartsWith("S3"))
            {
                return PLCModel.Samba35;
            }

            if (modelString.StartsWith("S4"))
            {
                return PLCModel.Samba43;
            }

            if (modelString.StartsWith("S7") || modelString.StartsWith("SO"))
            {
                return PLCModel.Samba70;
            }

            return PLCModel.Unknown;
        }

        #endregion
    }
}
