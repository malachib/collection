using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Experimental
{
    public class AsyncCondition
    {
        internal Func<bool> condition;
        internal SemaphoreSlim conditionSatisfied = new SemaphoreSlim(0);
        internal EventInfo eventInfo;

        public void Check()
        {
            if (condition()) conditionSatisfied.Release();
        }
    }


    public class AsyncConditionHelperBase
    {
        // Ah, maybe because this is internal but the dynamic code
        // is in a different assembly/module, we can't see this and 
        // that was the problem?
        internal AsyncCondition ac;

        protected void Check() => ac.Check();
    }

    public static class AsyncConditionExtensions
    {
        public static void Test1(string test)
        {

        }

        public static async Task WaitFor(Func<bool> condition, object eventInstance, string eventProperty, CancellationToken ct)
        {
            var ac = new AsyncCondition() { condition = condition };

            ac.eventInfo = eventInstance.GetType().GetRuntimeEvent(eventProperty);
            var dt = ac.eventInfo.EventHandlerType;

            // https://stackoverflow.com/questions/41784393/how-to-emit-a-type-in-net-core
            AssemblyName aName = new AssemblyName();
            aName.Name = "DynamicTypes";

            var invokeMethod = dt.GetTypeInfo().GetDeclaredMethod("Invoke");

            //AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName,
            //                                            AssemblyBuilderAccess.Run);
            var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect);

            var mb = ab.DefineDynamicModule("MainModule");

            var baseType = typeof(AsyncConditionHelperBase);


            var tb = mb.DefineType("AsyncConditionHelper", 
                TypeAttributes.Public | TypeAttributes.Class,
                baseType);

            //tb.DefineConstructor(MethodAttributes.Public,
            //  CallingConventions.Any, new Type[] { typeof(AsyncCondition) });

            tb.DefineDefaultConstructor(MethodAttributes.Public);

            var methodBuilder = tb.DefineMethod("eventResponder", 
                MethodAttributes.Public | MethodAttributes.Assembly,
                invokeMethod.ReturnType, 
                invokeMethod.GetParameters().Select(x => x.ParameterType).ToArray());

            var il = methodBuilder.GetILGenerator();
            var checkMethod = ac.GetType().GetRuntimeMethod("Check", new Type[0]);

            //var acField = tb.GetDeclaredField("ac");
            var acField = baseType.GetTypeInfo().GetDeclaredField("ac");

            MethodInfo simpleShow =
                typeof(AsyncConditionExtensions).GetRuntimeMethod("Test1", new[] { typeof(string) });

            // test code works +++
            il.Emit(OpCodes.Ldstr,
                "This event handler was constructed at run time.");
            il.Emit(OpCodes.Call, simpleShow);
            // ---

            /*
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, acField);
            //il.EmitCall(OpCodes.Call, checkMethod, null);

            //il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Pop); */

            il.Emit(OpCodes.Ldarg_0);
            var check2 = baseType.GetTypeInfo().GetDeclaredMethod("Check");
            il.EmitCall(OpCodes.Call, check2, null);
            il.Emit(OpCodes.Ret);

            var typeInfo = tb.CreateTypeInfo();
            //var type = typeInfo.MakePointerType();

            //var instance = (AsyncConditionHelperBase) Activator.CreateInstance(type);
            var ctor = typeInfo.DeclaredConstructors.Single();

            var instance = (AsyncConditionHelperBase) ctor.Invoke(new object[0]);

            instance.ac = ac;

            var eventResponder = typeInfo.GetDeclaredMethod("eventResponder");

            Delegate erd = eventResponder.CreateDelegate(dt, instance);

            ac.eventInfo.AddEventHandler(eventInstance, erd);

            //ac.eventInfo.AddMethod.Invoke(eventInstance, new[] { })
            //ac.eventInfo.AddMethod.Invoke(eventInstance, new[] { condition });

            await ac.conditionSatisfied.WaitAsync(ct);

            ac.eventInfo.RemoveEventHandler(eventInstance, erd);
        }
    }
}
