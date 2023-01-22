using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities
{
    public class AppRole : IdentityRole<Guid>
    {    /// <summary>
         /// Initializes a new instance of <see cref="ApplicationRole"/>.
         /// </summary>
         /// <remarks>
         /// The Id property is initialized to from a new GUID string value.
         /// </remarks>
        public AppRole()
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        /// <remarks>
        /// The Id property is initialized to from a new GUID string value.
        /// </remarks>
        public AppRole(string roleName) : base(roleName)
        {

        }


        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        /// <param name="description">Description of the role.</param>
        /// <remarks>
        /// The Id property is initialized to from a new GUID string value.
        /// </remarks>
        public AppRole(string roleName, string description) : base(roleName)
        {
            Description = description;
        }

        /// <summary>
        /// Gets or sets the description for this role.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Navigation property for the users in this role.
        /// </summary>
        public virtual ICollection<IdentityUserRole<Guid>> Users { get; set; }

        /// <summary>
        /// Navigation property for claims in this role.
        /// </summary>
        public virtual ICollection<IdentityRoleClaim<Guid>> Claims { get; set; }
    }
}
