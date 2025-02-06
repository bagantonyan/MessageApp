using System.Text;

namespace MessageApp.SenderClient.Helpers
{
    public static class StringHelper
    {
        public static string GenerateRandomStringWithMaxLength(int maxLength)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            StringBuilder result = new StringBuilder();

            Random random = new Random();

            int length = random.Next(1, maxLength + 1);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(validChars.Length);
                result.Append(validChars[index]);
            }

            return result.ToString();
        }
    }
}