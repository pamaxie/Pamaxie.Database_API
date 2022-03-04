using System;
using System.Threading.Tasks;
using Pamaxie.Data;

namespace Pamaxie.Database.Extensions.DataInteraction
{
    /// <summary>
    /// Defines how interactions with individual items in a database should be done
    /// </summary>
    /// <typeparam name="T">Storage Type</typeparam>
    /// <typeparam name="T2">Indexing Type</typeparam>
    public interface IPamInteractionBase<T, in T2>
    {
        ///  <summary>
        ///  Gets a <see cref="T"/> value from the service
        ///  </summary>
        ///  <param name="uniqueKey">The unique Identifier of <see cref="T"/> to find a database record by (Id, UniqueKey, etc..)</param>
        ///  <returns>Returns a <see cref="T"/> value</returns>
        public Task<T> GetAsync(T2 uniqueKey);

        /// <summary>
        /// Creates a new <see cref="T"/> value in the service, throws exception inside the service if the value already exists
        /// </summary>
        /// <param name="data">The value that should be created</param>
        /// <returns>The created <see cref="T"/> value</returns>
        /// <exception cref="ArgumentException">The value already exist in the service</exception>
        public Task<bool> CreateAsync(T data);

        /// <summary>
        /// Updates a <see cref="T"/> value inside the service,
        /// throws an exception if no <see cref="T"/> value with the key of <see cref="value"/> exists
        /// </summary>
        /// <param name="data">The <see cref="T"/> value that should be updated</param>
        /// <returns>The updated <see cref="T"/> value of the service</returns>
        /// <exception cref="ArgumentException">The <see cref="T"/> value does not exist in the service</exception>
        public Task<bool> UpdateAsync(T data);

        /// <summary>
        /// Checks if a given key exists in the database (does not read the key out)
        /// </summary>
        /// <param name="uniqueKey"><see cref="string"/> that is searched for if it exists in the database</param>
        /// <returns><see cref="bool"/> if the value could be found</returns>
        public Task<bool> ExistsAsync(T2 uniqueKey);

        /// <summary>
        /// Deletes a <see cref="T"/> value inside the service,
        /// returns a <see cref="bool"/> depending if the <see cref="T"/> value was deleted or not
        /// </summary>
        /// <param name="data">The <see cref="T"/> value that should be deleted</param>
        /// <returns><see cref="bool"/> if the operation was successful and the <see cref="T"/> value was deleted</returns>
        public Task<bool> DeleteAsync(T data);
    }
}