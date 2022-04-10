using Jwt.Data;
using Jwt.Data.Entities;
using JwtAPI.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace JwtAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly JwtDbContext jwtDbContext;
        private readonly IConfiguration configuration;

        public UserController(JwtDbContext jwtDbContext, IConfiguration configuration)
        {
            this.jwtDbContext = jwtDbContext;
            this.configuration = configuration;
        }

        
        [HttpPost("Register")]
        public async Task<ActionResult> Register(UserDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest();
            }
            CreateHashedPassword(userDto.password, out byte[] passwordHash, out byte[] passwordSalt);
            var user = new User();

            user.Name = userDto.name;
            user.UserName = userDto.userName;
            user.Email = userDto.email;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            
            jwtDbContext.Add(user);
            jwtDbContext.SaveChanges();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginRequest request)
        {
            try
            {              
                var user = jwtDbContext.Set<User>().Where(p => p.UserName == request.userName).FirstOrDefault();

                if (user == null) return BadRequest("User not found.!");

                if(!VerifyPasswordHash(request.password, user.PasswordHash, user.PasswordSalt)) { return BadRequest("Wrong password"); }

                string token = CreateToken(user);

                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest( ex.Message);
            }
           
        }
        private void CreateHashedPassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passowordHash,byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passowordHash);
            }

        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
