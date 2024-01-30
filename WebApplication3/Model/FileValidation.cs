using System.ComponentModel.DataAnnotations;


namespace WebApplication3.Model
{
    public class FileValidation : ValidationAttribute
    {
        private const string AllowedFileTypes = ".jpeg";

        protected override ValidationResult IsValid(object photo, ValidationContext validationContext)
        {
            if (photo == null)
                return ValidationResult.Success;

            var Photo = photo as IFormFile;

            if (Photo != null)
            {
                var extension = Path.GetExtension(Photo.FileName);

                if (extension.ToLower() != AllowedFileTypes)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Only {AllowedFileTypes} files are allowed.";
        }
    }
}
