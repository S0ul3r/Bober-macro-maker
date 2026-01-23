using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WWMBoberRotations.Models
{
    public class ComboAction
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionType Type { get; set; }
        
        public string? Key { get; set; }
        
        public int Duration { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public MouseButton Button { get; set; }

        public override string ToString()
        {
            return Type switch
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
        }
    }
}
