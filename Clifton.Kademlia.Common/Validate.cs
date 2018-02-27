using System;

namespace Clifton.Kademlia.Common
{
	public static class Validate
	{
		public static void IsTrue<T>(bool b, string errorMessage) where T : Exception, new()
		{
            if (!b)
            {
                throw (T)Activator.CreateInstance(typeof(T), new object[] { errorMessage });
            }
		}

		public static void IsFalse<T>(bool b, string errorMessage) where T : Exception, new()
        {
			if (b)
			{
                throw (T)Activator.CreateInstance(typeof(T), new object[] { errorMessage });
            }
        }
	}
}
