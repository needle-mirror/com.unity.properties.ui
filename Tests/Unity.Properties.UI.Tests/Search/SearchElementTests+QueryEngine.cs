using NUnit.Framework;

namespace Unity.Properties.UI.Tests
{
    sealed partial class SearchElementTests
    {
        [Test]
        public void Search_GetQueryEngine_IsNotNull()
        {
            Assert.That(m_SearchElement.GetQueryEngine<TestData>(), Is.Not.Null);
        }
    }
}