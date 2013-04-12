using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

// TODO: Implement support to resize arrays, when the property is settable.
// TODO: Implement support to dictionaries (half done).
// TODO: Implement Attributor support (simulate injection of Attributes into external classes).
// TODO: Implement more robust support for multivalued names in the NameValueCollection (half done).
// TODO: Allow multivalued names to be saved and loaded from sets like HashSet<T> (could be lists or arrays too).
// TODO: Allow to clear the cache from the StringValueAttribute, just like NameValueConversionCore have.
// TODO: Protects against unexpected usage: mess with context while converting.
// TODO: Implement variable attribute values, like Default, by calling a method on the class or delegate passes to the attribute.
// TODO: Implement multiple properties associated with the same name, and create a FieldDecisionAttribute.
// TODO: Case-insensitive NameValueCollection will not load when it's keys don't match the case indicated in the NameValueAttribute.
// TODO: Option to use case-sensitive NameValueCollection when loading.
// TODO: Profile and optimize the code, where needed.
// TODO: Make tests to this code. (already have some in Program.cs).

// DONE: Protects against unexpected usage: clear the cache while converting. == can do that: force cache rebuild.

namespace PayPal
{
    /// <summary>
    /// Attribute used to indicate the properties that can be converted from and into a NameValueCollection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class NameValueAttribute : Attribute
    {
        public NameValueAttribute()
        {
            this.EmptyIgnore = true;
        }

        /// <summary>
        /// Name of the name-value pair.
        /// You can use the KeyOrIndexName of the current object if it is an item of a collection.
        /// The KeyOrIndexName is designed by the parent.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Format used to convert the value.
        /// </summary>
        public string ValueFormat { get; set; }

        /// <summary>
        /// Regex used to recognize names that must be loaded in this property.
        /// The regex must contain a capture, if KeyOrIndexName has been specified, with the same name.
        /// </summary>
        public string NameRegex { get; set; }

        /// <summary>
        /// Used to indicate that the property is a list of items or a dictionary,
        /// and to assign a name used to refer to the current element index or key.
        /// Each element must be iterated, and child properties saved to the name-value collection.
        /// </summary>
        public string KeyOrIndexName { get; set; }

        /// <summary>
        /// Indicates the type of the element that should be created for this collection.
        /// If it is left null, then the default element type for that collection will be created,
        /// if it can be created.
        /// </summary>
        public Type CollectionElementType { get; set; }

        /// <summary>
        /// Indicates a type (primitive or string) to be used as key of this dictionary.
        /// If it is left null, then the default element type for that dictionary will be used.
        /// </summary>
        public Type CollectionKeyType { get; set; }

        /// <summary>
        /// Default value of property.
        /// A missing value when reading, will not touch the property in the object,
        /// so it must come initialized with the default value already.
        /// When writing, the behavior deppends on WriteDefault property of this attribute,
        /// it is used to decide if the property containing a default value should be written or not.
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// Writes the default value, when saving a name-value collection.
        /// This does not affect reading, an empty value is converted to the default value.
        /// Atention: If the Default value is Empty, and EmptyIgnore is true,
        /// then nothing will be written even if WriteDefault is true.
        /// [Default is false]
        /// </summary>
        public bool WriteDefault { get; set; }

        /// <summary>
        /// Indicates whether empty values in the NameValueCollection should be ignored or not.
        /// When they are ignored, it is the same as if they were missing,
        /// assuming the DefaultValue. In this case, if WriteDefault is true, the default will be
        /// written, unless the default is itself an empty value.
        /// [Default is true]
        /// </summary>
        public bool EmptyIgnore { get; set; }

        /// <summary>
        /// Tells the order in which this property must be saved.
        /// Negative values come first. [Default is 0]
        /// </summary>
        public float SaveOrder { get; set; }
    }

    /// <summary>
    /// When a property is of an abstract type, it must be decided what derived class
    /// will be created to fill that property.
    /// Note that the property must be null, for a decision, otherwise the already
    /// existing object will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public abstract class TypeDecisionAttribute : Attribute
    {
        /// <summary>
        /// Indicates a type decision, to be made either for the PropertyType or the CollectionElementType.
        /// If the type inherits from PropertyType, it will be used for the property.
        /// If the type inherits from CollectionElementType, it will be used for the element of the collection.
        /// </summary>
        /// <param name="evaluationIndex">Attributes are not compiled in order, so this will be used to sort multiple TypeDecisionAttributes.</param>
        /// <param name="type"></param>
        public TypeDecisionAttribute(int evaluationIndex, Type type)
        {
            if (!NameValueConversionCore.IsTypeCreatable(type))
                throw new Exception("Passed type is not creatable, or does not have a parameterless constructor.");

            this.Type = type;
            this.EvaluationIndex = evaluationIndex;
        }

        /// <summary>
        /// Decides whether the type is to be used or not, based on the passed NameValueCollection.
        /// </summary>
        /// <param name="context">Context containing the NameValueCollection to analyse.</param>
        /// <returns>True to use the type indicated in this attribute; otherwise False.</returns>
        public abstract bool Decide(NvcConversionContex context, object instance);

        /// <summary>
        /// Attributes are not compiled in order, so this will be used to sort multiple TypeDecisionAttributes.
        /// </summary>
        internal int EvaluationIndex { get; set; }

        /// <summary>
        /// Type to be used when the Decide method returns true.
        /// </summary>
        public Type Type { get; private set; }
    }

    /// <summary>
    /// Makes the final type decision, when all other attributes have failed to provide a decision.
    /// Note that attributes are evaluated in declaration order, so this one must be the last
    /// type decision attribute.
    /// Note that the property must be null, for a decision, otherwise the already
    /// existing object will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class TypeDecisionDefaultAttribute : TypeDecisionAttribute
    {
        public TypeDecisionDefaultAttribute(int evaluationIndex, Type type)
            : base(evaluationIndex, type)
        {
        }

        public override bool Decide(NvcConversionContex context, object instance)
        {
            return true;
        }
    }

