using Infrastructure.Entities;

namespace Infrastructure.Data.Specifications
{
    public class GetUserProfileByUserIdSpecification : BaseSpecification<UserProfile>
    {
        public GetUserProfileByUserIdSpecification(Guid userId)
            : base(s =>
                (s.UserId == userId)
            )
        {

        }
    }
}
