using Infrastructure.Entities;

namespace Infrastructure.Data.Specifications
{
    public class GetUserProfilesSpecification : BaseSpecification<UserProfile>
    {
        public GetUserProfilesSpecification(CommonSpecParams specParams)
               : base(x =>
                  ((x.FirstName.Contains(specParams.Search)) &&
                 (x.LastName.Contains(specParams.Search))) || string.IsNullOrEmpty(specParams.Search)
                )
        {
            AddOrderBy(x => x.FirstName);
            ApplyPaging(specParams.PageSize * (specParams.PageIndex - 1), specParams.PageSize);

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort)
                {
                    case "nameAsc":
                        AddOrderBy(p => p.FirstName);
                        break;
                    case "nameDesc":
                        AddOrderByDescending(p => p.FirstName);
                        break;
                    default:
                        AddOrderBy(p => p.FirstName);
                        break;
                }
            }
        }
    }
}
