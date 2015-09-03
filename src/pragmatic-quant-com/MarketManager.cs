using System;
using System.Collections.Generic;
using pragmatic_quant_com.Factories;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_com
{
    public sealed class MarketManager : Singleton<MarketManager>
    {
        #region private fields
        private readonly IDictionary<string, Market> marketByIds;
        #endregion
        #region private methods
        private string FormattedId(string id)
        {
            return id.Trim().ToLowerInvariant();
        }
        public MarketManager()
        {
            marketByIds= new Dictionary<string, Market>();
        }
        #endregion
        
        public void SetMarket(string mktId, object[,] marketBag)
        {
            var market = MarketFactory.Instance.Build(marketBag);

            var formatedId = FormattedId(mktId);
            if (marketByIds.ContainsKey(formatedId))
            {
                marketByIds[formatedId] = market;
            }
            else
            {
                lock (this)
                {
                    marketByIds.Add(formatedId, market);
                }
            }
        }
        public bool HasMarket(string mktId)
        {
            return marketByIds.ContainsKey(FormattedId(mktId));
        }
        public Market GetMarket(object mktObj)
        {
            var mktBag = mktObj as object[,];
            if (mktBag != null)
                return MarketFactory.Instance.Build(mktBag);

            if (!(mktObj is string))
                throw new ApplicationException(string.Format("Unable to build market from : {0}", mktObj));
            var mktId = (String) mktObj;

            Market mkt;
            if (!marketByIds.TryGetValue(FormattedId(mktId), out mkt))
            {
                throw new ApplicationException(string.Format("No market for id : {0}", mktId));
            }
            return mkt;
        }
    }
}