    /// <summary>
    /// Decides on a type based on whether a given name is found in the NameValueCollection.
    /// Note that the property must be null, for a decision, otherwise the already
    /// existing object will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class TypeDecisionByNameAttribute : TypeDecisionAttribute
    {
        public TypeDecisionByNameAttribute(int evaluationIndex, Type type, string nameToFind)
            : base(evaluationIndex, type)
        {
            this.NameToFind = nameToFind;
        }

        /// <summary>
        /// Indicates a type decision based on the name of an item in the name value collection.
        /// If that name is found, the type indicated in this attribute is used.
        /// </summary>
        public string NameToFind { get; private set; }

        public override bool Decide(NvcConversionContex context, object instance)
        {
            var nameToFind = NameValueConversionCore.ReplaceKeyOrIndexNames(this.NameToFind, context.KeyOrIndexValues);
            return context.Nvc.Keys.Cast<string>().Contains(nameToFind);
        }
    }

    /// <summary>
    /// Context object used in the conversion process between a NameValueCollection and an arbitrary object.
    /// This object must be disposed after using it in a conversion to ensure the conversion is terminated.
    /// </summary>
    public class NvcConversionContex : IDisposable
    {
        /// <summary>
        /// NameValueCollection that is being loaded or saved.
        /// </summary>
        public NameValueCollection Nvc { get; set; }

        /// <summary>
        /// Dictionary containing the values fo key/index to be replaced in the name of the item.
        /// </summary>
        public Dictionary<string, string> KeyOrIndexValues { get; set; }

        /// <summary>
        /// Validations that will be executed after everything is done.
        /// </summary>
        public Action FinalValidations { get; set; }

        /// <summary>
        /// Terminates the usage of this context, executing all pending operations.
        /// </summary>
        public void Dispose()
        {
            if (this.FinalValidations != null)
                this.FinalValidations();
        }
    }

    /// <summary>
    /// Represents an object that can save/load properties of a target object into/from a NameValueCollection.
    /// </summary>
    public interface INameValueConverter
    {
        /// <summary>
        /// Saves a property of the object into the NameValueCollection.
        /// </summary>
        /// <param name="context">Provides overall information about the saving process.</param>
        /// <param name="nvc">An empty NameValueCollection to insert the values into.</param>
        /// <param name="name">Name of the entry in the NameValueCollection.</param>
        /// <param name="value">Object that is to be converted to string, and then inserted into the NameValueCollection.</param>
        /// <param name="format">Format that is to be used in the conversion from object to string.</param>
        /// <returns>Returns true when the passed value is a simple value, or false, when it is a complex object.</returns>
        bool SaveProperty(NvcConversionContex context, NameValueCollection nvc, string name, object value, string format);

        /// <summary>
        /// Creates an object to set to a property of the target object,
        /// or to insert into a collection of the target object.
        /// </summary>
        /// <param name="context">Provides the NameValueCollection that is being used to load the object.</param>
        /// <param name="target">Target object containing the property.</param>
        /// <param name="property">The property that is to be set.</param>
        /// <param name="strKeyOrIndex">If we want to insert an object into the collection, we must provide a key/index.</param>
        /// <param name="strValue">String representing the value to be set, or inserted into the collection.</param>
        /// <param name="rollBack">Rollback action, that can be executed to undo everything this method did.</param>
        /// <param name="valueSet">Value that was set to the property, or inserted into the collection.</param>
        /// <returns>Returns true when the inserted value a simple object, or false, when it is a complex object.</returns>
        bool LoadProperty(NvcConversionContex context, object target, NameValueProperty property, string strKeyOrIndex, string strValue, ref Action rollBack, out object valueSet);
    }

    /// <summary>
    /// Default implementation of a INameValueConverter.
    /// It can convert most primitives, objects and collections with good performance.
    /// </summary>
    public class NameValueConverter : INameValueConverter
    {
        public static readonly NameValueConverter Default = new NameValueConverter();

