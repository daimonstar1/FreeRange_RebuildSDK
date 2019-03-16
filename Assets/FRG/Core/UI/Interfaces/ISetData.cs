namespace FRG.Core
{
    /// <summary>
    /// The main SetData function which PoolObject subclasses should override
    /// </summary>
    public interface ISetData<T> {
        void SetData(T data_);
    }

    internal interface ISetData {
        void SetData(object data);
    }

}