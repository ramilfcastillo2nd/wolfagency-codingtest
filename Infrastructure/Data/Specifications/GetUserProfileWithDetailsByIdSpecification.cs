using Infrastructure.Entities;

namespace Infrastructure.Data.Specifications
{
    public class GetUserProfileWithDetailsByIdSpecification : BaseSpecification<UserProfile>
    {
        public GetUserProfileWithDetailsByIdSpecification(int userProfileId)
            : base(x =>
                (x.id == userProfileId)
            )
        {
            AddInclude(x => x.AppUser);
        }
    }
}
