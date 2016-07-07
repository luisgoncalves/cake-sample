using Cake.Sample.Lib;

namespace Cake.Sample.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var super = new SuperClass(42);
            System.Console.WriteLine(super.DoSuperStuff());
        }
    }
}
