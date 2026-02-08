namespace UniversiteDomain.Exceptions.EtudiantExceptions;

[Serializable]
public class EtudiantNotInParcoursException : Exception
{
    public EtudiantNotInParcoursException() : base() { }
    public EtudiantNotInParcoursException(string message) : base(message) { }
    public EtudiantNotInParcoursException(string message, Exception inner) : base(message, inner) { }
}