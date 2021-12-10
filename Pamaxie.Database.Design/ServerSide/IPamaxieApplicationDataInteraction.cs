using Pamaxie.Data;

namespace Pamaxie.Database.Design
{
    /// <summary>
    /// Interface that defines Application interactions
    /// </summary>
    public interface IPamaxieApplicationDataInteraction : IPamaxieDataInteractionBase<PamaxieApplication>
    {
        /// <summary>
        /// Get the owner of the <see cref="IPamaxieApplication"/>
        /// </summary>
        /// <param name="value">The <see cref="IPamaxieApplication"/> to get the owner from</param>
        /// <returns>The <see cref="IPamaxieApplication"/>'s owner</returns>
        public PamaxieUser GetOwner(PamaxieApplication value);

        /// <summary>
        /// Enables or Disables the <see cref="IPamaxieApplication"/>
        /// </summary>
        /// <param name="value">The <see cref="IPamaxieApplication"/> that will be enabled or disabled</param>
        /// <returns>The enabled or disabled <see cref="IPamaxieApplication"/> from the database</returns>
        public PamaxieApplication EnableOrDisable(PamaxieApplication value);

        /// <summary>
        /// Verify the Authentication of the <see cref="AppAuthCredentials"/> in the <see cref="IPamaxieApplication"/>
        /// </summary>
        /// <param name="value">The <see cref="AppAuthCredentials"/> from the <see cref="IPamaxieApplication"/></param>
        /// <returns><see cref="bool"/> if the authentication was verified</returns>
        public bool VerifyAuthentication(PamaxieApplication value);
    }
}