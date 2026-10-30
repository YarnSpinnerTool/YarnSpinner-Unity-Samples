namespace Yarn.Unity.Samples
{

    using Yarn.Unity;

    /// <summary>
    /// Day
    /// </summary>
    /// <remarks>
    /// Automatically generated from Yarn project at Packages/dev.yarnspinner.unity.samples/Samples/BasicSaliency/BasicSaliency.yarnproject.
    /// </remarks>
    [System.CodeDom.Compiler.GeneratedCode("YarnSpinner", "3.2.4.0")]
    [Yarn.Unity.Attributes.YarnGeneratedEnum(Yarn.Shared.YarnEnum.BackingType.Int)]
    public enum Day
    {

        /// <summary>
        /// Monday
        /// </summary>
        Monday = 0,

        /// <summary>
        /// Tuesday
        /// </summary>
        Tuesday = 1,

        /// <summary>
        /// Wednesday
        /// </summary>
        Wednesday = 2,
    }

    /// <summary>
    /// TimeOfDay
    /// </summary>
    /// <remarks>
    /// Automatically generated from Yarn project at Packages/dev.yarnspinner.unity.samples/Samples/BasicSaliency/BasicSaliency.yarnproject.
    /// </remarks>
    [System.CodeDom.Compiler.GeneratedCode("YarnSpinner", "3.2.4.0")]
    [Yarn.Unity.Attributes.YarnGeneratedEnum(Yarn.Shared.YarnEnum.BackingType.Int)]
    public enum TimeOfDay
    {

        /// <summary>
        /// Morning
        /// </summary>
        Morning = 0,

        /// <summary>
        /// Evening
        /// </summary>
        Evening = 1,
    }

    [System.CodeDom.Compiler.GeneratedCode("YarnSpinner", "3.2.4.0")]
    internal static class BasicSaliencyVariableStorageTypeExtensions
    {
        internal static int GetBackingValue(this Day enumValue)
        {
            switch (enumValue)
            {
                case Day.Monday:
                    return 0;
                case Day.Tuesday:
                    return 1;
                case Day.Wednesday:
                    return 2;
                default:
                    throw new System.ArgumentException($"{enumValue} is not a valid enum case.");
            }
        }
        internal static int GetBackingValue(this TimeOfDay enumValue)
        {
            switch (enumValue)
            {
                case TimeOfDay.Morning:
                    return 0;
                case TimeOfDay.Evening:
                    return 1;
                default:
                    throw new System.ArgumentException($"{enumValue} is not a valid enum case.");
            }
        }
    }
    [System.CodeDom.Compiler.GeneratedCode("YarnSpinner", "3.2.4.0")]
    public static class DayEnumHelper
    {
        public static bool TryGetDayFromBacking(int backingInput, out Day day)
        {
            switch (backingInput)
            {
                case 0:
                    day = Day.Monday;
                    return true;
                case 1:
                    day = Day.Tuesday;
                    return true;
                case 2:
                    day = Day.Wednesday;
                    return true;
            }

            day = default;
            return false;
        }
        public static bool TryGetDayByName(string name, out Day day)
        {
            // because in yarn both EnumType.Value and .Value are valid forms of the enum we need to test if we are the short or full form first.
            if (name.StartsWith('.'))
            {
                name = $"Day{name}";
            }

            switch (name)
            {
                case "Day.Monday":
                    day = Day.Monday;
                    return true;
                case "Day.Tuesday":
                    day = Day.Tuesday;
                    return true;
                case "Day.Wednesday":
                    day = Day.Wednesday;
                    return true;
            }

            day = default;
            return false;
        }
    }
    [System.CodeDom.Compiler.GeneratedCode("YarnSpinner", "3.2.4.0")]
    public static class TimeOfDayEnumHelper
    {
        public static bool TryGetTimeOfDayFromBacking(int backingInput, out TimeOfDay timeofday)
        {
            switch (backingInput)
            {
                case 0:
                    timeofday = TimeOfDay.Morning;
                    return true;
                case 1:
                    timeofday = TimeOfDay.Evening;
                    return true;
            }

            timeofday = default;
            return false;
        }
        public static bool TryGetTimeOfDayByName(string name, out TimeOfDay timeofday)
        {
            // because in yarn both EnumType.Value and .Value are valid forms of the enum we need to test if we are the short or full form first.
            if (name.StartsWith('.'))
            {
                name = $"TimeOfDay{name}";
            }

            switch (name)
            {
                case "TimeOfDay.Morning":
                    timeofday = TimeOfDay.Morning;
                    return true;
                case "TimeOfDay.Evening":
                    timeofday = TimeOfDay.Evening;
                    return true;
            }

            timeofday = default;
            return false;
        }
    }
    [System.CodeDom.Compiler.GeneratedCode("YarnSpinner", "3.2.4.0")]
    public partial class BasicSaliencyVariableStorage : Yarn.Unity.InMemoryVariableStorage, Yarn.Unity.IGeneratedVariableStorage
    {
        // Accessor for Day $day
        /// <summary>
        /// the current day
        /// </summary>
        public Day Day
        {
            get => this.GetEnumValueOrDefault<Day>("$day");
            set => this.SetValue("$day", value.GetBackingValue());
        }

        // Accessor for TimeOfDay $time
        /// <summary>
        /// the current time
        /// </summary>
        public TimeOfDay Time
        {
            get => this.GetEnumValueOrDefault<TimeOfDay>("$time");
            set => this.SetValue("$time", value.GetBackingValue());
        }

        // Accessor for Number $gold
        /// <summary>
        /// the number of gold coins the player has
        /// </summary>
        public float Gold
        {
            get => this.GetValueOrDefault<float>("$gold");
            set => this.SetValue<float>("$gold", value);
        }

    }
}
