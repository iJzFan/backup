using System;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CHIS.Api.OpenApi
{
    [Serializable]
    internal class ModelStateException : Exception
    {
        private ModelStateDictionary modelState;


        public ModelStateException(ModelStateDictionary modelState) : base(GetErros(modelState))
        {
            this.modelState = modelState;
        }

        private static string GetErros(ModelStateDictionary modelState)
        {           
            StringBuilder b = new StringBuilder();
            foreach (var item in modelState)
            {
              
                foreach(var err in item.Value.Errors)
                {
                    b.Append(err.ErrorMessage);
                }                

            }
            return b.ToString();
        }
    }
}