using System.Xml;
using System.Xml.Schema;

namespace AccMotec.Logic
{
    public class MotecParser : IMotecParser
    {
        public MotecParser()
        {
            
        }

        /// <inheritdoc />
        public bool IsValidLdx(string? path)
        {
            var isValid = true;
            ValidatePath(path);
            if (!Path.GetExtension(path)?.Equals(".ldx") ?? true)
            {
                // doesn't have the right file extension
                return false;
            }

            var settings = new XmlReaderSettings();
            settings.Schemas.Add(string.Empty, "MotecLdx.xsd");
            settings.ValidationType = ValidationType.Schema;
            var validationHandler = new ValidationEventHandler(ValidateXsd);

            var reader = XmlReader.Create(path!, settings);
            var document = new XmlDocument();
            try
            {
                document.Load(reader);
                document.Validate(validationHandler);
            }
            catch (XmlSchemaValidationException ex)
            {
                isValid = false;
                Console.WriteLine(ex.Message);
            }

            return isValid;
        }

        public MotecFile? ParseFile(string? path)
        {
            ValidatePath(path);
            
            return null;
        }

        private void ValidatePath(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new FileNotFoundException("The path was not provided");
            }
            
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"The file [{path}] does not exist");
            }
        }

        private void ValidateXsd(object? sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error || e.Severity == XmlSeverityType.Warning)
            {
                // isValid = false;
                Console.WriteLine($"invalid schema [{e.Message}]");
            }
        }
    }
}