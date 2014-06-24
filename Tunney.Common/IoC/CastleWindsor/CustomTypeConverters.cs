using System;
using System.Collections.Generic;

using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Conversion;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Tunney.Common.IoC.CastleWindsor
{
    //public class CustomDictionaryStringKeyListOfStringValueTypeConverter : AbstractTypeConverter
    //{
    //    public override bool CanHandleType(Type type, Castle.Core.Configuration.IConfiguration configuration)
    //    {
    //        return this.CanHandleType(type);
    //    }

    //    public override bool CanHandleType(Type type)
    //    {
    //        if (!type.IsGenericType) return false;

    //        if (typeof(IDictionary<,>) == type.GetGenericTypeDefinition())
    //        {
    //            if (typeof(IDictionary<string, IList<string>>) == type) return true;
    //        }

    //        return false;
    //    }

    //    public override object PerformConversion(Castle.Core.Configuration.IConfiguration configuration, Type targetType)
    //    {
    //        try
    //        {
    //            Type[] argTypes = targetType.GetGenericArguments();

    //            if (2 != argTypes.Length || typeof(IDictionary<,>) != targetType.GetGenericTypeDefinition())
    //            {
    //                throw new ConverterException(@"Dictionary is expected, and MUST have two generic type arguments.");
    //            }

    //            string keyStringValue = configuration.Attributes["key"];

    //            IList<string> value = (IList<string>)configuration.GetValue(typeof(List<string>), new List<string>(100));

    //            object keyValue = Convert.ChangeType(keyStringValue, argTypes[0]);

    //            //object value = null;
    //            //try
    //            //{
    //            //    value = Convert.ChangeType(valueString, argTypes[1]);
    //            //}
    //            //catch
    //            //{
    //            //    value = base.Context.Kernel.Resolve(valueString, targetType);
    //            //}

    //            Dictionary<string, IList<string>> retval = new Dictionary<string, IList<string>>();
    //            retval.Add(keyStringValue, value);

    //            //object retval = Activator.CreateInstance(targetType, new object[] { keyValue, value });

    //            return retval;
    //        }
    //        catch (Exception)
    //        {
    //            throw;
    //        }
    //    }

    //    public override object PerformConversion(string value, Type targetType)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class CustomKeyValuePairWithIntListStringTypeConverter : AbstractTypeConverter
    {
        public override bool CanHandleType(Type type, Castle.Core.Configuration.IConfiguration configuration)
        {
            return this.CanHandleType(type);
        }

        public override bool CanHandleType(Type type)
        {
             if (!type.IsGenericType) return false;

            if (typeof(KeyValuePair<,>) == type.GetGenericTypeDefinition()) return true;

            return false;
        }

        public override object PerformConversion(Castle.Core.Configuration.IConfiguration configuration, Type targetType)
        {
            try
            {
                Type[] argTypes = targetType.GetGenericArguments();

                if (2 != argTypes.Length || typeof(KeyValuePair<,>) != targetType.GetGenericTypeDefinition())
                {
                    throw new ConverterException(@"KeyValuePair is expected, and MUST have two generic type arguments.");
                }

                string keyStringValue = configuration.Attributes["key"];

                string valueString = configuration.Value;

                object keyValue = Convert.ChangeType(keyStringValue, argTypes[0]);

                object value = null;
                try
                {
                    value = Convert.ChangeType(valueString, argTypes[1]);
                }
                catch
                {
                    value = base.Context.Kernel.Resolve(valueString, targetType);
                }
                object retval = Activator.CreateInstance(targetType, new object[] { keyValue, value });

                return retval;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override object PerformConversion(string value, Type targetType)
        {
            throw new NotImplementedException();
        }
    }

    public class TunneyCustomTypeConvertersInstaller : IWindsorInstaller
    {
        public TunneyCustomTypeConvertersInstaller()
        {
        }

        #region IWindsorInstaller Members

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            IConversionManager manager = (IConversionManager)container.Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);                        
            //manager.Add(new CustomDictionaryStringKeyListOfStringValueTypeConverter()); 
            manager.Add(new CustomKeyValuePairWithIntListStringTypeConverter());            
        }

        #endregion
    }
}