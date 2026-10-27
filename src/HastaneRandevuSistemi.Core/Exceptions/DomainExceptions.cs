namespace HastaneRandevuSistemi.Core.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
    }

    public class RandevuCakismaException : DomainException
    {
        public RandevuCakismaException(string message) : base(message)
        {
        }
    }

    public class YetkisizIslemException : DomainException
    {
        public YetkisizIslemException(string message) : base(message)
        {
        }
    }
}
