using System;
using PostSharp.Aspects;

namespace App.Domain.WEB.Utils
{
    public class StringArrayInterceptor : LocationInterceptionAspect
    {
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            string[] currValue = (string[]) args.GetCurrentValue();

            if (currValue is null)
                currValue = Array.Empty<string>();

            if (currValue.Length == 1)
                currValue = currValue[0].Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            
            args.SetNewValue(currValue);
            args.ProceedSetValue();
        }
    }
}