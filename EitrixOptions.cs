using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Eitrix
{
    /// <summary>
    /// A class for storing the game options
    /// </summary>
    public class EitrixOptions
    {
        // !!!!! ALERT:  CHANGE THIS EVERY TIME YOU CHANGE THE SCHEMA FOR OPTIONS !!!!!
        static string optionsFileName = "Options20101218";

        public IntOption SpecialLifetimeSeconds;
        public IntOption PowerLifetimeSeconds;
        public IntOption SecondsBetweenSpecials;
        public IntOption AntidotesAtStart;
        public PercentOption AntidoteFrequency;
        public IntOption RoundsPerTourney;
        public SpeedupRateOption SpeedupRate;
        public BoolOption TwoPlayersPerController;
        public IntOption MusicVolume;
        public IntOption SoundEffectVolume;
        public PercentOption ExpandGraphics;
        public BoolOption ExtraPieces;
        public BoolOption CooperativePlay;


        List<IOption> options = new List<IOption>();

        [XmlIgnore]
        public List<IOption> OptionList { get { return options; } }

        public EitrixOptions()
        {
            Init();
        }

        public void Init()
        {
            SpecialLifetimeSeconds = new IntOption("Special Lifetime (s)", 12, 1, 30, 1);
            PowerLifetimeSeconds = new IntOption("Power Lifetime (s)", 10, 1, 30, 1);
            SecondsBetweenSpecials = new IntOption("Seconds Between Specials", 8, 0, 30, 1);
            AntidotesAtStart = new IntOption("Antidotes At Start", 1, 0, 4, 1);
            AntidoteFrequency = new PercentOption("Antidotes Frequency", 50, 0, 100, 10);
            SpeedupRate = new SpeedupRateOption("SpeedupRate", 1);
            RoundsPerTourney = new IntOption("Rounds Per Tourney", 5, 1, 100, 1);
            TwoPlayersPerController = new BoolOption("Two Players Per Controller", false);
            MusicVolume = new IntOption("Music Volume", 5, 0, 10, 1);
            SoundEffectVolume = new IntOption("SoundEffects Volume", 7, 0, 10, 1);
            ExpandGraphics = new PercentOption("ExpandGraphics", 50, 0, 100, 10);
            ExtraPieces = new BoolOption("Extra Pieces", true);
            CooperativePlay = new BoolOption("Cooperative Play", false);
        }

        public void SetupList()
        {
            options = new List<IOption>();
            options.Add(SpecialLifetimeSeconds);
            options.Add(PowerLifetimeSeconds);
            options.Add(SecondsBetweenSpecials);
            options.Add(AntidotesAtStart);
            options.Add(AntidoteFrequency);
            options.Add(SpeedupRate);
            options.Add(RoundsPerTourney);
            options.Add(TwoPlayersPerController);
            options.Add(MusicVolume);
            options.Add(SoundEffectVolume);
            if(Globals.LowResolution) options.Add(ExpandGraphics);
            options.Add(ExtraPieces);
            options.Add(CooperativePlay);
            options.Add(new ConfigureKeyboard());
            options.Add(new ResetOptions());
        }

        public void SaveToDisk()
        {
            try
            {
                DiskStorage.SaveToFile(optionsFileName, (stream) =>
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EitrixOptions));
                    serializer.Serialize(stream, this);
                });
            }
            catch (Exception) { }

        }


        public static EitrixOptions ReadFromDisk()
        {
            EitrixOptions newOptions = new EitrixOptions();
            try
            {
                DiskStorage.ReadFromFile(optionsFileName, (stream) =>
                {
                    if (stream.Length > 0)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(EitrixOptions));
                        newOptions = (EitrixOptions)serializer.Deserialize(stream);
                    }
                });

            }
            catch (Exception) { }

            newOptions.SetupList();
            return newOptions;

        }
    }


    public interface IOption
    {
        string Name { get; set; }
        string ValueString { get; }
        void Increment();
        void Decrement();
        void Activate();
    }

    ///--------------------------------------------------------------
    /// <summary>
    /// Int Option
    /// </summary>
    ///--------------------------------------------------------------
    [Serializable]
    public class PercentOption : IOption
    {
        public int myValue;

        public string Name { get; set; }
        public string ValueString { get { return myValue.ToString() + "%"; } }


        public int min, max, step;

        public PercentOption() { } // for serialization
        public PercentOption(string name, int initialValue, int min, int max, int step)
        {
            Name = name;
            myValue = initialValue;
            this.min = min;
            this.max = max;
            this.step = step;
        }

        public void Activate() { }

        public void Increment()
        {
            myValue += step;
            if (myValue >= max) myValue = max;

        }
        public void Decrement()
        {
            myValue -= step;
            if (myValue <= min) myValue = min;
        }

        static public implicit operator float(PercentOption option)
        {
            return (float)option.myValue / 100f;
        }
        static public implicit operator double(PercentOption option)
        {
            return (double)option.myValue / 100.0;
        }

    }

    ///--------------------------------------------------------------
    /// <summary>
    /// Int Option
    /// </summary>
    ///--------------------------------------------------------------
    [Serializable]
    public class IntOption : IOption
    {
        public int myValue;

        public string Name { get; set; }
        public string ValueString { get { return myValue.ToString(); } }


        public int min, max, step;

        public IntOption() { } // for serialization
        public IntOption(string name, int initialValue, int min, int max, int step)
        {
            Name = name;
            myValue = initialValue;
            this.min = min;
            this.max = max;
            this.step = step;
        }

        public void Activate() { }

        public void Increment()
        {
            myValue += step;
            if (myValue >= max) myValue = max;

        }
        public void Decrement()
        {
            myValue -= step;
            if (myValue <= min) myValue = min;
        }

        static public implicit operator int(IntOption option)
        {
            return option.myValue;
        }
        static public implicit operator float(IntOption option)
        {
            return (float)option.myValue;
        }
        static public implicit operator double(IntOption option)
        {
            return (double)option.myValue;
        }

    }
    ///--------------------------------------------------------------
    /// <summary>
    /// Int Option
    /// </summary>
    ///--------------------------------------------------------------
    [Serializable]
    public class BoolOption : IOption
    {
        public bool myValue;

        public string Name { get; set; }
        public string ValueString { get { return myValue ? "On" : "Off"; } }


        public BoolOption() { } // for serialization
        public BoolOption(string name, bool initialValue)
        {
            Name = name;
            myValue = initialValue;
        }

        public void Activate() { }

        public void Increment()
        {
            myValue = !myValue;
        }
        public void Decrement()
        {
            myValue = !myValue;
        }

        static public implicit operator bool(BoolOption option)
        {
            return option.myValue;
        }

    }

    public class Tuple<T1, T2>
    {
        public T1 FirstValue;
        public T2 SecondValue;

        public Tuple() { }
        public Tuple(T1 firstValue, T2 secondValue)
        {
            this.FirstValue = firstValue;
            this.SecondValue = secondValue;
        }
    }

    ///--------------------------------------------------------------
    /// <summary>
    /// Int Option
    /// </summary>
    ///--------------------------------------------------------------
    [Serializable]
    public class SpeedupRateOption : IOption
    {
        public int myValue;

        static Tuple<string, double>[] values = new Tuple<string, double>[]{
            new Tuple<string, double>(){ FirstValue = "Slow", SecondValue = 0.5 },
            new Tuple<string, double>(){ FirstValue = "Moderate", SecondValue = 1 },
            new Tuple<string, double>(){ FirstValue = "Fast", SecondValue = 2 },
        };
        

        public string Name { get; set; }
        public string ValueString 
        {
            get { return values[myValue].FirstValue; }
        }


        public int min, max, step;

        public SpeedupRateOption() { } // for serialization
        public SpeedupRateOption(string name, int initialValue)
        {
            Name = name;
            myValue = initialValue;
            this.min = 0;
            this.max = values.Length - 1;
            this.step = 1;
            if (initialValue > max) initialValue = max;
        }

        public void Activate() { }

        public void Increment()
        {
            myValue += step;
            if (myValue >= max) myValue = max;

        }
        public void Decrement()
        {
            myValue -= step;
            if (myValue <= min) myValue = min;
        }

        static public implicit operator float(SpeedupRateOption option)
        {
            return (float)values[option.myValue].SecondValue;
        }

        static public implicit operator double(SpeedupRateOption option)
        {
            return (double)values[option.myValue].SecondValue;
        }

    }


    ///--------------------------------------------------------------
    /// <summary>
    /// This is a special tool for resetting options
    /// </summary>
    ///--------------------------------------------------------------
    public class ResetOptions : IOption
    {
        public string Name { get { return "Reset Options To Defaults"; } set { } }
        public string ValueString
        {
            get { return ""; }
        }


        public int min, max, step;

        public ResetOptions() { }

        public void Increment()
        {
        }

        public void Decrement()
        {
        }

        public void Activate()
        {
            Globals.Options.Init();
            Globals.Options.SetupList();
        }

    }

    ///--------------------------------------------------------------
    /// <summary>
    /// This is a special tool for resetting options
    /// </summary>
    ///--------------------------------------------------------------
    public class ConfigureKeyboard : IOption
    {
        public string Name { get { return "Configure Keyboard"; } set { } }
        public string ValueString
        {
            get { return ""; }
        }


        public int min, max, step;

        public ConfigureKeyboard() { }

        public void Increment()
        {
        }

        public void Decrement()
        {
        }

        public void Activate()
        {
            OptionsScreen.ConfigureKeyboard();
        }

    }

}
