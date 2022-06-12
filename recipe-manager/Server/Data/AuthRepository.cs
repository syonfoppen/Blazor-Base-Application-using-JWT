using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Recipe_Manager.Server.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthRepository(DataContext context, IConfiguration configuration)
        {
            this._context = context;
            this._configuration = configuration;
        }


        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(email.ToLower()));
            if (user == null)
            {
                response.Succes = false;
                response.Message = "Username or password incorrect.";
            }
            else if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                response.Succes = false;
                response.Message = "Username or password incorrect.";
            }
            else
            {
                response.Data = CreateToken(user);
            }
            return response;


        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();

            if (await UserExists(user.Email))
            {
                response.Succes = false;
                response.Message = $"User with email {user.Email} already exists.";
                return response;
            }


            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();


            response.Data = user.Id;
            return response;
        }

        public async Task<bool> UserExists(string email)
        {
            if (await _context.Users.AnyAsync(x => x.Email.ToLower().Equals(email)))
            {
                return true;
            }
            return false;
        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for(int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Admin", "true")
            };

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey( // Convert the loaded key from base64 to bytes.
                source: Convert.FromBase64String(_configuration.GetSection("JWT:PrivateKey").Value), // Use the private key to sign tokens
                bytesRead: out int _); // Discard the out variable 

            var signingCredentials = new SigningCredentials(
            key: new RsaSecurityKey(rsa),
            algorithm: SecurityAlgorithms.RsaSha512 // Important to use RSA version of the SHA algo 
            );

            var token = new JwtSecurityToken(
                            claims: claims,
                            expires: DateTime.Now.AddDays(int.Parse(_configuration.GetSection("JWT:DurationInDays").Value)),
                            signingCredentials: signingCredentials,
                            issuer: _configuration.GetSection("JWT:Issuer").Value,
                            audience: _configuration.GetSection("JWT:Audience").Value
                            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
