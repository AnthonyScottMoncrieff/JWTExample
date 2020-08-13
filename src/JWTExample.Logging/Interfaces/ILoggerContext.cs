using System;
using System.Threading.Tasks;

namespace JWTExample.Logging.Interfaces
{
    public interface ILoggerContext
    {
        void SubmitException(Exception exception);

        void AddMessageDetail(string item);

        void AddErrorDetail(string message);

        void SubmitLog(string orderDetailMessage);

        Task CommitAsync();
    }
}