namespace UniversiteDomaine.Exceptions.UeExceptions;



[Serializable]
public class DuplicateUeNumeroException : Exception
{
    public DuplicateUeNumeroException() : base() { }
    public DuplicateUeNumeroException(string message) : base(message) { }
    public DuplicateUeNumeroException(string message, Exception inner) : base(message, inner) { }
}
