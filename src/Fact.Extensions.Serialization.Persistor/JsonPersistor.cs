using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class RefDelegatePersistor : Persistor, IPersistor
    {
        readonly Delegate refDelegate;

        public RefDelegatePersistor(Delegate refDelegate)
        {
            this.refDelegate = refDelegate;
        }

        public void Persist(object instance)
        {
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var persistMethod = refDelegate.GetMethodInfo();
            var parameters = persistMethod.GetParameters();
            if (parameters[0].ParameterType != typeof(ModeEnum))
                throw new InvalidCastException("First parameter must be mode enum specifier");

            if (Mode == ModeEnum.Serialize)
            {
                var parameterValues = new object[parameters.Length];
                var index = 0;

                foreach (var p in parameters)
                {
                    // match each parameter to a field
                    var field = fields.FirstOrDefault(x => x.Name == p.Name);

                    // if no such field exists, then we abort method 3 processing
                    if (field == null) throw new InvalidOperationException("Persist method specifies invalid parameters: " + p.Name);

                    var value = field.GetValue(instance);

                    parameterValues[index++] = value;
                }

                parameterValues[0] = Mode;
                persistMethod.Invoke(refDelegate, parameterValues);
            }
            else
            {
                // effectively map fields to method parameters
                var parameterValues = new object[parameters.Length];

                parameterValues[0] = Mode;
                persistMethod.Invoke(refDelegate, parameterValues);

                var index = 0;

                foreach (var p in parameters)
                {
                    // match each parameter to a field
                    var field = fields.FirstOrDefault(x => x.Name == p.Name);

                    // if no such field exists, then we abort method 3 processing
                    if (field == null) throw new InvalidOperationException("Persist method specifies invalid parameters: " + p.Name);

                    var value = parameterValues[index++];

                    field.SetValue(instance, value);
                }
            }
        }
    }
}
