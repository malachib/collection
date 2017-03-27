using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fact.Extensions.Factories;
using System.Reflection;
using System.Diagnostics;

namespace Fact.Extensions.Serialization
{
    public interface IRefSerializerContext<TOut>
    {
        TOut Out { get; }
    }

    public interface IRefDeserializerContext<TIn>
    {
        TIn In { get; }
    }

    public interface IRefSerializerContext
    {
        bool IsSerializing { get; }
    }


    public class RefUserSerializer<TIn, TOut>
    {
        public RefSerializer<TIn, TOut>.Context Context { get; internal set; }
    }


    public class RefPropertySerializer : RefUserSerializer<IPropertyDeserializer, IPropertySerializer>
    {

    }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class RefSerializer<TIn, TOut> : ISerializationManager<TIn, TOut>
    {
        // special ref-method serializer lives here
        readonly object refMethod;

        public RefSerializer(object refMethod)
        {
            this.refMethod = refMethod;
        }

        public class Context : IRefSerializerContext,
            IRefSerializerContext<TOut>,
            IRefDeserializerContext<TIn>
        {
            internal TOut @out;
            internal TIn @in;
            internal bool isSerializing;

            public TOut Out => @out;
            public TIn In => @in;
            public bool IsSerializing => isSerializing;
        }

        static MethodInfo GetMethod(object refMethod)
        {
#if NETSTANDARD1_6
            var persistMethod = refMethod.GetType().GetTypeInfo().GetMethod("Persist");
            return persistMethod;
            //var parameters = persistMethod.GetParameters();
            //var parameterValues = new object[parameters.Length];
            //var index = 0;
#else
            throw new InvalidOperationException();
#endif
        }


        public object Deserialize(TIn input, Type type)
        {
            var ctx = new Context();

            ctx.@in = input;
            ctx.isSerializing = false;

            object instance = Activator.CreateInstance(type);

            throw new NotImplementedException();
        }

        public void Serialize(TOut output, object instance, Type type = null)
        {
            var ctx = new Context();

            ctx.@out = output;
            ctx.isSerializing = true;

            var persistMethod = GetMethod(refMethod);

#if NETSTANDARD1_6
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
#else
            FieldInfo[] fields = null;
            throw new InvalidOperationException();
#endif

            var parameters = persistMethod.GetParameters();
            var parameterValues = new object[parameters.Length];
            var index = 1;

            // TODO: Also account for RefUserSerializer mode

            Debug.Assert(parameters[0].ParameterType == typeof(IRefSerializerContext));

            parameterValues[0] = ctx;

            foreach (var p in parameters.Skip(1))
            {
                // match each parameter to a field
                var field = fields.FirstOrDefault(x => x.Name == p.Name);

                // if no such field exists, then we abort method 3 processing
                if (field == null) throw new InvalidOperationException("Persist method specifies invalid parameters: " + p.Name);

                var value = field.GetValue(instance);

                parameterValues[index++] = value;
            }

            persistMethod.Invoke(refMethod, parameterValues);
        }
    }


    public class RefSerializerFactory : ISerializerFactory<IPropertyDeserializer, IPropertyDeserializer>
    {
        bool IFactory<Type, IDeserializer<IPropertyDeserializer>>.CanCreate(Type id)
        {
            throw new NotImplementedException();
        }

        bool IFactory<Type, ISerializer<IPropertyDeserializer>>.CanCreate(Type id)
        {
            throw new NotImplementedException();
        }

        IDeserializer<IPropertyDeserializer> IFactory<Type, IDeserializer<IPropertyDeserializer>>.Create(Type id)
        {
            throw new NotImplementedException();
        }

        ISerializer<IPropertyDeserializer> IFactory<Type, ISerializer<IPropertyDeserializer>>.Create(Type id)
        {
            throw new NotImplementedException();
        }
    }
}
