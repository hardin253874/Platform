// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Connector.Payload
{
    class JilDynamicObjectReaderService : IDynamicObjectReaderService
    {
        public IObjectReader GetObjectReader( System.Dynamic.IDynamicMetaObjectProvider dynamicProvider )
        {
            return new SafeObjectReader(new JilDynamicObjectReader( dynamicProvider ));
        }
    }
}
