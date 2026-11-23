
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poddavki.Data;

namespace Poddavki.Tests
{
    [TestClass]
    public class PasswordHasherTests
    {
        [TestMethod]
        public void CreateHash_ShouldVerifyCorrectPassword()
        {
            string hash;
            string salt;

            PasswordHasher.CreateHash(
                "TestPassword123",
                out hash,
                out salt);

            Assert.IsTrue(
                PasswordHasher.Verify(
                    "TestPassword123", hash, salt));
        }

        [TestMethod]
        public void Verify_ShouldRejectIncorrectPassword()
        {
            string hash;
            string salt;

            PasswordHasher.CreateHash(
                "TestPassword123",
                out hash,
                out salt);

            Assert.IsFalse(
                PasswordHasher.Verify(
                    "WrongPassword", hash, salt));
        }

        [TestMethod]
        public void CreateHash_ShouldGenerateDifferentSalts()
        {
            string hash1;
            string salt1;

            string hash2;
            string salt2;

            PasswordHasher.CreateHash(
                "TestPassword123",
                out hash1,
                out salt1);

            PasswordHasher.CreateHash(
                "TestPassword123",
                out hash2,
                out salt2);

            Assert.AreNotEqual(salt1, salt2);
            Assert.AreNotEqual(hash1, hash2);
        }
    }
}
