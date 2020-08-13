using JWTExample.Logging.Interfaces;
using System;
using System.Threading.Tasks;

namespace JWTExample.Logging
{
    public class LoggerContext : ILoggerContext
    {
        public void AddErrorDetail(string message)
        {
            //Add error
        }

        public void AddMessageDetail(string item)
        {
            //Add message
        }

        public Task CommitAsync()
        {
            return Task.Run(() => { return; });
        }

        public void SubmitException(Exception exception)
        {
            //Submit exception
        }

        public void SubmitLog(string orderDetailMessage)
        {
            //submit log
        }
    }
}