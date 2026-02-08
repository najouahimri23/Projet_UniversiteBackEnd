namespace UniversiteDomaine.Exceptions.UeExceptions;


[Serializable]
public class UeIntituleTooShortException : Exception
{
    public UeIntituleTooShortException() : base() { }
    public UeIntituleTooShortException(string message) : base(message) { }
    public UeIntituleTooShortException(string message, Exception inner) : base(message, inner) { }
}