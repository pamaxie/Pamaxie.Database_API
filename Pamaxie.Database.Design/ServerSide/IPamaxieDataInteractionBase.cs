using Pamaxie.Data;
using System;

namespace Pamaxie.Database.Design
{
    /// <summary>
    /// Defines how interactions with idividual items in a database should be done
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPamaxieDataInteractionBase<T> where T : IDatabaseObject
    {
        /// <summary>
        /// Gets a <see cref="T"/> value from the service
        /// </summary>
        /// <param name="uniqueKey">The unique Identifier of <see cref="T"/> to find a database record by (Id, UniqueKey, etc..)</param>
        /// <returns>Returns a <see cref="T"/> value</returns>
        public T Get(string uniqueKey);

        /// <summary>
        /// Creates a new <see cref="T"/> value in the service, throws exception inside the service if the value already exists
        /// </summary>
        /// <param name="data">The value that should be created</param>
        /// <returns>The created <see cref="T"/> value</returns>
        /// <exception cref="ArgumentException">The value already exist in the service</exception>
        public T Create(T data);

        /// <summary>
        /// Creates a new <see cref="T"/> value to the service
        /// </summary>
        /// <param name="data">The <see cref="T"/> value that should be created</param>
        /// <param name="createdItem">How the <see cref="T"/> value looks like inside the service</param>
        /// <returns><see cref="bool"/> if the operation was successful and the <see cref="T"/> value was created</returns>
        public bool TryCreate(T data, out T createdItem);

        /// <summary>
        /// Updates a <see cref="T"/> value inside the service,
        /// throws an exception if no <see cref="T"/> value with the key of <see cref="value"/> exists
        /// </summary>
        /// <param name="data">The <see cref="T"/> value that should be updated</param>
        /// <returns>The updated <see cref="T"/> value of the service</returns>
        /// <exception cref="ArgumentException">The <see cref="T"/> value does not exist in the service</exception>
        public T Update(T data);

        /// <summary>
        /// Updates a <see cref="T"/> value inside the service
        /// </summary>
        /// <param name="data">The <see cref="T"/> value that should be updated</param>
        /// <param name="updatedItem">The updated <see cref="T"/> value of the service</param>
        /// <returns><see cref="bool"/> if the operation was successful and the <see cref="T"/> value was updated</returns>
        public bool TryUpdate(T data, out T updatedItem);

        /// <summary>
        /// Updates or creates a <see cref="T"/> value inside the service,
        /// returns a <see cref="bool"/> depending if a new <see cref="T"/> value was updated or created inside the service.
        /// </summary>
        /// <param name="data">The <see cref="T"/> value that should be updated or created in the service</param>
        /// <param name="updatedOrCreatedItem">The updated or created <see cref="T"/> value of the service</param>
        /// <returns><see cref="bool"/> if a new value was created</returns>
        /// <exception cref="ArgumentException">if <see cref="value"/> did not contain a valid key</exception>
        public bool UpdateOrCreate(T data, out T updatedOrCreatedItem);

        /// <summary>
        /// Checks if a given key exists in the database (does not read the key out)
        /// </summary>
        /// <param name="uniqueKey"><see cref="string"/> that is searched for if it exists in the database</param>
        /// <returns><see cref="bool"/> if the value could be found</returns>
        public bool Exists(string uniqueKey);

        /// <summary>
        /// Deletes a <see cref="T"/> value inside the service,
        /// returns a <see cref="bool"/> depending if the <see cref="T"/> value was deleted or not
        /// </summary>
        /// <param name="data">The <see cref="T"/> value that should be deleted</param>
        /// <returns><see cref="bool"/> if the operation was successful and the <see cref="T"/> value was deleted</returns>
        public bool Delete(T data);
    }
}