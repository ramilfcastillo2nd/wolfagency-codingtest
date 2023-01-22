using Core.DTO;
using Infrastructure.Data.Specifications;
using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<int> GetRoleByUserProfileId(int userProfileId);
        Task CreateUserProfile(UserProfile userProfile);
        Task<UserProfile> GetUserProfileByUserId(Guid userId);
        Task UpdateUserProfile(UpdateUserProfileInputDto request);
        Task<IReadOnlyList<UserProfile>> GetUserProfiles(CommonSpecParams specParams);
        Task<int> GetUserProfilesCount(CommonSpecParams specParams);
        Task<IReadOnlyList<UserProfile>> GetUserProfiles(Guid[] appUserIds, CommonSpecParams specParams);
        Task<int> GetUserProfilesCount(Guid[] appUserIds, CommonSpecParams specParams);
        Task<UserProfile> GetUserProfileById(int id);
    }
}
