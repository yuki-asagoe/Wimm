using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wimm.Ui.Validation
{
    public class FilePathValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool valid = false;
            string error;
            if(value is string path)
            {
                valid = File.Exists(path);
                if (!valid)
                {
                    error = "This file does not exist.";
                }
                else
                {
                    error = string.Empty;
                }
            }
            else
            {
                error = "The value is not string.";
            }
            return new ValidationResult(valid,error);
        }
    }
}
