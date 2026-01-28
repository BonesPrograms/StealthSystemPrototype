using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using XRL;
using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Represents the obviousness of an <see cref="IConcealedAction"/> capable of being detected by an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// The degree to which such an activity is detectable.
    /// </remarks>
    [HasModSensitiveStaticCache]
    [StealthSystemBaseClass]
    [Serializable]
    public class BaseAlert : IAlert<BaseAlert>
    {
        #region Serialization

        private string _Name;
        public string Name => _Name ??= GetType().ToStringWithGenerics();

        private bool? _IsBase;
        public virtual bool IsBase => _IsBase ??= GetType() == typeof(BaseAlert);

        public virtual int DefaultIntensity => 5;

        private int _Intensity;
        public virtual int Intensity
        {
            get => _Intensity;
            set => _Intensity = value;
        }

        private Dictionary<string, string> _Properties;
        public virtual Dictionary<string, string> Properties
        {
            get => _Properties;
            set => _Properties = value;
        }

        #endregion
        #region Constructors

        public BaseAlert()
        {
            _Name = null;
            _IsBase = null;
            _Intensity = DefaultIntensity;
        }
        public BaseAlert(int Intensity)
            : this()
        {
            _Intensity = Intensity;
        }
        public BaseAlert(int Intensity, Dictionary<string, string> Properties)
            : this(Intensity)
        {
            _Properties = Properties;
        }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(_Name);
            Writer.WriteOptimized(_IsBase);
            Writer.WriteOptimized(_Intensity);
            Writer.WriteOptimized(_Properties);
        }

        public virtual void Read(SerializationReader Reader)
        {
            _Name = Reader.ReadOptimizedString();
            _IsBase = Reader.ReadOptimizedNullableBool();
            _Intensity = Reader.ReadOptimizedInt32();
            _Properties = Reader.ReadOptimizedStringPairDictionary();
        }

        #endregion

        public virtual void Initialize()
        {
        }

        public virtual void Created()
        {
        }

        public virtual BaseAlert AdjustIntensity(int Amount)
            => new(Intensity + Amount);

        #region IDisposable

        public virtual void Dispose()
        {
        }

        #endregion
        #region Explicit Implementations

        IAlert IAlert.AdjustIntensity(int Amount)
            => AdjustIntensity(Amount);

        public BaseAlert Copy()
        {
            throw new NotImplementedException();
        }

        IAlert IAlert.Copy()
        {
            return Copy();
        }

        #endregion
    }
}
