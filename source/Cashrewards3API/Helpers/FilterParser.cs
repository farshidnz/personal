using Cashrewards3API.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cashrewards3API.Helpers
{
    public class FilterParser
    {
        //TODO : Replace dynamic return type 
        public static dynamic Parse(string filterString)
        {
            if (filterString  == null)
                return new { CategoryId = 0, InStoreFlag = MerchantInstoreFilterEnum.All};

            filterString = filterString.ToLower();

            if (filterString.IndexOf(" or ") > 0)
            {
                throw new Exception("Invalid filter format - OR operator currently is not supported in the filter.");
            }

            List<string> keyValuePairs = filterString.Replace('(', ' ').Replace(')', ' ').Split(' ').ToList();
            keyValuePairs.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            if (keyValuePairs.Count > 1 && !keyValuePairs.Any(x => x.Equals("and")))
            {
                throw new Exception("Invalid filter format - 'and' condition must be present if more than one filter specified.");
            }

            var categoryId = 0;

            // TODO : Instore need to be resolved
            var inStoreFlag = MerchantInstoreFilterEnum.All;

            foreach (var keyValuePair in keyValuePairs)
            {
                try
                {
                    string key = keyValuePair.Split('=').ElementAtOrDefault(0)?.Trim();
                    var value = keyValuePair.Split('=').ElementAtOrDefault(1)?.Trim();

                    if (key.Equals("categoryid"))
                        categoryId = int.Parse(value);

                    // TODO : Instore need to be resolved
                    if (key.Equals("instore"))
                        inStoreFlag = bool.Parse(value) ? MerchantInstoreFilterEnum.InStore : MerchantInstoreFilterEnum.Online;
                }
                catch
                {
                    throw new Exception("Invalid filter format");
                }
            }

            //return new { CategoryId = categoryId };
            return new {CategoryId= categoryId, InStoreFlag= inStoreFlag };
        }
    }
}