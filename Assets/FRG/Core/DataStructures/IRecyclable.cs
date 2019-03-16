namespace FRG.Core
{
    /// <summary>
    /// An object that can be reused.
    /// </summary>
    public interface IRecyclable
    {
        /// <summary>
        /// Clears the object for reuse.
        /// If it can't be reused, returns false.
        /// </summary>
        bool Recycle();
    }
}
