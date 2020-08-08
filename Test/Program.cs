using ClussPro.ObjectBasedFramework.Schema;
using MineData.World;
using System.Diagnostics;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Country country = new Country();
            //Schema.Deploy();
            Schema.UnDeploy();
        }
    }
}
