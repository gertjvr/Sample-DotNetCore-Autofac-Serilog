using System.Threading.Tasks;

namespace ConsoleApplication
{
    public interface IService
    {
        Task ServiceTheThing(string value);
    }
}