        /// <summary>
        /// Saves a property of the object into the NameValueCollection.
        /// </summary>
        /// <param name="context">Provides overall information about the saving process.</param>
        /// <param name="nvc">An empty NameValueCollection to insert the values into.</param>
        /// <param name="name">Name of the entry in the NameValueCollection.</param>
        /// <param name="value">Object that is to be converted to string, and then inserted into the NameValueCollection.</param>
        /// <param name="format">Format that is to be used in the conversion from object to string.</param>
        /// <returns>Returns true when the passed value is a simple value, or false, when it is a complex object.</returns>
        public virtual bool SaveProperty(NvcConversionContex context, NameValueCollection nvc, string name, object value, string format)
        {
            if (value != null)
            {
                string error;
                var strValue = StringValueConverters.ConvertFrom(value, format, out error);
                if (error == null)
                {
                    nvc[name] = strValue;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates an object to set to a property of the target object,
        /// or to insert into a collection of the target object.
        /// </summary>
        /// <param name="context">Provides the NameValueCollection that is being used to load the object.</param>
        /// <param name="target">Target object containing the property.</param>
        /// <param name="property">The property that is to be set.</param>
        /// <param name="strKeyOrIndex">If we want to insert an object into the collection, we must provide a key/index.</param>
        /// <param name="strValue">String representing the value to be set, or inserted into the collection.</param>
        /// <param name="rollBack">Rollback action, that can be executed to undo everything this method did.</param>
        /// <param name="valueSet">Value that was set to the property, or inserted into the collection.</param>
        /// <returns>Returns true when the inserted value a simple object, or false, when it is a complex object.</returns>
        public virtual bool LoadProperty(NvcConversionContex context, object target, NameValueProperty property, string strKeyOrIndex, string strValue, ref Action rollBack, out object valueSet)
        {
            if (property.PropertyInfo.CanRead)
            {
                var type = property.GetTypeToCreateForProperty(context, target);

                // value that we want to write
                var old = property[target];
                string error; // error in the conversion to a simple value
                valueSet = StringValueConverters.ConvertTo(strValue, type, out error);
                bool isSimpleValue = error == null;

                if (!isSimpleValue)
                {
                    if (old == null) valueSet = Activator.CreateInstance(type);
                    else valueSet = old;
                }

                // If it is possible to write, then do it.
                // Even if it is not possible, we need to return the value that we wanted to write
                // maybe the caller knows a way of doing it... setting a private property.
                if (property.PropertyInfo.CanWrite)
                {
                    if (isSimpleValue || old == null)
                    {
                        if (!isSimpleValue)
                            rollBack += () => property[target] = null;
                        property[target] = valueSet;
                    }
                }

                // If strKeyOrIndex has something, then it is telling us to load a valu inside the collection.
                if (strKeyOrIndex != null)
                {
                    // when strKeyOrIndex this means we are loading a collection
                    // and valueSet is the object added to the collection
                    var collection = valueSet;
                    valueSet = null;

                    // trying to load items into the collection
                    var list = collection as IList;
                    var array = collection as Array;

                    var elementType = property.GetTypeToCreateForElement(context, target);
                    if (list != null || array != null)
                    {
                        if (elementType != null)
                        {
                            if (property.CollectionKeyType == typeof(int))
                            {
                                int index = int.Parse(strKeyOrIndex);

                                // Trying to load the value into the list or array.
                                isSimpleValue = property.PropertyType.IsArray ?
                                    LoadItemToList(elementType, array, false, index, strValue, ref valueSet, ref rollBack) :
                                    LoadItemToList(elementType, list, true, index, strValue, ref valueSet, ref rollBack);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Dictionaries not yet supported.");
                    }
                }

                // If there is no error, then this is a final simple value.
                // It does not matter if we set is or not, it is what we want to set...
                // someone will know how to set it, OR throw exception if it is not possible at all.
                return isSimpleValue;
            }

            valueSet = null;
            return false;
        }

        /// <summary>
        /// Loads a name/value pair into a IList object.
        /// Supports fixed arrays and generic lists.
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="list"></param>
        /// <param name="canExpand"></param>
        /// <param name="index"></param>
        /// <param name="strValue"></param>
        /// <param name="valueSet"></param>
        /// <param name="rollBack"></param>
        /// <returns></returns>
        private static bool LoadItemToList(Type itemType, IList list, bool canExpand, int index, string strValue, ref object valueSet, ref Action rollBack)
        {
            if (list != null)
            {
                // Expand list until the index of the item is available.
                if (canExpand)
                    while (list.Count <= index)
                    {
                        var isertIndex = list.Count;
                        rollBack += () => list.RemoveAt(isertIndex);

                        // Cannot insert nulls in this collection, so we fill it with new empty objects.
                        // We don't know if this collection accepts null values.
                        list.Insert(isertIndex, itemType == typeof(string) ? "" : Activator.CreateInstance(itemType));
                    }

                if (itemType.IsPrimitive || itemType == typeof(string))
                {
                    // setting the value at the index
                    var old = list[index];

                    string error;
                    valueSet = StringValueConverters.ConvertTo(strValue, itemType, out error);
                    bool isSimpleValue = error == null;

                    if (isSimpleValue)
                    {
                        rollBack += () => list[index] = old;
                        list[index] = valueSet;
                    }

                    return isSimpleValue;
                }
                else
                {
                    valueSet = list[index];
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Contains informations that can be used in the process of conversion of a property.
    /// </summary>
    public sealed class NameValueProperty
    {
        internal Func<object, object> Getter { get; set; }
        internal Action<object, object> Setter { get; set; }

        /// <summary>
        /// NameValueAttribute placed on the property.
        /// </summary>
        internal NameValueAttribute Attribute { get; set; }

        public object DefaultValue { get; set; }

        /// <summary>
        /// PropertyInfo of the property.
        /// </summary>
        public PropertyInfo PropertyInfo { get; internal set; }

        /// <summary>
        /// Element type of the collection, when the property is a collection.
        /// </summary>
        public Type CollectionElementType { get; internal set; }

        /// <summary>
        /// Key type of the collection, when the property is a collection.
        /// For arrays and lists, this is always typeof(int).
        /// </summary>
        public Type CollectionKeyType { get; internal set; }

        /// <summary>
        /// Whether the property is a list or not.
        /// </summary>
        public bool IsList { get; internal set; }

        /// <summary>
        /// Whether the property is a dictionary or not.
        /// </summary>
        public bool IsDictionary { get; internal set; }

        /// <summary>
        /// Type returned be the property.
        /// </summary>
        public Type PropertyType { get { return this.PropertyInfo.PropertyType; } }

        /// <summary>
        /// Name of the property.
        /// </summary>
        public string Name { get { return this.PropertyInfo.Name; } }

        /// <summary>
        /// TypeDecisionAttributes that are used to decide the Type to use
        /// when the property needs to be populated with something.
        /// </summary>
        internal TypeDecisionAttribute[] TypeDecidersForProperty { get; set; }

        /// <summary>
        /// Type decision attributes that are used to decide the Type to use
        /// when the collection property needs an element to be inserted.
        /// </summary>
        internal TypeDecisionAttribute[] TypeDecidersForElement { get; set; }

        /// <summary>
        /// Gets a Type that should be used to create a new object populate this property.
        /// </summary>
        /// <param name="context">Conversion context used in the decision of the correct type to return.</param>
        /// <param name="target">Target object that contains the property being loaded.</param>
        /// <returns>Returns a Type to be used to populate this property in the given context, for the target object.</returns>
        public Type GetTypeToCreateForProperty(NvcConversionContex context, object target)
        {
            foreach (var eachDecider in this.TypeDecidersForProperty)
                if (eachDecider.Decide(context, target))
                    return eachDecider.Type;

            var type = this.PropertyType;
            if (!NameValueConversionCore.IsTypeCreatable(type))
                throw new Exception("Type is not creatable, or does not have a parameterless constructor.");
            return type;
        }

        /// <summary>
        /// Gets a Type that should be used to create a new object to insert into the collection
        /// hold by this property.
        /// </summary>
        /// <param name="context">Conversion context used in the decision of the correct type to return.</param>
        /// <param name="target">Target object that contains the collection property that will receive the new object.</param>
        /// <returns>Returns a Type to be used to insert into the collection property, in the given context, for the target object.</returns>
        public Type GetTypeToCreateForElement(NvcConversionContex context, object target)
        {
            if (this.TypeDecidersForElement != null)
                foreach (var eachDecider in this.TypeDecidersForElement)
                    if (eachDecider.Decide(context, target))
                        return eachDecider.Type;

            var type = this.CollectionElementType;
            if (!NameValueConversionCore.IsTypeCreatable(type))
                throw new Exception("Type is not creatable, or does not have a parameterless constructor.");
            return type;
        }

        /// <summary>
        /// Gets the value of the property represented by this object, from the given instance.
        /// Throws exception if cannot read.
        /// </summary>
        /// <param name="instance">Instance of the object to get the property value from.</param>
        /// <returns>Value of the property.</returns>
        private object GetValue(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            if (this.Getter == null)
                throw new ArgumentNullException("Cannot get this property. It is not readable.");

            return this.Getter(instance);
        }

        /// <summary>
        /// Sets the value of the property represented by this object, in the given instance.
        /// Throws exception if cannot write.
        /// </summary>
        /// <param name="instance">Instance of the object to set the property value to.</param>
        /// <param name="value">Value to set the property of the instance to.</param>
        private void SetValue(object instance, object value)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            if (this.Setter == null)
                throw new ArgumentNullException("Cannot set this property. It is not writable.");

            this.Setter(instance, value);
        }

        /// <summary>
        /// Returns whether this property has a getter or not.
        /// </summary>
        public bool CanGet
        {
            get { return this.Getter != null; }
        }

        /// <summary>
        /// Returns whether this property has a setter or not.
        /// </summary>
        public bool CanSet
        {
            get { return this.Setter != null; }
        }

        /// <summary>
        /// Gets or sets the value of the property of the instance object.
        /// </summary>
        /// <param name="instance">Instance containing a property represented by this object, to get or set the value.</param>
        /// <returns>Returns the value of the property from the instance object.</returns>
        public object this[object instance]
        {
            get { return this.GetValue(instance); }
            set { this.SetValue(instance, value); }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} [from {2}]", typeof(NameValueProperty).Name, this.Name, this.PropertyInfo.DeclaringType.Name);
        }
    }

    /// <summary>
    /// The core of the NameValueCollection conversion.
    /// </summary>
    public static class NameValueConversionCore
    {
        /// <summary>
        /// Saves an object to a NameValueCollection.
        /// </summary>
        /// <param name="objectToSave">Object to save in a NameValueCollection.</param>
        /// <returns>NameValueCollection containing the saved data.</returns>
        public static NameValueCollection Save(object objectToSave)
        {
            // Argument validation.
            if (objectToSave == null)
                throw new ArgumentNullException("objectToSave");

            // Creating the context for the saving operation.
            var context = new NvcConversionContex
            {
                Nvc = new NameValueCollection(),
                KeyOrIndexValues = new Dictionary<string, string>(),
            };

            // Saving contents of the object into the NameValueCollection.
            using (context)
            {
                SaveCore(context, objectToSave);
                return context.Nvc;
            }
        }

        /// <summary>
        /// Saves an object to a NameValueCollection, without clearing it first.
        /// </summary>
        /// <param name="objectToSave">Object to save in a NameValueCollection.</param>
        /// <param name="destination">NameValueCollection used to save data from objectToSave.</param>
        public static void SaveTo(object objectToSave, NameValueCollection destination)
        {
            // Argument validation.
            if (objectToSave == null)
                throw new ArgumentNullException("objectToSave");

            if (destination == null)
                throw new ArgumentNullException("destination");

            // Creating the context for the saving operation.
            var context = new NvcConversionContex
            {
                Nvc = destination,
                KeyOrIndexValues = new Dictionary<string, string>(),
            };

            // Saving contents of the object into the NameValueCollection.
            using (context)
                SaveCore(context, objectToSave);
        }

        /// <summary>
        /// Loads an object of type T from the nameValueCollection.
        /// </summary>
        /// <typeparam name="T">Type of the object that will be loaded from the NameValueCollection.</typeparam>
        /// <param name="nvcToLoad">NameValueCollection that contains the values to load the object.</param>
        /// <returns>Object of type T, loaded from the NameValueCollection.</returns>
        public static T Load<T>(NameValueCollection nvcToLoad)
            where T : new()
        {
            // Argument validation.
            if (nvcToLoad == null)
                throw new ArgumentNullException("nvcToLoad");

            // Creating new object to return.
            T obj = new T();

            // Creating the context for the loading operation.
            var context = new NvcConversionContex
            {
                Nvc = nvcToLoad,
                KeyOrIndexValues = new Dictionary<string, string>()
            };

            // Loading the data from the nvcToLoad into the target object.
            using (context)
                foreach (var eachKey in nvcToLoad.AllKeys)
                {
                    var eachValue = nvcToLoad[eachKey];
                    LoadCore(context, obj, eachKey, eachValue);
                }

            return obj;
        }

        /// <summary>
        /// Loads data from a NameValueCollection into a target object, without clearing it first.
        /// </summary>
        /// <param name="nvcToLoad">NameValueCollection that contains the values to load the object.</param>
        /// <param name="target">Target object where name-values will be loaded.</param>
        public static void LoadTo(NameValueCollection nvcToLoad, object target)
        {
            // Argument validation.
            if (nvcToLoad == null)
                throw new ArgumentNullException("nvcToLoad");

            if (target == null)
                throw new ArgumentNullException("target");

            // Creating the context for the loading operation.
            var context = new NvcConversionContex
            {
                Nvc = nvcToLoad,
                KeyOrIndexValues = new Dictionary<string, string>(),
            };

            // Loading the data from the nvcToLoad into the target object.
            using (context)
                foreach (var eachKey in nvcToLoad.AllKeys)
                {
                    var eachValue = nvcToLoad[eachKey];
                    LoadCore(context, target, eachKey, eachValue);
                }
        }

        /// <summary>
        /// Loads an object of passed type from the nameValueCollection.
        /// </summary>
        /// <param name="nvcToLoad">NameValueCollection that contains the values to load the object.</param>
        /// <param name="type">Type of the object that will be loaded from the NameValueCollection.</param>
        /// <returns>Object of passed type, loaded from the NameValueCollection.</returns>
        public static object Load(NameValueCollection nvcToLoad, Type type)
        {
            // Argument validation.
            if (nvcToLoad == null)
                throw new ArgumentNullException("nvcToLoad");

            // Creating new object to return.
            var result = Activator.CreateInstance(type);

            // Creating the context for the loading operation.
            var context = new NvcConversionContex
            {
                Nvc = nvcToLoad,
                KeyOrIndexValues = new Dictionary<string, string>(),
            };

            // Loading the data from the nvcToLoad into the result object.
            using (context)
                foreach (var eachKey in nvcToLoad.AllKeys)
                {
                    var eachValue = nvcToLoad[eachKey];
                    LoadCore(context, result, eachKey, eachValue);
                }

            return result;
        }

        /// <summary>
        /// Stores information gathered from a type that can be converted to/from a NameValueCollection.
        /// </summary>
        class Cache
        {
            public List<NameValueProperty> listProps = new List<NameValueProperty>();
            public Dictionary<string, NameValueProperty> mapName = new Dictionary<string, NameValueProperty>();
            public Dictionary<string, NameValueProperty> mapRegex = new Dictionary<string, NameValueProperty>();
            public List<NameValueProperty> listComplex = new List<NameValueProperty>();
        }

        static Dictionary<Type, Cache> mapTypeToCache = new Dictionary<Type, Cache>();

        /// <summary>
        /// Clears the static cache of informations about converted types, to free memory.
        /// Call this if you don't need to save or load more objects.
        /// You can call this while converting... forcing a cache rebuild.
        /// </summary>
        public static void ClearCache()
        {
            lock (mapTypeToCache)
                mapTypeToCache.Clear();
        }

        /// <summary>
        /// Initializes all values needed to make a fast conversion.
        /// This caches a lot of things, that would otherwise make the saving or loading processo too slow.
        /// </summary>
        static Cache InitCache(Type type)
        {
            Cache cache;

            lock (mapTypeToCache)
                if (mapTypeToCache.TryGetValue(type, out cache))
                    return cache;
                else
                    mapTypeToCache[type] = cache = new Cache();

            lock (cache)
            {
                foreach (var eachProperty in type.GetProperties())
                {
                    var attrs = Attribute.GetCustomAttributes(eachProperty, true);

                    var attr = attrs
                        .OfType<NameValueAttribute>()
                        .SingleOrDefault();

                    var attrDecisions = attrs
                        .OfType<TypeDecisionAttribute>()
                        .OrderBy(a => a.EvaluationIndex)
                        .ToArray();

                    if (attr != null)
                    {
                        // Note: Cannot use Delegate.CreateDelegate, because it throws errors about incompatible types.
                        //      var getter = !eachProperty.CanRead ? null :
                        //          (Func<T, object>)Delegate.CreateDelegate(typeof(Func<T, object>), eachProperty.GetGetMethod());
                        // Will use Expression.Compile, as that is more reliable.

                        // creating property getter and setter
                        Func<object, object> getter = null;
                        var paramInstance = Expression.Parameter(typeof(object), "instance"); // T instance
                        if (eachProperty.CanRead)
                        {
                            // (object instance) => (object)((T)instance).{prop.Name};
                            var lamdaGetter = Expression.Lambda<Func<object, object>>(
                                Expression.ConvertChecked( // (object)((T)instance).{prop.Name}
                                    Expression.Property( // ((T)instance).{prop.Name}
                                        Expression.ConvertChecked(paramInstance, type), // (T)instance
                                        eachProperty),
                                        typeof(object)),
                                paramInstance);

                            getter = lamdaGetter.Compile();
                        }

                        Action<object, object> setter = null;
                        if (eachProperty.CanWrite)
                        {
                            var paramValue = Expression.Parameter(typeof(object), "value"); // object value
                            // (object instance, object value) => ((T)instance).{prop.Name} = ({prop.PropertyType})object;
                            var lamdaSetter = Expression.Lambda<Action<object, object>>(
                                Expression.Assign( // instance.{prop.Name} = ({prop.PropertyType})object
                                    Expression.Property( // instance.{prop.Name}
                                        Expression.ConvertChecked(paramInstance, type), // (T)instance
                                        eachProperty),
                                    Expression.ConvertChecked(paramValue, eachProperty.PropertyType)), // ({prop.PropertyType})object
                                paramInstance,
                                paramValue);

                            setter = lamdaSetter.Compile();
                        }

                        // Getting key type, if it is a collection.
                        var keyType = attr.CollectionKeyType
                            ?? (eachProperty.PropertyType.IsArray ?
                                typeof(int) :
                                eachProperty.PropertyType.GetInterfaces()
                            // filter by the ones the are IList<T> or IDictionary<K,T>
                                    .Where(i => i.IsGenericType)
                                    .Where(i => i.GetGenericTypeDefinition() == typeof(IList<>) || i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                            // selecte typeof(int) for IList<T> or typeof(K) for IDictionary<K,T>
                                    .Select(i => i.GetGenericArguments())
                                    .Select(ga => ga.Length == 1 ? typeof(int) : ga[0])
                                    .Where(ke => ke.IsPrimitive || ke == typeof(string))
                                    .FirstOrDefault());

                        // Getting element type, if it is a collection.
                        var elementType = attr.CollectionElementType
                            ?? (eachProperty.PropertyType.IsArray ?
                                eachProperty.PropertyType.GetElementType() :
                                eachProperty.PropertyType.GetInterfaces()
                            // filter by the ones the are IList<T> or IDictionary<K,T>
                                    .Where(i => i.IsGenericType)
                                    .Where(i => i.GetGenericTypeDefinition() == typeof(IList<>) || i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                            // selecte the generic argument T from IList<T> or IDictionary<K,T>, where T : class, new() and is not abstract
                                    .Select(i => i.GetGenericArguments())
                                    .Select(ga => ga.Last())
                                    .Where(ke => NameValueConversionCore.IsTypeCreatable(ke))
                                    .FirstOrDefault());

                        var decidersForPropertyType = attrDecisions
                            .Where(x => eachProperty.PropertyType.IsAssignableFrom(x.Type))
                            .ToArray();

                        var decidersForElementType = elementType == null ? null : attrDecisions
                            .Where(x => elementType.IsAssignableFrom(x.Type))
                            .ToArray();

                        // Getting default value.
                        var defaultValue = attr.Default
                            ?? (eachProperty.PropertyType.IsValueType
                            ? Activator.CreateInstance(eachProperty.PropertyType)
                            : null);

                        // collecting information needed to SAVE and LOAD the object in a name-value collection
                        var info = new NameValueProperty
                        {
                            Attribute = attr,
                            Getter = getter,
                            Setter = setter,
                            DefaultValue = defaultValue,
                            PropertyInfo = eachProperty,
                            CollectionKeyType = keyType,
                            CollectionElementType = elementType,
                            IsList = typeof(IList).IsAssignableFrom(eachProperty.PropertyType),
                            IsDictionary = typeof(IDictionary).IsAssignableFrom(eachProperty.PropertyType),
                            TypeDecidersForProperty = decidersForPropertyType,
                            TypeDecidersForElement = decidersForElementType,
                        };

                        cache.listProps.Add(info);
                    }
                }

                // sorting list, and adding to other collections
                cache.listProps = cache.listProps.OrderBy(x => x.Attribute.SaveOrder).ToList();

                foreach (var info in cache.listProps)
                {
                    if (info.Attribute.Name == null && info.Attribute.KeyOrIndexName == null)
                        cache.listComplex.Add(info);

                    if (info.Attribute.Name != null)
                        cache.mapName.Add(info.Attribute.Name, info);

                    if (info.Attribute.NameRegex != null)
                        cache.mapRegex.Add(info.Attribute.NameRegex, info);
                }
            }

            return cache;
        }

        /// <summary>
        /// Core saving method.
        /// </summary>
        /// <param name="objectToSave">Object that is being saved. This object must be of type T.</param>
        /// <param name="nvc">NameValueCollection that is being loaded.</param>
        /// <param name="keyOrIndexValues">Dictionary containing the values fo key/index to be replaced in the name of the item.</param>
        public static void SaveCore(NvcConversionContex context, object objectToSave)
        {
            var cache = InitCache(objectToSave.GetType());
            var converter = objectToSave as INameValueConverter ?? NameValueConverter.Default;

            foreach (var eachProp in cache.listProps)
            {
                var nameFormat = eachProp.Attribute.Name ?? eachProp.PropertyInfo.Name;
                if (eachProp.IsList)
                {
                    // saving list values
                    var list = (IList)eachProp[objectToSave];
                    for (int it = 0; it < list.Count; it++)
                    {
                        context.KeyOrIndexValues.Add(eachProp.Attribute.KeyOrIndexName, it.ToString());

                        var name = ReplaceKeyOrIndexNames(nameFormat, context.KeyOrIndexValues);

                        var nvc = new NameValueCollection();
                        if (!converter.SaveProperty(context, nvc, name, list[it], eachProp.Attribute.ValueFormat))
                        {
                            if (list[it] != null)
                                SaveCore(context, list[it]);
                        }

                        context.KeyOrIndexValues.Remove(eachProp.Attribute.KeyOrIndexName);
                    }
                }
                else if (eachProp.IsDictionary)
                {
                    throw new Exception("Dictionary not yet supported.");
                }
                else
                {
                    // saving complex objects and simple values
                    var name = ReplaceKeyOrIndexNames(nameFormat, context.KeyOrIndexValues);
                    var value = eachProp[objectToSave];
                    if (!AreEqual(value, eachProp.DefaultValue) || eachProp.Attribute.WriteDefault)
                    {
                        var nvc = new NameValueCollection();
                        if (converter.SaveProperty(context, nvc, name, value, eachProp.Attribute.ValueFormat))
                        {
                            // If value is Empty:
                            // - remove it if EmptyIgnore is true;
                            // - save the default value, if WriteDefault is true;
                            // - remove it again if the Default value is Empty.
                            // Note: NameValueCollection return null for every name that it does not contain.
                            if (eachProp.Attribute.EmptyIgnore && nvc[name] == "")
                            {
                                nvc.Remove(name);
                                value = eachProp.DefaultValue;

                                if (!converter.SaveProperty(context, nvc, name, value, eachProp.Attribute.ValueFormat))
                                    throw new Exception("Value should be convertible to string.");

                                // Note: NameValueCollection return null for every name that it does not contain.
                                // So nvc[name] == "" means: one single element with value == ""
                                if (nvc[name] == "")
                                    nvc.Remove(name);
                            }

                            nvc.CopyTo(context.Nvc, replace: false);
                        }
                        else
                        {
                            nvc.CopyTo(context.Nvc, replace: false);

                            if (value != null)
                                SaveCore(context, value);
                        }
                    }
                }
            }
        }

        private static bool AreEqual(object a, object b)
        {
            return a == b || a != null && a.Equals(b) || b != null && b.Equals(a);
        }

        /// <summary>
        /// Core loading method.
        /// </summary>
        /// <param name="obj">Object that is being saved. This object must be of type T.</param>
        /// <param name="itemName">Item name of the entry in the NameValueCollection that is to be loaded.</param>
        /// <param name="strValue">Item value of the entry in the NameValueCollection that is to be loaded.</param>
        public static bool LoadCore(NvcConversionContex context, object objectToLoad, string itemName, string strValue)
        {
            var cache = InitCache(objectToLoad.GetType());
            var converter = objectToLoad as INameValueConverter ?? NameValueConverter.Default;

            // If property is a simple type.
            {
                NameValueProperty prop;
                if (cache.mapName.TryGetValue(itemName, out prop))
                {
                    // If name is in dictionary, we can then load the string value as a simple value.
                    // Note: strValue will never be null, since NameValueCollection cannot contain null values.
                    if (strValue == "" && prop.Attribute.EmptyIgnore)
                    {
                        prop[objectToLoad] = prop.DefaultValue;
                        return true;
                    }

                    // Set it if it has a setter, or check it when there is only a getter.
                    Action rollBack = null;
                    object valueThatWasSet;
                    bool isSimpleValueLoaded = converter.LoadProperty(context, objectToLoad, prop, null, strValue, ref rollBack, out valueThatWasSet);

                    // Simple values don't need roll-back information.
                    if (rollBack != null)
                        throw new Exception("Roll back should be created only for collections and complex types.");

                    if (!isSimpleValueLoaded)
                        throw new Exception("Unsupported property type");

                    if (prop.CanGet && !AreEqual(prop[objectToLoad], valueThatWasSet))
                        // Scheduling the validation to the end of the loading process.
                        context.FinalValidations += () =>
                        {
                            if (prop.CanGet && !AreEqual(prop[objectToLoad], valueThatWasSet))
                                if (!prop.CanSet)
                                    throw new Exception("Property has no setter and value is different from source");
                                else
                                    throw new Exception("The value didn't really change after setting the property.");
                        };

                    return true;
                }
            }

            // If name is not in dictionary, we must try complex properties.
            foreach (var eachComplex in cache.listComplex)
            {
                Action rollBack = null;
                object valueThatWasSet;
                bool finished = converter.LoadProperty(context, objectToLoad, eachComplex, null, strValue, ref rollBack, out valueThatWasSet);

                if (!finished)
                    if (LoadCore(context, valueThatWasSet, itemName, strValue))
                        return true;

                // Load failed, now we must be roll-back.
                if (rollBack != null)
                    rollBack.GetInvocationList().Cast<Action>().Reverse().ForEach(x => x());
            }

            // If name was not loaded into a complex property, we will try to load it in a collection.
            // A collection property requires a regex, so that the key/index of the element can be
            // extracted from the name.
            foreach (var eachKV in cache.mapRegex)
            {
                var pattern = ReplaceKeyOrIndexNames(eachKV.Key, context.KeyOrIndexValues);

                var match = Regex.Match(itemName, pattern, RegexOptions.IgnorePatternWhitespace);
                if (match.Success)
                {
                    string strKeyOrIndex = match.Groups[eachKV.Value.Attribute.KeyOrIndexName].Value;
                    if (eachKV.Value.IsList)
                    {
                        Action rollBack = null;
                        object valueThatWasSet;
                        bool finished = converter.LoadProperty(context, objectToLoad, eachKV.Value, strKeyOrIndex, strValue, ref rollBack, out valueThatWasSet);

                        var grp = match.Groups[eachKV.Value.Attribute.KeyOrIndexName];
                        var name2 = string.Format("{0}{{{1}}}{2}",
                            itemName.Substring(0, grp.Index),
                            eachKV.Value.Attribute.KeyOrIndexName,
                            itemName.Substring(grp.Index + grp.Length));

                        if (finished || LoadCore(context, valueThatWasSet, name2, strValue))
                            return true;

                        // Load failed, now we must be roll-back.
                        if (rollBack != null)
                            rollBack.GetInvocationList().Cast<Action>().Reverse().ForEach(x => x());
                    }
                    else
                        throw new Exception("Unsupported collection type.");
                }
            }

            return false;
        }

        internal static string ReplaceKeyOrIndexNames(string nameFormat, Dictionary<string, string> dic)
        {
            var name = nameFormat;
            foreach (var eachKV in dic)
                name = name.Replace('{' + eachKV.Key + '}', eachKV.Value);
            return name;
        }

        public static bool IsTypeCreatable(Type type)
        {
            if (type.IsAbstract || type.IsInterface || type.GetConstructor(Type.EmptyTypes) == null)
                if (type != typeof(string) && !type.IsValueType)
                    return false;
            return true;
        }
    }

    /// <summary>
    /// Atribute that can be used on enum values, to indicate an equivalent string value,
    /// when converting a a NameValueCollection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class StringValueAttribute : Attribute
    {
        public StringValueAttribute(string stringValue)
        {
            // Value cannot be null, but can be empty.
            if (stringValue == null)
                throw new ArgumentNullException("stringValue");

            this.StringValue = stringValue;
        }

        /// <summary>
        /// String representation of the enum item, that will be used when saving this enum into
        /// a NameValueCollection, or when restoring it from the NameValueCollection.
        /// </summary>
        public string StringValue { get; private set; }

        /// <summary>
        /// Represents information gathered from an enum type,
        /// about the string representations of each enum value.
        /// </summary>
        class EnumCodeMap
        {
            public readonly Dictionary<object, string> CodeByValue = new Dictionary<object, string>();
            public readonly Dictionary<string, object> ValueByCode = new Dictionary<string, object>();

            public EnumCodeMap(Type enumType)
            {
                foreach (var eachValue in Enum.GetValues(enumType))
                {
                    var value = eachValue;
                    var attr = ((Enum)eachValue).GetAttributeOfType<StringValueAttribute>();
                    string code = attr == null ? eachValue.ToString() : attr.StringValue;
                    CodeByValue[value] = code;
                    ValueByCode[code] = value;
                }
            }
        }

        static Dictionary<Type, EnumCodeMap> enumTypeMaps = new Dictionary<Type, EnumCodeMap>();

        /// <summary>
        /// Gets the string representation of an enum value.
        /// If the enum value does not have a StringValueAttribute, then returns ToString.
        /// </summary>
        /// <param name="value">Enum value to get the string representation from.</param>
        /// <returns>The string representation of the enum value. This will never return null.</returns>
        public static string GetStringValue(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (!(value is Enum))
                throw new ArgumentException("Argument value must be an enum value.", "value");

            EnumCodeMap enumCodeMap;
            lock (enumTypeMaps)
                if (!enumTypeMaps.TryGetValue(value.GetType(), out enumCodeMap))
                    enumTypeMaps[value.GetType()] = enumCodeMap = new EnumCodeMap(value.GetType());

            string result;
            enumCodeMap.CodeByValue.TryGetValue(value, out result);
            return result;
        }

        /// <summary>
        /// Gets an enum value from it's string representation.
        /// If no enum values in the type have that string representation, then returns null.
        /// </summary>
        /// <param name="strValue">String value that represents an enum value.</param>
        /// <param name="enumType">Enum type that contains the value represented by the passed string.</param>
        /// <returns>Enum value that is represented by the string, or null if not found.</returns>
        public static object GetEnumValue(string strValue, Type enumType)
        {
            if (strValue == null)
                throw new ArgumentNullException("strValue");

            if (enumType == null)
                throw new ArgumentNullException("enumType");

            if (!enumType.IsEnum)
                throw new ArgumentException("Argument enumType must be an enum type (i.e. IsEnum == true).", "enumType");

            EnumCodeMap enumCodeMap;
            lock (enumTypeMaps)
                if (!enumTypeMaps.TryGetValue(enumType, out enumCodeMap))
                    enumTypeMaps[enumType] = enumCodeMap = new EnumCodeMap(enumType);

            object result;
            enumCodeMap.ValueByCode.TryGetValue(strValue, out result);
            return result;
        }
    }

    /// <summary>
    /// Converts simple values to and from string.
    /// This class will be used to convert simple values when saving or restoring an object from a
    /// NameValueCollection.
    /// </summary>
    public static class StringValueConverters
    {
        /// <summary>
        /// Converts a string to a destination type.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="toType">Type that the string is to be converted to.</param>
        /// <param name="error">Out parameter returning an error message, if any.</param>
        /// <returns>Returns the converted object.</returns>
        public static object ConvertTo(string str, Type toType, out string error)
        {
            error = null;
            if (toType == typeof(string))
            {
                return str;
            }
            else if (toType == typeof(bool) || toType == typeof(bool?))
            {
                if (string.IsNullOrEmpty(str) && toType == typeof(bool?))
                    return null;

                if (str == "0") return false;
                if (str == "1") return true;
            }
            else
            {
                var nullableType = Nullable.GetUnderlyingType(toType);

                if (new Type[] {
                    typeof(int),   typeof(uint),
                    typeof(long),  typeof(ulong),
                    typeof(short), typeof(ushort),
                    typeof(byte),  typeof(sbyte),
                    typeof(float),  typeof(double), typeof(decimal),
                    }.Contains(nullableType ?? toType))
                {
                    if (nullableType != null && string.IsNullOrEmpty(str))
                        return null;

                    try
                    {
                        var value = Convert.ChangeType(str, nullableType ?? toType, CultureInfo.InvariantCulture);
                        return value;
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                    }
                }
                else if ((nullableType ?? toType).IsEnum)
                    return StringValueAttribute.GetEnumValue(str, nullableType ?? toType);
                else
                    error = "Invalid type to convert to.";
            }

            error = "Invalid value in string.";

            return null;
        }

        /// <summary>
        /// Converts an object to a string.
        /// </summary>
        /// <param name="value">Object to convert to string.</param>
        /// <param name="format">Format to used for the string conversion.</param>
        /// <param name="error">Out parameter returning an error message, if any.</param>
        /// <returns>Returns the converted string.</returns>
        public static string ConvertFrom(object value, string format, out string error)
        {
            error = null;

            if (value == null)
                return null;

            if (value is string)
                return string.Format(format ?? "{0}", value);

            if (value.GetType() == typeof(bool))
                return (bool)value ? "1" : "0";

            if (new Type[] {
                typeof(int),   typeof(uint),
                typeof(long),  typeof(ulong),
                typeof(short), typeof(ushort),
                typeof(byte),  typeof(sbyte),
                typeof(float),  typeof(double), typeof(decimal),
                }.Contains(value.GetType()))
            {
                return string.IsNullOrWhiteSpace(format) ?
                    Convert.ToString(value, CultureInfo.InvariantCulture) :
                    string.Format(CultureInfo.InvariantCulture, string.Format("{{0:{0}}}", format), value);
            }

            if (value.GetType().IsEnum)
                return StringValueAttribute.GetStringValue(value);

            error = "Unsupported value type.";
            return null;
        }
    }

    /// <summary>
    /// Extensions to let a NameValueCollection be converted to another type.
    /// </summary>
    public static class NameValueCollectionExtensions
    {
        /// <summary>
        /// Copies the entries from nvcSource to nvcTarget.
        /// </summary>
        /// <param name="nvcSource"></param>
        /// <param name="nvcTarget"></param>
        /// <param name="replace">When true, if the target already contains the named item, it will be cleared.</param>
        internal static void CopyTo(this NameValueCollection nvcSource, NameValueCollection nvcTarget, bool replace)
        {
            // Removing old keys.
            var sourceKeys = nvcSource.AllKeys;
            if (replace)
                foreach (var key in nvcTarget.AllKeys)
                    if (sourceKeys.Contains(key))
                        nvcTarget.Remove(key);

            // Copying values from source to target.
            nvcTarget.Add(nvcSource);
        }

        /// <summary>
        /// Enumerates a NameValueCollection using KeyValuePairs, for each key/value pair found.
        /// NameValueCollections can store more than one value per key,
        /// so this can return multiples repeated keys in a row.
        /// </summary>
        /// <param name="nvc">NameValueCollection to enumerate name/value pairs.</param>
        /// <returns>Enumerable that can enumerate all name/value pairs of the collection.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(this NameValueCollection nvc)
        {
            foreach (var key in nvc.AllKeys)
                foreach (var value in nvc.GetValues(key))
                    yield return new KeyValuePair<string, string>(key, value);
        }
    }
}
