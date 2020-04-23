using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Twinkle.View.Attributes;

namespace Twinkle.View.Infrastructure
{
    public class FieldDescriptor
    {
        public FieldDescriptor(string fieldName, Type fieldType)
        {
            FieldName = fieldName;
            FieldType = fieldType;
        }
        public string FieldName { get; }
        public Type FieldType { get; }
    }
    public static class ViewBuilder
    {

        public static TypeInfo CompileResultTypeInfo(AssemblyName nameDynamicAssembly, string typeName, string classAlias, string classPrefix, string[] classCriteria, List<List<Object>> propertyList)
        {
            TypeBuilder tb = GetTypeBuilder(nameDynamicAssembly, typeName, classAlias, classPrefix, classCriteria);
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            foreach (var field in propertyList)
                CreateProperty(
                    tb, 
                    ((FieldDescriptor)field[0]).FieldName, 
                    ((FieldDescriptor)field[0]).FieldType, 
                    (string)field[1],
                    (string)field[2],
                    (string)field[3]);

            TypeInfo objectTypeInfo = tb.CreateTypeInfo();
            return objectTypeInfo;
        }

        private static TypeBuilder GetTypeBuilder(AssemblyName nameDynamicAssembly, string typeName, string classAlias, string classPrefix, string[] classCriteria)
        {
            var typeSignature = typeName;
            var an = new AssemblyName(typeSignature);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(nameDynamicAssembly, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);

            var attrCtorParams = new Type[] { typeof(string), typeof(string), typeof(string []) };
            var attrCtorInfo = typeof(ViewBaseAttribute).GetConstructor(attrCtorParams);
            var attrBuilder = new CustomAttributeBuilder(attrCtorInfo, new object[] { classAlias, classPrefix, classCriteria });
            tb.SetCustomAttribute(attrBuilder);
            tb.SetParent(Assembly.Load("Twinkle.View").GetTypes().First(t => t.Name == "View"));

            return tb;
        }

        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType, string aliasControl, string xpathControl, string cssControl)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            var attrCtorParams = new Type[] { typeof(string), typeof(string), typeof(string) };
            var attrCtorInfo = typeof(ControlBaseAttribute).GetConstructor(attrCtorParams);
            var attrBuilder = new CustomAttributeBuilder(attrCtorInfo, new object[] { aliasControl, xpathControl, cssControl });
            propertyBuilder.SetCustomAttribute(attrBuilder);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }
}