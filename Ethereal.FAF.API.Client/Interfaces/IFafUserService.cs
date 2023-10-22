using Refit;

namespace Ethereal.FAF.API.Client
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFafUserService
    {
        /// <summary>
        /// Update account password
        /// </summary>
        /// <param name="currentPassword">Current Password</param>
        /// <param name="newPassword">New password</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Post("/users/changePassword")]
        public Task<ApiResponse<object>> ChangePassword(string currentPassword, string newPassword, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update account email
        /// </summary>
        /// <param name="newEmail">New Email</param>
        /// <param name="currentPassword">Current password</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Post("/users/changeEmail")]
        public Task<HttpResponseMessage> ChangeEmail(string newEmail, string currentPassword, CancellationToken cancellationToken = default);
    }
}
