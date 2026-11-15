namespace DAABLike
{
    /// <summary>
    /// Specifies how errors are handled when updating a data set.
    /// </summary>
    public enum UpdateBehavior
    {
        /// <summary>
        /// Stops the update operation when an error occurs.
        /// </summary>
        Standard,

        /// <summary>
        /// Continues the update operation when an error occurs.
        /// </summary>
        /// <remarks>
        /// The error is assigned to the <see cref="System.Data.DataRow.RowError"/>
        /// property of the affected row.
        /// </remarks>
        Continue,

        /// <summary>
        /// Executes the update operation within a transaction and rolls back the transaction if an error occurs.
        /// </summary>
        Transactional,
    }
}
