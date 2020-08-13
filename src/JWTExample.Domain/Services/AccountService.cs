using JWTExample.Data;
using JWTExample.Domain.Helpers.Interfaces;
using JWTExample.Domain.Mappers.Interfaces;
using JWTExample.Domain.Services.Interfaces;
using JWTExample.Models.Auth;
using JWTExample.Models.Entities;
using JWTExample.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace JWTExample.Domain.Services
{
    public class AccountService : IAccountService
    {
        private readonly DataContext _context;
        private readonly IAppSettings _appSettings;
        private readonly IMapper<Account, AccountResponse> _accountResponseMapper;
        private readonly IMapper<CreateRequest, Account> _createRequestMapper;
        private readonly IMapper<Account, AuthenticateResponse> _authMapper;
        private readonly IMapper<RegisterRequest, Account> _registerRequestMapper;
        private readonly IAccountUpdater _accountUpdater;

        public AccountService(
            DataContext context,
            IAppSettings appSettings,
            IMapper<Account, AccountResponse> accountResponseMapper,
            IMapper<CreateRequest, Account> createRequestMapper,
            IMapper<Account, AuthenticateResponse> authMapper,
            IMapper<RegisterRequest, Account> registerRequestMapper,
            IAccountUpdater accountUpdater)
        {
            _context = context;
            _appSettings = appSettings;
            _accountResponseMapper = accountResponseMapper;
            _createRequestMapper = createRequestMapper;
            _authMapper = authMapper;
            _registerRequestMapper = registerRequestMapper;
            _accountUpdater = accountUpdater;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest model, string ipAddress)
        {
            var account = await _context.Accounts.SingleOrDefaultAsync(x => x.Email == model.Email);

            if (account == null || !BC.Verify(model.Password, account.PasswordHash))
                throw new Exception("Email or password is incorrect");

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = generateJwtToken(account);
            var refreshToken = generateRefreshToken(ipAddress);

            // save refresh token
            account.RefreshTokens.Add(refreshToken);
            _context.Update(account);
            await _context.SaveChangesAsync();

            var response = _authMapper.Map(account);
            response.JwtToken = jwtToken;
            response.RefreshToken = refreshToken.Token;
            return response;
        }

        public async Task<AuthenticateResponse> RefreshToken(string token, string ipAddress)
        {
            var (refreshToken, account) = await getRefreshToken(token);

            // replace old refresh token with a new one and save
            var newRefreshToken = generateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            account.RefreshTokens.Add(newRefreshToken);
            _context.Update(account);
            await _context.SaveChangesAsync();

            // generate new jwt
            var jwtToken = generateJwtToken(account);

            var response = _authMapper.Map(account);
            response.JwtToken = jwtToken;
            response.RefreshToken = newRefreshToken.Token;
            return response;
        }

        public async Task RevokeToken(string token, string ipAddress)
        {
            var (refreshToken, account) = await getRefreshToken(token);

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _context.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task Register(RegisterRequest model)
        {
            // validate
            var accountExists = await _context.Accounts.AnyAsync(x => x.Email == model.Email);
            if (accountExists)
            {
                // send already registered error in email to prevent account enumeration
                throw new Exception("User already registered");
            }

            // map model to new account object
            var account = _registerRequestMapper.Map(model);

            // first registered account is an admin
            var isFirstAccount = (await _context.Accounts.CountAsync()) == 0;
            account.Role = isFirstAccount ? Role.Admin : Role.User;
            account.Created = DateTime.UtcNow;

            // hash password
            account.PasswordHash = BC.HashPassword(model.Password);

            // save account
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        public async Task ValidateResetToken(ValidateResetTokenRequest model)
        {
            var account = await _context.Accounts.SingleOrDefaultAsync(x =>
                x.ResetToken == model.Token &&
                x.ResetTokenExpires > DateTime.UtcNow);

            if (account == null)
                throw new Exception("Invalid token");
        }

        public async Task<IEnumerable<AccountResponse>> GetAll()
        {
            var accounts = await _context.Accounts.ToListAsync();
            return accounts.Select(x => _accountResponseMapper.Map(x));
        }

        public async Task<AccountResponse> GetById(int id)
        {
            var account = await getAccount(id);
            return _accountResponseMapper.Map(account);
        }

        public async Task<AccountResponse> Create(CreateRequest model)
        {
            // validate
            var matchingAccounts = await _context.Accounts.AnyAsync(x => x.Email == model.Email);
            if (matchingAccounts)
                throw new Exception($"Email '{model.Email}' is already registered");

            // map model to new account object
            var account = _createRequestMapper.Map(model);
            account.Created = DateTime.UtcNow;

            // hash password
            account.PasswordHash = BC.HashPassword(model.Password);

            // save account
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            return _accountResponseMapper.Map(account);
        }

        public async Task<AccountResponse> Update(int id, UpdateRequest model)
        {
            var account = await getAccount(id);

            // validate
            var matchingAccounts = await _context.Accounts.AnyAsync(x => x.Email == model.Email);
            if (account.Email != model.Email && matchingAccounts)
                throw new Exception($"Email '{model.Email}' is already taken");

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
                account.PasswordHash = BC.HashPassword(model.Password);

            // copy model to account and save
            _accountUpdater.Update(model, account);
            account.Updated = DateTime.UtcNow;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return _accountResponseMapper.Map(account);
        }

        public async Task Delete(int id)
        {
            var account = await getAccount(id);
            _context.Accounts.Remove(account);
            _context.SaveChanges();
        }

        // helper methods

        private async Task<Account> getAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) throw new KeyNotFoundException("Account not found");
            return account;
        }

        private async Task<(RefreshToken, Account)> getRefreshToken(string token)
        {
            var account = await _context.Accounts.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
            if (account == null) throw new Exception("Invalid token");
            var refreshToken = account.RefreshTokens.Single(x => x.Token == token);
            if (!refreshToken.IsActive) throw new Exception("Invalid token");
            return (refreshToken, account);
        }

        private string generateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken generateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = randomTokenString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        private string randomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
    }
}