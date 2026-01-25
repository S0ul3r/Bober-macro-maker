using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WWMBoberRotations.Models
{
    public class ComboAction
    {
        private int _duration;
        private int _delayAfter;

        [JsonConverter(typeof(StringEnumConverter))]
        public ActionType Type { get; set; }
        
        public string? Key { get; set; }
        
        public int Duration 
        { 
            get => _duration;
            set => _duration = Math.Max(0, value);
        }
        
        public int DelayAfter
        {
            get => _delayAfter;
            set => _delayAfter = Math.Max(0, value);
        }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public MouseButton Button { get; set; }

        public override string ToString()
        {
            var baseText = Type switch
            {
                ActionType.KeyPress => $"Press: {Key}",
                ActionType.KeyHold => $"Hold: {Key} for {Duration}ms",
                ActionType.MouseClick => Button switch
                {
                    MouseButton.XButton1 => "Click: Mouse 4",
                    MouseButton.XButton2 => "Click: Mouse 5",
                    _ => $"Click: {Button} Mouse Button"
                },
                ActionType.Delay => $"Delay: {Duration}ms",
                _ => "Unknown Action"
            };
            
            if (DelayAfter > 0 && Type != ActionType.Delay)
            {
                baseText += $", delay {DelayAfter}ms";
            }
            
            return baseText;
        }
    }
}
