using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ass
{

    /// <summary>
    /// 必须大于零 注意配合前端js代码
    /// </summary>
    public class BiggerZeroAttribute : ValidationAttribute, IClientModelValidator
    {
        private static readonly Regex _regex = new Regex(@"^(\d{15}|\d{18}|\d{17}[\d|X|x]{1})$", RegexOptions.IgnoreCase);

        

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            var errorMessage = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
            MergeAttribute(context.Attributes, "data-val-bigger0", errorMessage);
        }
        private bool MergeAttribute(IDictionary<string, string> attributes,
                                    string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }
            attributes.Add(key, value);
            return true;
        }

        public override bool IsValid(object value)
        {
            try
            {
                if (value == null) return false;
                if (value is int) return (int)value > 0;
                if (value is long) return (long)value > 0;
                if (value is decimal) return (decimal)value > 0;
                if (value is float) return (float)value > 0;
                if (value is double) return (double)value > 0;
                return false;
            }catch(Exception ex) { return false; }
          
        }
    }
}
