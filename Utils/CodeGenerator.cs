using System;
using System.Linq;

namespace Malshinon.Utils
{
    public static class CodeGenerator
    {
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GenerateSecretCode(int length = 12)
        {
            var random = new Random();
            var code = "";

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                code += chars[index];
            }

            return code;
        }

    }
}
