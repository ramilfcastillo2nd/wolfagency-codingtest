using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? HasChangePassword { get; set; }
        public int? LastTwoDigitRegisterConfirmation { get; set; }
        public int? LastTwoDigitForgotPasswordConfirmation { get; set; }
        public virtual ICollection<AppUserToken> RefreshTokens { get; set; }
        /// <summary>
        /// Navigation property for the roles this user belongs to.
        /// </summary>
        public virtual ICollection<IdentityUserRole<Guid>> Roles { get; set; }
        /// <summary>
        /// Navigation property for the claims this user possesses.
        /// </summary>
        public virtual ICollection<IdentityUserClaim<Guid>> Claims { get; set; }
    }
}
