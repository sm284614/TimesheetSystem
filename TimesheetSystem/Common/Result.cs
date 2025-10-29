namespace TimesheetSystem.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T? Value { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
        public static Result<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
    }
}
