using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.DTOs;
using EduCode.WebApi.Configuracion;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EduCode.WebApi.Servicios;

public sealed class ServicioTokenJwt : IServicioToken
{
    private readonly ConfiguracionJwt _configuracion;

    public ServicioTokenJwt(IOptions<ConfiguracionJwt> configuracion)
    {
        _configuracion = configuracion.Value;
    }

    public TokenDTO GenerarToken(Usuario usuario)
    {
        var expiracion = DateTime.UtcNow.AddMinutes(
            _configuracion.DuracionMinutos);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                usuario.Id.ToString()),
            new Claim(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString()),
            new Claim(
                ClaimTypes.Name,
                usuario.NombreUsuario),
            new Claim(
                ClaimTypes.Email,
                usuario.Email),
            new Claim(
                ClaimTypes.Role,
                usuario.Rol.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuracion.ClaveSecreta));

        var credenciales = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _configuracion.Emisor,
            audience: _configuracion.Audiencia,
            claims: claims,
            expires: expiracion,
            signingCredentials: credenciales);

        return new TokenDTO
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
            RefreshToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64)),
            FechaExpiracion = expiracion,
            FechaExpiracionRefreshToken = DateTime.UtcNow.AddDays(
                _configuracion.DuracionRefreshTokenDias)
        };
    }
}
