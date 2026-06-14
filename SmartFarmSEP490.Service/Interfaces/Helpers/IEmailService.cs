using System.Threading.Tasks;

namespace SmartFarmSEP490.Service.Interfaces.Helpers
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
