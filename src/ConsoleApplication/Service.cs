using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class Service :
        IService
    {
        public Task ServiceTheThing(string value)
        {
            return Task.CompletedTask;
        }
    }
}
