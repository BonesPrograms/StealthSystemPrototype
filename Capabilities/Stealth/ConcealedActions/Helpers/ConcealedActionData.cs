using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using XRL.World;
using XRL.World.Parts;
using XRL.Collections;

using StealthSystemPrototype.Alerts;

using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using System.Reflection;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class ConcealedActionData
        : Rack<BaseAlert>
        , IConcealedAction
    {
        public static string METHOD_GET_TYPE => "Method_" + nameof(GetType);

        public StringMap<object> StoredFields = new()
        {
            { METHOD_GET_TYPE, null },
            { nameof(Type.GenericTypeArguments), null },
            { nameof(Items), null },
            { nameof(ID), null },
            { nameof(Name), null },
            { nameof(Action), null },
            { nameof(Hider), null },
            { nameof(AlertObject), null },
            { nameof(AlertLocation), null },
            { nameof(SneakPerformance), null },
            { nameof(Aggressive), null },
            { nameof(Description), null },
            { nameof(Items), null },
        };

        public string ID => StoredFields[nameof(ID)] as string;
        public string Name => StoredFields[nameof(Name)] as string;
        public string Action => StoredFields[nameof(Action)] as string;
        public GameObject Hider => StoredFields[nameof(Hider)] as GameObject;
        public GameObject AlertObject => StoredFields[nameof(AlertObject)] as GameObject;
        public Cell AlertLocation => StoredFields[nameof(AlertLocation)] as Cell;
        public SneakPerformance SneakPerformance => StoredFields[nameof(SneakPerformance)] as SneakPerformance;
        public bool Aggressive => (bool)StoredFields[nameof(Aggressive)];
        public string Description => StoredFields[nameof(Description)] as string;

        public ConcealedActionData()
        {
        }
        public ConcealedActionData(IConcealedAction Source)
            : this()
        {
            StoredFields[METHOD_GET_TYPE] = Source.GetType();

            Items = Source.ToArray();
            Length = ((ICollection<BaseAlert>)Source).Count;
            Size = ((ICollection<BaseAlert>)Source).Count;
            StoredFields[nameof(Items)] = Items;
            StoredFields[nameof(ID)] = Source.GetAction();
            StoredFields[nameof(Name)] = Source.GetName();
            StoredFields[nameof(Action)] = Source.GetAction();
            StoredFields[nameof(Hider)] = Source.GetHider();
            StoredFields[nameof(AlertObject)] = Source.GetAlertObject();
            StoredFields[nameof(AlertLocation)] = Source.GetAlertLocation();
            StoredFields[nameof(SneakPerformance)] = Source.GetSneakPerformance();
            StoredFields[nameof(Aggressive)] = Source.GetAggressive();
            StoredFields[nameof(Description)] = Source.GetDescription();

            FieldInfo[] fields = Source.GetType().GetFields();

            foreach (FieldInfo fieldInfo in fields)
                if (!StoredFields.ContainsKey(fieldInfo.Name)
                    && (fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    StoredFields[fieldInfo.Name] = fieldInfo.GetValue(Source);
        }

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteComposite(StoredFields);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            StoredFields = Reader.ReadComposite<StringMap<object>>();
        }

        #endregion

        public string GetID()
            => ID;

        public string GetName()
            => Name;

        public string GetAction()
            => Action;

        public GameObject GetHider()
            => Hider;

        public GameObject GetAlertObject()
            => AlertObject;

        public Cell GetAlertLocation()
            => AlertLocation;

        public SneakPerformance GetSneakPerformance()
            => SneakPerformance;

        public bool GetAggressive()
            => Aggressive;

        public string GetDescription()
            => Description;

        public virtual ConcealedActionData Initialize()
            => this;

        IConcealedAction IConcealedAction.Initialize()
            => Initialize();

        public virtual void Configure()
        {
        }

        public static explicit operator BaseConcealedAction(ConcealedActionData Operand)
        {
            if (Operand.StoredFields[METHOD_GET_TYPE] is Type storedType
                && Activator.CreateInstance(storedType) is BaseConcealedAction newBaseConcealedAction)
            {
                Dictionary<string, FieldInfo> newFields = new();
                if (storedType.GetFields() is FieldInfo[] fieldsArray)
                    foreach (FieldInfo fieldInfo in fieldsArray)
                        if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                            && !fieldInfo.IsLiteral)
                            newFields[fieldInfo.Name] = fieldInfo;

                if (Operand.StoredFields[nameof(Items)] is BaseAlert[] operandStoredItems
                    && !operandStoredItems.IsNullOrEmpty())
                    newBaseConcealedAction.AddRange((IReadOnlyList<BaseAlert>)operandStoredItems);

                foreach ((string fieldName, object fieldValue) in Operand.StoredFields)
                {
                    if (newFields.ContainsKey(fieldName)
                        && newFields[fieldName] is FieldInfo newField)
                        newField.SetValue(newBaseConcealedAction, fieldValue);
                }
                return newBaseConcealedAction;
            }
            return new(Operand);
        }

        public static explicit operator ConcealedActionData(BaseConcealedAction Operand)
            => new(Operand);
    }
}
