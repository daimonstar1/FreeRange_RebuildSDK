namespace FRG.Core {

    /// <summary>
    /// An object that can be reused.
    /// </summary>
    public interface ICapacity
    {
        /// <summary>
        /// The maximum potential size of the collection, stream, etc before a reallocation must occur.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Reserve at least the given capacity.
        /// </summary>
        void EnsureCapacity(int capacity);

        /// <summary>
        /// Remove any excess beyond the current size.
        /// </summary>
        void TrimExcess();
    }
}
