using System;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Product.Fixings
{
    public static class FixingParser
    {
        #region private fields
        private const string AssetTemplate = "Asset(name, currency)";
        #endregion
        #region private methods
        private static void ThrowFixingParsingException(string fixingType, string template, string actual)
        {
            throw new Exception(string.Format("Unable to parse {0} fixing type, should be {1} , but was : {2}",
                fixingType, template, actual));
        }
        private static bool TryParseAsset(DateTime date, string[] args, out IFixing asset)
        {
            Currency currency = null;
            bool validAsset = args.Length == 2 && Currency.TryParse(args[1], out currency);
            if (!validAsset)
            {
                asset = null;
                return false;
            }

            var assetId = new AssetId(args[0], currency);
            asset = new EquitySpot(date, assetId);
            return true;
        }
        #endregion

        public static IFixing Parse(string fixingDesc, DateTime date)
        {
            var elems = fixingDesc.Trim().Split('(', ')');

            if (elems.Length < 2)
                throw new Exception("Unable to parse fixing : " + fixingDesc);

            var fixingType = elems[0];
            var args = elems[1].Split(',');

            IFixing fixing;
            bool parsingSucces;
            switch (fixingType.ToLowerInvariant())
            {
                case "asset" :
                    parsingSucces = TryParseAsset(date, args, out fixing);
                    break;

                default :
                    throw new ArgumentException(string.Format("Unknow fixing type : {0}", fixingType));
            }

            if (!parsingSucces)
                ThrowFixingParsingException(fixingType, AssetTemplate, fixingDesc);

            return fixing;
        }
    }
}