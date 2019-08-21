using Devzhou.Data.Repository;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Devzhou.IdentityServer.Profile
{
    public class CustomerProfileService : IProfileService
    {
        private readonly UserRepository _userRepository;

        public CustomerProfileService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            if (context.RequestedClaimTypes.Any())
            {
               //Find user by SubjectId 
                var user = await _userRepository.FirstOrDefaultAsync(x => x.SubjectId == context.Subject.GetSubjectId());
                if (user != null)
                {
                    // Create user claims
                    context.AddRequestedClaims(user.CreateClaims());
                }
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.SubjectId == context.Subject.GetSubjectId());
            context.IsActive = user != null;
        }
    }
}
