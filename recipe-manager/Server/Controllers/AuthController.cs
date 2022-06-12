using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using shared.Dtos.User;

namespace Recipe_Manager.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;

        public AuthController(IAuthRepository authRepo)
        {
            this._authRepo = authRepo;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegisterDto reqeust)
        {
            var response = await _authRepo.Register(
                new User {Email = reqeust.Email, Name = reqeust.Name}, reqeust.Password
            );

            if (!response.Succes)
            {
                return BadRequest(response);
            }

            return Ok(response);

        }

        [HttpPost("Login")]
        public async Task<ActionResult<ServiceResponse<string>>> Login(UserLoginDto reqeust)
        {
            var response = await _authRepo.Login(
                reqeust.Email, reqeust.Password
            );

            if (!response.Succes)
            {
                return BadRequest(response);
            }

            return Ok(response);

        }


    }
}
