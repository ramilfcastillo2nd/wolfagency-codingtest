using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Specifications
{
    public class GetUserProfilesByUserIdsSpecification : BaseSpecification<UserProfile>
    {
        public GetUserProfilesByUserIdsSpecification(Guid[] userIds, CommonSpecParams specParams)
             : base(x =>
                   (userIds.Contains(x.UserId)) &&
                  ((x.FirstName.ToLower().Contains(specParams.Search)) ||
                 (x.LastName.ToLower().Contains(specParams.Search)) || (string.IsNullOrEmpty(specParams.Search)))
                )
        {
            AddInclude(x => x.AppUser);
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
