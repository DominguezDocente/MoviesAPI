using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MoviesAPI.DTOs;
using System.Security.Cryptography;

namespace MoviesAPI.Services
{
    public interface IHashService
    {
        public HashResultDTO Hash(string input);
        public HashResultDTO Hash(string input, byte[] salt);
    }

    public class HashService : IHashService
    {
        public HashResultDTO Hash(string input)
        {
            byte[] salt = new byte[16];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return Hash(input, salt);
        }

        public HashResultDTO Hash(string input, byte[] salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: input,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10_000,
                numBytesRequested: 256 / 8
            ));

            return new HashResultDTO
            {
                Hash = hashed,
                Salt = salt
            };
        }
    }
}
