using MediaRatings.Domain.repositories;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _jwtSecret;

        public AuthService(IUserRepository userRepository, string jwtSecret)
        {
            _userRepository = userRepository;
            _jwtSecret = jwtSecret;
        }

        public async Task<UserAccount> RegisterUserAsync(string username, string password)
        {
            var user = await _userRepository.CreateAsync(username, password);
            return user;
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            bool valid = await _userRepository.ValidateCredentialsAsync(username, password);
            if (!valid)
            {
                return null;
            }

            var user = await _userRepository.FindByUsernameAsync(username);

            // create JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user!.UserId.ToString()),
                    new Claim("username", user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
