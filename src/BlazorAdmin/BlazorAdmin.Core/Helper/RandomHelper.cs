namespace FluentCodeServer.Core
{
	public static class RandomHelper
	{
		public static string RandomNumber(int length)
		{
			if (length > 9)
			{
				length = 9;
			}
			if (length < 4)
			{
				length = 4;
			}
			var random = new Random();

			int number = random.Next((int)Math.Pow(10, length - 1), (int)Math.Pow(10, length) - 1);
			return number.ToString();
		}

		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var random = new Random();
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}
	}
}
