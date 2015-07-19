using System;

namespace pragmatic_quant_model.Basic.Structure
{
    public abstract class Singleton<T> where T : new()
    {
        #region private fields
        private static readonly Lazy<T> value = new Lazy<T>(() => new T(), true);
        #endregion 
        public static T Instance
        {
            get { return value.Value; }
        }
    }
}
