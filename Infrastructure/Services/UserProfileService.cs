using Core.DTO;
using Core.Enums;
using Infrastructure.Data.Repositories.Interfaces;
using Infrastructure.Data.Specifications;
using Infrastructure.Entities;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        public UserProfileService(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<UserProfile> GetUserProfileById(int id)
        {
            var userProfile = await _unitOfWork.Repository<UserProfile>().GetByIdAsync(id);
            return userProfile;
        }

        public async Task UpdateUserProfile(UpdateUserProfileInputDto request)
        {
            //Check if id is existing
            var userProfile = await _unitOfWork.Repository<UserProfile>().GetByIdAsync(request.Id);

            userProfile.FirstName = request.FirstName;
            userProfile.LastName = request.LastName;
            userProfile.LastName = request.LastName;

            _unitOfWork.Repository<UserProfile>().Update(userProfile);
            await _unitOfWork.Complete();
        }

        public async Task CreateUserProfile(UserProfile userProfile)
        {
            _unitOfWork.Repository<UserProfile>().Add(userProfile);
            await _unitOfWork.Complete();
        }

        public async Task<UserProfile> GetUserProfileByUserId(Guid userId)
        {
            var specs = new GetUserProfileByUserIdSpecification(userId);
            var userProfile = await _unitOfWork.Repository<UserProfile>().GetEntityWithSpec(specs);
            return userProfile;
        }


        public async Task<IReadOnlyList<UserProfile>> GetUserProfiles(CommonSpecParams specParams)
        {
            var spec = new GetUserProfilesSpecification(specParams);
            var userProfiles = await _unitOfWork.Repository<UserProfile>().ListAsync(spec);
            return userProfiles;
        }

        public async Task<IReadOnlyList<UserProfile>> GetUserProfiles(Guid[] appUserIds, CommonSpecParams specParams)
        {
            var spec = new GetUserProfilesByUserIdsSpecification(appUserIds, specParams);
            var userProfiles = await _unitOfWork.Repository<UserProfile>().ListAsync(spec);
            return userProfiles;
        }

        public async Task<int> GetUserProfilesCount(CommonSpecParams specParams)
        {
            var specs = new GetUserProfilesCountSpecification(specParams);
            var count = await _unitOfWork.Repository<UserProfile>().CountAsync(specs);
            return count;
        }

        public async Task<int> GetUserProfilesCount(Guid[] appUserIds, CommonSpecParams specParams)
        {
            var specs = new GetUserProfilesByUserIdsCountSpecification(appUserIds, specParams);
            var count = await _unitOfWork.Repository<UserProfile>().CountAsync(specs);
            return count;
        }

        public async Task<int> GetRoleByUserProfileId(int userProfileId)
        {
            var specs = new GetUserProfileWithDetailsByIdSpecification(userProfileId);
            var userProfile = await _unitOfWork.Repository<UserProfile>().GetEntityWithSpec(specs);

            var role = await _userManager.GetRolesAsync(userProfile.AppUser);
            var roleValue = role.First().ToLower();
            return (int)Enum.Parse<UserRole>(roleValue);
        }
    }
}

