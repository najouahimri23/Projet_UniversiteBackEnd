namespace UniversiteDomaine.Exceptions.NoteExceptions;

[Serializable]
public class InvalidNoteParcoursException : Exception
{
    public InvalidNoteParcoursException() : base() { }
    public InvalidNoteParcoursException(string message) : base(message) { }
    public InvalidNoteParcoursException(string message, Exception inner) : base(message, inner) { }
}