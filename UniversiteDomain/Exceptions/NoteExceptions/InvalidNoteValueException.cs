namespace UniversiteDomaine.Exceptions.NoteExceptions;

[Serializable]
public class InvalidNoteValueException : Exception
{
    public InvalidNoteValueException() : base() { }
    public InvalidNoteValueException(string message) : base(message) { }
    public InvalidNoteValueException(string message, Exception inner) : base(message, inner) { }
}