using System;
using System.Text;

namespace Core.Extensions
{
    public static class ExceptionExtensions
    {
        public static string FullMessage(this Exception e)
        {
            var sb = new StringBuilder();

            sb.AppendLine(e.Message);
            while (e != null)
            {
                sb.Append("\t"); sb.AppendLine(e.StackTrace);

                e = e.InnerException;
            }

            return sb.ToString();
        }

        public static string BriefMessage(this Exception e)
        {
            var sb = new StringBuilder();

            sb.AppendLine(e.Message);
            sb.AppendLine(e.StackTrace);

            return sb.ToString();
        }
    }
}
