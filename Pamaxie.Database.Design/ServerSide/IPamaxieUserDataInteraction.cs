using System.Collections.Generic;
using Pamaxie.Data;

namespace Pamaxie.Database.Design
{
    /// <summary>
    /// Interface that defines User interactions
    /// </summary>
    public interface IPamaxieUserDataInteraction : IPamaxieDataInteractionBase<PamaxieUser>
    {
        /// <summary>
        /// Gets a list of <see cref="IPamaxieApplication"/> from a <see cref="IPamaxieUser"/>
        /// </summary>
        /// <param name="value">The key of the <see cref="IPamaxieUser"/> who owns the applications</param>
        /// <returns>A list of all applications the <see cref="IPamaxieUser"/> owns</returns>
        public IEnumerable<PamaxieApplication> GetAllApplications(PamaxieUser value);

        /// <summary>
        /// Verifies the email of the <see cref="IPamaxieUser"/>
        /// </summary>
        /// <param name="value">The <see cref="IPamaxieUser"/> that will have their email verified</param>
        /// <returns><see cref="bool"/> if the operation was successful and the email was verified</returns>
        public bool VerifyEmail(PamaxieUser value);
    }
}