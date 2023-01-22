using Infrastructure.Entities;

namespace Infrastructure.Data.Specifications
{
    public class GetUserProfilesByUserIdsCountSpecification : BaseSpecification<UserProfile>
    {
        public GetUserProfilesByUserIdsCountSpecification(Guid[] userIds, CommonSpecParams specParams)
             : base(x =>
                   (userIds.Contains(x.UserId)) &&
                  ((x.FirstName.ToLower().Contains(specParams.Search)) ||
                 (x.LastName.ToLower().Contains(specParams.Search)) || (string.IsNullOrEmpty(specParams.Search)))
                )
        {

        }
    }
}
