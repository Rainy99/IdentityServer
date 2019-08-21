using IdentityModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Devzhou.Data.Entity
{
    public class User : IEntityBase
    {

        public User()
        {
            Id = ObjectId.GenerateNewId().ToString();
            SubjectId = Id;
        }

        /// <summary>
        /// Mongo Object Id
        /// </summary>
        [BsonElement(Order = 0)]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// IdentityServer Subject ID (mandatory)
        /// </summary>
        public string SubjectId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        /// <summary>
        /// Creates an IdentityServer claims principal
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ClaimsPrincipal CreatePrincipal()
        {
            if (string.IsNullOrWhiteSpace(SubjectId)) throw new ArgumentException("SubjectId is mandatory", nameof(SubjectId));
            var claims = CreateClaims();

            var id = new ClaimsIdentity(claims.Distinct(new ClaimComparer()), "IdentityServer4", JwtClaimTypes.Name, JwtClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }

        /// <summary>
        /// Create claims
        /// </summary>
        /// <returns></returns>

        public List<Claim> CreateClaims()
        {
            var claims = new List<Claim> { new Claim(JwtClaimTypes.Subject, SubjectId) };

            if (string.IsNullOrWhiteSpace(UserName))
            {
                claims.Add(new Claim(JwtClaimTypes.Name, UserName));
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                claims.Add(new Claim(JwtClaimTypes.Email, Email));
            }

            return claims;
        }

    }
}
