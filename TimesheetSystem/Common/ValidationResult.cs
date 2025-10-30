namespace TimesheetSystem.Common
{
    public class ValidationResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Value { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public static ValidationResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
        public static ValidationResult<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
    }
}
