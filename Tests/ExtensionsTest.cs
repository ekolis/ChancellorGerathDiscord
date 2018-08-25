using ChancellorGerath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	/// <summary>
	/// Tests extension methods.
	/// </summary>
	[TestClass]
	public class ExtensionsTest
	{
		[TestMethod]
		public void ReplaceSingle()
		{
			Assert.AreEqual("fredbob", "bobbob".ReplaceSingle("bob", "fred"));
			Assert.AreEqual("98210", "90210".ReplaceSingle("0", "8"));
			Assert.AreEqual("test", "test".ReplaceSingle("q", "w"));
		}
	}
}