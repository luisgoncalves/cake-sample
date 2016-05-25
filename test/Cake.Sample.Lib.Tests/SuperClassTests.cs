using Xunit;

namespace Cake.Sample.Lib.Tests
{
    public class SuperClassTests
    {
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(6)]
        public void SuperStuff_Is_Done(int i)
        {
            var sut = new SuperClass(i);
            var super = sut.DoSuperStuff();

            Assert.Contains(i.ToString(), super);
        }
    }
}
