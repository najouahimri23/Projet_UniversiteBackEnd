namespace UniversiteDomaine.Exceptions.NoteExceptions;

[Serializable]
public class DuplicateNoteException : Exception
{
    public DuplicateNoteException() : base() { }
    public DuplicateNoteException(string message) : base(message) { }
    public DuplicateNoteException(string message, Exception inner) : base(message, inner) { }
}