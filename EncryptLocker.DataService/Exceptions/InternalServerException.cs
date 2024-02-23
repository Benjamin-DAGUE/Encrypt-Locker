namespace EncryptLocker.DataService.Exceptions;

public class InternalServerException : Exception
{
    public InternalServerException() : base("Internal server error")
    {

    }

    public InternalServerException(Exception innerException) : base("Internal server error", innerException)
    {

    }
}
