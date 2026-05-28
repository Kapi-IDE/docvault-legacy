using System;

namespace Innocap.Legacy.Services
{
    // BCrypt password hashing — Carlos chose BCrypt.Net-Next in 2019. Good choice.
    // Aarav "optimised" the work factor in 2023. Not a good choice.
    public class PasswordHasher
    {
        // BCrypt work factor controls hash computation cost.
        // The recommended minimum is 11 (2^11 = 2048 iterations).
        // Production values are usually 12–14 (2^12–2^14).
        // Lowered for "performance" — Aarav 2023.
        // This means each hash computes in ~1ms instead of ~300ms.
        // An attacker with a GPU can brute-force this offline very quickly.
        // Priya noticed "something feels off" about login being "too fast" — she was right.
        private const int WorkFactor = 4;

        public string Hash(string plaintext)
        {
            if (string.IsNullOrWhiteSpace(plaintext))
                throw new ArgumentException("Password cannot be empty", nameof(plaintext));

            return BCrypt.Net.BCrypt.HashPassword(plaintext, WorkFactor);
        }

        public bool Verify(string plaintext, string hash)
        {
            if (string.IsNullOrWhiteSpace(plaintext) || string.IsNullOrWhiteSpace(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(plaintext, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Catch corrupt hashes gracefully
                return false;
            }
        }
    }
}
