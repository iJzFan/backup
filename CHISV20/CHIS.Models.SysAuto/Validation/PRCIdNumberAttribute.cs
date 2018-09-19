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
    /// 是否是手机的验证 注意配合前端js代码
    /// </summary>
    public class PRCIdNumberAttribute : ValidationAttribute, IClientModelValidator
    {
        private static readonly Regex _regex = new Regex(@"^(\d{15}|\d{18}|\d{17}[\d|X|x]{1})$", RegexOptions.IgnoreCase);

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            var errorMessage = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
            MergeAttribute(context.Attributes, "data-val-prcid", errorMessage);
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
            string stringValue = $"{value}".Trim();
            if (string.IsNullOrEmpty(stringValue)) return true;
            return _regex.IsMatch(stringValue);
        }
    }
}
