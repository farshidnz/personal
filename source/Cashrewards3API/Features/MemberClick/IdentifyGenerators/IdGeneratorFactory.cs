using System.Collections.Generic;

namespace Cashrewards3API.Features.MemberClick
{
    public class IdGeneratorFactory
    {
        private Dictionary<string, IIdGeneratorService> _serviceDic;

        public IdGeneratorFactory(IEnumerable<IIdGeneratorService> services)
        {
            _serviceDic = new Dictionary<string, IIdGeneratorService>();

            if (services != null)
            {
                foreach (var item in services)
                {
                    _serviceDic.Add(getServiceKey(item), item);
                }
            }
        }

        public IIdGeneratorService GetService(int networkId)
        {
            switch (networkId)
            {
                case Common.Constants.Networks.ChineseAN:
                    return this.getServiceByKey(getServiceKey<Base62UniqueGeneratorService>());
                case Common.Constants.Networks.Bupa:
                    return this.getServiceByKey(getServiceKey<BupaUniqueGeneratorService>());
                default:
                    return this.getServiceByKey(getServiceKey<Base62MemberClientUniqueGeneratorService>());
            }
        }

        private IIdGeneratorService getServiceByKey(string key)
        {
            return _serviceDic[key];
        }

        private string getServiceKey<T>()
            where T : IIdGeneratorService
        {
            return typeof(T).Name;
        }

        private string getServiceKey(IIdGeneratorService service)
        {
            return service.GetType().Name;
        }
    }
}
