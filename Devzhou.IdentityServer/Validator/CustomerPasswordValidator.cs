using Devzhou.Data.Repository;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace Devzhou.IdentityServer.Validator
{
    public class CustomerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserRepository _userRepository;

        public CustomerPasswordValidator(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var result = await _userRepository.FirstOrDefaultAsync(x => x.UserName == context.UserName && x.Password == context.Password);
            if (result != null)
            {
                context.Result = new GrantValidationResult(result.SubjectId, "pwd");
            }
            else
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid custom credential");
            }
        }
    }
}
