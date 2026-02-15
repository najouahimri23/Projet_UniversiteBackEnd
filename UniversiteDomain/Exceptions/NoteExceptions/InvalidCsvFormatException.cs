namespace UniversiteDomaine.Exceptions.NoteExceptions;

[Serializable]
public class InvalidCsvFormatException : Exception
{
    public List<string> Errors { get; }

    public InvalidCsvFormatException() : base() 
    { 
        Errors = new List<string>(); 
    }
    
    public InvalidCsvFormatException(string message) : base(message) 
    { 
        Errors = new List<string> { message }; 
    }
    
    public InvalidCsvFormatException(List<string> errors) 
        : base($"Le fichier CSV contient {errors.Count} erreur(s):\n{string.Join("\n", errors)}")
    {
        Errors = errors;
    }
    
    public InvalidCsvFormatException(string message, Exception inner) : base(message, inner) 
    { 
        Errors = new List<string> { message }; 
    }
}