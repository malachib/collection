using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Utilizing a combination of field-reflection on the instance and method reflection on this Persistor
    /// class, we match up field names to parameter names and pass by ref so as to handle both
    /// serialization and deserialization
    /// </summary>
    public class RefPersistor : Persistor, IPersistor
    {
        /// <summary>
        /// FIX: bad name, refers to method#3 described on IPersistor interface
        /// </summary>
        readonly Persistor refMethod;

        public RefPersistor(Persistor refMethod)
        {
            this.refMethod = refMethod;
        }

        public void Persist(object instance)
        {
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            Persist(instance, fields);
        }

        internal void Persist(object instance, IEnumerable<FieldInfo> fields)
        {
            if (Mode == ModeEnum.Serialize)
            {
                // effectively map fields to method parameters
                var persistMethod = refMethod.GetType().GetTypeInfo().GetMethod("Persist");
                var parameters = persistMethod.GetParameters();
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

                refMethod.Mode = ModeEnum.Serialize;
                persistMethod.Invoke(refMethod, parameterValues);
            }
            else
            {
                // effectively map fields to method parameters
                var persistMethod = refMethod.GetType().GetTypeInfo().GetMethod("Persist");
                var parameters = persistMethod.GetParameters();
                var parameterValues = new object[parameters.Length];

                refMethod.Mode = ModeEnum.Deserialize;
                persistMethod.Invoke(refMethod, parameterValues);


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
