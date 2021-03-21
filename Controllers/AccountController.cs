using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grup_gadu_api.Data;
using grup_gadu_api.DTOs;
using grup_gadu_api.Entities;
using grup_gadu_api.Interfaces;

namespace grup_gadu_api.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public AccountController(DataContext context, ITokenService tokenService, IUserRepository userRepository, IMapper mapper)
    {
      _mapper = mapper;
      _userRepository = userRepository;
      _tokenService = tokenService;
      _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {

      if (await UserExists(registerDto.Login)) return BadRequest("Login is taken");

      using HMACSHA512 hmac = new HMACSHA512();
      AppUser user = _mapper.Map<AppUser>(registerDto);

      user.Login = registerDto.Login.ToLower();
      user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
      user.PasswordSalt = hmac.Key;
      
      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      return new UserDto { Login = user.Login, Token = _tokenService.CreateToken(user), Id = user.Id };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
      AppUser user = await _userRepository.GetUserByLoginAsync(loginDto.Login);
      if (user == null) return Unauthorized("Invalid login");

      using HMACSHA512 hmac = new HMACSHA512(user.PasswordSalt);
      byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

      for (int i = 0; i < computedHash.Length; i++)
      {
        if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
      }

      return new UserDto
      {
        Login = user.Login,
        Token = _tokenService.CreateToken(user),
        Id = user.Id
      };
    }

    private async Task<bool> UserExists(string login)
    {
      return await _context.Users.AnyAsync(user => user.Login == login.ToLower());
    }
  }
}